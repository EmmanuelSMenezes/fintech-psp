# ========================================
# Validador de Correções da Trilha Integrada
# ========================================
# Script para validar as correções identificadas na trilha PSP-Sicoob

param(
    [string]$IntegrationServiceUrl = "http://localhost:5005",
    [string]$TransactionServiceUrl = "http://localhost:5002",
    [string]$CompanyServiceUrl = "http://localhost:5004"
)

$ErrorActionPreference = "Continue"

Write-Host "🔍 VALIDANDO CORREÇÕES DA TRILHA INTEGRADA" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Green
Write-Host ""

# Headers padrão
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

# Função para testar endpoint
function Test-Endpoint {
    param(
        [string]$Url,
        [string]$Description,
        [string]$Method = "GET",
        [string]$Body = $null
    )
    
    Write-Host "🔍 Testando: $Description" -ForegroundColor Cyan
    Write-Host "   URL: $Method $Url" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $headers
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "   ✅ IMPLEMENTADO" -ForegroundColor Green
        return $true
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode -eq 404) {
            Write-Host "   ❌ NÃO IMPLEMENTADO (404)" -ForegroundColor Red
        } elseif ($statusCode -eq 500) {
            Write-Host "   ⚠️  IMPLEMENTADO MAS COM ERRO (500)" -ForegroundColor Yellow
        } else {
            Write-Host "   ⚠️  STATUS: $statusCode" -ForegroundColor Yellow
        }
        return $false
    }
}

