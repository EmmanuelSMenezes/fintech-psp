# Script simples para testar Docker
Write-Host "Testando Docker..." -ForegroundColor Yellow

# Verificar Docker
try {
    docker version
    Write-Host "Docker OK!" -ForegroundColor Green
}
catch {
    Write-Host "Docker nao encontrado!" -ForegroundColor Red
    exit 1
}

# Parar containers
Write-Host "Parando containers..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down

# Subir apenas infraestrutura
Write-Host "Subindo infraestrutura..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

Write-Host "Aguardando 30 segundos..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar status
Write-Host "Status dos containers:" -ForegroundColor Cyan
docker ps

Write-Host "Teste concluido!" -ForegroundColor Green
