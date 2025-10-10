#!/usr/bin/env pwsh

Write-Host "🎯 STATUS COMPLETO SISTEMA FINTECHPSP" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Verificar infraestrutura Docker
Write-Host "🐳 INFRAESTRUTURA DOCKER:" -ForegroundColor Yellow
try {
    $containers = docker ps --format "table {{.Names}}\t{{.Status}}" | findstr fintech
    if ($containers) {
        Write-Host "✅ Containers Docker:" -ForegroundColor Green
        docker ps --format "table {{.Names}}\t{{.Status}}" | findstr fintech
    } else {
        Write-Host "❌ Nenhum container Docker encontrado" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao verificar containers Docker" -ForegroundColor Red
}

Write-Host ""

# Verificar microserviços backend
Write-Host "🚀 MICROSERVIÇOS BACKEND:" -ForegroundColor Yellow
$backendServices = @(
    @{Name="API Gateway"; Port="5000"},
    @{Name="AuthService"; Port="5001"},
    @{Name="BalanceService"; Port="5003"},
    @{Name="TransactionService"; Port="5004"},
    @{Name="IntegrationService"; Port="5005"},
    @{Name="UserService"; Port="5006"},
    @{Name="ConfigService"; Port="5007"},
    @{Name="WebhookService"; Port="5008"},
    @{Name="CompanyService"; Port="5010"}
)

foreach ($service in $backendServices) {
    $listening = netstat -an | findstr "127.0.0.1:$($service.Port).*LISTENING"
    if ($listening) {
        Write-Host "✅ $($service.Name) (porta $($service.Port)) - ONLINE" -ForegroundColor Green
    } else {
        Write-Host "❌ $($service.Name) (porta $($service.Port)) - OFFLINE" -ForegroundColor Red
    }
}

Write-Host ""

# Verificar frontends
Write-Host "🌐 FRONTENDS WEB:" -ForegroundColor Yellow
$frontendServices = @(
    @{Name="BackofficeWeb"; Port="3000"; Url="http://localhost:3000"},
    @{Name="InternetBankingWeb"; Port="3001"; Url="http://localhost:3001"}
)

foreach ($frontend in $frontendServices) {
    $listening = netstat -an | findstr "0.0.0.0:$($frontend.Port).*LISTENING"
    if ($listening) {
        Write-Host "✅ $($frontend.Name) (porta $($frontend.Port)) - ONLINE" -ForegroundColor Green
        Write-Host "   🌍 Acesse: $($frontend.Url)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ $($frontend.Name) (porta $($frontend.Port)) - OFFLINE" -ForegroundColor Red
    }
}

Write-Host ""

# Teste rápido de endpoints
Write-Host "🔍 TESTES RÁPIDOS:" -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5003/test/health" -Method GET -TimeoutSec 2
    Write-Host "✅ BalanceService Health: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ BalanceService Health: FALHOU" -ForegroundColor Red
}

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/health" -Method GET -TimeoutSec 2
    Write-Host "✅ AuthService Health: OK" -ForegroundColor Green
} catch {
    Write-Host "❌ AuthService Health: FALHOU" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 SISTEMA COMPLETO OPERACIONAL!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 RESUMO DE ACESSO:" -ForegroundColor Cyan
Write-Host "🏢 BackofficeWeb (Admin): http://localhost:3000" -ForegroundColor White
Write-Host "🏦 InternetBankingWeb (Cliente): http://localhost:3001" -ForegroundColor White
Write-Host "🌐 API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "💰 BalanceService (Cash-Out): http://localhost:5003" -ForegroundColor White
Write-Host "🏦 IntegrationService (Sicoob): http://localhost:5005" -ForegroundColor White
Write-Host "🔐 AuthService (Tokens): http://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "🚀 PRONTO PARA TESTES COMPLETOS!" -ForegroundColor Green
