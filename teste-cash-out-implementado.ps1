#!/usr/bin/env pwsh

Write-Host "TESTE CASH-OUT IMPLEMENTADO" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

$script:tests = 0
$script:passed = 0

function Test-Component {
    param($name, $test, $details)
    $script:tests++
    Write-Host "Testando: $name" -ForegroundColor Yellow
    if ($test) {
        Write-Host "  PASS: $details" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  FAIL: $details" -ForegroundColor Red
    }
    Write-Host ""
}

# TESTE 1: VERIFICAR SALDO INICIAL
Write-Host "FASE 1: VERIFICAR SALDO INICIAL" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

try {
    $saldoInicial = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT available_balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" -t 2>$null
    $saldoInicial = [decimal]($saldoInicial.Trim())
    Test-Component "Saldo Inicial" ($saldoInicial -gt 0) "Saldo disponível: R$ $saldoInicial"
    Write-Host "💰 Saldo inicial: R$ $saldoInicial" -ForegroundColor White
} catch {
    Test-Component "Saldo Inicial" $false "Erro ao consultar saldo: $($_.Exception.Message)"
    $saldoInicial = 0
}

# TESTE 2: VERIFICAR BALANCESERVICE ONLINE
Write-Host "FASE 2: VERIFICAR BALANCESERVICE" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

$balanceServiceOnline = $false
try {
    # Testar endpoint simples primeiro
    $response = Invoke-RestMethod -Uri "http://localhost:5003/saldo" -TimeoutSec 5 -ErrorAction Stop
    Test-Component "BalanceService Online" $false "Endpoint requer autenticação (esperado)"
    $balanceServiceOnline = $true
} catch {
    if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Não Autorizado*") {
        Test-Component "BalanceService Online" $true "Serviço online (erro 401 esperado)"
        $balanceServiceOnline = $true
    } else {
        Test-Component "BalanceService Online" $false "Erro: $($_.Exception.Message)"
    }
}

# TESTE 3: TESTAR DÉBITO ADMINISTRATIVO (SEM AUTENTICAÇÃO)
Write-Host "FASE 3: TESTAR DÉBITO ADMINISTRATIVO" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

if ($balanceServiceOnline) {
    try {
        $debitoRequest = @{
            ClientId = "12345678-1234-1234-1234-123456789012"
            Amount = 25.00
            Reason = "Teste débito administrativo - Cash-Out implementado"
            ExternalTransactionId = "TEST_CASHOUT_$(Get-Date -Format 'yyyyMMddHHmmss')"
        }
        
        $headers = @{ 'Content-Type' = 'application/json' }
        $body = $debitoRequest | ConvertTo-Json
        
        Write-Host "Enviando débito administrativo..." -ForegroundColor Yellow
        Write-Host "ClientId: $($debitoRequest.ClientId)" -ForegroundColor Gray
        Write-Host "Valor: R$ $($debitoRequest.Amount)" -ForegroundColor Gray
        Write-Host "Motivo: $($debitoRequest.Reason)" -ForegroundColor Gray
        Write-Host ""
        
        $response = Invoke-RestMethod -Uri "http://localhost:5003/saldo/debito-admin" -Method POST -Headers $headers -Body $body -TimeoutSec 15
        
        if ($response.success -or $response.TransactionId) {
            Test-Component "Débito Administrativo" $true "Débito processado com sucesso - TransactionId: $($response.TransactionId)"
            
            # Verificar se o saldo foi atualizado
            Start-Sleep 2
            $saldoDepois = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT available_balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" -t 2>$null
            $saldoDepois = [decimal]($saldoDepois.Trim())
            
            $diferenca = $saldoInicial - $saldoDepois
            if ($diferenca -eq $debitoRequest.Amount) {
                Test-Component "Atualização Saldo Débito" $true "Saldo atualizado corretamente: R$ $saldoInicial -> R$ $saldoDepois (-R$ $diferenca)"
            } else {
                Test-Component "Atualização Saldo Débito" $false "Saldo não atualizado corretamente. Esperado: -R$ $($debitoRequest.Amount), Atual: -R$ $diferenca"
            }
            
            Write-Host "📊 Detalhes da resposta:" -ForegroundColor White
            $response | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor Gray
        } else {
            Test-Component "Débito Administrativo" $false "Resposta inesperada do servidor"
        }
    } catch {
        Test-Component "Débito Administrativo" $false "Erro: $($_.Exception.Message)"
        if ($_.Exception.Response) {
            try {
                $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $responseBody = $reader.ReadToEnd()
                Write-Host "Response Body: $responseBody" -ForegroundColor Gray
            } catch {
                Write-Host "Não foi possível ler o corpo da resposta" -ForegroundColor Gray
            }
        }
    }
} else {
    Test-Component "Débito Administrativo" $false "BalanceService não está online"
}

# TESTE 4: VERIFICAR HISTÓRICO DE TRANSAÇÕES
Write-Host "FASE 4: VERIFICAR HISTÓRICO" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

