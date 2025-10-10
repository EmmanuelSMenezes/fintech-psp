#!/usr/bin/env pwsh

Write-Host "🔐 TESTE DE LOGIN DOS FRONTENDS" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

# Credenciais de teste
$credentials = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
}

Write-Host "📧 Credenciais de teste:" -ForegroundColor Yellow
Write-Host "   Email: $($credentials.email)"
Write-Host "   Password: $($credentials.password)"
Write-Host ""

# Teste 1: AuthService direto
Write-Host "🧪 TESTE 1: AuthService Direto" -ForegroundColor Yellow
$headers = @{'Content-Type' = 'application/json'}
$body = $credentials | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri 'http://localhost:5001/auth/login' -Method POST -Headers $headers -Body $body
    Write-Host "✅ AuthService Status: $($response.StatusCode)" -ForegroundColor Green
    
    $content = $response.Content | ConvertFrom-Json
    Write-Host "✅ Token recebido: $($content.accessToken.Substring(0,30))..." -ForegroundColor Green
    Write-Host "✅ User: $($content.user.email)" -ForegroundColor Green
    Write-Host "✅ Role: $($content.user.role)" -ForegroundColor Green
} catch {
    Write-Host "❌ AuthService FALHOU: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Teste 2: Verificar se frontends estão online
Write-Host "🧪 TESTE 2: Status dos Frontends" -ForegroundColor Yellow

try {
    $backoffice = Invoke-WebRequest -Uri 'http://localhost:3000' -Method GET -TimeoutSec 3
    Write-Host "✅ BackofficeWeb (3000): Status $($backoffice.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ BackofficeWeb (3000): OFFLINE" -ForegroundColor Red
}

try {
    $internetbanking = Invoke-WebRequest -Uri 'http://localhost:3001' -Method GET -TimeoutSec 3
    Write-Host "✅ InternetBankingWeb (3001): Status $($internetbanking.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "❌ InternetBankingWeb (3001): OFFLINE" -ForegroundColor Red
}

Write-Host ""

# Teste 3: Verificar portas
Write-Host "🧪 TESTE 3: Verificação de Portas" -ForegroundColor Yellow

$ports = @(
    @{Name="AuthService"; Port="5001"},
    @{Name="BackofficeWeb"; Port="3000"},
    @{Name="InternetBankingWeb"; Port="3001"}
)

foreach ($service in $ports) {
    $listening = netstat -an | findstr "LISTENING" | findstr ":$($service.Port)"
    if ($listening) {
        Write-Host "✅ $($service.Name) (porta $($service.Port)) - LISTENING" -ForegroundColor Green
    } else {
        Write-Host "❌ $($service.Name) (porta $($service.Port)) - NOT LISTENING" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "🎯 INSTRUÇÕES PARA TESTE MANUAL:" -ForegroundColor Cyan
Write-Host "1. Acesse: http://localhost:3000 (BackofficeWeb)" -ForegroundColor White
Write-Host "2. Acesse: http://localhost:3001 (InternetBankingWeb)" -ForegroundColor White
Write-Host "3. Use as credenciais:" -ForegroundColor White
Write-Host "   Email: admin@fintechpsp.com" -ForegroundColor White
Write-Host "   Senha: admin123" -ForegroundColor White
Write-Host ""
Write-Host "🔧 Se ainda houver erro 401, verifique:" -ForegroundColor Yellow
Write-Host "- Console do navegador (F12)" -ForegroundColor White
Write-Host "- Logs do AuthService" -ForegroundColor White
Write-Host "- Configuração CORS" -ForegroundColor White
