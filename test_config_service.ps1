# Test ConfigService endpoints
Write-Host "=== TESTE CONFIGSERVICE ===" -ForegroundColor Yellow

# Login
try {
    $login = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body '{"email":"admin@fintechpsp.com","password":"admin123"}' -ContentType 'application/json'
    $auth = @{Authorization="Bearer $($login.accessToken)"}
    Write-Host "✅ Login: SUCESSO" -ForegroundColor Green
} catch {
    Write-Host "❌ Login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 1: Health check direto no ConfigService
Write-Host "`n--- Test 1: Health Check Direto ---" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri 'http://localhost:5007/config/health' -Method GET -TimeoutSec 10
    Write-Host "✅ Health Direto: $($health.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ Health Direto: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Roteamento via API Gateway
Write-Host "`n--- Test 2: Roteamento via API Gateway ---" -ForegroundColor Cyan
try {
    $roteamento = Invoke-RestMethod -Uri 'http://localhost:5000/banking/configs/roteamento' -Method GET -Headers $auth -TimeoutSec 15
    Write-Host "✅ Roteamento Gateway: SUCESSO - ConfigId: $($roteamento.ConfigId)" -ForegroundColor Green
} catch {
    Write-Host "❌ Roteamento Gateway: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Roteamento direto no ConfigService
Write-Host "`n--- Test 3: Roteamento Direto ---" -ForegroundColor Cyan
try {
    $roteamento = Invoke-RestMethod -Uri 'http://localhost:5007/banking/configs/roteamento' -Method GET -Headers $auth -TimeoutSec 15
    Write-Host "✅ Roteamento Direto: SUCESSO - ConfigId: $($roteamento.ConfigId)" -ForegroundColor Green
} catch {
    Write-Host "❌ Roteamento Direto: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Bancos via API Gateway
Write-Host "`n--- Test 4: Bancos via API Gateway ---" -ForegroundColor Cyan
try {
    $bancos = Invoke-RestMethod -Uri 'http://localhost:5000/banking/bancos' -Method GET -Headers $auth -TimeoutSec 15
    Write-Host "✅ Bancos Gateway: SUCESSO - Bancos: $($bancos.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Bancos Gateway: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Bancos direto no ConfigService
Write-Host "`n--- Test 5: Bancos Direto ---" -ForegroundColor Cyan
try {
    $bancos = Invoke-RestMethod -Uri 'http://localhost:5007/banking/bancos' -Method GET -Headers $auth -TimeoutSec 15
    Write-Host "✅ Bancos Direto: SUCESSO - Bancos: $($bancos.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Bancos Direto: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== FIM DOS TESTES ===" -ForegroundColor Yellow
