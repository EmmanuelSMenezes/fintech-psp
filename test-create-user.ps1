Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  2. CRIAÇÃO DE USUÁRIO ADMIN/OPERADOR  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Primeiro fazer login para obter token admin
Write-Host "🔐 Fazendo login como admin..." -ForegroundColor Yellow

$loginHeaders = @{
    'Content-Type' = 'application/json'
}

$loginBody = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders -TimeoutSec 15
    $adminToken = $loginResponse.accessToken
    
    Write-Host "✅ Login admin realizado com sucesso!" -ForegroundColor Green
    Write-Host ""
    
    # Criar usuário operador
    Write-Host "👤 Criando usuário operador..." -ForegroundColor Yellow
    
    $userHeaders = @{
        'Content-Type' = 'application/json'
        'Authorization' = "Bearer $adminToken"
    }
    
    $userData = @{
        email = "operador@fintechpsp.com"
        password = "operador123"
        name = "Operador Sistema"
        role = "Operator"
        permissions = @("view_dashboard", "manage_transactions", "view_reports")
        isActive = $true
    } | ConvertTo-Json
    
    Write-Host "📤 REQUEST - Criar Usuário:" -ForegroundColor Cyan
    Write-Host "POST http://localhost:5000/admin/usuarios" -ForegroundColor White
    Write-Host "Headers: Authorization: Bearer [token]" -ForegroundColor White
    Write-Host "Body: $userData" -ForegroundColor White
    Write-Host ""
    
    try {
        $userResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/usuarios' -Method POST -Body $userData -Headers $userHeaders -TimeoutSec 15
        
        Write-Host "📥 RESPONSE - Usuário Criado:" -ForegroundColor Green
        Write-Host "✅ Status: 201 Created" -ForegroundColor Green
        Write-Host "👤 User ID: $($userResponse.id)" -ForegroundColor Green
        Write-Host "📧 Email: $($userResponse.email)" -ForegroundColor Green
        Write-Host "🎭 Role: $($userResponse.role)" -ForegroundColor Green
        Write-Host "✅ Active: $($userResponse.isActive)" -ForegroundColor Green
        Write-Host ""
        
        # Testar login do novo usuário
        Write-Host "🔍 Testando login do novo operador..." -ForegroundColor Yellow
        
        $operatorLoginBody = @{
            email = "operador@fintechpsp.com"
            password = "operador123"
        } | ConvertTo-Json
        
        try {
            $operatorLoginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $operatorLoginBody -Headers $loginHeaders -TimeoutSec 15
            
            Write-Host "✅ Login operador realizado com sucesso!" -ForegroundColor Green
            Write-Host "🔑 Token operador obtido: $($operatorLoginResponse.accessToken.Substring(0,30))..." -ForegroundColor Green
            Write-Host "🎭 Role confirmado: $($operatorLoginResponse.user.role)" -ForegroundColor Green
            Write-Host ""
            Write-Host "🎉 CRIAÇÃO E TESTE DE OPERADOR - SUCESSO!" -ForegroundColor Green
            
        } catch {
            Write-Host "❌ Login operador falhou: $($_.Exception.Message)" -ForegroundColor Red
        }
        
    } catch {
        Write-Host "❌ Criação de usuário falhou: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
    
} catch {
    Write-Host "❌ Login admin falhou: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
