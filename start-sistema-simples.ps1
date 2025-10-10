# Script para subir sistema completo
Write-Host "Iniciando Sistema FintechPSP Completo..." -ForegroundColor Cyan

# Funcao para iniciar servico
function Start-ServiceInNewWindow {
    param($ServiceName, $ProjectPath, $Port, $Color = "White")
    
    Write-Host "Iniciando $ServiceName na porta $Port..." -ForegroundColor $Color
    
    $command = "dotnet run --project $ProjectPath --urls `"http://localhost:$Port`""
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "Write-Host '$ServiceName - Porta $Port' -ForegroundColor $Color; $command"
    
    Start-Sleep 2
}

Write-Host "Verificando infraestrutura..." -ForegroundColor Yellow

# Verificar PostgreSQL
try {
    $dbTest = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT 1;" 2>$null
    if ($dbTest -match "1") {
        Write-Host "PostgreSQL: ONLINE" -ForegroundColor Green
    } else {
        Write-Host "PostgreSQL: PROBLEMA" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "PostgreSQL: OFFLINE" -ForegroundColor Red
    exit 1
}

Write-Host "Iniciando microservicos..." -ForegroundColor Yellow

# Servicos na ordem de dependencia
$services = @(
    @{ Name = "AuthService"; Path = "src/Services/FintechPSP.AuthService/FintechPSP.AuthService.csproj"; Port = "5001"; Color = "Green" },
    @{ Name = "ConfigService"; Path = "src/Services/FintechPSP.ConfigService/FintechPSP.ConfigService.csproj"; Port = "5007"; Color = "Yellow" },
    @{ Name = "UserService"; Path = "src/Services/FintechPSP.UserService/FintechPSP.UserService.csproj"; Port = "5006"; Color = "Cyan" },
    @{ Name = "CompanyService"; Path = "src/Services/FintechPSP.CompanyService/FintechPSP.CompanyService.csproj"; Port = "5009"; Color = "Magenta" },
    @{ Name = "BalanceService"; Path = "src/Services/FintechPSP.BalanceService/FintechPSP.BalanceService.csproj"; Port = "5003"; Color = "Blue" },
    @{ Name = "IntegrationService"; Path = "src/Services/FintechPSP.IntegrationService/FintechPSP.IntegrationService.csproj"; Port = "5005"; Color = "DarkGreen" }
)

# Iniciar cada servico
foreach ($service in $services) {
    Start-ServiceInNewWindow $service.Name $service.Path $service.Port $service.Color
}

Write-Host "Iniciando API Gateway..." -ForegroundColor Yellow
Start-ServiceInNewWindow "APIGateway" "src/Gateway/FintechPSP.APIGateway/FintechPSP.APIGateway.csproj" "5000" "Blue"

Write-Host "Aguardando inicializacao (30 segundos)..." -ForegroundColor Gray
Start-Sleep 30

Write-Host "Testando conectividade..." -ForegroundColor Yellow

# Testar servicos
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
    try {
        $response = Invoke-RestMethod -Uri $check.Url -Method GET -TimeoutSec 3 -ErrorAction Stop
        Write-Host "$($check.Name): ONLINE" -ForegroundColor Green
        $onlineServices++
    } catch {
        Write-Host "$($check.Name): OFFLINE" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "RESUMO FINAL" -ForegroundColor Cyan
Write-Host "Servicos Online: $onlineServices/$($healthChecks.Count)" -ForegroundColor White
$successRate = [math]::Round(($onlineServices / $healthChecks.Count) * 100, 2)
Write-Host "Taxa de Sucesso: $successRate%" -ForegroundColor White

if ($successRate -ge 70) {
    Write-Host "SISTEMA INICIADO COM SUCESSO!" -ForegroundColor Green
    Write-Host ""
    Write-Host "URLs Principais:" -ForegroundColor Cyan
    Write-Host "  API Gateway:     http://localhost:5000" -ForegroundColor White
    Write-Host "  Auth Service:    http://localhost:5001" -ForegroundColor White
    Write-Host "  Balance Service: http://localhost:5003" -ForegroundColor White
    Write-Host "  Integration:     http://localhost:5005" -ForegroundColor White
    Write-Host ""
    Write-Host "Proximos Passos:" -ForegroundColor Yellow
    Write-Host "  1. Execute: .\test-trilha-completa-e2e.ps1" -ForegroundColor White
    Write-Host "  2. Execute: .\monitor-sistema-psp.ps1" -ForegroundColor White
} else {
    Write-Host "ALGUNS SERVICOS NAO INICIARAM" -ForegroundColor Yellow
    Write-Host "Verifique os logs nas janelas abertas." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Script concluido!" -ForegroundColor Green
