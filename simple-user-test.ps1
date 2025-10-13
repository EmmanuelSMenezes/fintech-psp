Write-Host "Testando criação de usuário operador..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Criar usuário
$userHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

$userData = @{
    email = "operador@fintechpsp.com"
    password = "operador123"
    name = "Operador Sistema"
    role = "Operator"
} | ConvertTo-Json

Write-Host "Criando usuário operador..." -ForegroundColor Yellow

try {
    $userResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/users' -Method POST -Body $userData -Headers $userHeaders
    Write-Host "✅ Usuário criado: $($userResponse.email)" -ForegroundColor Green

    # Testar login do novo usuário
    Write-Host "Testando login do operador..." -ForegroundColor Yellow
    $operatorLogin = @{email = "operador@fintechpsp.com"; password = "operador123"} | ConvertTo-Json
    $operatorResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $operatorLogin -Headers $loginHeaders
    Write-Host "✅ Login operador OK: $($operatorResponse.user.role)" -ForegroundColor Green

} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}
