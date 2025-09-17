# =====================================================
# FintechPSP - Script para iniciar todos os serviços
# =====================================================

Write-Host "🚀 Iniciando FintechPSP Microservices..." -ForegroundColor Green

# Verificar se a infraestrutura está rodando
Write-Host "📋 Verificando infraestrutura..." -ForegroundColor Yellow
$containers = docker ps --format "table {{.Names}}" | Select-String -Pattern "(postgres|rabbitmq|redis)"
if ($containers.Count -lt 3) {
    Write-Host "❌ Infraestrutura não está completa. Iniciando..." -ForegroundColor Red
    docker-compose -f docker/docker-compose-infra.yml up -d
    Start-Sleep -Seconds 10
}

Write-Host "✅ Infraestrutura OK!" -ForegroundColor Green

# Configurar variáveis de ambiente
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "https://localhost:7001;http://localhost:5000"

# Array com os serviços e suas portas
$services = @(
    @{ Name = "APIGateway"; Path = "src/Gateway/FintechPSP.APIGateway"; Port = "5000"; HttpsPort = "7001" },
    @{ Name = "AuthService"; Path = "src/Services/FintechPSP.AuthService"; Port = "5001"; HttpsPort = "7002" },
    @{ Name = "TransactionService"; Path = "src/Services/FintechPSP.TransactionService"; Port = "5002"; HttpsPort = "7003" },
    @{ Name = "BalanceService"; Path = "src/Services/FintechPSP.BalanceService"; Port = "5003"; HttpsPort = "7004" },
    @{ Name = "WebhookService"; Path = "src/Services/FintechPSP.WebhookService"; Port = "5004"; HttpsPort = "7005" },
    @{ Name = "IntegrationService"; Path = "src/Services/FintechPSP.IntegrationService"; Port = "5005"; HttpsPort = "7006" },
    @{ Name = "UserService"; Path = "src/Services/FintechPSP.UserService"; Port = "5006"; HttpsPort = "7007" },
    @{ Name = "ConfigService"; Path = "src/Services/FintechPSP.ConfigService"; Port = "5007"; HttpsPort = "7008" }
)

# Função para iniciar um serviço
function Start-Service($service) {
    Write-Host "🔄 Iniciando $($service.Name) na porta $($service.Port)..." -ForegroundColor Cyan
    
    $env:ASPNETCORE_URLS = "https://localhost:$($service.HttpsPort);http://localhost:$($service.Port)"
    
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", $service.Path -WindowStyle Minimized
    Start-Sleep -Seconds 3
}

# Iniciar todos os serviços
foreach ($service in $services) {
    Start-Service $service
}

Write-Host ""
Write-Host "🎉 Todos os serviços foram iniciados!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 URLs dos Serviços:" -ForegroundColor Yellow
Write-Host "🌐 API Gateway: https://localhost:7001" -ForegroundColor White
Write-Host "🔐 Auth Service: https://localhost:7002" -ForegroundColor White
Write-Host "💳 Transaction Service: https://localhost:7003" -ForegroundColor White
Write-Host "💰 Balance Service: https://localhost:7004" -ForegroundColor White
Write-Host "🔗 Webhook Service: https://localhost:7005" -ForegroundColor White
Write-Host "🏦 Integration Service: https://localhost:7006" -ForegroundColor White
Write-Host "👤 User Service: https://localhost:7007" -ForegroundColor White
Write-Host "⚙️ Config Service: https://localhost:7008" -ForegroundColor White
Write-Host ""
Write-Host "📊 Infraestrutura:" -ForegroundColor Yellow
Write-Host "🐘 PostgreSQL: localhost:5432" -ForegroundColor White
Write-Host "🐰 RabbitMQ: localhost:15672 (admin: guest/guest)" -ForegroundColor White
Write-Host "🔴 Redis: localhost:6379" -ForegroundColor White
Write-Host ""
Write-Host "📮 Para testar:" -ForegroundColor Green
Write-Host "1. Importe a collection: postman/FintechPSP-Collection.json" -ForegroundColor White
Write-Host "2. Configure base_url: https://localhost:7001" -ForegroundColor White
Write-Host "3. Execute 'Obter Token OAuth 2.0' primeiro" -ForegroundColor White
Write-Host "4. Teste os endpoints de QR Code!" -ForegroundColor White
Write-Host ""
Write-Host "⏳ Aguarde ~30 segundos para todos os serviços iniciarem completamente..." -ForegroundColor Yellow

# Aguardar um pouco para os serviços iniciarem
Start-Sleep -Seconds 30

# Testar se os serviços estão respondendo
Write-Host "🔍 Testando conectividade dos serviços..." -ForegroundColor Cyan

foreach ($service in $services) {
    try {
        $response = Invoke-WebRequest -Uri "https://localhost:$($service.HttpsPort)/health" -Method GET -SkipCertificateCheck -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            Write-Host "✅ $($service.Name): OK" -ForegroundColor Green
        } else {
            Write-Host "⚠️ $($service.Name): Resposta $($response.StatusCode)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "❌ $($service.Name): Não respondeu" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🚀 Sistema pronto para testes!" -ForegroundColor Green
Write-Host "📮 Use o Postman com a collection em: postman/FintechPSP-Collection.json" -ForegroundColor Cyan
