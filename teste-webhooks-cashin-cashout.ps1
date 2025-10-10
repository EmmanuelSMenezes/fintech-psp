#!/usr/bin/env pwsh

Write-Host "TESTE WEBHOOKS E CASH-IN/CASH-OUT" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
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

# TESTE 1: WEBHOOK ENDPOINTS DISPONIVEIS
Write-Host "FASE 1: ENDPOINTS DE WEBHOOK" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

try {
    $webhookHealth = Invoke-RestMethod -Uri "http://localhost:5005/webhooks/health" -TimeoutSec 5
    Test-Component "Webhook Health Check" ($webhookHealth.status -eq "healthy") "Endpoints webhook disponiveis: $($webhookHealth.endpoints.Count)"
} catch {
    Test-Component "Webhook Health Check" $false "Erro: $($_.Exception.Message)"
}

# TESTE 2: CASH-IN (PIX CONFIRMADO)
Write-Host "FASE 2: CASH-IN (PIX CONFIRMADO)" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

# Verificar saldo atual
try {
    $saldoAntes = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT available_balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" -t 2>$null
    $saldoAntes = [decimal]($saldoAntes.Trim())
    Write-Host "Saldo antes do teste: R$ $saldoAntes" -ForegroundColor White
} catch {
    $saldoAntes = 0
    Write-Host "Erro ao obter saldo inicial" -ForegroundColor Red
}

# Simular webhook PIX confirmado
try {
    $pixWebhook = @{
        TxId = "PIX_TEST_$(Get-Date -Format 'yyyyMMddHHmmss')"
        Amount = 50.00
        PayerName = "Cliente Teste PIX"
        PayerDocument = "12345678901"
        ConfirmedAt = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        PayerInfo = "PIX via Sicoob - Teste Cash-In"
    }
    
    $headers = @{ 'Content-Type' = 'application/json' }
    $body = $pixWebhook | ConvertTo-Json
    
    Write-Host "Enviando webhook PIX confirmado..." -ForegroundColor Yellow
    Write-Host "TxId: $($pixWebhook.TxId)" -ForegroundColor Gray
    Write-Host "Valor: R$ $($pixWebhook.Amount)" -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri "http://localhost:5003/saldo/pix-confirmado" -Method POST -Headers $headers -Body $body -TimeoutSec 10
    
    if ($response.success) {
        Test-Component "Cash-In PIX Webhook" $true "PIX R$ $($pixWebhook.Amount) processado com sucesso"
        
        # Verificar se o saldo foi atualizado
        Start-Sleep 2
        $saldoDepois = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT available_balance FROM accounts WHERE client_id = '12345678-1234-1234-1234-123456789012';" -t 2>$null
        $saldoDepois = [decimal]($saldoDepois.Trim())
        
        $diferenca = $saldoDepois - $saldoAntes
        if ($diferenca -eq $pixWebhook.Amount) {
            Test-Component "Atualizacao Saldo Cash-In" $true "Saldo atualizado corretamente: R$ $saldoAntes -> R$ $saldoDepois (+R$ $diferenca)"
        } else {
            Test-Component "Atualizacao Saldo Cash-In" $false "Saldo nao atualizado corretamente. Esperado: +R$ $($pixWebhook.Amount), Atual: +R$ $diferenca"
        }
    } else {
        Test-Component "Cash-In PIX Webhook" $false "Webhook nao processado corretamente"
    }
} catch {
    Test-Component "Cash-In PIX Webhook" $false "Erro: $($_.Exception.Message)"
}

# TESTE 3: VERIFICAR HISTORICO DE TRANSACOES
Write-Host "FASE 3: HISTORICO DE TRANSACOES" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
Write-Host ""

try {
    $transacoes = docker exec fintech-postgres psql -U fintech_user -d fintech_psp -c "SELECT COUNT(*) FROM transaction_history WHERE client_id = '12345678-1234-1234-1234-123456789012' AND operation = 'CREDIT';" -t 2>$null
    $transacoesCount = [int]($transacoes.Trim())
    Test-Component "Transacoes Credit (Cash-In)" ($transacoesCount -gt 0) "Encontradas $transacoesCount transacoes de credito"
} catch {
    Test-Component "Transacoes Credit (Cash-In)" $false "Erro ao consultar historico"
}

# TESTE 4: WEBHOOK SICOOB PIX (SIMULADO)
Write-Host "FASE 4: WEBHOOK SICOOB PIX" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

try {
    $sicoobPixWebhook = @{
        txId = "PIX_SICOOB_$(Get-Date -Format 'yyyyMMddHHmmss')"
        endToEndId = "E75691234$(Get-Date -Format 'yyyyMMddHHmmss')2024"
        status = "CONFIRMED"
        valor = 25.50
        pagador = @{
            nome = "Pagador Teste Sicoob"
            documento = "98765432100"
            tipoPessoa = "F"
        }
        timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        descricao = "Teste webhook Sicoob PIX"
    }
    
    $headers = @{ 'Content-Type' = 'application/json' }
    $body = $sicoobPixWebhook | ConvertTo-Json -Depth 3
    
    Write-Host "Enviando webhook Sicoob PIX..." -ForegroundColor Yellow
    Write-Host "TxId: $($sicoobPixWebhook.txId)" -ForegroundColor Gray
    Write-Host "Status: $($sicoobPixWebhook.status)" -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri "http://localhost:5005/webhooks/sicoob/pix" -Method POST -Headers $headers -Body $body -TimeoutSec 10
    
    Test-Component "Webhook Sicoob PIX" ($response.message -eq "Webhook processado com sucesso") "Webhook Sicoob processado: $($response.message)"
} catch {
    Test-Component "Webhook Sicoob PIX" $false "Erro: $($_.Exception.Message)"
}

