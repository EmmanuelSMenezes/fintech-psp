# Teste simples do fluxo webhook -> saldo
Write-Host "🚀 TESTE SIMPLES - WEBHOOK PIX" -ForegroundColor Green

# URLs
$baseUrl = "http://localhost:5000"
$integrationUrl = "http://localhost:5005"
$balanceUrl = "http://localhost:5003"

# 1. Obter token
Write-Host "`n1️⃣ Obtendo token..." -ForegroundColor Yellow

$tokenRequest = @{
    grant_type = "client_credentials"
    client_id = "integration_test"
    client_secret = "test_secret_000"
    scope = "pix banking admin"
} | ConvertTo-Json

try {
    $tokenResponse = Invoke-RestMethod -Uri "$baseUrl/auth/token" -Method POST -Body $tokenRequest -ContentType "application/json"
    $token = $tokenResponse.access_token
    Write-Host "✅ Token obtido" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao obter token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Verificar saldo inicial
Write-Host "`n2️⃣ Verificando saldo inicial..." -ForegroundColor Yellow

$clientId = "550e8400-e29b-41d4-a716-446655440000"  # Cliente de teste
try {
    $saldoInicial = Invoke-RestMethod -Uri "$balanceUrl/saldo/$clientId" -Method GET -Headers $headers
    Write-Host "💰 Saldo inicial: R$ $($saldoInicial.availableBalance)" -ForegroundColor Cyan
}
catch {
    Write-Host "⚠️ Erro ao obter saldo: $($_.Exception.Message)" -ForegroundColor Yellow
    $saldoInicial = @{ availableBalance = 0 }
}

# 3. Simular webhook PIX confirmado
Write-Host "`n3️⃣ Simulando webhook PIX confirmado..." -ForegroundColor Yellow

$txId = "TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
$webhookPayload = @{
    txId = $txId
    endToEndId = "E756$(Get-Date -Format 'yyyyMMddHHmmss')000001"
    valor = 150.00
    status = "CONFIRMED"
    pagador = @{
        documento = "12345678901"
        nome = "João Silva Teste"
    }
    timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
} | ConvertTo-Json -Depth 3

try {
    $webhookResponse = Invoke-RestMethod -Uri "$integrationUrl/webhooks/sicoob/pix" -Method POST -Body $webhookPayload -ContentType "application/json"
    Write-Host "✅ Webhook enviado" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro webhook: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Aguardar e verificar saldo
Write-Host "`n4️⃣ Aguardando processamento..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

try {
    $saldoFinal = Invoke-RestMethod -Uri "$balanceUrl/saldo/$clientId" -Method GET -Headers $headers
    Write-Host "💰 Saldo final: R$ $($saldoFinal.availableBalance)" -ForegroundColor Cyan
    
    $diferenca = $saldoFinal.availableBalance - $saldoInicial.availableBalance
    
    if ($diferenca -eq 150.00) {
        Write-Host "`n🎉 SUCESSO! Saldo creditado: +R$ $diferenca" -ForegroundColor Green
    } else {
        Write-Host "`n⚠️ Diferença: R$ $diferenca (esperado R$ 150)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "❌ Erro ao verificar saldo final: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🏁 TESTE CONCLUÍDO!" -ForegroundColor Green
