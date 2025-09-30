#!/usr/bin/env pwsh

Write-Host "TESTE DE APIS CORRETAS DOS FRONTENDS" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Função para testar APIs
function Test-API {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        Write-Host "Testando $Name..." -ForegroundColor Yellow
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 10
        }
        
        if ($Body) {
            $params.Body = $Body
            $params.ContentType = 'application/json'
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "OK $Name`: SUCESSO" -ForegroundColor Green
        return @{ Success = $true; Data = $response }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode) {
            Write-Host "ERRO $Name`: STATUS $statusCode - $($_.Exception.Message)" -ForegroundColor Red
        } else {
            Write-Host "ERRO $Name`: $($_.Exception.Message)" -ForegroundColor Red
        }
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

Write-Host "1. TESTANDO LOGIN E OBTENDO TOKEN" -ForegroundColor Magenta
Write-Host "==================================" -ForegroundColor Magenta

$loginBody = '{"email":"admin@fintechpsp.com","password":"admin123"}'
$loginResult = Test-API -Name "Login API Gateway" -Url "http://localhost:5000/auth/login" -Method "POST" -Body $loginBody

if (-not $loginResult.Success) {
    Write-Host "ERRO CRITICO: Nao foi possivel fazer login. Parando testes." -ForegroundColor Red
    exit 1
}

$token = $loginResult.Data.accessToken
$authHeaders = @{ Authorization = "Bearer $token" }

Write-Host ""
Write-Host "2. TESTANDO HEALTH CHECKS DOS SERVICOS" -ForegroundColor Magenta
Write-Host "=======================================" -ForegroundColor Magenta

$healthChecks = @(
    @{ Name = "TransactionService Health"; Url = "http://localhost:5002/transacoes/health" },
    @{ Name = "BalanceService Health"; Url = "http://localhost:5003/saldo/health" },
    @{ Name = "WebhookService Health"; Url = "http://localhost:5004/webhooks/health" },
    @{ Name = "IntegrationService Health"; Url = "http://localhost:5005/integrations/health" },
    @{ Name = "UserService Health"; Url = "http://localhost:5006/health" },
    @{ Name = "ConfigService Health"; Url = "http://localhost:5007/health" },
    @{ Name = "AuthService Health"; Url = "http://localhost:5001/health" },
    @{ Name = "CompanyService Health"; Url = "http://localhost:5009/health" }
)

$healthSuccessCount = 0
$healthErrorCount = 0

foreach ($health in $healthChecks) {
    $result = Test-API -Name $health.Name -Url $health.Url
    if ($result.Success) { $healthSuccessCount++ } else { $healthErrorCount++ }
    Start-Sleep -Milliseconds 200
}

Write-Host ""
Write-Host "3. TESTANDO APIS CORRETAS DO INTERNET BANKING" -ForegroundColor Magenta
Write-Host "==============================================" -ForegroundColor Magenta

# Obter clientId do token para usar nas rotas de balance
$clientId = "123e4567-e89b-12d3-a456-426614174000"  # Mock client ID

$internetBankingAPIs = @(
    @{ Name = "Banking Contas"; Url = "http://localhost:5000/banking/contas" },
    @{ Name = "Banking Configs Roteamento"; Url = "http://localhost:5000/banking/configs/roteamento" },
    @{ Name = "Banking Bancos"; Url = "http://localhost:5000/banking/bancos" },
    @{ Name = "Banking Balance"; Url = "http://localhost:5000/banking/balance/$clientId" },
    @{ Name = "Banking Extrato"; Url = "http://localhost:5000/banking/balance/$clientId/extrato?startDate=2024-01-01&endDate=2024-12-31" }
)

$ibSuccessCount = 0
$ibErrorCount = 0

foreach ($api in $internetBankingAPIs) {
    $result = Test-API -Name $api.Name -Url $api.Url -Headers $authHeaders
    if ($result.Success) { $ibSuccessCount++ } else { $ibErrorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "4. TESTANDO APIS CORRETAS DO BACKOFFICE" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta

$backofficeAPIs = @(
    @{ Name = "Admin Users"; Url = "http://localhost:5000/admin/users" },
    @{ Name = "Admin Companies"; Url = "http://localhost:5000/admin/companies" },
    @{ Name = "Admin Contas"; Url = "http://localhost:5000/admin/contas" },
    @{ Name = "Admin Transacoes"; Url = "http://localhost:5000/admin/transacoes" },
    @{ Name = "Admin Balance Reports"; Url = "http://localhost:5000/admin/reports/$clientId" },
    @{ Name = "Admin Configs"; Url = "http://localhost:5000/admin/configs/sistema" },
    @{ Name = "Admin Webhooks"; Url = "http://localhost:5000/admin/webhooks" }
)

$boSuccessCount = 0
$boErrorCount = 0

foreach ($api in $backofficeAPIs) {
    $result = Test-API -Name $api.Name -Url $api.Url -Headers $authHeaders
    if ($result.Success) { $boSuccessCount++ } else { $boErrorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "5. TESTANDO TRANSACOES PIX/TED/BOLETO" -ForegroundColor Magenta
Write-Host "=====================================" -ForegroundColor Magenta

$transactionAPIs = @(
    @{ Name = "Transaction Status"; Url = "http://localhost:5000/transactions/123/status" },
    @{ Name = "QR Code Health"; Url = "http://localhost:5002/qrcode/health" }
)

$txSuccessCount = 0
$txErrorCount = 0

foreach ($api in $transactionAPIs) {
    $result = Test-API -Name $api.Name -Url $api.Url -Headers $authHeaders
    if ($result.Success) { $txSuccessCount++ } else { $txErrorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "RESUMO FINAL" -ForegroundColor Cyan
Write-Host "============" -ForegroundColor Cyan
Write-Host "Health Checks: $healthSuccessCount sucessos, $healthErrorCount erros" -ForegroundColor $(if($healthErrorCount -eq 0) {"Green"} else {"Red"})
Write-Host "Internet Banking APIs: $ibSuccessCount sucessos, $ibErrorCount erros" -ForegroundColor $(if($ibErrorCount -eq 0) {"Green"} else {"Red"})
Write-Host "Backoffice APIs: $boSuccessCount sucessos, $boErrorCount erros" -ForegroundColor $(if($boErrorCount -eq 0) {"Green"} else {"Red"})
Write-Host "Transaction APIs: $txSuccessCount sucessos, $txErrorCount erros" -ForegroundColor $(if($txErrorCount -eq 0) {"Green"} else {"Red"})

$totalErrors = $healthErrorCount + $ibErrorCount + $boErrorCount + $txErrorCount
if ($totalErrors -eq 0) {
    Write-Host ""
    Write-Host "SUCESSO TOTAL: Todas as APIs estao funcionando!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ATENCAO: $totalErrors APIs com problemas identificados!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "PROXIMOS PASSOS:" -ForegroundColor Yellow
    Write-Host "1. Verificar se os servicos estao rodando corretamente" -ForegroundColor White
    Write-Host "2. Verificar logs dos containers com problemas" -ForegroundColor White
    Write-Host "3. Verificar configuracoes do Ocelot Gateway" -ForegroundColor White
    Write-Host "4. Verificar permissoes e scopes dos tokens JWT" -ForegroundColor White
}
