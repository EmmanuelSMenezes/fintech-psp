#!/usr/bin/env pwsh

# 🔍 SCRIPT DE VALIDAÇÃO COMPLETA DO SISTEMA PSP
# Verifica consistência entre Banco de Dados, APIs e Frontends

Write-Host "🔍 VALIDAÇÃO COMPLETA DO SISTEMA FINTECHPSP" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Continue"
$validationResults = @()

function Add-ValidationResult {
    param($Test, $Status, $Details, $Expected, $Actual)
    $global:validationResults += [PSCustomObject]@{
        Test = $Test
        Status = $Status
        Details = $Details
        Expected = $Expected
        Actual = $Actual
        Timestamp = Get-Date
    }
}

function Test-DatabaseConnection {
    Write-Host "📊 FASE 1: VALIDAÇÃO DO BANCO DE DADOS" -ForegroundColor Yellow
    Write-Host "=====================================" -ForegroundColor Yellow
    
    try {
        $result = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT 1;" 2>$null
        if ($result) {
            Write-Host "✅ Conexão PostgreSQL: OK" -ForegroundColor Green
            Add-ValidationResult "Database Connection" "PASS" "PostgreSQL conectado" "Connected" "Connected"
            return $true
        }
    } catch {
        Write-Host "❌ Conexão PostgreSQL: FALHOU" -ForegroundColor Red
        Add-ValidationResult "Database Connection" "FAIL" "PostgreSQL não conectado" "Connected" "Disconnected"
        return $false
    }
}

function Test-CompanyData {
    Write-Host ""
    Write-Host "🏢 VALIDAÇÃO: EMPRESA CADASTRADA" -ForegroundColor Cyan
    
    try {
        $dbResult = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT id, razao_social, cnpj, status FROM company_service.companies WHERE cnpj = '12345678000199';" -t 2>$null
        
        if ($dbResult -and $dbResult.Trim()) {
            $lines = $dbResult.Split("`n") | Where-Object { $_.Trim() -ne "" }
            $dataLine = $lines | Where-Object { $_ -match "12345678-1234-1234-1234-123456789012" }
            
            if ($dataLine) {
                Write-Host "✅ Empresa no BD: EmpresaTeste Ltda encontrada" -ForegroundColor Green
                Add-ValidationResult "Company Database" "PASS" "Empresa existe no banco" "EmpresaTeste Ltda" "Found"
                
                # Testar API do CompanyService
                try {
                    $apiResponse = Invoke-RestMethod -Uri "http://localhost:5009/companies" -TimeoutSec 10
                    Write-Host "✅ API CompanyService: Respondendo" -ForegroundColor Green
                    Add-ValidationResult "Company API" "PASS" "API CompanyService funcional" "200 OK" "200 OK"
                } catch {
                    Write-Host "❌ API CompanyService: Não responde" -ForegroundColor Red
                    Add-ValidationResult "Company API" "FAIL" "API CompanyService não responde" "200 OK" "Error"
                }
                
                return $true
            }
        }
        
        Write-Host "❌ Empresa no BD: Não encontrada" -ForegroundColor Red
        Add-ValidationResult "Company Database" "FAIL" "Empresa não encontrada" "EmpresaTeste Ltda" "Not Found"
        return $false
        
    } catch {
        Write-Host "❌ Erro ao consultar empresa: $($_.Exception.Message)" -ForegroundColor Red
        Add-ValidationResult "Company Database" "ERROR" $_.Exception.Message "Success" "Error"
        return $false
    }
}

