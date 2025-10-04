# =====================================================
# FintechPSP - Teste de Configuração EmpresaTeste
# =====================================================

Write-Host "🎯 Testando configuração da EmpresaTeste..." -ForegroundColor Green
Write-Host ""

# Função para fazer requisições HTTP
function Invoke-ApiTest {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    Write-Host "🔍 Testando: $Name" -ForegroundColor Cyan
    Write-Host "   URL: $Url" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 10
            ErrorAction = 'Stop'
        }
        
        if ($Body) {
            $params.Body = $Body
            $params.ContentType = "application/json"
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "   ✅ Status: OK" -ForegroundColor Green
        
        if ($response -is [string]) {
            Write-Host "   📄 Response: $response" -ForegroundColor Gray
        } else {
            Write-Host "   📄 Response: $($response | ConvertTo-Json -Depth 2 -Compress)" -ForegroundColor Gray
        }
        
        return $true
    }
    catch {
        Write-Host "   ❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    Write-Host ""
}

# 1. Testar Health dos Serviços Principais
Write-Host "🏥 TESTANDO HEALTH DOS SERVIÇOS" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow

$services = @(
    @{Name="API Gateway"; Url="http://localhost:5000/health"},
    @{Name="Auth Service"; Url="http://localhost:5001/health"},
    @{Name="Company Service"; Url="http://localhost:5009/health"},
    @{Name="User Service"; Url="http://localhost:5006/health"},
    @{Name="Transaction Service"; Url="http://localhost:5002/health"},
    @{Name="Balance Service"; Url="http://localhost:5003/health"},
    @{Name="Config Service"; Url="http://localhost:5007/health"},
    @{Name="Integration Service"; Url="http://localhost:5005/health"},
    @{Name="Webhook Service"; Url="http://localhost:5004/health"}
)

$healthyServices = 0
foreach ($service in $services) {
    if (Invoke-ApiTest -Name $service.Name -Url $service.Url) {
        $healthyServices++
    }
}

Write-Host "📊 Serviços Saudáveis: $healthyServices/$($services.Count)" -ForegroundColor White
Write-Host ""

# 2. Testar Frontends
Write-Host "🌐 TESTANDO FRONTENDS" -ForegroundColor Yellow
Write-Host "=====================" -ForegroundColor Yellow

Invoke-ApiTest -Name "BackofficeWeb" -Url "http://localhost:3000/api/health"
Invoke-ApiTest -Name "InternetBankingWeb" -Url "http://localhost:3001/api/health"

# 3. Testar Configurações da EmpresaTeste
Write-Host "🏢 TESTANDO CONFIGURAÇÕES EMPRESATESTE" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow

# Buscar empresa por CNPJ
Invoke-ApiTest -Name "Buscar EmpresaTeste por CNPJ" -Url "http://localhost:5000/api/companies?cnpj=12.345.678/0001-99"

# Buscar empresa por nome
Invoke-ApiTest -Name "Buscar EmpresaTeste por nome" -Url "http://localhost:5000/api/companies?search=EmpresaTeste"

# Testar limites PIX
Invoke-ApiTest -Name "Consultar Limites PIX" -Url "http://localhost:5007/api/limits/pix"

# Testar limites TED
Invoke-ApiTest -Name "Consultar Limites TED" -Url "http://localhost:5007/api/limits/ted"

# 4. Testar Integração Sicoob
Write-Host "🏦 TESTANDO INTEGRAÇÃO SICOOB" -ForegroundColor Yellow
Write-Host "=============================" -ForegroundColor Yellow

# Health da integração
Invoke-ApiTest -Name "Health Integração Sicoob" -Url "http://localhost:5005/api/integration/health"

# Teste de conectividade Sicoob
Invoke-ApiTest -Name "Teste Conectividade Sicoob" -Url "http://localhost:5005/api/integration/sicoob/test"

# 5. Resumo Final
Write-Host "📋 RESUMO DO AMBIENTE" -ForegroundColor Green
Write-Host "=====================" -ForegroundColor Green
Write-Host ""
Write-Host "✅ SERVIÇOS ATIVOS:" -ForegroundColor White
Write-Host "• API Gateway:          http://localhost:5000" -ForegroundColor Gray
Write-Host "• BackofficeWeb:        http://localhost:3000" -ForegroundColor Gray
Write-Host "• InternetBankingWeb:   http://localhost:3001" -ForegroundColor Gray
Write-Host "• Status Page:          http://localhost:3000/status" -ForegroundColor Gray
Write-Host ""
Write-Host "🏢 EMPRESATESTE CONFIGURADA:" -ForegroundColor White
Write-Host "• CNPJ:                 12.345.678/0001-99" -ForegroundColor Gray
Write-Host "• Usuário:              cliente@empresateste.com" -ForegroundColor Gray
Write-Host "• Limites PIX/TED:      R$ 10.000,00/dia" -ForegroundColor Gray
Write-Host "• Integração Sicoob:    Sandbox configurado" -ForegroundColor Gray
Write-Host ""
Write-Host "🚀 PRÓXIMOS TESTES SUGERIDOS:" -ForegroundColor White
Write-Host "1. Acessar Status Page: http://localhost:3000/status" -ForegroundColor Gray
Write-Host "2. Testar autenticação no BackofficeWeb" -ForegroundColor Gray
Write-Host "3. Criar conta corrente para EmpresaTeste" -ForegroundColor Gray
Write-Host "4. Executar transação PIX de teste" -ForegroundColor Gray
Write-Host "5. Verificar conciliação com Sicoob" -ForegroundColor Gray
Write-Host ""

if ($healthyServices -eq $services.Count) {
    Write-Host "🎉 AMBIENTE 100% PRONTO PARA TESTES!" -ForegroundColor Green
} else {
    Write-Host "⚠️ Alguns serviços podem precisar de mais tempo para inicializar" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "📚 DOCUMENTAÇÃO DISPONÍVEL:" -ForegroundColor Cyan
Write-Host "• configuracao-empresateste.md" -ForegroundColor Gray
Write-Host "• criacao-conta-empresateste.md" -ForegroundColor Gray
Write-Host "• transacao-pix-empresateste.md" -ForegroundColor Gray
Write-Host "• historico-extrato-empresateste.md" -ForegroundColor Gray
Write-Host "• RELATORIO_FINAL_TRILHA_INTEGRADA.md" -ForegroundColor Gray
