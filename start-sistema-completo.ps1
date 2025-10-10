# ========================================
# SCRIPT PARA SUBIR SISTEMA COMPLETO
# ========================================

Write-Host "🚀 INICIANDO SISTEMA FINTECH PSP COMPLETO" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Função para iniciar serviço em nova janela
function Start-ServiceInNewWindow {
    param($ServiceName, $ProjectPath, $Port, $Color = "White")
    
    Write-Host "🔄 Iniciando $ServiceName na porta $Port..." -ForegroundColor $Color
    
    $command = "dotnet run --project $ProjectPath --urls `"http://localhost:$Port`""
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host '🚀 $ServiceName - Porta $Port' -ForegroundColor $Color; $command"
    
    Start-Sleep 3
}

# Função para testar se serviço está respondendo
function Test-ServiceHealth {
    param($ServiceName, $Url, $MaxRetries = 5)
    
    for ($i = 1; $i -le $MaxRetries; $i++) {
        try {
            $response = Invoke-RestMethod -Uri $Url -Method GET -TimeoutSec 3 -ErrorAction Stop
            Write-Host "✅ $ServiceName: ONLINE" -ForegroundColor Green
            return $true
        } catch {
            Write-Host "⏳ $ServiceName: Tentativa $i/$MaxRetries..." -ForegroundColor Yellow
            Start-Sleep 2
        }
    }
    Write-Host "❌ $ServiceName: OFFLINE após $MaxRetries tentativas" -ForegroundColor Red
    return $false
}

Write-Host "📋 FASE 1: VERIFICANDO INFRAESTRUTURA" -ForegroundColor Yellow
Write-Host ""

