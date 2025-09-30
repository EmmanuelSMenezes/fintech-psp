Write-Host "=== TESTE EXTRATO ==="

# Login
$body = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

try {
    $login = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body $body -ContentType 'application/json'
    Write-Host "✅ Login: SUCESSO"
    Write-Host "ClientId: $($login.user.id)"
    
    $auth = @{
        Authorization = "Bearer $($login.accessToken)"
    }
    
    # Teste Extrato com parâmetros corretos (máximo 90 dias)
    $startDate = "2024-01-01"
    $endDate = "2024-03-31"
    $clientId = $login.user.id
    
    $url = "http://localhost:5000/banking/balance/$clientId/extrato?startDate=$startDate&endDate=$endDate"
    Write-Host "URL: $url"
    
    try {
        $extrato = Invoke-RestMethod -Uri $url -Method GET -Headers $auth
        Write-Host "✅ Extrato: SUCESSO"
        Write-Host "Transações: $($extrato.transactions.Count)"
        Write-Host "Total: $($extrato.totalCount)"
        Write-Host "Saldo Atual: $($extrato.currentBalance)"
    }
    catch {
        Write-Host "❌ Extrato: $($_.Exception.Message)"
        if ($_.Exception.Response) {
            Write-Host "Status: $($_.Exception.Response.StatusCode)"
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "Response Body: $responseBody"
            }
            catch {
                Write-Host "Não foi possível ler o corpo da resposta"
            }
        }
    }
}
catch {
    Write-Host "❌ Login: $($_.Exception.Message)"
}

Write-Host "=== FIM TESTE EXTRATO ==="
