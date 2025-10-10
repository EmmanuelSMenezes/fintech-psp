#!/usr/bin/env pwsh

Write-Host "VERIFICACAO SIMPLES DOS SERVICOS" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Função para testar serviço
function Test-Service {
    param(
        [string]$Name,
        [string]$Url
    )
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $Name - OK (Status: $($response.StatusCode))" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ $Name - ERRO (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "❌ $Name - OFFLINE (Erro: $($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

# Lista de serviços
$services = @(
    @{ Name = "API Gateway"; Url = "http://localhost:5000/" },
    @{ Name = "Auth Service"; Url = "http://localhost:5001/" },
    @{ Name = "Transaction Service"; Url = "http://localhost:5002/" },
    @{ Name = "Balance Service"; Url = "http://localhost:5003/" },
    @{ Name = "Integration Service"; Url = "http://localhost:5005/" },
    @{ Name = "User Service"; Url = "http://localhost:5006/" },
    @{ Name = "Config Service"; Url = "http://localhost:5007/" },
    @{ Name = "Webhook Service"; Url = "http://localhost:5008/" },
    @{ Name = "Company Service"; Url = "http://localhost:5009/" }
)

$onlineCount = 0
$totalCount = $services.Count

Write-Host "Testando $totalCount servicos..." -ForegroundColor Yellow
Write-Host ""

foreach ($service in $services) {
    if (Test-Service -Name $service.Name -Url $service.Url) {
        $onlineCount++
    }
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "RESUMO:" -ForegroundColor Cyan
Write-Host "Online: $onlineCount/$totalCount" -ForegroundColor Green
Write-Host "Offline: $($totalCount - $onlineCount)/$totalCount" -ForegroundColor Red

if ($onlineCount -eq $totalCount) {
    Write-Host ""
    Write-Host "🎉 TODOS OS SERVICOS ESTAO ONLINE!" -ForegroundColor Green
    Write-Host "Pronto para executar os testes QA" -ForegroundColor Cyan
} elseif ($onlineCount -gt 0) {
    Write-Host ""
    Write-Host "⚠️ ALGUNS SERVICOS OFFLINE" -ForegroundColor Yellow
    Write-Host "Execute: docker-compose -f docker-compose-complete.yml up -d" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ TODOS OS SERVICOS OFFLINE" -ForegroundColor Red
    Write-Host "Execute: docker-compose -f docker-compose-complete.yml up -d" -ForegroundColor White
}

Write-Host ""
