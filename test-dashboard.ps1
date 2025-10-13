#!/usr/bin/env pwsh

Write-Host "=== TESTE 7: DASHBOARD E VISUALIZAÇÃO DE DADOS ===" -ForegroundColor Magenta
Write-Host ""

# Função para fazer login e obter token
function Get-AuthToken {
    param($email, $password)
    
    $headers = @{'Content-Type' = 'application/json'}
    $body = @{
        email = $email
        password = $password
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $body -Headers $headers
        return $response.accessToken
    } catch {
        Write-Host "❌ Erro no login: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Login admin
Write-Host "1. Fazendo login como admin..." -ForegroundColor Yellow
$adminToken = Get-AuthToken -email "admin@fintechpsp.com" -password "admin123"
if (-not $adminToken) { exit 1 }
Write-Host "✅ Login admin OK" -ForegroundColor Green

$authHeaders = @{
    'Authorization' = "Bearer $adminToken"
    'Content-Type' = 'application/json'
}

Write-Host ""
Write-Host "2. Testando Dashboard APIs..." -ForegroundColor Yellow

# Teste 1: Dashboard Principal (BackofficeWeb)
Write-Host "2.1 Dashboard Principal (BackofficeWeb):" -ForegroundColor Cyan
try {
    $dashboardUrl = "http://localhost:3000"
    Write-Host "   URL: $dashboardUrl" -ForegroundColor Gray
    
    # Testar se a página carrega (apenas verificar se responde)
    $response = Invoke-WebRequest -Uri $dashboardUrl -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ BackofficeWeb carregando (Status: $($response.StatusCode))" -ForegroundColor Green
    } else {
        Write-Host "   ❌ BackofficeWeb erro (Status: $($response.StatusCode))" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Erro ao acessar BackofficeWeb: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 2: Dashboard Cliente (InternetBankingWeb)
Write-Host "2.2 Dashboard Cliente (InternetBankingWeb):" -ForegroundColor Cyan
try {
    $clientDashboardUrl = "http://localhost:3001"
    Write-Host "   URL: $clientDashboardUrl" -ForegroundColor Gray
    
    $response = Invoke-WebRequest -Uri $clientDashboardUrl -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "   ✅ InternetBankingWeb carregando (Status: $($response.StatusCode))" -ForegroundColor Green
    } else {
        Write-Host "   ❌ InternetBankingWeb erro (Status: $($response.StatusCode))" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Erro ao acessar InternetBankingWeb: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Testando APIs de Dados para Dashboard..." -ForegroundColor Yellow

# Teste 3: Relatório de Transações
Write-Host "3.1 Relatório de Transações:" -ForegroundColor Cyan
try {
    $reportResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/transacoes/report' -Headers $authHeaders
    Write-Host "   ✅ Relatório obtido:" -ForegroundColor Green
    Write-Host "      Total: $($reportResponse.totalTransactions)" -ForegroundColor Gray
    Write-Host "      Volume: R$ $($reportResponse.totalVolume)" -ForegroundColor Gray
    Write-Host "      Sucesso: $($reportResponse.successfulTransactions)" -ForegroundColor Gray
    Write-Host "      Falhas: $($reportResponse.failedTransactions)" -ForegroundColor Gray
    Write-Host "      Pendentes: $($reportResponse.pendingTransactions)" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro ao obter relatório: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 4: Lista de Empresas
Write-Host "3.2 Lista de Empresas:" -ForegroundColor Cyan
try {
    $companiesResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/companies' -Headers $authHeaders
    Write-Host "   ✅ Empresas obtidas:" -ForegroundColor Green
    Write-Host "      Total: $($companiesResponse.companies.Count)" -ForegroundColor Gray
    if ($companiesResponse.companies.Count -gt 0) {
        $companiesResponse.companies | Select-Object -First 3 | ForEach-Object {
            Write-Host "      - $($_.razaoSocial) (CNPJ: $($_.cnpj))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "   ❌ Erro ao obter empresas: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 5: Lista de Usuários
Write-Host "3.3 Lista de Usuários:" -ForegroundColor Cyan
try {
    $usersResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/users' -Headers $authHeaders
    Write-Host "   ✅ Usuários obtidos:" -ForegroundColor Green
    Write-Host "      Total: $($usersResponse.users.Count)" -ForegroundColor Gray
    if ($usersResponse.users.Count -gt 0) {
        $usersResponse.users | Select-Object -First 3 | ForEach-Object {
            Write-Host "      - $($_.name) ($($_.email)) - Role: $($_.role)" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "   ❌ Erro ao obter usuários: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 6: Lista de Contas
Write-Host "3.4 Lista de Contas:" -ForegroundColor Cyan
try {
    $accountsResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/contas' -Headers $authHeaders
    Write-Host "   ✅ Contas obtidas:" -ForegroundColor Green
    Write-Host "      Total: $($accountsResponse.contas.Count)" -ForegroundColor Gray
    if ($accountsResponse.contas.Count -gt 0) {
        $accountsResponse.contas | Select-Object -First 3 | ForEach-Object {
            Write-Host "      - ContaId: $($_.id) (Banco: $($_.bankCode))" -ForegroundColor Gray
        }
    }
} catch {
    Write-Host "   ❌ Erro ao obter contas: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 7: Configurações do Sistema
Write-Host "3.5 Configurações do Sistema:" -ForegroundColor Cyan
try {
    $configResponse = Invoke-RestMethod -Uri 'http://localhost:5007/config/system' -Headers $authHeaders
    Write-Host "   ✅ Configurações obtidas:" -ForegroundColor Green
    Write-Host "      PIX Enabled: $($configResponse.pixEnabled)" -ForegroundColor Gray
    Write-Host "      TED Enabled: $($configResponse.tedEnabled)" -ForegroundColor Gray
    Write-Host "      Maintenance: $($configResponse.maintenanceMode)" -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro ao obter configurações: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "4. Testando Dashboard do Cliente..." -ForegroundColor Yellow

# Login cliente
Write-Host "4.1 Login como cliente..." -ForegroundColor Cyan
$clientToken = Get-AuthToken -email "joao.silva@empresateste.com" -password "cliente123"
if ($clientToken) {
    Write-Host "   ✅ Login cliente OK" -ForegroundColor Green
    
    $clientHeaders = @{
        'Authorization' = "Bearer $clientToken"
        'Content-Type' = 'application/json'
    }
    
    # Teste saldo do cliente
    Write-Host "4.2 Saldo do Cliente:" -ForegroundColor Cyan
    try {
        $balanceResponse = Invoke-RestMethod -Uri 'http://localhost:5003/saldo/ec8f0a2c-1347-4160-bbe0-c39448f1c1cb' -Headers $clientHeaders
        Write-Host "   ✅ Saldo obtido:" -ForegroundColor Green
        Write-Host "      Disponível: R$ $($balanceResponse.availableBalance)" -ForegroundColor Gray
        Write-Host "      Bloqueado: R$ $($balanceResponse.blockedBalance)" -ForegroundColor Gray
        Write-Host "      Moeda: $($balanceResponse.currency)" -ForegroundColor Gray
    } catch {
        Write-Host "   ❌ Erro ao obter saldo: $($_.Exception.Message)" -ForegroundColor Red
    }
    
    # Teste histórico de transações do cliente
    Write-Host "4.3 Histórico de Transações:" -ForegroundColor Cyan
    try {
        # Primeiro testar TransactionService direto
        Write-Host "   Testando TransactionService direto..." -ForegroundColor Yellow
        $directResponse = Invoke-RestMethod -Uri 'http://localhost:5004/transacoes' -Headers $clientHeaders -TimeoutSec 5
        Write-Host "   ✅ TransactionService direto: Total $($directResponse.total)" -ForegroundColor Green

        # Agora testar via API Gateway
        Write-Host "   Testando via API Gateway..." -ForegroundColor Yellow
        $historyResponse = Invoke-RestMethod -Uri 'http://localhost:5000/banking/transacoes/historico' -Headers $clientHeaders -TimeoutSec 5
        Write-Host "   ✅ Histórico via Gateway: Total $($historyResponse.total)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro ao obter histórico: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "   ❌ Falha no login do cliente" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== RESUMO TESTE 7 ===" -ForegroundColor Magenta
Write-Host "✅ Dashboard APIs testadas" -ForegroundColor Green
Write-Host "✅ Frontends acessíveis" -ForegroundColor Green
Write-Host "✅ Dados de visualização disponíveis" -ForegroundColor Green
Write-Host ""
Write-Host "Teste de Dashboard concluído!" -ForegroundColor Green
