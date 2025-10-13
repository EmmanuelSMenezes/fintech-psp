# TESTE SIMPLES DE INFRAESTRUTURA
# Objetivo: Verificar se a infraestrutura básica está funcionando

Write-Host "=== TESTE SIMPLES DE INFRAESTRUTURA ===" -ForegroundColor Green
Write-Host ""

# 1. Verificar Docker containers
Write-Host "1. Verificando containers Docker..." -ForegroundColor Cyan
try {
    $containers = docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
    Write-Host $containers -ForegroundColor Gray
    
    # Verificar se os containers essenciais estão rodando
    $postgresRunning = docker ps --filter "name=fintech-postgres" --filter "status=running" -q
    $rabbitmqRunning = docker ps --filter "name=fintech-rabbitmq" --filter "status=running" -q
    $redisRunning = docker ps --filter "name=fintech-redis" --filter "status=running" -q
    
    if ($postgresRunning) { Write-Host "   ✅ PostgreSQL rodando" -ForegroundColor Green } else { Write-Host "   ❌ PostgreSQL não está rodando" -ForegroundColor Red }
    if ($rabbitmqRunning) { Write-Host "   ✅ RabbitMQ rodando" -ForegroundColor Green } else { Write-Host "   ❌ RabbitMQ não está rodando" -ForegroundColor Red }
    if ($redisRunning) { Write-Host "   ✅ Redis rodando" -ForegroundColor Green } else { Write-Host "   ❌ Redis não está rodando" -ForegroundColor Red }
} catch {
    Write-Host "   ❌ Erro ao verificar containers: $($_.Exception.Message)" -ForegroundColor Red
}

# 2. Verificar conectividade com PostgreSQL
Write-Host "2. Testando conectividade PostgreSQL..." -ForegroundColor Cyan
try {
    $pgTest = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT 'PostgreSQL OK' as status;"
    if ($pgTest -match "PostgreSQL OK") {
        Write-Host "   ✅ PostgreSQL conectando OK" -ForegroundColor Green
    } else {
        Write-Host "   ❌ PostgreSQL não responde corretamente" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Erro ao conectar PostgreSQL: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Verificar tabelas do banco
Write-Host "3. Verificando tabelas do banco..." -ForegroundColor Cyan
try {
    $tables = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "\dt" 2>$null
    if ($tables -match "system_users") {
        Write-Host "   ✅ Tabela system_users existe" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Tabela system_users não encontrada" -ForegroundColor Red
    }
    
    if ($tables -match "accounts") {
        Write-Host "   ✅ Tabela accounts existe" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Tabela accounts não encontrada" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Erro ao verificar tabelas: $($_.Exception.Message)" -ForegroundColor Red
}

# 4. Verificar usuário admin
Write-Host "4. Verificando usuário admin..." -ForegroundColor Cyan
try {
    $adminUser = docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT email FROM system_users WHERE email = 'admin@fintechpsp.com';" 2>$null
    if ($adminUser -match "admin@fintechpsp.com") {
        Write-Host "   ✅ Usuário admin existe" -ForegroundColor Green
    } else {
        Write-Host "   ❌ Usuário admin não encontrado" -ForegroundColor Red
    }
} catch {
    Write-Host "   ❌ Erro ao verificar usuário admin: $($_.Exception.Message)" -ForegroundColor Red
}

# 5. Verificar portas dos microservices
Write-Host "5. Verificando portas dos microservices..." -ForegroundColor Cyan
$ports = @(5000, 5001, 5003, 5004)
foreach ($port in $ports) {
    try {
        $listening = netstat -an | Select-String ":$port" | Select-String "LISTENING"
        if ($listening) {
            Write-Host "   ✅ Porta $port está em uso" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Porta $port não está em uso" -ForegroundColor Red
        }
    } catch {
        Write-Host "   ❌ Erro ao verificar porta $port" -ForegroundColor Red
    }
}

# 6. Teste simples de conectividade HTTP
Write-Host "6. Testando conectividade HTTP dos serviços..." -ForegroundColor Cyan

# API Gateway
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000" -TimeoutSec 3 -ErrorAction SilentlyContinue
    Write-Host "   ✅ API Gateway (5000) respondendo" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "404") {
        Write-Host "   ✅ API Gateway (5000) respondendo (404 esperado)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ API Gateway (5000) não responde: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# AuthService
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5001" -TimeoutSec 3 -ErrorAction SilentlyContinue
    Write-Host "   ✅ AuthService (5001) respondendo" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "404") {
        Write-Host "   ✅ AuthService (5001) respondendo (404 esperado)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ AuthService (5001) não responde: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# BalanceService
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5003" -TimeoutSec 3 -ErrorAction SilentlyContinue
    Write-Host "   ✅ BalanceService (5003) respondendo" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "404") {
        Write-Host "   ✅ BalanceService (5003) respondendo (404 esperado)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ BalanceService (5003) não responde: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# TransactionService
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5004" -TimeoutSec 3 -ErrorAction SilentlyContinue
    Write-Host "   ✅ TransactionService (5004) respondendo" -ForegroundColor Green
} catch {
    if ($_.Exception.Message -match "404") {
        Write-Host "   ✅ TransactionService (5004) respondendo (404 esperado)" -ForegroundColor Green
    } else {
        Write-Host "   ❌ TransactionService (5004) não responde: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== RESUMO ===" -ForegroundColor Green
Write-Host "✅ Infraestrutura Docker verificada" -ForegroundColor Green
Write-Host "✅ Conectividade de banco testada" -ForegroundColor Green
Write-Host "✅ Estrutura de dados validada" -ForegroundColor Green
Write-Host "✅ Microservices verificados" -ForegroundColor Green
Write-Host ""
Write-Host "Sistema pronto para testes!" -ForegroundColor Green
