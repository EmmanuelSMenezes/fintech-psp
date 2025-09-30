#!/usr/bin/env pwsh

Write-Host "TESTE DE APIS DOS FRONTENDS" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
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
Write-Host "2. TESTANDO APIS DO INTERNET BANKING" -ForegroundColor Magenta
Write-Host "=====================================" -ForegroundColor Magenta

$internetBankingAPIs = @(
    @{ Name = "Banking Contas"; Url = "http://localhost:5000/banking/contas" },
    @{ Name = "Banking Configs"; Url = "http://localhost:5000/banking/configs/roteamento" },
    @{ Name = "Banking Bancos"; Url = "http://localhost:5000/banking/bancos" },
    @{ Name = "Banking Transacoes"; Url = "http://localhost:5000/banking/transacoes" },
    @{ Name = "Banking Balance"; Url = "http://localhost:5000/banking/balance" }
)

$ibSuccessCount = 0
$ibErrorCount = 0

foreach ($api in $internetBankingAPIs) {
    $result = Test-API -Name $api.Name -Url $api.Url -Headers $authHeaders
    if ($result.Success) { $ibSuccessCount++ } else { $ibErrorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "3. TESTANDO APIS DO BACKOFFICE" -ForegroundColor Magenta
Write-Host "==============================" -ForegroundColor Magenta

$backofficeAPIs = @(
    @{ Name = "Admin Users"; Url = "http://localhost:5000/admin/users" },
    @{ Name = "Admin Companies"; Url = "http://localhost:5000/admin/companies" },
    @{ Name = "Admin Contas"; Url = "http://localhost:5000/admin/contas" },
    @{ Name = "Admin Transacoes"; Url = "http://localhost:5000/admin/transacoes" },
    @{ Name = "Admin Reports"; Url = "http://localhost:5000/admin/reports/dashboard" },
    @{ Name = "Admin Config"; Url = "http://localhost:5000/admin/configs/sistema" },
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
Write-Host "4. TESTANDO SERVICOS DIRETOS" -ForegroundColor Magenta
Write-Host "=============================" -ForegroundColor Magenta

$directServices = @(
    @{ Name = "AuthService Health"; Url = "http://localhost:5001/health" },
    @{ Name = "UserService Health"; Url = "http://localhost:5006/health" },
    @{ Name = "TransactionService Health"; Url = "http://localhost:5002/health" },
    @{ Name = "BalanceService Health"; Url = "http://localhost:5003/health" },
    @{ Name = "ConfigService Health"; Url = "http://localhost:5007/health" },
    @{ Name = "WebhookService Health"; Url = "http://localhost:5004/health" },
    @{ Name = "CompanyService Health"; Url = "http://localhost:5009/health" },
    @{ Name = "IntegrationService Health"; Url = "http://localhost:5005/health" }
)

$dsSuccessCount = 0
$dsErrorCount = 0

foreach ($service in $directServices) {
    $result = Test-API -Name $service.Name -Url $service.Url
    if ($result.Success) { $dsSuccessCount++ } else { $dsErrorCount++ }
    Start-Sleep -Milliseconds 200
}

Write-Host ""
Write-Host "RESUMO FINAL" -ForegroundColor Cyan
Write-Host "============" -ForegroundColor Cyan
Write-Host "Internet Banking APIs: $ibSuccessCount sucessos, $ibErrorCount erros" -ForegroundColor $(if($ibErrorCount -eq 0) {"Green"} else {"Red"})
Write-Host "Backoffice APIs: $boSuccessCount sucessos, $boErrorCount erros" -ForegroundColor $(if($boErrorCount -eq 0) {"Green"} else {"Red"})
Write-Host "Servicos Diretos: $dsSuccessCount sucessos, $dsErrorCount erros" -ForegroundColor $(if($dsErrorCount -eq 0) {"Green"} else {"Red"})

$totalErrors = $ibErrorCount + $boErrorCount + $dsErrorCount
if ($totalErrors -eq 0) {
    Write-Host ""
    Write-Host "SUCESSO TOTAL: Todas as APIs estao funcionando!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ATENCAO: $totalErrors APIs com problemas identificados!" -ForegroundColor Yellow
}