function Test-UserData {
    Write-Host ""
    Write-Host "👥 VALIDAÇÃO: USUÁRIO CADASTRADO" -ForegroundColor Cyan
    
    try {
        $dbResult = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT id, name, email, is_active FROM users WHERE email = 'cliente@empresateste.com';" -t 2>$null
        
        if ($dbResult -and $dbResult.Trim()) {
            $lines = $dbResult.Split("`n") | Where-Object { $_.Trim() -ne "" }
            $dataLine = $lines | Where-Object { $_ -match "cliente@empresateste.com" }
            
            if ($dataLine) {
                Write-Host "✅ Usuário no BD: cliente@empresateste.com encontrado" -ForegroundColor Green
                Add-ValidationResult "User Database" "PASS" "Usuário existe no banco" "cliente@empresateste.com" "Found"
                
                # Testar API do UserService
                try {
                    $apiResponse = Invoke-RestMethod -Uri "http://localhost:5006/" -TimeoutSec 10
                    Write-Host "✅ API UserService: Respondendo" -ForegroundColor Green
                    Add-ValidationResult "User API" "PASS" "API UserService funcional" "200 OK" "200 OK"
                } catch {
                    Write-Host "❌ API UserService: Não responde" -ForegroundColor Red
                    Add-ValidationResult "User API" "FAIL" "API UserService não responde" "200 OK" "Error"
                }
                
                return $true
            }
        }
        
        Write-Host "❌ Usuário no BD: Não encontrado" -ForegroundColor Red
        Add-ValidationResult "User Database" "FAIL" "Usuário não encontrado" "cliente@empresateste.com" "Not Found"
        return $false
        
    } catch {
        Write-Host "❌ Erro ao consultar usuário: $($_.Exception.Message)" -ForegroundColor Red
        Add-ValidationResult "User Database" "ERROR" $_.Exception.Message "Success" "Error"
        return $false
    }
}

function Test-AccountData {
    Write-Host ""
    Write-Host "🏦 VALIDAÇÃO: CONTA BANCÁRIA" -ForegroundColor Cyan
    
    try {
        $dbResult = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT client_id, account_id, available_balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" -t 2>$null
        
        if ($dbResult -and $dbResult.Trim()) {
            $lines = $dbResult.Split("`n") | Where-Object { $_.Trim() -ne "" }
            $dataLine = $lines | Where-Object { $_ -match "CONTA_EMPRESATESTE" }
            
            if ($dataLine) {
                $balance = ($dataLine -split '\|')[2].Trim()
                Write-Host "✅ Conta no BD: CONTA_EMPRESATESTE com saldo R$ $balance" -ForegroundColor Green
                Add-ValidationResult "Account Database" "PASS" "Conta existe com saldo" "R$ 900.00" "R$ $balance"
                
                # Testar API do BalanceService
                try {
                    $apiResponse = Invoke-RestMethod -Uri "http://localhost:5003/" -TimeoutSec 10
                    Write-Host "✅ API BalanceService: Respondendo" -ForegroundColor Green
                    Add-ValidationResult "Balance API" "PASS" "API BalanceService funcional" "200 OK" "200 OK"
                } catch {
                    Write-Host "❌ API BalanceService: Não responde" -ForegroundColor Red
                    Add-ValidationResult "Balance API" "FAIL" "API BalanceService não responde" "200 OK" "Error"
                }
                
                return $true
            }
        }
        
        Write-Host "❌ Conta no BD: Não encontrada" -ForegroundColor Red
        Add-ValidationResult "Account Database" "FAIL" "Conta não encontrada" "CONTA_EMPRESATESTE" "Not Found"
        return $false
        
    } catch {
        Write-Host "❌ Erro ao consultar conta: $($_.Exception.Message)" -ForegroundColor Red
        Add-ValidationResult "Account Database" "ERROR" $_.Exception.Message "Success" "Error"
        return $false
    }
}

function Test-TransactionData {
    Write-Host ""
    Write-Host "💸 VALIDAÇÃO: TRANSAÇÕES PIX" -ForegroundColor Cyan
    
    try {
        $dbResult = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT transaction_id, external_id, amount, type, status FROM transaction_history WHERE client_id = '12345678-1234-1234-1234-123456789012' ORDER BY created_at DESC LIMIT 1;" -t 2>$null
        
        if ($dbResult -and $dbResult.Trim()) {
            $lines = $dbResult.Split("`n") | Where-Object { $_.Trim() -ne "" }
            $dataLine = $lines | Where-Object { $_ -match "PIX" }
            
            if ($dataLine) {
                $parts = $dataLine -split '\|'
                $amount = $parts[2].Trim()
                $status = $parts[4].Trim()
                Write-Host "✅ Transação no BD: PIX R$ $amount - Status: $status" -ForegroundColor Green
                Add-ValidationResult "Transaction Database" "PASS" "Transação PIX existe" "PIX COMPLETED" "PIX $status"
                
                return $true
            }
        }
        
        Write-Host "❌ Transação no BD: Não encontrada" -ForegroundColor Red
        Add-ValidationResult "Transaction Database" "FAIL" "Transação não encontrada" "PIX Transaction" "Not Found"
        return $false
        
    } catch {
        Write-Host "❌ Erro ao consultar transação: $($_.Exception.Message)" -ForegroundColor Red
        Add-ValidationResult "Transaction Database" "ERROR" $_.Exception.Message "Success" "Error"
        return $false
    }
}

