Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  5. GERAÇÃO E CONFIGURAÇÃO INICIAL     " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Login admin
Write-Host "🔐 Fazendo login como admin..." -ForegroundColor Yellow

$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$adminToken = $loginResponse.accessToken

Write-Host "✅ Login admin realizado com sucesso!" -ForegroundColor Green
Write-Host ""

# Testar ConfigService Health
Write-Host "🏥 Testando ConfigService Health..." -ForegroundColor Yellow

try {
    $healthResponse = Invoke-RestMethod -Uri 'http://localhost:5007/config/health' -Method GET
    Write-Host "✅ ConfigService Health: $($healthResponse.status)" -ForegroundColor Green
    Write-Host "Service: $($healthResponse.service)" -ForegroundColor Green
} catch {
    Write-Host "❌ ConfigService Health falhou: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Testar configurações do sistema
Write-Host "⚙️ Obtendo configurações do sistema..." -ForegroundColor Yellow

try {
    $systemConfigResponse = Invoke-RestMethod -Uri 'http://localhost:5007/config/system' -Method GET
    Write-Host "✅ Configurações do Sistema:" -ForegroundColor Green
    Write-Host "  PIX Enabled: $($systemConfigResponse.pixEnabled)" -ForegroundColor Green
    Write-Host "  TED Enabled: $($systemConfigResponse.tedEnabled)" -ForegroundColor Green
    Write-Host "  Boleto Enabled: $($systemConfigResponse.boletoEnabled)" -ForegroundColor Green
    Write-Host "  Crypto Enabled: $($systemConfigResponse.cryptoEnabled)" -ForegroundColor Green
    Write-Host "  Maintenance Mode: $($systemConfigResponse.maintenanceMode)" -ForegroundColor Green
    Write-Host "  Rate Limit: $($systemConfigResponse.rateLimitPerMinute)/min" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao obter configurações: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Testar limites PIX
Write-Host "💰 Obtendo limites PIX..." -ForegroundColor Yellow

try {
    $pixLimitsResponse = Invoke-RestMethod -Uri 'http://localhost:5007/config/limits/pix' -Method GET
    Write-Host "✅ Limites PIX:" -ForegroundColor Green
    Write-Host "  Min Amount: R$ $($pixLimitsResponse.minAmount)" -ForegroundColor Green
    Write-Host "  Max Amount: R$ $($pixLimitsResponse.maxAmount)" -ForegroundColor Green
    Write-Host "  Daily Limit: R$ $($pixLimitsResponse.dailyLimit)" -ForegroundColor Green
    Write-Host "  Monthly Limit: R$ $($pixLimitsResponse.monthlyLimit)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao obter limites PIX: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Testar taxas
Write-Host "💳 Obtendo taxas do sistema..." -ForegroundColor Yellow

try {
    $feesResponse = Invoke-RestMethod -Uri 'http://localhost:5007/config/fees' -Method GET
    Write-Host "✅ Taxas do Sistema:" -ForegroundColor Green
    foreach ($fee in $feesResponse.fees) {
        Write-Host "  $($fee.type): Taxa fixa R$ $($fee.fixedFee) + $($fee.percentageFee)%" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ Erro ao obter taxas: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
