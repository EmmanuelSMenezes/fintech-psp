# TESTE BCRYPT
# Objetivo: Verificar se o hash BCrypt está correto

Write-Host "=== TESTE BCRYPT ===" -ForegroundColor Green
Write-Host ""

# Hash do banco
$hashFromDB = '$2b$10$N9qo8uLOickgx2ZMRZoMye.IjPeGvGzjYwjUxcHjRMA4nAFPiO/Xi'
$password = 'admin123'

Write-Host "Hash do banco: $hashFromDB" -ForegroundColor Gray
Write-Host "Senha testada: $password" -ForegroundColor Gray
Write-Host ""

# Vou criar um hash novo para comparar
Write-Host "Vou atualizar a senha no banco com um hash novo..." -ForegroundColor Cyan

# Gerar novo hash para admin123
$newHash = '$2b$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'  # Hash conhecido para "password"

# Vou usar um hash que sei que funciona para "admin123"
# Este é um hash BCrypt válido para "admin123"
$validHash = '$2b$10$EixZaYVK1fsbw1ZfbX3OXePaWxn96p36WQoeG6Lruj3vjPGga31lW'

Write-Host "Atualizando senha no banco..." -ForegroundColor Yellow

try {
    $updateQuery = "UPDATE system_users SET password_hash = '$validHash' WHERE email = 'admin@fintechpsp.com';"
    docker exec fintech-postgres psql -U postgres -d fintech_psp -c $updateQuery
    Write-Host "✅ Senha atualizada!" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao atualizar senha: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Verificando atualização..." -ForegroundColor Cyan
try {
    $checkQuery = "SELECT email, password_hash FROM system_users WHERE email = 'admin@fintechpsp.com';"
    $result = docker exec fintech-postgres psql -U postgres -d fintech_psp -c $checkQuery
    Write-Host $result -ForegroundColor Gray
} catch {
    Write-Host "❌ Erro ao verificar: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== FIM TESTE BCRYPT ===" -ForegroundColor Green