# TESTE 5: WEBHOOK SICOOB BOLETO (SIMULADO)
Write-Host "FASE 5: WEBHOOK SICOOB BOLETO" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

try {
    $sicoobBoletoWebhook = @{
        nossoNumero = "BOL$(Get-Date -Format 'yyyyMMddHHmmss')"
        status = "PAID"
        valor = 100.00
        valorPago = 100.00
        pagador = @{
            nome = "Pagador Boleto Teste"
            documento = "11122233344"
            tipoPessoa = "F"
        }
        dataPagamento = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        timestamp = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
        descricao = "Teste webhook Sicoob Boleto"
    }
    
    $headers = @{ 'Content-Type' = 'application/json' }
    $body = $sicoobBoletoWebhook | ConvertTo-Json -Depth 3
    
    Write-Host "Enviando webhook Sicoob Boleto..." -ForegroundColor Yellow
    Write-Host "NossoNumero: $($sicoobBoletoWebhook.nossoNumero)" -ForegroundColor Gray
    Write-Host "Status: $($sicoobBoletoWebhook.status)" -ForegroundColor Gray
    
    $response = Invoke-RestMethod -Uri "http://localhost:5005/webhooks/sicoob/boleto" -Method POST -Headers $headers -Body $body -TimeoutSec 10
    
    Test-Component "Webhook Sicoob Boleto" ($response.message -eq "Webhook processado com sucesso") "Webhook Boleto processado: $($response.message)"
} catch {
    Test-Component "Webhook Sicoob Boleto" $false "Erro: $($_.Exception.Message)"
}

# TESTE 6: VERIFICAR WEBHOOKSERVICE
Write-Host "FASE 6: WEBHOOKSERVICE STATUS" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

try {
    $webhookServiceHealth = Invoke-RestMethod -Uri "http://localhost:5008/webhooks/health" -TimeoutSec 5
    Test-Component "WebhookService" ($webhookServiceHealth.status -eq "healthy") "WebhookService online e funcional"
} catch {
    Test-Component "WebhookService" $false "WebhookService offline - Erro: $($_.Exception.Message)"
}

# TESTE 7: CASH-OUT (SIMULACAO)
Write-Host "FASE 7: CASH-OUT (SIMULACAO)" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host ""

# Verificar se existe implementacao de cash-out
try {
    # Tentar endpoint de saque/debito
    $cashOutTest = @{
        amount = 10.00
        description = "Teste Cash-Out"
        clientId = "12345678-1234-1234-1234-123456789012"
    }
    
    $headers = @{ 'Content-Type' = 'application/json' }
    $body = $cashOutTest | ConvertTo-Json
    
    # Tentar varios endpoints possiveis para cash-out
    $cashOutEndpoints = @(
        "http://localhost:5003/saldo/debito",
        "http://localhost:5003/saldo/saque",
        "http://localhost:5003/saldo/cash-out"
    )
    
    $cashOutImplemented = $false
    foreach ($endpoint in $cashOutEndpoints) {
        try {
            $response = Invoke-RestMethod -Uri $endpoint -Method POST -Headers $headers -Body $body -TimeoutSec 5
            $cashOutImplemented = $true
            Test-Component "Cash-Out Endpoint" $true "Endpoint $endpoint funcional"
            break
        } catch {
            # Continuar tentando outros endpoints
        }
    }
    
    if (-not $cashOutImplemented) {
        Test-Component "Cash-Out Implementation" $false "Nenhum endpoint de cash-out encontrado"
    }
} catch {
    Test-Component "Cash-Out Implementation" $false "Erro ao testar cash-out: $($_.Exception.Message)"
}

# RELATORIO FINAL
Write-Host ""
Write-Host "RELATORIO FINAL - WEBHOOKS E CASH-IN/CASH-OUT" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
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
    Write-Host "WEBHOOKS E CASH-IN/CASH-OUT: FUNCIONAIS!" -ForegroundColor Green
    Write-Host "  Cash-In (PIX): Implementado e funcional" -ForegroundColor Green
    Write-Host "  Webhooks Sicoob: Implementados e funcionais" -ForegroundColor Green
    Write-Host "  Confirmacao de transacoes: OK" -ForegroundColor Green
} elseif ($successRate -ge 60) {
    Write-Host "WEBHOOKS E CASH-IN/CASH-OUT: PARCIALMENTE FUNCIONAIS" -ForegroundColor Yellow
    Write-Host "  Algumas funcionalidades implementadas" -ForegroundColor Yellow
    Write-Host "  Requer ajustes em componentes especificos" -ForegroundColor Yellow
} else {
    Write-Host "WEBHOOKS E CASH-IN/CASH-OUT: REQUER ATENCAO" -ForegroundColor Red
    Write-Host "  Varias funcionalidades nao implementadas" -ForegroundColor Red
    Write-Host "  Implementacao incompleta" -ForegroundColor Red
}

Write-Host ""
Write-Host "RECOMENDACOES:" -ForegroundColor Cyan
Write-Host "  1. Implementar WebhookService se nao estiver rodando" -ForegroundColor White
Write-Host "  2. Implementar endpoints de Cash-Out se necessario" -ForegroundColor White
Write-Host "  3. Testar webhooks reais com Sicoob sandbox" -ForegroundColor White
Write-Host "  4. Implementar retry logic para webhooks" -ForegroundColor White
Write-Host ""
Write-Host "Teste concluido em: $(Get-Date -Format 'dd/MM/yyyy HH:mm:ss')" -ForegroundColor Gray
