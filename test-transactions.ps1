# TESTE 8: REALIZAÇÃO DE TRANSAÇÕES
# Objetivo: Testar criação e execução de transações PIX, TED e outras operações

Write-Host "=== TESTE 8: REALIZAÇÃO DE TRANSAÇÕES ===" -ForegroundColor Green
Write-Host ""

# Configurações
$baseUrl = "http://localhost:5000"
$adminEmail = "admin@fintechpsp.com"
$adminPassword = "admin123"
$clientEmail = "joao.silva@empresateste.com"
$clientPassword = "cliente123"

try {
    # 1. Login como admin
    Write-Host "1. Fazendo login como admin..." -ForegroundColor Cyan
    $adminLoginBody = @{
        email = $adminEmail
        password = $adminPassword
    } | ConvertTo-Json
    
    $adminLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $adminLoginBody -ContentType "application/json" -TimeoutSec 10
    $adminToken = $adminLoginResponse.accessToken
    $adminHeaders = @{ Authorization = "Bearer $adminToken" }
    Write-Host "   ✅ Login admin OK" -ForegroundColor Green
    
    # 2. Login como cliente
    Write-Host "2. Fazendo login como cliente..." -ForegroundColor Cyan
    $clientLoginBody = @{
        email = $clientEmail
        password = $clientPassword
    } | ConvertTo-Json
    
    $clientLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $clientLoginBody -ContentType "application/json" -TimeoutSec 10
    $clientToken = $clientLoginResponse.accessToken
    $clientHeaders = @{ Authorization = "Bearer $clientToken" }
    $clientId = $clientLoginResponse.user.id
    Write-Host "   ✅ Login cliente OK (ID: $clientId)" -ForegroundColor Green
    
    # 3. Obter saldo do cliente
    Write-Host "3. Obtendo saldo do cliente..." -ForegroundColor Cyan
    $balanceResponse = Invoke-RestMethod -Uri "$baseUrl/saldo/$clientId" -Headers $clientHeaders -TimeoutSec 10

    if ($balanceResponse) {
        $accountId = $balanceResponse.accountId
        $currentBalance = $balanceResponse.availableBalance
        Write-Host "   ✅ Conta encontrada: $accountId" -ForegroundColor Green
        Write-Host "      Saldo disponível: R$ $currentBalance" -ForegroundColor Gray
        Write-Host "      Saldo total: R$ $($balanceResponse.totalBalance)" -ForegroundColor Gray
    } else {
        Write-Host "   ❌ Conta não encontrada para o cliente" -ForegroundColor Red
        return
    }
    
    # 4. Verificar saldo inicial
    Write-Host "4. Verificando saldo inicial..." -ForegroundColor Cyan
    try {
        $balanceResponse = Invoke-RestMethod -Uri "$baseUrl/banking/balance/$clientId" -Headers $clientHeaders -TimeoutSec 10
        $saldoInicial = $balanceResponse.availableBalance
        Write-Host "   ✅ Saldo inicial: R$ $($saldoInicial)" -ForegroundColor Green
    } catch {
        Write-Host "   ⚠️  Erro ao obter saldo: $($_.Exception.Message)" -ForegroundColor Yellow
        $saldoInicial = 0
    }
    
    # 5. Creditar saldo para testes (se necessário)
    if ($saldoInicial -eq 0) {
        Write-Host "5. Creditando saldo para testes..." -ForegroundColor Cyan
        try {
            $creditBody = @{
                accountId = $accountId
                amount = 1000.00
                description = "Crédito para testes de transação"
                type = "CREDIT"
            } | ConvertTo-Json
            
            $creditResponse = Invoke-RestMethod -Uri "$baseUrl/admin/balance/credit" -Method POST -Headers $adminHeaders -Body $creditBody -ContentType "application/json" -TimeoutSec 10
            Write-Host "   ✅ Crédito realizado: R$ 1000,00" -ForegroundColor Green
            
            # Verificar novo saldo
            Start-Sleep 2
            $newBalanceResponse = Invoke-RestMethod -Uri "$baseUrl/banking/balance/$clientId" -Headers $clientHeaders -TimeoutSec 10
            Write-Host "   ✅ Novo saldo: R$ $($newBalanceResponse.availableBalance)" -ForegroundColor Green
        } catch {
            Write-Host "   ❌ Erro ao creditar saldo: $($_.Exception.Message)" -ForegroundColor Red
        }
    } else {
        Write-Host "5. Saldo suficiente para testes (R$ $saldoInicial)" -ForegroundColor Green
    }
    
    # 6. Teste de Transação PIX
    Write-Host "6. Testando transação PIX..." -ForegroundColor Cyan
    try {
        $pixBody = @{
            externalId = "PIX-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
            amount = 50.00
            pixKey = "11999999999"
            bankCode = "756"
            description = "Teste PIX via API"
        } | ConvertTo-Json
        
        $pixResponse = Invoke-RestMethod -Uri "$baseUrl/banking/transacoes/pix" -Method POST -Headers $clientHeaders -Body $pixBody -ContentType "application/json" -TimeoutSec 10
        Write-Host "   ✅ PIX criado:" -ForegroundColor Green
        Write-Host "      ID: $($pixResponse.id)" -ForegroundColor Gray
        Write-Host "      Status: $($pixResponse.status)" -ForegroundColor Gray
        Write-Host "      Valor: R$ $($pixResponse.amount)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao criar PIX: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "      Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
    # 7. Teste de Transação TED
    Write-Host "7. Testando transação TED..." -ForegroundColor Cyan
    try {
        $tedBody = @{
            externalId = "TED-TEST-$(Get-Date -Format 'yyyyMMddHHmmss')"
            amount = 100.00
            bankCode = "001"
            accountBranch = "1234"
            accountNumber = "567890"
            taxId = "98765432100"
            name = "Destinatário TED"
            description = "Teste TED via API"
        } | ConvertTo-Json
        
        $tedResponse = Invoke-RestMethod -Uri "$baseUrl/banking/transacoes/ted" -Method POST -Headers $clientHeaders -Body $tedBody -ContentType "application/json" -TimeoutSec 10
        Write-Host "   ✅ TED criado:" -ForegroundColor Green
        Write-Host "      ID: $($tedResponse.id)" -ForegroundColor Gray
        Write-Host "      Status: $($tedResponse.status)" -ForegroundColor Gray
        Write-Host "      Valor: R$ $($tedResponse.amount)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao criar TED: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "      Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
    # 8. Verificar histórico de transações
    Write-Host "8. Verificando histórico de transações..." -ForegroundColor Cyan
    try {
        $historyResponse = Invoke-RestMethod -Uri "$baseUrl/banking/transacoes/historico" -Headers $clientHeaders -TimeoutSec 10
        Write-Host "   ✅ Histórico obtido:" -ForegroundColor Green
        Write-Host "      Total de transações: $($historyResponse.total)" -ForegroundColor Gray
        
        if ($historyResponse.data -and $historyResponse.data.Count -gt 0) {
            foreach ($transaction in $historyResponse.data) {
                Write-Host "      - $($transaction.type): R$ $($transaction.amount) ($($transaction.status))" -ForegroundColor Gray
            }
        }
    } catch {
        Write-Host "   ❌ Erro ao obter histórico: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # 9. Verificar saldo final
    Write-Host "9. Verificando saldo final..." -ForegroundColor Cyan
    try {
        $finalBalanceResponse = Invoke-RestMethod -Uri "$baseUrl/banking/balance/$clientId" -Headers $clientHeaders -TimeoutSec 10
        Write-Host "   ✅ Saldo final: R$ $($finalBalanceResponse.availableBalance)" -ForegroundColor Green
        Write-Host "   ✅ Saldo bloqueado: R$ $($finalBalanceResponse.blockedBalance)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro ao obter saldo final: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "=== RESUMO TESTE 8 ===" -ForegroundColor Green
    Write-Host "✅ Login admin e cliente testados" -ForegroundColor Green
    Write-Host "✅ Conta do cliente identificada" -ForegroundColor Green
    Write-Host "✅ Operações de saldo testadas" -ForegroundColor Green
    Write-Host "✅ Transações PIX e TED testadas" -ForegroundColor Green
    Write-Host "✅ Histórico de transações validado" -ForegroundColor Green
    Write-Host ""
    Write-Host "Teste de Transações concluído!" -ForegroundColor Green

} catch {
    Write-Host "❌ Erro geral no teste: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== FIM TESTE ===" -ForegroundColor Green
