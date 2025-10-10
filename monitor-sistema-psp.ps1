# ========================================
# MONITOR SISTEMA PSP - DASHBOARD TEMPO REAL
# ========================================

param(
    [int]$IntervalSeconds = 30,
    [switch]$ContinuousMode,
    [switch]$AlertsOnly
)

function Get-ServiceStatus {
    param($ServiceName, $Url)
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        $response = Invoke-RestMethod -Uri $Url -Method GET -TimeoutSec 5 -ErrorAction Stop
        $stopwatch.Stop()
        return @{
            Name = $ServiceName
            Status = "ONLINE"
            ResponseTime = $stopwatch.Elapsed.TotalMilliseconds
            LastCheck = Get-Date
        }
    } catch {
        return @{
            Name = $ServiceName
            Status = "OFFLINE"
            ResponseTime = -1
            LastCheck = Get-Date
            Error = $_.Exception.Message
        }
    }
}

function Get-DatabaseMetrics {
    try {
        # Conexões ativas
        $connections = docker exec fintech-postgres psql -U postgres -d fintech_psp -t -c "SELECT count(*) FROM pg_stat_activity WHERE state = 'active';" 2>$null
        
        # Tamanho do banco
        $dbSize = docker exec fintech-postgres psql -U postgres -d fintech_psp -t -c "SELECT pg_size_pretty(pg_database_size('fintech_psp'));" 2>$null
        
        # Transações por minuto (últimos 5 minutos)
        $recentTransactions = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COUNT(*) FROM transaction_history WHERE created_at > NOW() - INTERVAL '5 minutes';" 2>$null
        
        return @{
            Status = "ONLINE"
            ActiveConnections = $connections.Trim()
            DatabaseSize = $dbSize.Trim()
            RecentTransactions = $recentTransactions.Trim()
            LastCheck = Get-Date
        }
    } catch {
        return @{
            Status = "OFFLINE"
            Error = $_.Exception.Message
            LastCheck = Get-Date
        }
    }
}

function Get-BusinessMetrics {
    try {
        # Total de empresas
        $totalCompanies = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COUNT(*) FROM company_service.companies;" 2>$null
        
        # Total de usuários
        $totalUsers = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COUNT(*) FROM users;" 2>$null
        
        # Total de contas
        $totalAccounts = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COUNT(*) FROM accounts;" 2>$null
        
        # Saldo total do sistema
        $totalBalance = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COALESCE(SUM(available_balance), 0) FROM accounts;" 2>$null
        
        # Transações hoje
        $todayTransactions = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -t -c "SELECT COUNT(*) FROM transaction_history WHERE DATE(created_at) = CURRENT_DATE;" 2>$null
        
        return @{
            TotalCompanies = $totalCompanies.Trim()
            TotalUsers = $totalUsers.Trim()
            TotalAccounts = $totalAccounts.Trim()
            TotalBalance = $totalBalance.Trim()
            TodayTransactions = $todayTransactions.Trim()
            LastCheck = Get-Date
        }
    } catch {
        return @{
            Error = $_.Exception.Message
            LastCheck = Get-Date
        }
    }
}