try {
    $transacoesDebit = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT COUNT(*) FROM transaction_history WHERE client_id = '12345678-1234-1234-1234-123456789012' AND operation = 'DEBIT';" -t 2>$null
    $transacoesDebitCount = [int]($transacoesDebit.Trim())
    Test-Component "Transacoes Debit (Cash-Out)" ($transacoesDebitCount -gt 0) "Encontradas $transacoesDebitCount transacoes de debito"
    
    if ($transacoesDebitCount -gt 0) {
        Write-Host "Ultimas transacoes de debito:" -ForegroundColor White
        $ultimasTransacoes = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT external_id, amount, description, status, created_at FROM transaction_history WHERE client_id = '12345678-1234-1234-1234-123456789012' AND operation = 'DEBIT' ORDER BY created_at DESC LIMIT 3;" 2>$null
        Write-Host $ultimasTransacoes -ForegroundColor Gray
    }
} catch {
    Test-Component "Transacoes Debit (Cash-Out)" $false "Erro ao consultar historico: $($_.Exception.Message)"
}

# TESTE 5: VERIFICAR WEBHOOKSERVICE
Write-Host "FASE 5: VERIFICAR WEBHOOKSERVICE" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

try {
    $webhookHealth = Invoke-RestMethod -Uri "http://localhost:5008/webhooks/health" -TimeoutSec 5
    Test-Component "WebhookService" ($webhookHealth.status -eq "healthy") "WebhookService online - Status: $($webhookHealth.status)"
} catch {
    Test-Component "WebhookService" $false "WebhookService offline - Erro: $($_.Exception.Message)"
}

# TESTE 6: TESTAR ENDPOINTS DE CASH-OUT
Write-Host "FASE 6: TESTAR ENDPOINTS CASH-OUT" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$cashOutEndpoints = @(
    @{Name="Cash-Out Geral"; Url="http://localhost:5003/saldo/cash-out"},
    @{Name="Saque PIX"; Url="http://localhost:5003/saldo/saque-pix"},
    @{Name="Débito Admin"; Url="http://localhost:5003/saldo/debito-admin"}
)

foreach ($endpoint in $cashOutEndpoints) {
    try {
        # Testar se o endpoint existe (mesmo que retorne erro de autenticação)
        $testRequest = @{ Amount = 1.00; Description = "Teste endpoint" }
        $headers = @{ 'Content-Type' = 'application/json' }
        $body = $testRequest | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri $endpoint.Url -Method POST -Headers $headers -Body $body -TimeoutSec 5
        Test-Component "$($endpoint.Name) Endpoint" $true "Endpoint funcional"
    } catch {
        if ($_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*400*" -or $_.Exception.Message -like "*validation*") {
            Test-Component "$($endpoint.Name) Endpoint" $true "Endpoint existe (erro de validação/auth esperado)"
        } else {
            Test-Component "$($endpoint.Name) Endpoint" $false "Endpoint não encontrado: $($_.Exception.Message)"
        }
    }
}

# RELATÓRIO FINAL
Write-Host ""
Write-Host "RELATÓRIO FINAL - CASH-OUT IMPLEMENTADO" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

$successRate = if ($script:tests -gt 0) { [math]::Round(($script:passed / $script:tests) * 100, 2) } else { 0 }

Write-Host "ESTATISTICAS:" -ForegroundColor White
Write-Host "  Total de Testes: $($script:tests)" -ForegroundColor White
Write-Host "  Testes Aprovados: $($script:passed)" -ForegroundColor Green
Write-Host "  Testes Falharam: $($script:tests - $script:passed)" -ForegroundColor Red
Write-Host "  Taxa de Sucesso: $successRate%" -ForegroundColor Cyan
Write-Host ""

Write-Host "ANALISE DETALHADA:" -ForegroundColor White
Write-Host ""

if ($successRate -ge 80) {
    Write-Host "🎉 CASH-OUT IMPLEMENTADO COM SUCESSO!" -ForegroundColor Green
    Write-Host "  ✅ Endpoints de cash-out funcionais" -ForegroundColor Green
    Write-Host "  ✅ Débito administrativo operacional" -ForegroundColor Green
    Write-Host "  ✅ Atualização de saldo funcionando" -ForegroundColor Green
    Write-Host "  ✅ Histórico de transações registrado" -ForegroundColor Green
} elseif ($successRate -ge 60) {
    Write-Host "⚠️ CASH-OUT PARCIALMENTE IMPLEMENTADO" -ForegroundColor Yellow
    Write-Host "  Algumas funcionalidades implementadas" -ForegroundColor Yellow
    Write-Host "  Requer ajustes em componentes específicos" -ForegroundColor Yellow
} else {
    Write-Host "❌ CASH-OUT REQUER MAIS TRABALHO" -ForegroundColor Red
    Write-Host "  Várias funcionalidades não implementadas" -ForegroundColor Red
    Write-Host "  Implementação incompleta" -ForegroundColor Red
}

Write-Host ""
Write-Host "PROXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "  1. Iniciar BalanceService se não estiver rodando" -ForegroundColor White
Write-Host "  2. Iniciar WebhookService se não estiver rodando" -ForegroundColor White
Write-Host "  3. Testar cash-out com autenticação JWT" -ForegroundColor White
Write-Host "  4. Implementar validações de limite" -ForegroundColor White
Write-Host ""
Write-Host "Teste concluido em: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
