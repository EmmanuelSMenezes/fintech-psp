# ========================================
# Verificação de Status dos Serviços
# ========================================
# Script para verificar se todos os serviços necessários estão rodando

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$IntegrationUrl = "http://localhost:5005", 
    [string]$TransactionUrl = "http://localhost:5002",
    [string]$UserUrl = "http://localhost:5003",
    [string]$CompanyUrl = "http://localhost:5004"
)

$ErrorActionPreference = "Continue"

Write-Host "VERIFICANDO STATUS DOS SERVICOS FINTECH PSP" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""

# Função para verificar serviço
function Test-Service {
    param(
        [string]$Name,
        [string]$Url,
        [string]$HealthEndpoint = "/health"
    )
    
    Write-Host "📡 Testando $Name..." -ForegroundColor Yellow -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri "$Url$HealthEndpoint" -Method GET -TimeoutSec 5
        
        if ($response.StatusCode -eq 200) {
            Write-Host " ✅ ONLINE" -ForegroundColor Green
            
            # Tentar parsear JSON para mais detalhes
            try {
                $healthData = $response.Content | ConvertFrom-Json
                if ($healthData.status) {
                    Write-Host "   Status: $($healthData.status)" -ForegroundColor Gray
                }
                if ($healthData.service) {
                    Write-Host "   Service: $($healthData.service)" -ForegroundColor Gray
                }
                if ($healthData.timestamp) {
                    Write-Host "   Timestamp: $($healthData.timestamp)" -ForegroundColor Gray
                }
            }
            catch {
                # Ignorar erro de parsing JSON
            }
            
            return $true
        }
        else {
            Write-Host " ❌ OFFLINE (Status: $($response.StatusCode))" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host " ❌ OFFLINE (Erro: $($_.Exception.Message))" -ForegroundColor Red
        return $false
    }
}

# Função para verificar endpoint específico
function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET"
    )
    
    Write-Host "🎯 Testando endpoint $Name..." -ForegroundColor Cyan -NoNewline
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method $Method -TimeoutSec 5
        Write-Host " ✅ DISPONÍVEL (Status: $($response.StatusCode))" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host " ❌ INDISPONÍVEL" -ForegroundColor Red
        return $false
    }
}

# Lista de serviços para verificar
$services = @(
    @{ Name = "API Gateway"; Url = $BaseUrl },
    @{ Name = "Transaction Service"; Url = $TransactionUrl },
    @{ Name = "Integration Service"; Url = $IntegrationUrl },
    @{ Name = "User Service"; Url = $UserUrl },
    @{ Name = "Company Service"; Url = $CompanyUrl }
)

$onlineServices = 0
$totalServices = $services.Count

# Verificar cada serviço
foreach ($service in $services) {
    if (Test-Service -Name $service.Name -Url $service.Url) {
        $onlineServices++
    }
    Write-Host ""
}

Write-Host "📊 RESUMO DOS SERVIÇOS" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
Write-Host "Online: $onlineServices/$totalServices" -ForegroundColor $(if ($onlineServices -eq $totalServices) { "Green" } else { "Yellow" })
Write-Host ""

# Verificar endpoints específicos importantes
Write-Host "🎯 VERIFICANDO ENDPOINTS ESPECÍFICOS" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

$endpoints = @(
    @{ Name = "QR Code Dinâmico"; Url = "$TransactionUrl/transacoes/pix/qrcode/dinamico"; Method = "POST" },
    @{ Name = "Sicoob PIX Cobrança"; Url = "$IntegrationUrl/integrations/sicoob/pix/cobranca"; Method = "POST" },
    @{ Name = "Sicoob QR Code"; Url = "$IntegrationUrl/integrations/sicoob/cobranca/v3/qrcode"; Method = "POST" },
    @{ Name = "Health Integration"; Url = "$IntegrationUrl/integrations/health"; Method = "GET" }
)

$availableEndpoints = 0
foreach ($endpoint in $endpoints) {
    if (Test-Endpoint -Name $endpoint.Name -Url $endpoint.Url -Method $endpoint.Method) {
        $availableEndpoints++
    }
}

Write-Host ""
Write-Host "📊 RESUMO DOS ENDPOINTS" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan
Write-Host "Disponíveis: $availableEndpoints/$($endpoints.Count)" -ForegroundColor $(if ($availableEndpoints -eq $endpoints.Count) { "Green" } else { "Yellow" })
Write-Host ""

# Verificar configurações do Sicoob
Write-Host "🏦 VERIFICANDO CONFIGURAÇÕES SICOOB" -ForegroundColor Cyan
Write-Host "===================================" -ForegroundColor Cyan
Write-Host ""

try {
    $response = Invoke-WebRequest -Uri "$IntegrationUrl/integrations/sicoob/test-connectivity" -Method GET -TimeoutSec 10
    
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ Conectividade Sicoob: OK" -ForegroundColor Green
        
        try {
            $connectivityData = $response.Content | ConvertFrom-Json
            Write-Host "   Timestamp: $($connectivityData.timestamp)" -ForegroundColor Gray
            
            if ($connectivityData.tests) {
                foreach ($test in $connectivityData.tests) {
                    $status = if ($test.success) { "✅" } else { "❌" }
                    Write-Host "   $status $($test.name): $($test.message)" -ForegroundColor Gray
                }
            }
        }
        catch {
            Write-Host "   Detalhes não disponíveis" -ForegroundColor Gray
        }
    }
    else {
        Write-Host "❌ Conectividade Sicoob: FALHA (Status: $($response.StatusCode))" -ForegroundColor Red
    }
}
catch {
    Write-Host "❌ Conectividade Sicoob: ERRO ($($_.Exception.Message))" -ForegroundColor Red
}

Write-Host ""

# Verificar Docker containers (se disponível)
Write-Host "🐳 VERIFICANDO CONTAINERS DOCKER" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

try {
    $dockerOutput = docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>$null
    
    if ($dockerOutput) {
        Write-Host $dockerOutput -ForegroundColor Gray
    }
    else {
        Write-Host "⚠️  Docker não disponível ou nenhum container rodando" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "⚠️  Não foi possível verificar containers Docker" -ForegroundColor Yellow
}

Write-Host ""

# Recomendações finais
Write-Host "💡 RECOMENDAÇÕES" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green

if ($onlineServices -lt $totalServices) {
    Write-Host "❗ Alguns serviços estão offline. Execute:" -ForegroundColor Yellow
    Write-Host "   docker-compose -f docker-compose-complete.yml up -d" -ForegroundColor White
    Write-Host ""
}

if ($onlineServices -eq $totalServices -and $availableEndpoints -eq $endpoints.Count) {
    Write-Host "🎉 Todos os serviços estão online e prontos para teste!" -ForegroundColor Green
    Write-Host "   Execute: .\test-qrcode-dinamico-sicoob.ps1" -ForegroundColor White
    Write-Host ""
}
else {
    Write-Host "⚠️  Aguarde alguns minutos para os serviços inicializarem completamente" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "🔗 URLs dos Serviços:" -ForegroundColor Cyan
Write-Host "   API Gateway: $BaseUrl" -ForegroundColor Gray
Write-Host "   Transaction Service: $TransactionUrl" -ForegroundColor Gray
Write-Host "   Integration Service: $IntegrationUrl" -ForegroundColor Gray
Write-Host "   User Service: $UserUrl" -ForegroundColor Gray
Write-Host "   Company Service: $CompanyUrl" -ForegroundColor Gray
Write-Host ""

Write-Host "Verificacao concluida!" -ForegroundColor Green
