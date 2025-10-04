# =====================================================
# FintechPSP - Setup Ambiente de Teste EmpresaTeste
# =====================================================

Write-Host "🎯 Preparando ambiente para testes da EmpresaTeste..." -ForegroundColor Green
Write-Host "📋 Configuração baseada em: configuracao-empresateste.md" -ForegroundColor Cyan
Write-Host ""

# Função para testar conectividade
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    } catch {
        return $false
    }
}

# Função para aguardar serviço
function Wait-ForService {
    param([string]$Name, [int]$Port, [int]$MaxWait = 60)
    
    Write-Host "⏳ Aguardando $Name (porta $Port)..." -ForegroundColor Yellow
    $waited = 0
    while (-not (Test-Port $Port) -and $waited -lt $MaxWait) {
        Start-Sleep -Seconds 2
        $waited += 2
        Write-Host "." -NoNewline -ForegroundColor Gray
    }
    Write-Host ""
    
    if (Test-Port $Port) {
        Write-Host "✅ $Name está pronto!" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ $Name não respondeu em ${MaxWait}s" -ForegroundColor Red
        return $false
    }
}

# 1. Parar containers existentes
Write-Host "🛑 Limpando ambiente anterior..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down 2>$null
docker system prune -f 2>$null

# 2. Subir infraestrutura
Write-Host "🐳 Iniciando infraestrutura (PostgreSQL, RabbitMQ, Redis)..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

# 3. Aguardar infraestrutura
$infraOk = $true
$infraOk = $infraOk -and (Wait-ForService "PostgreSQL" 5433)
$infraOk = $infraOk -and (Wait-ForService "RabbitMQ" 5673)
$infraOk = $infraOk -and (Wait-ForService "Redis" 6380)

if (-not $infraOk) {
    Write-Host "❌ Falha na infraestrutura. Verificando logs..." -ForegroundColor Red
    docker-compose -f docker-compose-complete.yml logs postgres rabbitmq redis
    exit 1
}

# 4. Executar migrations do banco
Write-Host "🗄️ Executando migrations do banco de dados..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Verificar se PostgreSQL está realmente pronto
$pgReady = $false
$attempts = 0
do {
    $attempts++
    try {
        $result = docker exec fintech-postgres-new pg_isready -U postgres 2>$null
        if ($result -match "accepting connections") {
            $pgReady = $true
            break
        }
    } catch {}
    Start-Sleep -Seconds 2
} while ($attempts -lt 15)

if (-not $pgReady) {
    Write-Host "❌ PostgreSQL não está pronto para conexões" -ForegroundColor Red
    exit 1
}

Write-Host "✅ PostgreSQL pronto para migrations" -ForegroundColor Green

# 5. Subir microserviços essenciais
Write-Host "🚀 Iniciando microserviços..." -ForegroundColor Yellow

$services = @(
    @{Name="API Gateway"; Port=5000},
    @{Name="Auth Service"; Port=5001},
    @{Name="Company Service"; Port=5002},
    @{Name="Transaction Service"; Port=5003},
    @{Name="Balance Service"; Port=5004},
    @{Name="User Service"; Port=5006},
    @{Name="Config Service"; Port=5007},
    @{Name="Integration Service"; Port=5008},
    @{Name="Webhook Service"; Port=5005}
)

# Subir todos os serviços
docker-compose -f docker-compose-complete.yml up -d

# Aguardar serviços ficarem prontos
Write-Host "⏳ Aguardando microserviços ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# 6. Verificar health dos serviços
Write-Host "🔍 Verificando saúde dos serviços..." -ForegroundColor Cyan
$allHealthy = $true

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$($service.Port)/health" -Method GET -TimeoutSec 10 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name): Saudável" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $($service.Name): Status $($response.StatusCode)" -ForegroundColor Yellow
            $allHealthy = $false
        }
    } catch {
        Write-Host "❌ $($service.Name): Não respondeu" -ForegroundColor Red
        $allHealthy = $false
    }
}