# Função para verificar configuração
function Test-Configuration {
    param(
        [string]$ConfigName,
        [scriptblock]$TestScript
    )
    
    Write-Host "⚙️ Verificando: $ConfigName" -ForegroundColor Cyan
    
    try {
        $result = & $TestScript
        if ($result) {
            Write-Host "   ✅ CONFIGURADO" -ForegroundColor Green
        } else {
            Write-Host "   ❌ NÃO CONFIGURADO" -ForegroundColor Red
        }
        return $result
    }
    catch {
        Write-Host "   ❌ ERRO: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

Write-Host "📋 VALIDANDO CORREÇÕES IDENTIFICADAS" -ForegroundColor Magenta
Write-Host "====================================" -ForegroundColor Magenta
Write-Host ""

# ========================================
# 1. VALIDAÇÃO CNPJ VIA SICOOB
# ========================================
Write-Host "1️⃣ VALIDAÇÃO CNPJ" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow

$cnpjEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/validacao/cnpj" -Description "Endpoint validação CNPJ Sicoob" -Method "POST" -Body '{"cnpj":"12345678000199"}'

if (-not $cnpjEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar validação via Receita Federal" -ForegroundColor Cyan
    Write-Host "   📝 Endpoint sugerido: POST /integrations/receita-federal/cnpj" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 2. REGISTRO DE CONTA VIRTUAL SICOOB
# ========================================
Write-Host "2️⃣ REGISTRO DE CONTA VIRTUAL" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow

$contaVirtualEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/conta/virtual" -Description "Endpoint conta virtual Sicoob" -Method "POST" -Body '{"accountType":"VIRTUAL","clientId":"test"}'

if (-not $contaVirtualEndpoint) {
    Write-Host "   💡 SUGESTÃO: Usar conta principal com sub-identificadores" -ForegroundColor Cyan
    Write-Host "   📝 Implementar mapeamento interno de contas virtuais" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 3. WEBHOOKS SICOOB
# ========================================
Write-Host "3️⃣ WEBHOOKS SICOOB" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow

$webhookPixEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/webhooks/sicoob/pix" -Description "Webhook PIX Sicoob" -Method "POST" -Body '{"txId":"test","status":"CONFIRMED"}'

$webhookBoletoEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/webhooks/sicoob/boleto" -Description "Webhook Boleto Sicoob" -Method "POST" -Body '{"nossoNumero":"test","status":"PAID"}'

if (-not $webhookPixEndpoint -or -not $webhookBoletoEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar endpoints de webhook" -ForegroundColor Cyan
    Write-Host "   📝 Configurar URLs no painel Sicoob" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 4. CONCILIAÇÃO AUTOMÁTICA
# ========================================
Write-Host "4️⃣ CONCILIAÇÃO AUTOMÁTICA" -ForegroundColor Yellow
Write-Host "=========================" -ForegroundColor Yellow

$conciliacaoEndpoint = Test-Endpoint -Url "$TransactionServiceUrl/reconciliation/sicoob" -Description "Endpoint conciliação Sicoob" -Method "POST" -Body '{"startDate":"2024-10-01","endDate":"2024-10-31"}'

$conciliacaoJob = Test-Configuration -ConfigName "Job de Conciliação Diária" -TestScript {
    # Verificar se existe configuração de job
    $jobConfig = Test-Endpoint -Url "$TransactionServiceUrl/jobs/reconciliation/config" -Description "Config job conciliação"
    return $jobConfig
}

if (-not $conciliacaoEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar processo de conciliação" -ForegroundColor Cyan
    Write-Host "   📝 Job diário para comparar transações" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 5. CACHE DE TOKENS OAUTH
# ========================================
Write-Host "5️⃣ CACHE DE TOKENS OAUTH" -ForegroundColor Yellow
Write-Host "========================" -ForegroundColor Yellow

$tokenCacheEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/token/cache/status" -Description "Status cache token OAuth"

if (-not $tokenCacheEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar cache de tokens" -ForegroundColor Cyan
    Write-Host "   📝 Usar IMemoryCache ou Redis" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 6. RETRY POLICY
# ========================================
Write-Host "6️⃣ RETRY POLICY" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor Yellow

$retryConfigEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/retry/config" -Description "Configuração retry policy"

if (-not $retryConfigEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar Polly retry policies" -ForegroundColor Cyan
    Write-Host "   📝 Configurar backoff exponencial" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 7. MONITORAMENTO DE CERTIFICADOS
# ========================================
Write-Host "7️⃣ MONITORAMENTO DE CERTIFICADOS" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow

$certMonitorEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/certificates/status" -Description "Status certificados mTLS"

if (-not $certMonitorEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar monitoramento de certificados" -ForegroundColor Cyan
    Write-Host "   📝 Alertas para expiração próxima" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 8. RATE LIMITING
# ========================================
Write-Host "8️⃣ RATE LIMITING" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor Yellow

$rateLimitEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/integrations/sicoob/rate-limit/status" -Description "Status rate limiting"

if (-not $rateLimitEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar throttling" -ForegroundColor Cyan
    Write-Host "   📝 Queue para requisições Sicoob" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# 9. LOGS DE AUDITORIA
# ========================================
Write-Host "9️⃣ LOGS DE AUDITORIA" -ForegroundColor Yellow
Write-Host "====================" -ForegroundColor Yellow

$auditLogsEndpoint = Test-Endpoint -Url "$IntegrationServiceUrl/audit/sicoob/logs" -Description "Logs de auditoria Sicoob"

if (-not $auditLogsEndpoint) {
    Write-Host "   💡 SUGESTÃO: Implementar logs estruturados" -ForegroundColor Cyan
    Write-Host "   📝 Mascarar dados sensíveis" -ForegroundColor Gray
}

Write-Host ""

# ========================================
# RESUMO FINAL
# ========================================
Write-Host "📊 RESUMO DAS VALIDAÇÕES" -ForegroundColor Green
Write-Host "========================" -ForegroundColor Green
Write-Host ""

$implementados = 0
$total = 9

# Contar implementações (simulado para exemplo)
$checks = @(
    $cnpjEndpoint,
    $contaVirtualEndpoint,
    ($webhookPixEndpoint -and $webhookBoletoEndpoint),
    $conciliacaoEndpoint,
    $tokenCacheEndpoint,
    $retryConfigEndpoint,
    $certMonitorEndpoint,
    $rateLimitEndpoint,
    $auditLogsEndpoint
)

$implementados = ($checks | Where-Object { $_ }).Count

Write-Host "✅ Implementados: $implementados/$total" -ForegroundColor Green
Write-Host "❌ Pendentes: $($total - $implementados)/$total" -ForegroundColor Red
Write-Host ""

if ($implementados -eq $total) {
    Write-Host "🎉 TODAS AS CORREÇÕES IMPLEMENTADAS!" -ForegroundColor Green
} elseif ($implementados -ge ($total * 0.7)) {
    Write-Host "⚠️  MAIORIA DAS CORREÇÕES IMPLEMENTADAS" -ForegroundColor Yellow
    Write-Host "   Foque nos itens pendentes para completar a integração" -ForegroundColor Cyan
} else {
    Write-Host "❌ MUITAS CORREÇÕES PENDENTES" -ForegroundColor Red
    Write-Host "   Priorize a implementação das correções críticas" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "📋 PRÓXIMOS PASSOS:" -ForegroundColor Cyan
Write-Host "1. Implementar correções pendentes" -ForegroundColor Gray
Write-Host "2. Executar trilha integrada completa" -ForegroundColor Gray
Write-Host "3. Validar em ambiente de produção" -ForegroundColor Gray
Write-Host "4. Configurar monitoramento contínuo" -ForegroundColor Gray
Write-Host ""
Write-Host "🔗 Para executar a trilha: .\executar-trilha-integrada.ps1" -ForegroundColor White