function Show-Dashboard {
    param($Services, $Database, $Business)
    
    Clear-Host
    Write-Host "🏦 FINTECH PSP - DASHBOARD DE MONITORAMENTO" -ForegroundColor Cyan
    Write-Host "=============================================" -ForegroundColor Cyan
    Write-Host "Última Atualização: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    # Status dos Microserviços
    Write-Host "🔧 MICROSERVIÇOS" -ForegroundColor Yellow
    Write-Host "----------------" -ForegroundColor Yellow
    foreach ($service in $Services) {
        $statusColor = if ($service.Status -eq "ONLINE") { "Green" } else { "Red" }
        $statusIcon = if ($service.Status -eq "ONLINE") { "✅" } else { "❌" }
        $responseTime = if ($service.ResponseTime -gt 0) { "$([math]::Round($service.ResponseTime, 0))ms" } else { "N/A" }
        Write-Host "$statusIcon $($service.Name.PadRight(20)) $($service.Status.PadRight(8)) $responseTime" -ForegroundColor $statusColor
    }
    
    Write-Host ""
    
    # Status do Banco de Dados
    Write-Host "🗄️ BANCO DE DADOS" -ForegroundColor Yellow
    Write-Host "------------------" -ForegroundColor Yellow
    if ($Database.Status -eq "ONLINE") {
        Write-Host "✅ PostgreSQL               ONLINE" -ForegroundColor Green
        Write-Host "   Conexões Ativas:         $($Database.ActiveConnections)" -ForegroundColor White
        Write-Host "   Tamanho do Banco:        $($Database.DatabaseSize)" -ForegroundColor White
        Write-Host "   Transações (5min):       $($Database.RecentTransactions)" -ForegroundColor White
    } else {
        Write-Host "❌ PostgreSQL               OFFLINE" -ForegroundColor Red
        Write-Host "   Erro: $($Database.Error)" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # Métricas de Negócio
    Write-Host "📊 MÉTRICAS DE NEGÓCIO" -ForegroundColor Yellow
    Write-Host "----------------------" -ForegroundColor Yellow
    if (-not $Business.Error) {
        Write-Host "🏢 Empresas Cadastradas:     $($Business.TotalCompanies)" -ForegroundColor White
        Write-Host "👥 Usuários Ativos:          $($Business.TotalUsers)" -ForegroundColor White
        Write-Host "🏦 Contas Criadas:           $($Business.TotalAccounts)" -ForegroundColor White
        Write-Host "💰 Saldo Total Sistema:      R$ $($Business.TotalBalance)" -ForegroundColor Green
        Write-Host "💸 Transações Hoje:          $($Business.TodayTransactions)" -ForegroundColor Cyan
    } else {
        Write-Host "❌ Erro ao obter métricas: $($Business.Error)" -ForegroundColor Red
    }
    
    Write-Host ""
    Write-Host "🔄 Próxima atualização em $IntervalSeconds segundos..." -ForegroundColor Gray
    Write-Host "Pressione Ctrl+C para sair" -ForegroundColor Gray
}

# ========================================
# EXECUÇÃO DO MONITOR
# ========================================

Write-Host "🚀 Iniciando Monitor do Sistema PSP..." -ForegroundColor Cyan
Write-Host "Intervalo: $IntervalSeconds segundos" -ForegroundColor Gray
Write-Host ""

$services = @(
    @{ Name = "AuthService"; Url = "http://localhost:5001/" },
    @{ Name = "BalanceService"; Url = "http://localhost:5003/" },
    @{ Name = "IntegrationService"; Url = "http://localhost:5005/" },
    @{ Name = "UserService"; Url = "http://localhost:5006/" },
    @{ Name = "ConfigService"; Url = "http://localhost:5007/" },
    @{ Name = "CompanyService"; Url = "http://localhost:5009/" }
)

do {
    # Coletar métricas
    $serviceStatuses = @()
    foreach ($service in $services) {
        $serviceStatuses += Get-ServiceStatus $service.Name $service.Url
    }
    
    $databaseMetrics = Get-DatabaseMetrics
    $businessMetrics = Get-BusinessMetrics
    
    # Mostrar dashboard
    Show-Dashboard $serviceStatuses $databaseMetrics $businessMetrics
    
    # Verificar alertas críticos
    $offlineServices = $serviceStatuses | Where-Object { $_.Status -eq "OFFLINE" }
    if ($offlineServices.Count -gt 0 -and $AlertsOnly) {
        Write-Host ""
        Write-Host "🚨 ALERTAS CRÍTICOS:" -ForegroundColor Red
        foreach ($service in $offlineServices) {
            Write-Host "   ❌ $($service.Name) está OFFLINE" -ForegroundColor Red
        }
    }
    
    if ($ContinuousMode) {
        Start-Sleep $IntervalSeconds
    }
} while ($ContinuousMode)

Write-Host ""
Write-Host "✅ Monitor finalizado." -ForegroundColor Green