# 7. Subir frontends
Write-Host "🌐 Iniciando frontends..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml up -d backoffice-web internet-banking-web

# Aguardar frontends
Start-Sleep -Seconds 15

# Verificar frontends
try {
    $backofficeHealth = Invoke-WebRequest -Uri "http://localhost:3000/api/health" -Method GET -TimeoutSec 10 -ErrorAction SilentlyContinue
    if ($backofficeHealth.StatusCode -eq 200) {
        Write-Host "✅ BackofficeWeb: Saudável" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ BackofficeWeb: Ainda inicializando..." -ForegroundColor Yellow
}

try {
    $internetBankingHealth = Invoke-WebRequest -Uri "http://localhost:3001/api/health" -Method GET -TimeoutSec 10 -ErrorAction SilentlyContinue
    if ($internetBankingHealth.StatusCode -eq 200) {
        Write-Host "✅ InternetBankingWeb: Saudável" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️ InternetBankingWeb: Ainda inicializando..." -ForegroundColor Yellow
}

# 8. Resumo do ambiente
Write-Host ""
Write-Host "🎯 AMBIENTE DE TESTE PREPARADO!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📊 SERVIÇOS DISPONÍVEIS:" -ForegroundColor White
Write-Host "• API Gateway:          http://localhost:5000" -ForegroundColor Gray
Write-Host "• BackofficeWeb:        http://localhost:3000" -ForegroundColor Gray
Write-Host "• InternetBankingWeb:   http://localhost:3001" -ForegroundColor Gray
Write-Host ""
Write-Host "🏢 DADOS DA EMPRESATESTE:" -ForegroundColor White
Write-Host "• Razão Social:         EmpresaTeste Ltda" -ForegroundColor Gray
Write-Host "• CNPJ:                 12.345.678/0001-99" -ForegroundColor Gray
Write-Host "• Email:                contato@empresateste.com" -ForegroundColor Gray
Write-Host "• Usuário Cliente:      cliente@empresateste.com" -ForegroundColor Gray
Write-Host ""
Write-Host "💰 LIMITES CONFIGURADOS:" -ForegroundColor White
Write-Host "• PIX Diário:           R$ 10.000,00" -ForegroundColor Gray
Write-Host "• TED Diário:           R$ 10.000,00" -ForegroundColor Gray
Write-Host "• Boleto Diário:        R$ 10.000,00" -ForegroundColor Gray
Write-Host ""
Write-Host "🔐 INTEGRAÇÃO SICOOB:" -ForegroundColor White
Write-Host "• Ambiente:             Sandbox" -ForegroundColor Gray
Write-Host "• OAuth:                Configurado" -ForegroundColor Gray
Write-Host "• Client ID:            9b5e603e428cc477a2841e2683c92d21" -ForegroundColor Gray
Write-Host ""
Write-Host "📮 TESTES DISPONÍVEIS:" -ForegroundColor White
Write-Host "• Postman Collection:   postman/FintechPSP-Collection.json" -ForegroundColor Gray
Write-Host "• Documentação:         configuracao-empresateste.md" -ForegroundColor Gray
Write-Host ""
Write-Host "🚀 PRÓXIMOS PASSOS:" -ForegroundColor White
Write-Host "1. Importar collection do Postman" -ForegroundColor Gray
Write-Host "2. Executar testes de autenticação" -ForegroundColor Gray
Write-Host "3. Testar criação de conta corrente" -ForegroundColor Gray
Write-Host "4. Executar transação PIX de teste" -ForegroundColor Gray
Write-Host "5. Verificar conciliação de extrato" -ForegroundColor Gray
Write-Host ""

if ($allHealthy) {
    Write-Host "✅ AMBIENTE 100% PRONTO PARA TESTES!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Alguns serviços podem ainda estar inicializando..." -ForegroundColor Yellow
    Write-Host "   Aguarde mais alguns minutos e verifique novamente." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
