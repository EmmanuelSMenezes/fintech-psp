Write-Host "Testando criação de usuário cliente..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Criar usuário cliente
$userHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

$userData = @{
    name = "João Silva"
    email = "joao.silva@empresateste.com"
    password = "cliente123"
    role = "cliente"
    isActive = $true
    document = "12345678900"
    phone = "11999999999"
} | ConvertTo-Json

Write-Host "Criando usuário cliente..." -ForegroundColor Yellow

try {
    $userResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/users' -Method POST -Body $userData -Headers $userHeaders
    Write-Host "✅ Usuário cliente criado: $($userResponse.email)" -ForegroundColor Green
    Write-Host "ID: $($userResponse.id)" -ForegroundColor Green
    Write-Host "Role: $($userResponse.role)" -ForegroundColor Green
    
    # Testar login do cliente
    Write-Host "Testando login do cliente..." -ForegroundColor Yellow
    $clientLogin = @{email = "joao.silva@empresateste.com"; password = "cliente123"} | ConvertTo-Json
    $clientResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $clientLogin -Headers $loginHeaders
    Write-Host "✅ Login cliente OK: $($clientResponse.user.role)" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}
