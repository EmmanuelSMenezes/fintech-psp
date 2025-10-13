# Script completo para testar o ambiente Docker
Write-Host "TESTE COMPLETO DO AMBIENTE DOCKER" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan

# Parar tudo primeiro
Write-Host "Parando containers existentes..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down

# Subir infraestrutura
Write-Host "Subindo infraestrutura..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

Write-Host "Aguardando infraestrutura (30s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar se PostgreSQL está funcionando
Write-Host "Testando PostgreSQL..." -ForegroundColor Yellow
try {
    $pgTest = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT 1;"
    if ($pgTest -match "1 row") {
        Write-Host "PostgreSQL OK!" -ForegroundColor Green
    }
}
catch {
    Write-Host "PostgreSQL com problemas!" -ForegroundColor Red
}

# Verificar RabbitMQ
Write-Host "Testando RabbitMQ..." -ForegroundColor Yellow
try {
    $rabbitTest = docker exec fintech-rabbitmq rabbitmq-diagnostics ping
    if ($rabbitTest -match "Ping succeeded") {
        Write-Host "RabbitMQ OK!" -ForegroundColor Green
    }
}
catch {
    Write-Host "RabbitMQ com problemas!" -ForegroundColor Red
}

# Verificar Redis
Write-Host "Testando Redis..." -ForegroundColor Yellow
try {
    $redisTest = docker exec fintech-redis redis-cli ping
    if ($redisTest -eq "PONG") {
        Write-Host "Redis OK!" -ForegroundColor Green
    }
}
catch {
    Write-Host "Redis com problemas!" -ForegroundColor Red
}

# Subir AuthService
Write-Host "Subindo AuthService..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up -d auth-service

Write-Host "Aguardando AuthService (60s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

# Testar AuthService
Write-Host "Testando AuthService..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5001/auth/login" -Method POST -Body '{"test":"test"}' -ContentType "application/json" -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 400 -or $response.StatusCode -eq 200) {
        Write-Host "AuthService respondendo!" -ForegroundColor Green
    }
}
catch {
    Write-Host "AuthService com problemas!" -ForegroundColor Red
    Write-Host "Logs do AuthService:" -ForegroundColor Yellow
    docker logs fintech-auth-service --tail 5
}

# Subir mais serviços
Write-Host "Subindo mais servicos..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up -d user-service config-service company-service

Write-Host "Aguardando servicos (60s)..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

# Status final
Write-Host "Status final dos containers:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host ""
Write-Host "Teste completo finalizado!" -ForegroundColor Green
Write-Host "Para ver logs: docker-compose -f docker-compose-complete.yml logs -f [service-name]" -ForegroundColor Yellow
