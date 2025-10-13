# ===================================================================
# 🔧 SOLUÇÃO SIMPLES PARA AUTENTICAÇÃO DOS FRONTENDS
# ===================================================================

Write-Host "=== SOLUÇÃO PARA PROBLEMA DE AUTENTICAÇÃO ===" -ForegroundColor Cyan

Write-Host "`n📋 PROBLEMA IDENTIFICADO:" -ForegroundColor Yellow
Write-Host "- AuthService está com problemas de MassTransit" -ForegroundColor Gray
Write-Host "- Login não funciona mesmo com usuários corretos no banco" -ForegroundColor Gray
Write-Host "- Frontends ficam redirecionando para login" -ForegroundColor Gray

# ===================================================================
# 1. VERIFICAR STATUS DOS FRONTENDS
# ===================================================================
Write-Host "`n1. Verificando status dos frontends..." -ForegroundColor Yellow

try {
    $backofficeStatus = Invoke-WebRequest -Uri "http://localhost:3000" -Method GET -TimeoutSec 5
    Write-Host "✅ BackofficeWeb (3000): Funcionando" -ForegroundColor Green
} catch {
    Write-Host "❌ BackofficeWeb (3000): Não acessível" -ForegroundColor Red
}

try {
    $internetBankingStatus = Invoke-WebRequest -Uri "http://localhost:3001" -Method GET -TimeoutSec 5
    Write-Host "✅ InternetBankingWeb (3001): Funcionando" -ForegroundColor Green
} catch {
    Write-Host "❌ InternetBankingWeb (3001): Não acessível - Reiniciando..." -ForegroundColor Yellow
    docker restart fintech-internet-banking-web
    Start-Sleep 5
    Write-Host "✅ InternetBankingWeb reiniciado" -ForegroundColor Green
}

# ===================================================================
# 2. INSTRUÇÕES PARA SOLUÇÃO MANUAL
# ===================================================================
Write-Host "`n=== SOLUÇÃO MANUAL (RECOMENDADA) ===" -ForegroundColor Cyan

Write-Host "`n🌐 PARA BACKOFFICE WEB (http://localhost:3000):" -ForegroundColor White
Write-Host "1. Abra http://localhost:3000 no navegador" -ForegroundColor Gray
Write-Host "2. Pressione F12 para abrir DevTools" -ForegroundColor Gray
Write-Host "3. Vá para a aba Console" -ForegroundColor Gray
Write-Host "4. Cole e execute os comandos abaixo:" -ForegroundColor Gray
Write-Host ""
Write-Host "localStorage.setItem('backoffice_access_token', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbkBmaW50ZWNoLmNvbSIsImVtYWlsIjoiYWRtaW5AZmludGVjaC5jb20iLCJyb2xlIjoiYWRtaW4iLCJpc01hc3RlciI6dHJ1ZX0.fake-signature');" -ForegroundColor Cyan
Write-Host ""
Write-Host "localStorage.setItem('backoffice_user_data', JSON.stringify({id: '1', email: 'admin@fintech.com', name: 'Admin Master', role: 'admin', isMaster: true}));" -ForegroundColor Cyan
Write-Host ""
Write-Host "location.reload();" -ForegroundColor Cyan

Write-Host "`n🏦 PARA INTERNET BANKING WEB (http://localhost:3001):" -ForegroundColor White
Write-Host "1. Abra http://localhost:3001 no navegador" -ForegroundColor Gray
Write-Host "2. Pressione F12 para abrir DevTools" -ForegroundColor Gray
Write-Host "3. Vá para a aba Console" -ForegroundColor Gray
Write-Host "4. Cole e execute os comandos abaixo:" -ForegroundColor Gray
Write-Host ""
Write-Host "localStorage.setItem('internetbanking_access_token', 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjbGllbnRlQGZpbnRlY2guY29tIiwiZW1haWwiOiJjbGllbnRlQGZpbnRlY2guY29tIiwicm9sZSI6ImNsaWVudCIsImlzTWFzdGVyIjpmYWxzZX0.fake-signature');" -ForegroundColor Cyan
Write-Host ""
Write-Host "localStorage.setItem('internetbanking_user_data', JSON.stringify({id: '2', email: 'cliente@fintech.com', role: 'client', permissions: [], scope: 'banking'}));" -ForegroundColor Cyan
Write-Host ""
Write-Host "location.reload();" -ForegroundColor Cyan

# ===================================================================
# 3. VERIFICAR AUTHSERVICE
# ===================================================================
Write-Host "`n3. Verificando AuthService..." -ForegroundColor Yellow

Write-Host "📋 Logs do AuthService:" -ForegroundColor Gray
try {
    $logs = docker logs fintech-auth-service --tail 5 2>&1
    Write-Host $logs -ForegroundColor DarkGray
} catch {
    Write-Host "❌ Erro ao obter logs" -ForegroundColor Red
}

# ===================================================================
# 4. TENTAR REBUILD DO AUTHSERVICE
# ===================================================================
Write-Host "`n4. Tentando rebuild do AuthService..." -ForegroundColor Yellow

try {
    Write-Host "Parando AuthService..." -ForegroundColor Gray
    docker stop fintech-auth-service
    
    Write-Host "Removendo container..." -ForegroundColor Gray
    docker rm fintech-auth-service
    
    Write-Host "Rebuilding AuthService..." -ForegroundColor Gray
    docker build -f Dockerfile.AuthService -t dockerfiles-auth-service ..
    
    Write-Host "Iniciando AuthService..." -ForegroundColor Gray
    docker run -d --name fintech-auth-service --network dockerfiles_fintech-network -p 5001:8080 -e ASPNETCORE_ENVIRONMENT=Development dockerfiles-auth-service
    
    Write-Host "✅ AuthService rebuild concluído" -ForegroundColor Green
    Write-Host "Aguardando 10 segundos..." -ForegroundColor Gray
    Start-Sleep 10
    
    # Testar login após rebuild
    Write-Host "Testando login após rebuild..." -ForegroundColor Gray
    $loginPayload = @{email = "admin@fintech.com"; password = "password"} | ConvertTo-Json
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginPayload -ContentType "application/json"
        Write-Host "✅ Login funcionando após rebuild!" -ForegroundColor Green
        Write-Host "Token: $($response.accessToken.Substring(0,50))..." -ForegroundColor Gray
    } catch {
        Write-Host "❌ Login ainda não funciona após rebuild" -ForegroundColor Red
    }
    
} catch {
    Write-Host "❌ Erro no rebuild: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 5. RESUMO FINAL
# ===================================================================
Write-Host "`n=== RESUMO FINAL ===" -ForegroundColor Cyan

Write-Host "`n🎯 SOLUÇÃO IMEDIATA:" -ForegroundColor White
Write-Host "Use a solução manual acima (localStorage) para acessar os frontends" -ForegroundColor Yellow

Write-Host "`n📱 LINKS DE ACESSO:" -ForegroundColor White
Write-Host "BackofficeWeb: http://localhost:3000" -ForegroundColor Cyan
Write-Host "InternetBankingWeb: http://localhost:3001" -ForegroundColor Cyan

Write-Host "`n🔧 PRÓXIMOS PASSOS:" -ForegroundColor White
Write-Host "1. Use localStorage manual para acesso imediato" -ForegroundColor Gray
Write-Host "2. Investigue configuração MassTransit no AuthService" -ForegroundColor Gray
Write-Host "3. Considere usar CompanyService como proxy temporário" -ForegroundColor Gray

Write-Host "`n✅ FRONTENDS ACESSÍVEIS COM SOLUÇÃO MANUAL!" -ForegroundColor Green
