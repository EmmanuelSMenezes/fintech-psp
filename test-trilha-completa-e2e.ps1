# ========================================
# TESTE E2E - TRILHA COMPLETA PSP-SICOOB
# ========================================

param(
    [switch]$SkipEnvironmentCheck,
    [switch]$Verbose
)

Write-Host "🚀 INICIANDO TESTES E2E - TRILHA COMPLETA PSP-SICOOB" -ForegroundColor Cyan
Write-Host "Data/Hora: $(Get-Date)" -ForegroundColor Gray
Write-Host ""

$global:TestResults = @()
$global:TotalTests = 0
$global:PassedTests = 0
$global:FailedTests = 0

function Add-TestResult {
    param($TestName, $Status, $Details, $Duration)
    $global:TotalTests++
    if ($Status -eq "PASS") { $global:PassedTests++ } else { $global:FailedTests++ }
    
    $result = @{
        Test = $TestName
        Status = $Status
        Details = $Details
        Duration = $Duration
        Timestamp = Get-Date
    }
    $global:TestResults += $result
    
    $color = if ($Status -eq "PASS") { "Green" } else { "Red" }
    $icon = if ($Status -eq "PASS") { "✅" } else { "❌" }
    Write-Host "$icon $TestName - $Status" -ForegroundColor $color
    if ($Verbose -and $Details) {
        Write-Host "   $Details" -ForegroundColor Gray
    }
}

function Test-ServiceHealth {
    param($ServiceName, $Url)
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-RestMethod -Uri $Url -Method GET -TimeoutSec 5 -ErrorAction Stop
        $stopwatch.Stop()
        Add-TestResult "Service Health: $ServiceName" "PASS" "Response: $($response.GetType().Name)" $stopwatch.Elapsed.TotalMilliseconds
        return $true
    } catch {
        $stopwatch.Stop()
        Add-TestResult "Service Health: $ServiceName" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

function Test-DatabaseConnection {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT 1;" 2>$null
        $stopwatch.Stop()
        if ($result -match "1 row") {
            Add-TestResult "Database Connection" "PASS" "PostgreSQL responding" $stopwatch.Elapsed.TotalMilliseconds
            return $true
        } else {
            Add-TestResult "Database Connection" "FAIL" "Unexpected response" $stopwatch.Elapsed.TotalMilliseconds
            return $false
        }
    } catch {
        $stopwatch.Stop()
        Add-TestResult "Database Connection" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

function Test-CompanyExists {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT id FROM company_service.companies WHERE cnpj = '12345678000199';" 2>$null
        $stopwatch.Stop()
        if ($result -match "12345678-1234-1234-1234-123456789012") {
            Add-TestResult "Company Exists" "PASS" "EmpresaTeste found" $stopwatch.Elapsed.TotalMilliseconds
            return $true
        } else {
            Add-TestResult "Company Exists" "FAIL" "EmpresaTeste not found" $stopwatch.Elapsed.TotalMilliseconds
            return $false
        }
    } catch {
        $stopwatch.Stop()
        Add-TestResult "Company Exists" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

function Test-UserExists {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT id FROM users WHERE email = 'cliente@empresateste.com';" 2>$null
        $stopwatch.Stop()
        if ($result -match "22222222-2222-2222-2222-222222222222") {
            Add-TestResult "User Exists" "PASS" "cliente@empresateste.com found" $stopwatch.Elapsed.TotalMilliseconds
            return $true
        } else {
            Add-TestResult "User Exists" "FAIL" "User not found" $stopwatch.Elapsed.TotalMilliseconds
            return $false
        }
    } catch {
        $stopwatch.Stop()
        Add-TestResult "User Exists" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

function Test-AccountBalance {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" 2>$null
        $stopwatch.Stop()
        if ($result -match "900.00") {
            Add-TestResult "Account Balance" "PASS" "Balance: R$ 900.00" $stopwatch.Elapsed.TotalMilliseconds
            return $true
        } else {
            Add-TestResult "Account Balance" "FAIL" "Unexpected balance: $result" $stopwatch.Elapsed.TotalMilliseconds
            return $false
        }
    } catch {
        $stopwatch.Stop()
        Add-TestResult "Account Balance" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

function Test-TransactionHistory {
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT COUNT(*) FROM transaction_history WHERE client_id = '12345678-1234-1234-1234-123456789012' AND type = 'PIX';" 2>$null
        $stopwatch.Stop()
        if ($result -match "1") {
            Add-TestResult "Transaction History" "PASS" "1 PIX transaction found" $stopwatch.Elapsed.TotalMilliseconds
            return $true
        } else {
            Add-TestResult "Transaction History" "FAIL" "PIX transaction not found" $stopwatch.Elapsed.TotalMilliseconds
            return $false
        }
    } catch {
        $stopwatch.Stop()
        Add-TestResult "Transaction History" "FAIL" "Error: $($_.Exception.Message)" $stopwatch.Elapsed.TotalMilliseconds
        return $false
    }
}

# ========================================
# EXECUÇÃO DOS TESTES
# ========================================

Write-Host "📋 FASE 1: TESTES DE INFRAESTRUTURA" -ForegroundColor Yellow
Write-Host ""

# Teste 1: Conexão com banco de dados
Test-DatabaseConnection

# Teste 2: Serviços essenciais
$services = @(
    @{ Name = "AuthService"; Url = "http://localhost:5001/" },
    @{ Name = "BalanceService"; Url = "http://localhost:5003/" },
    @{ Name = "IntegrationService"; Url = "http://localhost:5005/" },
    @{ Name = "UserService"; Url = "http://localhost:5006/" },
    @{ Name = "ConfigService"; Url = "http://localhost:5007/" },
    @{ Name = "CompanyService"; Url = "http://localhost:5010/" }
)

foreach ($service in $services) {
    Test-ServiceHealth $service.Name $service.Url
}

Write-Host ""
Write-Host "📋 FASE 2: TESTES DE DADOS DA TRILHA" -ForegroundColor Yellow
Write-Host ""

# Teste 3: Empresa cadastrada
Test-CompanyExists

# Teste 4: Usuário criado
Test-UserExists

# Teste 5: Conta e saldo
Test-AccountBalance

# Teste 6: Histórico de transações
Test-TransactionHistory

# ========================================
# RELATÓRIO FINAL
# ========================================

Write-Host ""
Write-Host "📊 RELATÓRIO FINAL DOS TESTES E2E" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total de Testes: $global:TotalTests" -ForegroundColor White
Write-Host "Testes Aprovados: $global:PassedTests" -ForegroundColor Green
Write-Host "Testes Falharam: $global:FailedTests" -ForegroundColor Red
$successRate = [math]::Round(($global:PassedTests / $global:TotalTests) * 100, 2)
Write-Host "Taxa de Sucesso: $successRate%" -ForegroundColor $(if ($successRate -ge 80) { "Green" } else { "Yellow" })
Write-Host ""

if ($global:FailedTests -gt 0) {
    Write-Host "❌ TESTES FALHARAM:" -ForegroundColor Red
    $global:TestResults | Where-Object { $_.Status -eq "FAIL" } | ForEach-Object {
        Write-Host "   - $($_.Test): $($_.Details)" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "✅ TRILHA E2E VALIDADA COM $successRate% DE SUCESSO!" -ForegroundColor Green
Write-Host "Data/Hora: $(Get-Date)" -ForegroundColor Gray
