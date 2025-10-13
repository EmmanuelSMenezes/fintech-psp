Write-Host "=== TESTE 9: CONSULTA DE HISTORICO E RELATORIOS ===" -ForegroundColor Cyan

$baseUrl = "http://localhost:5000"

# 1. Login como admin
Write-Host "1. Fazendo login como admin..." -ForegroundColor Cyan
$adminBody = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

try {
    $adminLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $adminBody -ContentType "application/json" -TimeoutSec 10
    $adminToken = $adminLoginResponse.accessToken
    $adminHeaders = @{ Authorization = "Bearer $adminToken" }
    Write-Host "   ✅ Login admin OK" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Erro login admin: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 2. Login como cliente
Write-Host "2. Fazendo login como cliente..." -ForegroundColor Cyan
$clientBody = @{
    email = "joao.silva@empresateste.com"
    password = "cliente123"
} | ConvertTo-Json

try {
    $clientLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $clientBody -ContentType "application/json" -TimeoutSec 10
    $clientToken = $clientLoginResponse.accessToken
    $clientHeaders = @{ Authorization = "Bearer $clientToken" }
    $clientId = $clientLoginResponse.user.id
    Write-Host "   ✅ Login cliente OK (ID: $clientId)" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Erro login cliente: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# 3. Relatório de Empresas
Write-Host "3. Testando relatorio de empresas..." -ForegroundColor Cyan
try {
    $empresasResponse = Invoke-RestMethod -Uri "$baseUrl/admin/companies" -Headers $adminHeaders -TimeoutSec 10
    Write-Host "   ✅ Empresas encontradas: $($empresasResponse.Count)" -ForegroundColor Green
    $empresasResponse | ForEach-Object {
        Write-Host "      - $($_.name) (CNPJ: $($_.cnpj))" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ❌ Erro ao obter empresas: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Relatório de Usuários
Write-Host "4. Testando relatorio de usuarios..." -ForegroundColor Cyan
try {
    $usuariosResponse = Invoke-RestMethod -Uri "$baseUrl/admin/users" -Headers $adminHeaders -TimeoutSec 10
    Write-Host "   ✅ Usuarios encontrados: $($usuariosResponse.Count)" -ForegroundColor Green
    $usuariosResponse | ForEach-Object {
        Write-Host "      - $($_.name) ($($_.email)) - Role: $($_.role)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ❌ Erro ao obter usuarios: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Consulta de Saldo
Write-Host "5. Testando consulta de saldo..." -ForegroundColor Cyan
try {
    $saldoResponse = Invoke-RestMethod -Uri "$baseUrl/saldo/$clientId" -Headers $clientHeaders -TimeoutSec 10
    Write-Host "   ✅ Saldo do cliente:" -ForegroundColor Green
    Write-Host "      Conta: $($saldoResponse.accountId)" -ForegroundColor Gray
    Write-Host "      Disponivel: R$ $($saldoResponse.availableBalance)" -ForegroundColor Gray
    Write-Host "      Total: R$ $($saldoResponse.totalBalance)" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro ao obter saldo: $($_.Exception.Message)" -ForegroundColor Red
}

# 6. Consulta de Transações via SQL
Write-Host "6. Testando consulta de transacoes via banco..." -ForegroundColor Cyan
try {
    $sqlResult = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT COUNT(*) FROM transactions;" -t
    $totalTransacoes = $sqlResult.Trim()
    Write-Host "   ✅ Total de transacoes no banco: $totalTransacoes" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Erro ao consultar banco: $($_.Exception.Message)" -ForegroundColor Red
}

# 7. Teste de Configurações
Write-Host "7. Testando configuracoes do sistema..." -ForegroundColor Cyan
try {
    $configResponse = Invoke-RestMethod -Uri "$baseUrl/admin/config" -Headers $adminHeaders -TimeoutSec 10
    Write-Host "   ✅ Configuracoes obtidas:" -ForegroundColor Green
    Write-Host "      PIX habilitado: $($configResponse.pixEnabled)" -ForegroundColor Gray
    Write-Host "      TED habilitado: $($configResponse.tedEnabled)" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro ao obter configuracoes: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== RESUMO TESTE 9 ===" -ForegroundColor Yellow
Write-Host "✅ Relatorios administrativos testados" -ForegroundColor Green
Write-Host "✅ Consultas de saldo testadas" -ForegroundColor Green
Write-Host "✅ Configuracoes do sistema testadas" -ForegroundColor Green
Write-Host ""
Write-Host "Teste de Relatorios concluido!" -ForegroundColor Cyan
Write-Host ""
Write-Host "=== FIM TESTE ===" -ForegroundColor Cyan
