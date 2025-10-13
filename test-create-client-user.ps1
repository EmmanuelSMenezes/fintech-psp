Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  4. GERAÇÃO DE USUÁRIO PARA O CLIENTE  " -ForegroundColor Cyan
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

# Criar usuário cliente vinculado à empresa
Write-Host "👤 Criando usuário cliente..." -ForegroundColor Yellow

$userHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $adminToken"
}

# Usar dados consistentes da empresa criada anteriormente
$userData = @{
    name = "João Silva"
    email = "joao.silva@empresateste.com"
    password = "cliente123"
    role = "cliente"
    isActive = $true
    document = "12345678900"
    phone = "(11) 99999-9999"
    address = "Rua Teste, 123 - São Paulo/SP"
} | ConvertTo-Json

Write-Host "📤 REQUEST - Criar Usuário Cliente:" -ForegroundColor Cyan
Write-Host "POST http://localhost:5000/admin/users" -ForegroundColor White
Write-Host "Headers: Authorization: Bearer [token]" -ForegroundColor White
Write-Host "Body: $userData" -ForegroundColor White
Write-Host ""

try {
    $userResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/users' -Method POST -Body $userData -Headers $userHeaders -TimeoutSec 15
    
    Write-Host "📥 RESPONSE - Usuário Cliente Criado:" -ForegroundColor Green
    Write-Host "✅ Status: 201 Created" -ForegroundColor Green
    Write-Host "👤 User ID: $($userResponse.id)" -ForegroundColor Green
    Write-Host "📧 Email: $($userResponse.email)" -ForegroundColor Green
    Write-Host "🎭 Role: $($userResponse.role)" -ForegroundColor Green
    Write-Host "✅ Active: $($userResponse.active)" -ForegroundColor Green
    Write-Host ""
    
    # Salvar ID do usuário para próximos testes
    $global:clientUserId = $userResponse.id
    
    # Testar login do novo usuário cliente
    Write-Host "🔍 Testando login do novo cliente..." -ForegroundColor Yellow
    
    $clientLoginBody = @{
        email = "joao.silva@empresateste.com"
        password = "cliente123"
    } | ConvertTo-Json
    
    try {
        $clientLoginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $clientLoginBody -Headers $loginHeaders -TimeoutSec 15
        
        Write-Host "✅ Login cliente realizado com sucesso!" -ForegroundColor Green
        Write-Host "🔑 Token cliente obtido: $($clientLoginResponse.accessToken.Substring(0,30))..." -ForegroundColor Green
        Write-Host "🎭 Role confirmado: $($clientLoginResponse.user.role)" -ForegroundColor Green
        Write-Host ""
        
        # Salvar token do cliente para próximos testes
        $global:clientToken = $clientLoginResponse.accessToken
        $global:clientUser = $clientLoginResponse.user
        
        Write-Host "🎉 CRIAÇÃO E TESTE DE USUÁRIO CLIENTE - SUCESSO!" -ForegroundColor Green
        
    } catch {
        Write-Host "❌ Login cliente falhou: $($_.Exception.Message)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Criação de usuário cliente falhou: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
