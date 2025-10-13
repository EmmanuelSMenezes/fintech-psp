# Script para testar um microservico
Write-Host "Testando microservico..." -ForegroundColor Yellow

# Subir AuthService
Write-Host "Subindo AuthService..." -ForegroundColor Cyan
docker-compose -f docker-compose-complete.yml up -d auth-service

Write-Host "Aguardando 60 segundos..." -ForegroundColor Yellow
Start-Sleep -Seconds 60

# Verificar status
Write-Host "Status dos containers:" -ForegroundColor Cyan
docker ps

# Verificar logs do AuthService
Write-Host "Logs do AuthService:" -ForegroundColor Cyan
docker logs fintech-auth-service --tail 20

Write-Host "Teste concluido!" -ForegroundColor Green