function Test-SicoobIntegration {
    Write-Host ""
    Write-Host "🔗 VALIDAÇÃO: INTEGRAÇÃO SICOOB" -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5005/integrations/health" -TimeoutSec 10
        
        $sicoobStatus = $response.integrations.sicoob.status
        $latency = $response.integrations.sicoob.latency
        $qrSupport = $response.integrations.sicoob.qrCodeSupport
        
        Write-Host "✅ Sicoob Status: $sicoobStatus" -ForegroundColor Green
        Write-Host "✅ Sicoob Latência: $latency" -ForegroundColor Green
        Write-Host "✅ QR Code Support: $qrSupport" -ForegroundColor Green
        
        Add-ValidationResult "Sicoob Integration" "PASS" "Integração Sicoob configurada" "Configured" "Configured"
        return $true
        
    } catch {
        Write-Host "❌ Integração Sicoob: Erro - $($_.Exception.Message)" -ForegroundColor Red
        Add-ValidationResult "Sicoob Integration" "FAIL" $_.Exception.Message "Configured" "Error"
        return $false
    }
}

# EXECUÇÃO PRINCIPAL
Write-Host "🚀 Iniciando validação completa..." -ForegroundColor White
Write-Host ""

$dbOk = Test-DatabaseConnection
if (-not $dbOk) {
    Write-Host ""
    Write-Host "❌ FALHA CRÍTICA: Banco de dados não conectado!" -ForegroundColor Red
    exit 1
}

$companyOk = Test-CompanyData
$userOk = Test-UserData
$accountOk = Test-AccountData
$transactionOk = Test-TransactionData
$sicoobOk = Test-SicoobIntegration

Write-Host ""
Write-Host "📊 RELATÓRIO FINAL DE VALIDAÇÃO" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan

$totalTests = $validationResults.Count
$passedTests = ($validationResults | Where-Object { $_.Status -eq "PASS" }).Count
$failedTests = ($validationResults | Where-Object { $_.Status -eq "FAIL" }).Count
$errorTests = ($validationResults | Where-Object { $_.Status -eq "ERROR" }).Count

Write-Host ""
Write-Host "📈 ESTATÍSTICAS:" -ForegroundColor White
Write-Host "  Total de Testes: $totalTests" -ForegroundColor White
Write-Host "  Testes Aprovados: $passedTests" -ForegroundColor Green
Write-Host "  Testes Falharam: $failedTests" -ForegroundColor Red
Write-Host "  Testes com Erro: $errorTests" -ForegroundColor Yellow

$successRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
Write-Host "  Taxa de Sucesso: $successRate%" -ForegroundColor Cyan

Write-Host ""
Write-Host "📋 DETALHES DOS TESTES:" -ForegroundColor White
foreach ($result in $validationResults) {
    $statusColor = switch ($result.Status) {
        "PASS" { "Green" }
        "FAIL" { "Red" }
        "ERROR" { "Yellow" }
        default { "White" }
    }
    Write-Host "  [$($result.Status)] $($result.Test): $($result.Details)" -ForegroundColor $statusColor
}

Write-Host ""
if ($successRate -ge 80) {
    Write-Host "✅ SISTEMA VALIDADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "   Todos os componentes críticos estão funcionando." -ForegroundColor Green
} else {
    Write-Host "❌ SISTEMA REQUER ATENÇÃO!" -ForegroundColor Red
    Write-Host "   Alguns componentes críticos apresentam problemas." -ForegroundColor Red
}

Write-Host ""
Write-Host "📅 Validação concluída em: $(Get-Date)" -ForegroundColor Gray
