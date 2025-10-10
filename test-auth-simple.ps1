Write-Host "TESTES DE AUTENTICACAO - AUTHSERVICE" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5001"

Write-Host "TC001: Login com credenciais validas" -ForegroundColor Yellow
try {
    $loginData = @{
        email = "admin@fintechpsp.com"
        password = "admin123"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginData -ContentType "application/json" -TimeoutSec 10
    Write-Host "✅ TC001 PASSOU - Login realizado com sucesso" -ForegroundColor Green
    Write-Host "Token recebido: $($response.token -ne $null)" -ForegroundColor Gray
} catch {
    Write-Host "❌ TC001 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC002: Login com credenciais invalidas" -ForegroundColor Yellow
try {
    $invalidLoginData = @{
        email = "admin@fintechpsp.com"
        password = "senhaerrada"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $invalidLoginData -ContentType "application/json" -TimeoutSec 10
    Write-Host "❌ TC002 FALHOU - Login deveria ter sido rejeitado" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✅ TC002 PASSOU - Login rejeitado corretamente (401)" -ForegroundColor Green
    } else {
        Write-Host "❌ TC002 FALHOU - Status inesperado: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "TC003: OAuth 2.0 Client Credentials" -ForegroundColor Yellow
try {
    $oauthData = @{
        grant_type = "client_credentials"
        client_id = "fintech_admin"
        client_secret = "admin_secret_789"
        scope = "pix banking admin"
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$baseUrl/auth/token" -Method POST -Body $oauthData -ContentType "application/json" -TimeoutSec 10
    Write-Host "✅ TC003 PASSOU - Token OAuth obtido com sucesso" -ForegroundColor Green
    Write-Host "Access token recebido: $($response.access_token -ne $null)" -ForegroundColor Gray
} catch {
    Write-Host "❌ TC003 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC004: Health Check" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method GET -TimeoutSec 10
    Write-Host "✅ TC004 PASSOU - Health check OK" -ForegroundColor Green
} catch {
    Write-Host "❌ TC004 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "RESUMO DOS TESTES DE AUTENTICACAO" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
