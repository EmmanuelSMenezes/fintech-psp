#!/usr/bin/env pwsh

Write-Host "TESTE DE ROTAS - INTERNET BANKING WEB" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Função para testar rotas do frontend
function Test-FrontendRoute {
    param(
        [string]$Name,
        [string]$Url
    )
    
    try {
        Write-Host "Testando $Name..." -ForegroundColor Yellow
        
        $response = Invoke-WebRequest -Uri $Url -Method GET -TimeoutSec 10
        
        if ($response.StatusCode -eq 200) {
            Write-Host "OK $Name`: STATUS 200" -ForegroundColor Green
            return $true
        } else {
            Write-Host "AVISO $Name`: STATUS $($response.StatusCode)" -ForegroundColor Yellow
            return $false
        }
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($statusCode) {
            Write-Host "ERRO $Name`: STATUS $statusCode - $($_.Exception.Message)" -ForegroundColor Red
        } else {
            Write-Host "ERRO $Name`: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $false
    }
}

Write-Host "TESTANDO ROTAS DO INTERNET BANKING (localhost:3001)" -ForegroundColor Magenta
Write-Host "===================================================" -ForegroundColor Magenta
Write-Host ""

# Lista de rotas para testar
$routes = @(
    @{ Name = "Home/Root"; Url = "http://localhost:3001/" },
    @{ Name = "Login"; Url = "http://localhost:3001/auth/signin" },
    @{ Name = "Dashboard"; Url = "http://localhost:3001/dashboard" },
    @{ Name = "Contas"; Url = "http://localhost:3001/contas" },
    @{ Name = "Transacoes"; Url = "http://localhost:3001/transacoes" },
    @{ Name = "Historico"; Url = "http://localhost:3001/historico" },
    @{ Name = "Priorizacao"; Url = "http://localhost:3001/priorizacao" },
    @{ Name = "Gestao Acessos"; Url = "http://localhost:3001/gestao-acessos" },
    @{ Name = "Configuracao"; Url = "http://localhost:3001/configuracao" },
    @{ Name = "Profile"; Url = "http://localhost:3001/profile" },
    @{ Name = "Calendar"; Url = "http://localhost:3001/calendar" },
    @{ Name = "Blank Page"; Url = "http://localhost:3001/blank" },
    @{ Name = "Alerts"; Url = "http://localhost:3001/alerts" },
    @{ Name = "Buttons"; Url = "http://localhost:3001/buttons" },
    @{ Name = "Modals"; Url = "http://localhost:3001/modals" },
    @{ Name = "Health API"; Url = "http://localhost:3001/api/health" }
)

$successCount = 0
$errorCount = 0
$results = @()

foreach ($route in $routes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    
    $results += @{
        Name = $route.Name
        Url = $route.Url
        Success = $success
    }
    
    if ($success) {
        $successCount++
    } else {
        $errorCount++
    }
    
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "RESUMO DOS TESTES" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "Total de rotas testadas: $($routes.Count)" -ForegroundColor White
Write-Host "Sucessos: $successCount" -ForegroundColor Green
Write-Host "Erros: $errorCount" -ForegroundColor Red
Write-Host ""

if ($errorCount -gt 0) {
    Write-Host "ROTAS COM PROBLEMAS:" -ForegroundColor Red
    Write-Host "====================" -ForegroundColor Red
    foreach ($result in $results) {
        if (-not $result.Success) {
            Write-Host "- $($result.Name): $($result.Url)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

Write-Host "ROTAS FUNCIONANDO:" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
foreach ($result in $results) {
    if ($result.Success) {
        Write-Host "- $($result.Name): $($result.Url)" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "PROXIMO: Testando BackofficeWeb..." -ForegroundColor Yellow
