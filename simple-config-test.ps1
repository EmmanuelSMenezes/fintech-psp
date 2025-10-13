Write-Host "Testando ConfigService..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken
Write-Host "✅ Login admin OK" -ForegroundColor Green

$authHeaders = @{
    'Authorization' = "Bearer $token"
    'Accept' = 'application/json'
}

# Testar health
try {
    $health = Invoke-RestMethod -Uri 'http://localhost:5007/config/health' -Method GET
    Write-Host "✅ ConfigService Health: $($health.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ ConfigService Health falhou: $($_.Exception.Message)" -ForegroundColor Red
}

# Testar configurações do sistema
try {
    $systemConfig = Invoke-RestMethod -Uri 'http://localhost:5007/config/system' -Method GET -Headers $authHeaders
    Write-Host "✅ Sistema Config:" -ForegroundColor Green
    Write-Host "  PIX: $($systemConfig.pixEnabled)" -ForegroundColor Green
    Write-Host "  TED: $($systemConfig.tedEnabled)" -ForegroundColor Green
    Write-Host "  Maintenance: $($systemConfig.maintenanceMode)" -ForegroundColor Green
} catch {
    Write-Host "❌ Sistema Config falhou: $($_.Exception.Message)" -ForegroundColor Red
}

# Testar limites PIX
try {
    $pixLimits = Invoke-RestMethod -Uri 'http://localhost:5007/config/limits/pix' -Method GET -Headers $authHeaders
    Write-Host "✅ Limites PIX:" -ForegroundColor Green
    Write-Host "  Max: R$ $($pixLimits.maxAmount)" -ForegroundColor Green
    Write-Host "  Daily: R$ $($pixLimits.dailyLimit)" -ForegroundColor Green
} catch {
    Write-Host "❌ Limites PIX falhou: $($_.Exception.Message)" -ForegroundColor Red
}

# Testar taxas
try {
    $fees = Invoke-RestMethod -Uri 'http://localhost:5007/config/fees' -Method GET -Headers $authHeaders
    Write-Host "✅ Taxas obtidas: $($fees.fees.Count) tipos" -ForegroundColor Green
} catch {
    Write-Host "❌ Taxas falhou: $($_.Exception.Message)" -ForegroundColor Red
}
