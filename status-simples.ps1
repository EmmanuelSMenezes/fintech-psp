#!/usr/bin/env pwsh

Write-Host "🎯 STATUS SISTEMA FINTECHPSP" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

# Verificar portas dos microserviços
Write-Host "🚀 MICROSERVIÇOS:" -ForegroundColor Yellow
$services = @(
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

foreach ($service in $services) {
    $listening = netstat -an | findstr "127.0.0.1:$($service.Port).*LISTENING"
    if ($listening) {
        Write-Host "✅ $($service.Name) (porta $($service.Port)) - ONLINE" -ForegroundColor Green
    } else {
        Write-Host "❌ $($service.Name) (porta $($service.Port)) - OFFLINE" -ForegroundColor Red
    }
}

Write-Host ""

# Verificar infraestrutura Docker
Write-Host "📊 INFRAESTRUTURA:" -ForegroundColor Yellow
try {
    $postgres = docker ps --filter "name=fintech-postgres" --format "{{.Status}}"
    if ($postgres) {
        Write-Host "✅ PostgreSQL: $postgres" -ForegroundColor Green
    } else {
        Write-Host "❌ PostgreSQL: OFFLINE" -ForegroundColor Red
    }
    
    $rabbitmq = docker ps --filter "name=fintech-rabbitmq" --format "{{.Status}}"
    if ($rabbitmq) {
        Write-Host "✅ RabbitMQ: $rabbitmq" -ForegroundColor Green
    } else {
        Write-Host "❌ RabbitMQ: OFFLINE" -ForegroundColor Red
    }
    
    $redis = docker ps --filter "name=fintech-redis" --format "{{.Status}}"
    if ($redis) {
        Write-Host "✅ Redis: $redis" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis: OFFLINE" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao verificar containers Docker" -ForegroundColor Red
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

Write-Host ""
Write-Host "🎯 SISTEMA PRONTO PARA TESTES!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 ENDPOINTS PRINCIPAIS:" -ForegroundColor Cyan
Write-Host "- BalanceService (Cash-Out): http://localhost:5003" -ForegroundColor White
Write-Host "- IntegrationService (Sicoob): http://localhost:5005" -ForegroundColor White
Write-Host "- AuthService (Tokens): http://localhost:5001" -ForegroundColor White
Write-Host "- API Gateway: http://localhost:5000" -ForegroundColor White
