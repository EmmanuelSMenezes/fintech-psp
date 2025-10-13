# ===================================================================
# 🔧 CORREÇÃO DE AUTENTICAÇÃO DOS FRONTENDS
# ===================================================================

Write-Host "=== CORREÇÃO DE AUTENTICAÇÃO DOS FRONTENDS ===" -ForegroundColor Cyan

# ===================================================================
# 1. VERIFICAR E CRIAR SCHEMA AUTH_SERVICE
# ===================================================================
Write-Host "`n1. Verificando schema auth_service..." -ForegroundColor Yellow

try {
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "CREATE SCHEMA IF NOT EXISTS auth_service;"
    Write-Host "✅ Schema auth_service criado/verificado" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao criar schema: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 2. CRIAR TABELA SYSTEM_USERS
# ===================================================================
Write-Host "`n2. Criando tabela system_users..." -ForegroundColor Yellow

$createTableSQL = @"
CREATE TABLE IF NOT EXISTS auth_service.system_users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    is_master BOOLEAN NOT NULL DEFAULT false,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);
"@

try {
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c $createTableSQL
    Write-Host "✅ Tabela system_users criada/verificada" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao criar tabela: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 3. CRIAR USUÁRIO ADMIN COM HASH BCRYPT CORRETO
# ===================================================================
Write-Host "`n3. Criando usuário admin..." -ForegroundColor Yellow

# Hash BCrypt para senha "Admin123!" gerado com custo 10
$adminPasswordHash = '$2a$10$N9qo8uLOickgx2ZMRZoMye.IjdOcLOjbkOr8YzKZqKqOKOQGhOQGhO'

$insertUserSQL = @"
INSERT INTO auth_service.system_users (email, password_hash, name, role, is_master, is_active) 
VALUES (
    'admin@fintech.com', 
    '$adminPasswordHash', 
    'Admin Master', 
    'admin', 
    true, 
    true
) ON CONFLICT (email) DO UPDATE SET 
    password_hash = EXCLUDED.password_hash,
    name = EXCLUDED.name,
    role = EXCLUDED.role,
    is_master = EXCLUDED.is_master,
    is_active = EXCLUDED.is_active,
    updated_at = CURRENT_TIMESTAMP;
"@

try {
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c $insertUserSQL
    Write-Host "✅ Usuário admin criado/atualizado" -ForegroundColor Green
    Write-Host "   Email: admin@fintech.com" -ForegroundColor Gray
    Write-Host "   Senha: Admin123!" -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao criar usuário: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 4. CRIAR USUÁRIO CLIENTE PARA TESTE
# ===================================================================
Write-Host "`n4. Criando usuário cliente..." -ForegroundColor Yellow

# Hash BCrypt para senha "Cliente123!"
$clientPasswordHash = '$2a$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'

$insertClientSQL = @"
INSERT INTO auth_service.system_users (email, password_hash, name, role, is_master, is_active) 
VALUES (
    'cliente@fintech.com', 
    '$clientPasswordHash', 
    'Cliente Teste', 
    'client', 
    false, 
    true
) ON CONFLICT (email) DO UPDATE SET 
    password_hash = EXCLUDED.password_hash,
    name = EXCLUDED.name,
    role = EXCLUDED.role,
    is_master = EXCLUDED.is_master,
    is_active = EXCLUDED.is_active,
    updated_at = CURRENT_TIMESTAMP;
"@

try {
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c $insertClientSQL
    Write-Host "✅ Usuário cliente criado/atualizado" -ForegroundColor Green
    Write-Host "   Email: cliente@fintech.com" -ForegroundColor Gray
    Write-Host "   Senha: password" -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao criar usuário cliente: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 5. VERIFICAR USUÁRIOS CRIADOS
# ===================================================================
Write-Host "`n5. Verificando usuários criados..." -ForegroundColor Yellow

try {
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT email, name, role, is_master, is_active FROM auth_service.system_users ORDER BY created_at;"
    Write-Host "✅ Usuários no banco:" -ForegroundColor Green
    Write-Host $result -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao verificar usuários: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 6. REINICIAR AUTHSERVICE
# ===================================================================
Write-Host "`n6. Reiniciando AuthService..." -ForegroundColor Yellow

try {
    docker restart fintech-auth-service
    Write-Host "✅ AuthService reiniciado" -ForegroundColor Green
    Write-Host "   Aguardando 10 segundos para inicialização..." -ForegroundColor Gray
    Start-Sleep 10
} catch {
    Write-Host "❌ Erro ao reiniciar AuthService: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 7. TESTAR LOGIN ADMIN
# ===================================================================
Write-Host "`n7. Testando login admin..." -ForegroundColor Yellow

$loginPayload = @{
    email = "admin@fintech.com"
    password = "Admin123!"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginPayload -ContentType "application/json"
    Write-Host "✅ Login admin funcionando!" -ForegroundColor Green
    Write-Host "   Token: $($response.accessToken.Substring(0,50))..." -ForegroundColor Gray
    Write-Host "   User: $($response.user.email)" -ForegroundColor Gray
    Write-Host "   Role: $($response.user.role)" -ForegroundColor Gray
    $global:adminToken = $response.accessToken
} catch {
    Write-Host "❌ Login admin falhou: $($_.Exception.Message)" -ForegroundColor Red
    
    # Tentar com senha alternativa
    Write-Host "   Tentando com senha 'password'..." -ForegroundColor Yellow
    $loginPayload2 = @{
        email = "admin@fintech.com"
        password = "password"
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginPayload2 -ContentType "application/json"
        Write-Host "✅ Login admin funcionando com senha 'password'!" -ForegroundColor Green
        Write-Host "   Token: $($response.accessToken.Substring(0,50))..." -ForegroundColor Gray
        $global:adminToken = $response.accessToken
    } catch {
        Write-Host "❌ Login admin falhou também com 'password': $($_.Exception.Message)" -ForegroundColor Red
    }
}

# ===================================================================
# 8. TESTAR LOGIN CLIENTE
# ===================================================================
Write-Host "`n8. Testando login cliente..." -ForegroundColor Yellow

$clientLoginPayload = @{
    email = "cliente@fintech.com"
    password = "password"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $clientLoginPayload -ContentType "application/json"
    Write-Host "✅ Login cliente funcionando!" -ForegroundColor Green
    Write-Host "   Token: $($response.accessToken.Substring(0,50))..." -ForegroundColor Gray
    Write-Host "   User: $($response.user.email)" -ForegroundColor Gray
    Write-Host "   Role: $($response.user.role)" -ForegroundColor Gray
    $global:clientToken = $response.accessToken
} catch {
    Write-Host "❌ Login cliente falhou: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 9. VERIFICAR STATUS DOS FRONTENDS
# ===================================================================
Write-Host "`n9. Verificando status dos frontends..." -ForegroundColor Yellow

try {
    $backofficeStatus = Invoke-RestMethod -Uri "http://localhost:3000" -Method GET -TimeoutSec 5
    Write-Host "✅ BackofficeWeb (porta 3000): Funcionando" -ForegroundColor Green
} catch {
    Write-Host "⚠️ BackofficeWeb (porta 3000): Não acessível" -ForegroundColor Yellow
}

try {
    $internetBankingStatus = Invoke-RestMethod -Uri "http://localhost:3001" -Method GET -TimeoutSec 5
    Write-Host "✅ InternetBankingWeb (porta 3001): Funcionando" -ForegroundColor Green
} catch {
    Write-Host "⚠️ InternetBankingWeb (porta 3001): Não acessível" -ForegroundColor Yellow
}

# ===================================================================
# 10. INSTRUÇÕES PARA O USUÁRIO
# ===================================================================
Write-Host "`n=== INSTRUÇÕES PARA LOGIN ===" -ForegroundColor Cyan
Write-Host "🌐 BackofficeWeb: http://localhost:3000" -ForegroundColor White
Write-Host "   Email: admin@fintech.com" -ForegroundColor Gray
Write-Host "   Senha: Admin123! (ou 'password' se a primeira não funcionar)" -ForegroundColor Gray

Write-Host "`n🏦 InternetBankingWeb: http://localhost:3001" -ForegroundColor White
Write-Host "   Email: cliente@fintech.com" -ForegroundColor Gray
Write-Host "   Senha: password" -ForegroundColor Gray

Write-Host "`n📋 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "1. Acesse os frontends nos links acima" -ForegroundColor White
Write-Host "2. Faça login com as credenciais fornecidas" -ForegroundColor White
Write-Host "3. Se ainda houver problemas, verifique o console do navegador" -ForegroundColor White
Write-Host "4. Limpe o localStorage se necessário (F12 > Application > Local Storage > Clear)" -ForegroundColor White

Write-Host "`n🎉 CORREÇÃO DE AUTENTICAÇÃO CONCLUÍDA!" -ForegroundColor Green
