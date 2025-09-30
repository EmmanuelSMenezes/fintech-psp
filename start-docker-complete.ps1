# =====================================================
# FintechPSP - Script para subir sistema completo com Docker
# =====================================================

Write-Host "🚀 Iniciando FintechPSP com Docker Compose..." -ForegroundColor Green

# Parar containers existentes se houver conflito
Write-Host "🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down 2>$null

# Limpar volumes antigos se necessário
Write-Host "🧹 Limpando recursos antigos..." -ForegroundColor Yellow
docker system prune -f 2>$null

# Construir e subir todos os serviços
Write-Host "🔨 Construindo e iniciando todos os serviços..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up --build -d

# Aguardar infraestrutura ficar pronta
Write-Host "⏳ Aguardando infraestrutura ficar pronta..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar se PostgreSQL está pronto
Write-Host "🐘 Verificando PostgreSQL..." -ForegroundColor Cyan
$maxAttempts = 10
$attempt = 0
do {
    $attempt++
    Write-Host "Tentativa $attempt de $maxAttempts..." -ForegroundColor Gray
    $pgReady = docker exec fintech-postgres-new pg_isready -U postgres 2>$null
    if ($pgReady -match "accepting connections") {
        Write-Host "✅ PostgreSQL está pronto!" -ForegroundColor Green
        break
    }
    Start-Sleep -Seconds 5
} while ($attempt -lt $maxAttempts)

if ($attempt -eq $maxAttempts) {
    Write-Host "❌ PostgreSQL não ficou pronto a tempo" -ForegroundColor Red
    exit 1
}

# Inicializar banco de dados
Write-Host "📊 Inicializando banco de dados..." -ForegroundColor Cyan
Get-Content init-database.sql | docker exec -i fintech-postgres-new psql -U postgres

# Aguardar serviços ficarem prontos
Write-Host "⏳ Aguardando serviços ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 45

# Verificar status dos serviços
Write-Host "🔍 Verificando status dos serviços..." -ForegroundColor Cyan

$services = @(
    @{ Name = "API Gateway"; Port = "5000"; Container = "fintech-api-gateway" },
    @{ Name = "Auth Service"; Port = "5001"; Container = "fintech-auth-service" },
    @{ Name = "Transaction Service"; Port = "5002"; Container = "fintech-transaction-service" },
    @{ Name = "Balance Service"; Port = "5003"; Container = "fintech-balance-service" },
    @{ Name = "Webhook Service"; Port = "5004"; Container = "fintech-webhook-service" },
    @{ Name = "Integration Service"; Port = "5005"; Container = "fintech-integration-service" },
    @{ Name = "User Service"; Port = "5006"; Container = "fintech-user-service" },
    @{ Name = "Config Service"; Port = "5007"; Container = "fintech-config-service" }
)

foreach ($service in $services) {
    try {
        $status = docker ps --filter "name=$($service.Container)" --format "{{.Status}}"
        if ($status -match "Up") {
            Write-Host "✅ $($service.Name): Rodando" -ForegroundColor Green
        } else {
            Write-Host "❌ $($service.Name): Não está rodando" -ForegroundColor Red
        }
    } catch {
        Write-Host "❌ $($service.Name): Erro ao verificar status" -ForegroundColor Red
    }
}

# Mostrar logs dos serviços se houver erro
Write-Host ""
Write-Host "📋 Para ver logs de um serviço específico, use:" -ForegroundColor Yellow
Write-Host "docker-compose -f docker-compose-complete.yml logs [nome-do-serviço]" -ForegroundColor White
Write-Host ""
Write-Host "Exemplo: docker-compose -f docker-compose-complete.yml logs transaction-service" -ForegroundColor Gray

Write-Host ""
Write-Host "🎉 Sistema iniciado!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 URLs dos Serviços:" -ForegroundColor Yellow
Write-Host "🌐 API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "🔐 Auth Service: http://localhost:5001" -ForegroundColor White
Write-Host "💳 Transaction Service: http://localhost:5002" -ForegroundColor White
Write-Host "💰 Balance Service: http://localhost:5003" -ForegroundColor White
Write-Host "🔗 Webhook Service: http://localhost:5004" -ForegroundColor White
Write-Host "🏦 Integration Service: http://localhost:5005" -ForegroundColor White
Write-Host "👤 User Service: http://localhost:5006" -ForegroundColor White
Write-Host "⚙️ Config Service: http://localhost:5007" -ForegroundColor White
Write-Host ""
Write-Host "📊 Infraestrutura:" -ForegroundColor Yellow
Write-Host "🐘 PostgreSQL: localhost:5433" -ForegroundColor White
Write-Host "🐰 RabbitMQ: localhost:15673 (admin: guest/guest)" -ForegroundColor White
Write-Host "🔴 Redis: localhost:6380" -ForegroundColor White
Write-Host ""
Write-Host "📮 Para testar:" -ForegroundColor Green
Write-Host "1. Importe a collection: postman/FintechPSP-Collection.json" -ForegroundColor White
Write-Host "2. Configure base_url: http://localhost:5000" -ForegroundColor White
Write-Host "3. Execute 'Obter Token OAuth 2.0' primeiro" -ForegroundColor White
Write-Host "4. Teste os endpoints de QR Code!" -ForegroundColor White
Write-Host ""
Write-Host "🛑 Para parar todos os serviços:" -ForegroundColor Red
Write-Host "docker-compose -f docker-compose-complete.yml down" -ForegroundColor White

# Testar conectividade básica
Write-Host ""
Write-Host "🔍 Testando conectividade básica..." -ForegroundColor Cyan

try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -Method GET -TimeoutSec 10 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ API Gateway: Respondendo" -ForegroundColor Green
    } else {
        Write-Host "⚠️ API Gateway: Status $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ API Gateway: Não respondeu (pode estar ainda inicializando)" -ForegroundColor Red
}

Write-Host ""
Write-Host "🚀 Sistema pronto para uso!" -ForegroundColor Green
Write-Host "📝 Aguarde alguns minutos para todos os servicos terminarem de inicializar" -ForegroundColor Yellow
