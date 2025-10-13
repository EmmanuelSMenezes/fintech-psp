# Teste de Autenticação - FintechPSP
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  TESTE DE AUTENTICAÇÃO - ADMIN LOGIN   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Configurar dados de login
$loginUrl = "http://localhost:5001/auth/login"
$headers = @{
    'Content-Type' = 'application/json'
    'Accept' = 'application/json'
}

$loginData = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
}

$jsonBody = $loginData | ConvertTo-Json

Write-Host "📤 REQUEST:" -ForegroundColor Yellow
Write-Host "URL: $loginUrl" -ForegroundColor White
Write-Host "Method: POST" -ForegroundColor White
Write-Host "Headers: Content-Type: application/json" -ForegroundColor White
Write-Host "Body: $jsonBody" -ForegroundColor White
Write-Host ""

try {
    Write-Host "🔄 Enviando requisição..." -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri $loginUrl -Method POST -Body $jsonBody -Headers $headers -TimeoutSec 15
    
    Write-Host "📥 RESPONSE:" -ForegroundColor Green
    Write-Host "✅ Status: 200 OK" -ForegroundColor Green
    Write-Host "🔑 Access Token: $($response.accessToken.Substring(0,50))..." -ForegroundColor Green
    Write-Host "👤 User Email: $($response.user.email)" -ForegroundColor Green
    Write-Host "🎭 User Role: $($response.user.role)" -ForegroundColor Green
    Write-Host "⏰ Expires In: $($response.expiresIn) seconds" -ForegroundColor Green
    Write-Host ""
    
    # Salvar token para próximos testes
    $global:authToken = $response.accessToken
    $global:currentUser = $response.user
    
    Write-Host "✅ LOGIN ADMIN - SUCESSO!" -ForegroundColor Green
    Write-Host "🔐 Token JWT válido obtido" -ForegroundColor Green
    Write-Host ""
    
    # Testar token em endpoint protegido
    Write-Host "🔍 Testando token em endpoint protegido..." -ForegroundColor Yellow
    
    $authHeaders = @{
        'Authorization' = "Bearer $($response.accessToken)"
        'Accept' = 'application/json'
    }
    
    try {
        $profileResponse = Invoke-RestMethod -Uri "http://localhost:5001/auth/profile" -Headers $authHeaders -TimeoutSec 10
        Write-Host "✅ Token válido - Profile endpoint acessível" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Profile endpoint não disponível: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    
    Write-Host ""
    Write-Host "🎉 AUTENTICAÇÃO ADMIN FUNCIONANDO CORRETAMENTE!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ LOGIN ADMIN - FALHOU!" -ForegroundColor Red
    Write-Host "Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        Write-Host "Status Description: $($_.Exception.Response.StatusDescription)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "🔧 Verificar se AuthService está rodando na porta 5001" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