# Verificar PostgreSQL
try {
    $dbTest = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT 1;" 2>$null
    if ($dbTest -match "1 row") {
        Write-Host "✅ PostgreSQL: ONLINE" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL: PROBLEMA" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "❌ PostgreSQL: OFFLINE" -ForegroundColor Red
    exit 1
}

# Verificar Redis
try {
    $redisTest = docker exec fintech-redis redis-cli ping 2>$null
    if ($redisTest -match "PONG") {
        Write-Host "✅ Redis: ONLINE" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis: PROBLEMA" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Redis: OFFLINE" -ForegroundColor Red
}

# Verificar RabbitMQ
try {
    $rabbitTest = docker exec fintech-rabbitmq rabbitmqctl status 2>$null
    if ($rabbitTest -match "running") {
        Write-Host "✅ RabbitMQ: ONLINE" -ForegroundColor Green
    } else {
        Write-Host "❌ RabbitMQ: PROBLEMA" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ RabbitMQ: OFFLINE" -ForegroundColor Red
}

Write-Host ""
Write-Host "📋 FASE 2: INICIANDO MICROSERVIÇOS" -ForegroundColor Yellow
Write-Host ""

# Array com os serviços na ordem de dependência
$services = @(
    @{ Name = "AuthService"; Path = "src/Services/FintechPSP.AuthService/FintechPSP.AuthService.csproj"; Port = "5001"; Color = "Green" },
    @{ Name = "ConfigService"; Path = "src/Services/FintechPSP.ConfigService/FintechPSP.ConfigService.csproj"; Port = "5007"; Color = "Yellow" },
    @{ Name = "UserService"; Path = "src/Services/FintechPSP.UserService/FintechPSP.UserService.csproj"; Port = "5006"; Color = "Cyan" },
    @{ Name = "CompanyService"; Path = "src/Services/FintechPSP.CompanyService/FintechPSP.CompanyService.csproj"; Port = "5009"; Color = "Magenta" },
    @{ Name = "BalanceService"; Path = "src/Services/FintechPSP.BalanceService/FintechPSP.BalanceService.csproj"; Port = "5003"; Color = "Blue" },
    @{ Name = "IntegrationService"; Path = "src/Services/FintechPSP.IntegrationService/FintechPSP.IntegrationService.csproj"; Port = "5005"; Color = "DarkGreen" },
    @{ Name = "TransactionService"; Path = "src/Services/FintechPSP.TransactionService/FintechPSP.TransactionService.csproj"; Port = "5002"; Color = "Red" },
    @{ Name = "WebhookService"; Path = "src/Services/FintechPSP.WebhookService/FintechPSP.WebhookService.csproj"; Port = "5004"; Color = "DarkYellow" }
)

# Iniciar cada serviço
foreach ($service in $services) {
    Start-ServiceInNewWindow $service.Name $service.Path $service.Port $service.Color
}

Write-Host ""
Write-Host "📋 FASE 3: INICIANDO API GATEWAY" -ForegroundColor Yellow
Write-Host ""

# Iniciar API Gateway
Start-ServiceInNewWindow "APIGateway" "src/Gateway/FintechPSP.APIGateway/FintechPSP.APIGateway.csproj" "5000" "Blue"

Write-Host ""
Write-Host "📋 FASE 4: AGUARDANDO INICIALIZAÇÃO" -ForegroundColor Yellow
Write-Host ""

Write-Host "⏳ Aguardando 30 segundos para inicialização completa..." -ForegroundColor Gray
Start-Sleep 30

Write-Host ""
Write-Host "📋 FASE 5: TESTANDO CONECTIVIDADE" -ForegroundColor Yellow
Write-Host ""

# Testar cada serviço
$healthChecks = @(
    @{ Name = "AuthService"; Url = "http://localhost:5001/" },
    @{ Name = "BalanceService"; Url = "http://localhost:5003/" },
    @{ Name = "IntegrationService"; Url = "http://localhost:5005/" },
    @{ Name = "UserService"; Url = "http://localhost:5006/" },
    @{ Name = "ConfigService"; Url = "http://localhost:5007/" },
    @{ Name = "CompanyService"; Url = "http://localhost:5009/" },
    @{ Name = "APIGateway"; Url = "http://localhost:5000/health" }
)

$onlineServices = 0
foreach ($check in $healthChecks) {
    if (Test-ServiceHealth $check.Name $check.Url) {
        $onlineServices++
    }
}

Write-Host ""
Write-Host "📊 RESUMO FINAL" -ForegroundColor Cyan
Write-Host "===============" -ForegroundColor Cyan
Write-Host "Serviços Online: $onlineServices/$($healthChecks.Count)" -ForegroundColor White
$successRate = [math]::Round(($onlineServices / $healthChecks.Count) * 100, 2)
Write-Host "Taxa de Sucesso: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } else { "Yellow" })
Write-Host ""

if ($successRate -ge 80) {
    Write-Host "🎉 SISTEMA FINTECH PSP INICIADO COM SUCESSO!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 URLs Principais:" -ForegroundColor Cyan
    Write-Host "   API Gateway:     http://localhost:5000" -ForegroundColor White
    Write-Host "   Auth Service:    http://localhost:5001" -ForegroundColor White
    Write-Host "   Balance Service: http://localhost:5003" -ForegroundColor White
    Write-Host "   Integration:     http://localhost:5005" -ForegroundColor White
    Write-Host ""
    Write-Host "📋 Próximos Passos:" -ForegroundColor Yellow
    Write-Host "   1. Execute: .\test-trilha-completa-e2e.ps1" -ForegroundColor White
    Write-Host "   2. Execute: .\monitor-sistema-psp.ps1 -ContinuousMode" -ForegroundColor White
    Write-Host "   3. Acesse: http://localhost:5000/health" -ForegroundColor White
} else {
    Write-Host "⚠️ ALGUNS SERVIÇOS NÃO INICIARAM CORRETAMENTE" -ForegroundColor Yellow
    Write-Host "Verifique os logs nas janelas abertas e tente novamente." -ForegroundColor Gray
}

Write-Host ""
Write-Host "✅ Script de inicialização concluído!" -ForegroundColor Green
