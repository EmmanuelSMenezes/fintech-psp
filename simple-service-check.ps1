# Simple Service Status Check
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$IntegrationUrl = "http://localhost:5005", 
    [string]$TransactionUrl = "http://localhost:5002"
)

$ErrorActionPreference = "Continue"

Write-Host "VERIFICANDO STATUS DOS SERVICOS" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green
Write-Host ""

function Test-Service {
    param([string]$Name, [string]$Url)
    
    Write-Host "Testando $Name..." -ForegroundColor Yellow -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri "$Url/health" -Method GET -TimeoutSec 5
        
        if ($response.StatusCode -eq 200) {
            Write-Host " ONLINE" -ForegroundColor Green
            return $true
        }
        else {
            Write-Host " OFFLINE (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host " OFFLINE (Erro: $($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

# Verificar serviços
$services = @(
    @{ Name = "API Gateway"; Url = $BaseUrl },
    @{ Name = "Transaction Service"; Url = $TransactionUrl },
    @{ Name = "Integration Service"; Url = $IntegrationUrl }
)

$onlineServices = 0
foreach ($service in $services) {
    if (Test-Service -Name $service.Name -Url $service.Url) {
        $onlineServices++
    }
}

Write-Host ""
Write-Host "RESUMO: $onlineServices/$($services.Count) servicos online" -ForegroundColor Cyan

if ($onlineServices -eq $services.Count) {
    Write-Host "Todos os servicos estao prontos para teste!" -ForegroundColor Green
    Write-Host "Execute: .\test-qrcode-dinamico-sicoob.ps1" -ForegroundColor White
}
else {
    Write-Host "Alguns servicos estao offline. Execute:" -ForegroundColor Yellow
    Write-Host "docker-compose -f docker-compose-complete.yml up -d" -ForegroundColor White
}

Write-Host ""
Write-Host "URLs dos Servicos:" -ForegroundColor Cyan
Write-Host "API Gateway: $BaseUrl" -ForegroundColor Gray
Write-Host "Transaction Service: $TransactionUrl" -ForegroundColor Gray
Write-Host "Integration Service: $IntegrationUrl" -ForegroundColor Gray
