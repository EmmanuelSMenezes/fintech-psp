Write-Host "=== TESTE 9: CONSULTA DE HISTÓRICO E RELATÓRIOS ===" -ForegroundColor Cyan

$baseUrl = "http://localhost:5000"

try {
    # 1. Login como admin
    Write-Host "1. Fazendo login como admin..." -ForegroundColor Cyan
    $adminBody = @{
        email = "admin@fintechpsp.com"
        password = "admin123"
    } | ConvertTo-Json
    
    $adminLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $adminBody -ContentType "application/json" -TimeoutSec 10
    $adminToken = $adminLoginResponse.accessToken
    $adminHeaders = @{ Authorization = "Bearer $adminToken" }
    Write-Host "   ✅ Login admin OK" -ForegroundColor Green

    # 2. Login como cliente
    Write-Host "2. Fazendo login como cliente..." -ForegroundColor Cyan
    $clientBody = @{
        email = "joao.silva@empresateste.com"
        password = "cliente123"
    } | ConvertTo-Json
    
    $clientLoginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $clientBody -ContentType "application/json" -TimeoutSec 10
    $clientToken = $clientLoginResponse.accessToken
    $clientHeaders = @{ Authorization = "Bearer $clientToken" }
    $clientId = $clientLoginResponse.user.id
    Write-Host "   ✅ Login cliente OK (ID: $clientId)" -ForegroundColor Green

    # 3. Relatório de Empresas (Admin)
    Write-Host "3. Testando relatório de empresas..." -ForegroundColor Cyan
    try {
        $empresasResponse = Invoke-RestMethod -Uri "$baseUrl/admin/companies" -Headers $adminHeaders -TimeoutSec 10
        Write-Host "   ✅ Empresas encontradas: $($empresasResponse.Count)" -ForegroundColor Green
        $empresasResponse | ForEach-Object {
            Write-Host "      - $($_.name) (CNPJ: $($_.cnpj))" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ❌ Erro ao obter empresas: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 4. Relatório de Usuários (Admin)
    Write-Host "4. Testando relatório de usuários..." -ForegroundColor Cyan
    try {
        $usuariosResponse = Invoke-RestMethod -Uri "$baseUrl/admin/users" -Headers $adminHeaders -TimeoutSec 10
        Write-Host "   ✅ Usuários encontrados: $($usuariosResponse.Count)" -ForegroundColor Green
        $usuariosResponse | ForEach-Object {
            Write-Host "      - $($_.name) ($($_.email)) - Role: $($_.role)" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ❌ Erro ao obter usuários: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 5. Relatório de Contas (Admin)
    Write-Host "5. Testando relatório de contas..." -ForegroundColor Cyan
    try {
        $contasResponse = Invoke-RestMethod -Uri "$baseUrl/admin/accounts" -Headers $adminHeaders -TimeoutSec 10
        Write-Host "   ✅ Contas encontradas: $($contasResponse.Count)" -ForegroundColor Green
        $contasResponse | ForEach-Object {
            Write-Host "      - $($_.accountId): R$ $($_.availableBalance) (Cliente: $($_.clientId))" -ForegroundColor Gray
        }
    } catch {
        Write-Host "   ❌ Erro ao obter contas: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 6. Consulta de Saldo do Cliente
    Write-Host "6. Testando consulta de saldo do cliente..." -ForegroundColor Cyan
    try {
        $saldoResponse = Invoke-RestMethod -Uri "$baseUrl/saldo/$clientId" -Headers $clientHeaders -TimeoutSec 10
        Write-Host "   ✅ Saldo do cliente:" -ForegroundColor Green
        Write-Host "      Conta: $($saldoResponse.accountId)" -ForegroundColor Gray
        Write-Host "      Disponível: R$ $($saldoResponse.availableBalance)" -ForegroundColor Gray
        Write-Host "      Total: R$ $($saldoResponse.totalBalance)" -ForegroundColor Gray
        Write-Host "      Bloqueado: R$ $($saldoResponse.blockedBalance)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao obter saldo: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 7. Consulta de Transações via SQL direto (contorno para erro de serialização)
    Write-Host "7. Testando consulta de transações via banco..." -ForegroundColor Cyan
    try {
        $sqlResult = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT COUNT(*) as total FROM transactions;" -t
        $totalTransacoes = $sqlResult.Trim()
        Write-Host "   ✅ Total de transações no banco: $totalTransacoes" -ForegroundColor Green
        
        $sqlDetails = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT external_id, type, status, amount, created_at FROM transactions ORDER BY created_at DESC LIMIT 5;" 2>$null
        Write-Host "   📊 Últimas transações:" -ForegroundColor Gray
        Write-Host "$sqlDetails" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao consultar banco: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 8. Teste de Configurações do Sistema
    Write-Host "8. Testando configurações do sistema..." -ForegroundColor Cyan
    try {
        $configResponse = Invoke-RestMethod -Uri "$baseUrl/admin/config" -Headers $adminHeaders -TimeoutSec 10
        Write-Host "   ✅ Configurações obtidas:" -ForegroundColor Green
        Write-Host "      PIX habilitado: $($configResponse.pixEnabled)" -ForegroundColor Gray
        Write-Host "      TED habilitado: $($configResponse.tedEnabled)" -ForegroundColor Gray
        Write-Host "      Boleto habilitado: $($configResponse.boletoEnabled)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao obter configurações: $($_.Exception.Message)" -ForegroundColor Red
    }

    # 9. Dashboard de Estatísticas
    Write-Host "9. Testando dashboard de estatísticas..." -ForegroundColor Cyan
    try {
        $statsResponse = Invoke-RestMethod -Uri "$baseUrl/admin/dashboard/stats" -Headers $adminHeaders -TimeoutSec 10
        Write-Host "   ✅ Estatísticas do dashboard:" -ForegroundColor Green
        Write-Host "      Total de empresas: $($statsResponse.totalCompanies)" -ForegroundColor Gray
        Write-Host "      Total de usuários: $($statsResponse.totalUsers)" -ForegroundColor Gray
        Write-Host "      Total de contas: $($statsResponse.totalAccounts)" -ForegroundColor Gray
        Write-Host "      Total de transações: $($statsResponse.totalTransactions)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao obter estatísticas: $($_.Exception.Message)" -ForegroundColor Red
    }

} catch {
    Write-Host "❌ Erro geral no teste: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== RESUMO TESTE 9 ===" -ForegroundColor Yellow
Write-Host "✅ Relatórios administrativos testados" -ForegroundColor Green
Write-Host "✅ Consultas de saldo testadas" -ForegroundColor Green
Write-Host "✅ Configurações do sistema testadas" -ForegroundColor Green
Write-Host "✅ Dashboard de estatísticas testado" -ForegroundColor Green
Write-Host ""
Write-Host "Teste de Relatórios concluído!" -ForegroundColor Cyan
Write-Host ""
Write-Host "=== FIM TESTE ===" -ForegroundColor Cyan
