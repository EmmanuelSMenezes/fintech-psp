Write-Host "Testando login admin..." -ForegroundColor Cyan

$headers = @{
    'Content-Type' = 'application/json'
}

$body = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $body -Headers $headers -TimeoutSec 15
    
    Write-Host "✅ Login realizado com sucesso!" -ForegroundColor Green
    Write-Host "Token: $($response.accessToken.Substring(0,30))..." -ForegroundColor Green
    Write-Host "User: $($response.user.email)" -ForegroundColor Green
    Write-Host "Role: $($response.user.role)" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erro no login: $($_.Exception.Message)" -ForegroundColor Red
}
