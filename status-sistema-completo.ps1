#!/usr/bin/env pwsh

Write-Host "🎯 STATUS COMPLETO DO SISTEMA FINTECHPSP" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

# Verificar infraestrutura Docker
Write-Host "📊 INFRAESTRUTURA DOCKER:" -ForegroundColor Yellow
try {
    $containers = docker ps --format "table {{.Names}}\t{{.Status}}" | findstr fintech
    if ($containers) {
        Write-Host "✅ Containers Docker funcionando:" -ForegroundColor Green
        docker ps --format "table {{.Names}}\t{{.Status}}" | findstr fintech
    } else {
        Write-Host "❌ Nenhum container Docker encontrado" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao verificar containers Docker" -ForegroundColor Red
}

Write-Host ""

# Verificar portas dos microserviços
Write-Host "🚀 MICROSERVIÇOS (PORTAS):" -ForegroundColor Yellow
$ports = @("5000", "5001", "5003", "5004", "5005", "5006", "5007", "5008", "5010")
foreach ($port in $ports) {
    $listening = netstat -an | findstr "127.0.0.1:$port.*LISTENING"
    if ($listening) {
        Write-Host "✅ Porta $port - ATIVA" -ForegroundColor Green
    } else {
        Write-Host "❌ Porta $port - INATIVA" -ForegroundColor Red
    }
}

Write-Host ""

# Testar endpoints específicos
Write-Host "🔍 TESTES DE ENDPOINTS:" -ForegroundColor Yellow

# BalanceService Health
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5003/test/health" -Method GET -TimeoutSec 3
    Write-Host "✅ BalanceService Health: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ BalanceService Health: FALHOU" -ForegroundColor Red
}

# AuthService Health  
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5001/health" -Method GET -TimeoutSec 3
    Write-Host "✅ AuthService Health: OK" -ForegroundColor Green
} catch {
    Write-Host "❌ AuthService Health: FALHOU" -ForegroundColor Red
}

Write-Host ""

# Verificar banco de dados
Write-Host "🗄️ BANCO DE DADOS:" -ForegroundColor Yellow
try {
    $dbTest = docker exec fintech-postgres pg_isready -U postgres 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ PostgreSQL: ONLINE" -ForegroundColor Green
        
        # Verificar dados de teste
        $companyCheck = docker exec fintech-postgres psql -U postgres -d fintech_psp -t -c "SELECT COUNT(*) FROM company_service.companies WHERE cnpj = '12345678000199';" 2>$null
        if ($companyCheck -and $companyCheck.Trim() -eq "1") {
            Write-Host "✅ Dados de teste: EmpresaTeste encontrada" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Dados de teste: EmpresaTeste não encontrada" -ForegroundColor Yellow
        }
        
        $accountCheck = docker exec fintech-postgres psql -U postgres -d fintech_psp -t -c "SELECT balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" 2>$null
        if ($accountCheck) {
            $balance = $accountCheck.Trim()
            Write-Host "✅ Saldo da conta teste: R$ $balance" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Conta de teste não encontrada" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ PostgreSQL: OFFLINE" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Erro ao verificar banco de dados" -ForegroundColor Red
}

Write-Host ""
Write-Host "🎯 RESUMO:" -ForegroundColor Cyan
Write-Host "- Sistema FintechPSP com 8 microserviços" -ForegroundColor White
Write-Host "- Cash-Out implementado no BalanceService" -ForegroundColor White  
Write-Host "- Integração Sicoob no IntegrationService" -ForegroundColor White
Write-Host "- Dados de teste configurados" -ForegroundColor White
Write-Host ""
Write-Host "📋 ENDPOINTS PRINCIPAIS:" -ForegroundColor Cyan
Write-Host "- API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "- AuthService: http://localhost:5001" -ForegroundColor White
Write-Host "- BalanceService: http://localhost:5003 (Cash-Out)" -ForegroundColor White
Write-Host "- TransactionService: http://localhost:5004" -ForegroundColor White
Write-Host "- IntegrationService: http://localhost:5005 (Sicoob)" -ForegroundColor White
Write-Host "- UserService: http://localhost:5006" -ForegroundColor White
Write-Host "- ConfigService: http://localhost:5007" -ForegroundColor White
Write-Host "- WebhookService: http://localhost:5008" -ForegroundColor White
Write-Host "- CompanyService: http://localhost:5010" -ForegroundColor White
Write-Host ""
Write-Host "🚀 PRONTO PARA TESTES!" -ForegroundColor Green
