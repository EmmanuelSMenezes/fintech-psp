Write-Host "=== TESTE SIMPLES TRANSACTIONSERVICE ===" -ForegroundColor Cyan
Write-Host ""

# 1. Login cliente
Write-Host "1. Login cliente..." -ForegroundColor Yellow
$headers = @{'Content-Type' = 'application/json'}
$body = @{
    email = "joao.silva@empresateste.com"
    password = "cliente123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $body -Headers $headers -TimeoutSec 10
    Write-Host "   ✅ Login OK!" -ForegroundColor Green
    Write-Host "   Email: $($loginResponse.user.email)" -ForegroundColor Gray
    Write-Host "   ID: $($loginResponse.user.id)" -ForegroundColor Gray
    Write-Host "   Role: $($loginResponse.user.role)" -ForegroundColor Gray
    
    $token = $loginResponse.accessToken
    Write-Host "   Token: $($token.Substring(0,50))..." -ForegroundColor Gray
    
    # 2. Testar endpoint sem autenticação primeiro
    Write-Host ""
    Write-Host "2. Testando endpoint público..." -ForegroundColor Yellow
    try {
        $healthResponse = Invoke-RestMethod -Uri 'http://localhost:5004/' -TimeoutSec 5
        Write-Host "   ✅ TransactionService respondendo: $($healthResponse.service)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ TransactionService não responde: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # 3. Testar com token
    Write-Host ""
    Write-Host "3. Testando com token..." -ForegroundColor Yellow
    $authHeaders = @{
        'Authorization' = "Bearer $token"
        'Content-Type' = 'application/json'
    }
    
    try {
        $transResponse = Invoke-RestMethod -Uri 'http://localhost:5004/transacoes' -Headers $authHeaders -TimeoutSec 10
        Write-Host "   ✅ Transações OK: Total $($transResponse.total)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
            Write-Host "   Headers enviados:" -ForegroundColor Yellow
            $authHeaders.GetEnumerator() | ForEach-Object {
                Write-Host "     $($_.Key): $($_.Value)" -ForegroundColor Gray
            }
        }
    }
    
    # 4. Testar via API Gateway
    Write-Host ""
    Write-Host "4. Testando via API Gateway..." -ForegroundColor Yellow
    try {
        $gatewayResponse = Invoke-RestMethod -Uri 'http://localhost:5000/banking/transacoes/historico' -Headers $authHeaders -TimeoutSec 10
        Write-Host "   ✅ Gateway OK: Total $($gatewayResponse.total)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro Gateway: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
} catch {
    Write-Host "   ❌ Login falhou: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== FIM TESTE ===" -ForegroundColor Cyan
