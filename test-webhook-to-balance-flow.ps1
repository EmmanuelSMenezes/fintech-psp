# 🧪 TESTE COMPLETO: WEBHOOK → SALDO ATUALIZADO
# Este script testa o fluxo completo de PIX confirmado

Write-Host "🚀 INICIANDO TESTE DE FLUXO WEBHOOK → SALDO" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# Configurações
$baseUrl = "http://localhost:5000"
$integrationUrl = "http://localhost:5008"
$balanceUrl = "http://localhost:5004"

# 1. Obter token de autenticação
Write-Host "`n1️⃣ Obtendo token de autenticação..." -ForegroundColor Yellow

$tokenRequest = @{
    grant_type = "client_credentials"
    client_id = "fintechpsp-client"
    client_secret = "fintechpsp-secret"
    scope = "transactions balance webhooks banking"
} | ConvertTo-Json

try {
    $tokenResponse = Invoke-RestMethod -Uri "$baseUrl/auth/token" -Method POST -Body $tokenRequest -ContentType "application/json"
    $token = $tokenResponse.access_token
    Write-Host "✅ Token obtido com sucesso" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao obter token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Headers para autenticação
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Verificar saldo inicial
Write-Host "`n2️⃣ Verificando saldo inicial..." -ForegroundColor Yellow

try {
    $saldoInicial = Invoke-RestMethod -Uri "$balanceUrl/balance" -Method GET -Headers $headers
    Write-Host "💰 Saldo inicial: R$ $($saldoInicial.availableBalance)" -ForegroundColor Cyan
}
catch {
    Write-Host "⚠️ Erro ao obter saldo inicial: $($_.Exception.Message)" -ForegroundColor Yellow
    $saldoInicial = @{ availableBalance = 0 }
}

# 3. Gerar QR Code PIX dinâmico
Write-Host "`n3️⃣ Gerando QR Code PIX dinâmico..." -ForegroundColor Yellow

$qrCodeRequest = @{
    externalId = "TEST-WEBHOOK-$(Get-Date -Format 'yyyyMMddHHmmss')"
    amount = 150.00
    pixKey = "cliente@empresateste.com"
    bankCode = "756"
    description = "Teste de webhook para saldo"
    expiresIn = 3600
} | ConvertTo-Json

try {
    $qrCodeResponse = Invoke-RestMethod -Uri "$baseUrl/transacoes/pix/qrcode/dinamico" -Method POST -Body $qrCodeRequest -Headers $headers
    Write-Host "✅ QR Code gerado - TxId: $($qrCodeResponse.transactionId)" -ForegroundColor Green
    $txId = $qrCodeResponse.transactionId
}
catch {
    Write-Host "❌ Erro ao gerar QR Code: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 4. Simular webhook de PIX confirmado
Write-Host "`n4️⃣ Simulando webhook de PIX confirmado..." -ForegroundColor Yellow

$webhookPayload = @{
    txId = $txId
    endToEndId = "E756$(Get-Date -Format 'yyyyMMddHHmmss')000001"
    amount = 150.00
    status = "CONFIRMED"
    payerDocument = "12345678901"
    payerName = "João Silva Teste"
    timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
} | ConvertTo-Json

try {
    $webhookResponse = Invoke-RestMethod -Uri "$integrationUrl/webhooks/sicoob/pix" -Method POST -Body $webhookPayload -ContentType "application/json"
    Write-Host "✅ Webhook processado: $($webhookResponse.message)" -ForegroundColor Green
}
catch {
    Write-Host "❌ Erro ao processar webhook: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

# 5. Aguardar processamento (eventos assíncronos)
Write-Host "`n5️⃣ Aguardando processamento dos eventos..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# 6. Verificar saldo atualizado
Write-Host "`n6️⃣ Verificando saldo atualizado..." -ForegroundColor Yellow

try {
    $saldoFinal = Invoke-RestMethod -Uri "$balanceUrl/balance" -Method GET -Headers $headers
    Write-Host "💰 Saldo final: R$ $($saldoFinal.availableBalance)" -ForegroundColor Cyan
    
    $diferenca = $saldoFinal.availableBalance - $saldoInicial.availableBalance
    
    if ($diferenca -eq 150.00) {
        Write-Host "🎉 SUCESSO! Saldo foi creditado corretamente (+R$ $diferenca)" -ForegroundColor Green
    } else {
        Write-Host "⚠️ ATENÇÃO: Diferença esperada R$ 150,00, encontrada R$ $diferenca" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "❌ Erro ao verificar saldo final: $($_.Exception.Message)" -ForegroundColor Red
}

# 7. Verificar histórico de transações
Write-Host "`n7️⃣ Verificando histórico de transações..." -ForegroundColor Yellow

try {
    $startDate = (Get-Date).AddDays(-1).ToString("yyyy-MM-dd")
    $endDate = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
    
    $historico = Invoke-RestMethod -Uri "$balanceUrl/history?startDate=$startDate&endDate=$endDate" -Method GET -Headers $headers
    
    $transacaoEncontrada = $historico | Where-Object { $_.externalId -eq $txId }
    
    if ($transacaoEncontrada) {
        Write-Host "✅ Transação encontrada no histórico:" -ForegroundColor Green
        Write-Host "   - TxId: $($transacaoEncontrada.externalId)" -ForegroundColor Cyan
        Write-Host "   - Status: $($transacaoEncontrada.status)" -ForegroundColor Cyan
        Write-Host "   - Valor: R$ $($transacaoEncontrada.amount)" -ForegroundColor Cyan
    } else {
        Write-Host "⚠️ Transação não encontrada no histórico" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️ Erro ao verificar histórico: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host "`n🏁 TESTE CONCLUÍDO!" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green

# 8. Resumo dos resultados
Write-Host "`n📊 RESUMO DOS RESULTADOS:" -ForegroundColor Magenta
Write-Host "- Saldo inicial: R$ $($saldoInicial.availableBalance)" -ForegroundColor White
Write-Host "- Saldo final: R$ $($saldoFinal.availableBalance)" -ForegroundColor White
Write-Host "- Diferença: R$ $($saldoFinal.availableBalance - $saldoInicial.availableBalance)" -ForegroundColor White
Write-Host "- TxId testado: $txId" -ForegroundColor White

if ($diferenca -eq 150.00) {
    Write-Host "`n🎯 RESULTADO: FLUXO FUNCIONANDO CORRETAMENTE!" -ForegroundColor Green
} else {
    Write-Host "`n🔧 RESULTADO: FLUXO PRECISA DE AJUSTES" -ForegroundColor Yellow
}
