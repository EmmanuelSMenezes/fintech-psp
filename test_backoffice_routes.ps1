#!/usr/bin/env pwsh

Write-Host "TESTE DE ROTAS - BACKOFFICE WEB" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
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

Write-Host "TESTANDO ROTAS DO BACKOFFICE (localhost:3000)" -ForegroundColor Magenta
Write-Host "==============================================" -ForegroundColor Magenta
Write-Host ""

# Lista de rotas principais para testar
$mainRoutes = @(
    @{ Name = "Home/Dashboard"; Url = "http://localhost:3000/" },
    @{ Name = "Login"; Url = "http://localhost:3000/auth/signin" },
    @{ Name = "Empresas"; Url = "http://localhost:3000/empresas" },
    @{ Name = "Usuarios"; Url = "http://localhost:3000/usuarios" },
    @{ Name = "Contas"; Url = "http://localhost:3000/contas" },
    @{ Name = "Priorizacao"; Url = "http://localhost:3000/priorizacao" },
    @{ Name = "Transacoes"; Url = "http://localhost:3000/transacoes" },
    @{ Name = "Historico"; Url = "http://localhost:3000/historico" },
    @{ Name = "Status"; Url = "http://localhost:3000/status" },
    @{ Name = "Acessos"; Url = "http://localhost:3000/acessos" },
    @{ Name = "Configuracao"; Url = "http://localhost:3000/configuracao" }
)

# Lista de rotas de configurações
$configRoutes = @(
    @{ Name = "Config Sistema"; Url = "http://localhost:3000/configuracoes/sistema" },
    @{ Name = "Config Usuarios"; Url = "http://localhost:3000/configuracoes/usuarios" }
)

# Lista de rotas de integrações
$integrationRoutes = @(
    @{ Name = "Integracoes Status"; Url = "http://localhost:3000/integracoes/status" },
    @{ Name = "Integracoes Webhooks"; Url = "http://localhost:3000/integracoes/webhooks" }
)

# Lista de rotas de relatórios
$reportRoutes = @(
    @{ Name = "Relatorios Extrato"; Url = "http://localhost:3000/relatorios/extrato" },
    @{ Name = "Relatorios Financeiro"; Url = "http://localhost:3000/relatorios/financeiro" }
)

# Lista de rotas de UI/outros
$uiRoutes = @(
    @{ Name = "Profile"; Url = "http://localhost:3000/profile" },
    @{ Name = "Calendar"; Url = "http://localhost:3000/calendar" },
    @{ Name = "Blank Page"; Url = "http://localhost:3000/blank" },
    @{ Name = "Alerts"; Url = "http://localhost:3000/alerts" },
    @{ Name = "Buttons"; Url = "http://localhost:3000/buttons" },
    @{ Name = "Modals"; Url = "http://localhost:3000/modals" },
    @{ Name = "Health API"; Url = "http://localhost:3000/api/health" }
)

$allRoutes = $mainRoutes + $configRoutes + $integrationRoutes + $reportRoutes + $uiRoutes
$successCount = 0
$errorCount = 0
$results = @()

Write-Host "ROTAS PRINCIPAIS:" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
foreach ($route in $mainRoutes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    $results += @{ Name = $route.Name; Url = $route.Url; Success = $success; Category = "Principal" }
    if ($success) { $successCount++ } else { $errorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "ROTAS DE CONFIGURACAO:" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan
foreach ($route in $configRoutes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    $results += @{ Name = $route.Name; Url = $route.Url; Success = $success; Category = "Configuracao" }
    if ($success) { $successCount++ } else { $errorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "ROTAS DE INTEGRACOES:" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan
foreach ($route in $integrationRoutes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    $results += @{ Name = $route.Name; Url = $route.Url; Success = $success; Category = "Integracoes" }
    if ($success) { $successCount++ } else { $errorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "ROTAS DE RELATORIOS:" -ForegroundColor Cyan
Write-Host "====================" -ForegroundColor Cyan
foreach ($route in $reportRoutes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    $results += @{ Name = $route.Name; Url = $route.Url; Success = $success; Category = "Relatorios" }
    if ($success) { $successCount++ } else { $errorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "ROTAS DE UI/OUTROS:" -ForegroundColor Cyan
Write-Host "===================" -ForegroundColor Cyan
foreach ($route in $uiRoutes) {
    $success = Test-FrontendRoute -Name $route.Name -Url $route.Url
    $results += @{ Name = $route.Name; Url = $route.Url; Success = $success; Category = "UI" }
    if ($success) { $successCount++ } else { $errorCount++ }
    Start-Sleep -Milliseconds 300
}

Write-Host ""
Write-Host "RESUMO DOS TESTES" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "Total de rotas testadas: $($allRoutes.Count)" -ForegroundColor White
Write-Host "Sucessos: $successCount" -ForegroundColor Green
Write-Host "Erros: $errorCount" -ForegroundColor Red
Write-Host ""

if ($errorCount -gt 0) {
    Write-Host "ROTAS COM PROBLEMAS:" -ForegroundColor Red
    Write-Host "====================" -ForegroundColor Red
    foreach ($result in $results) {
        if (-not $result.Success) {
            Write-Host "- [$($result.Category)] $($result.Name): $($result.Url)" -ForegroundColor Red
        }
    }
    Write-Host ""
}

Write-Host "ROTAS FUNCIONANDO:" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
foreach ($result in $results) {
    if ($result.Success) {
        Write-Host "- [$($result.Category)] $($result.Name): $($result.Url)" -ForegroundColor Green
    }
}

Write-Host ""
if ($errorCount -gt 0) {
    Write-Host "ANALISE NECESSARIA: Algumas rotas apresentaram problemas!" -ForegroundColor Yellow
} else {
    Write-Host "SUCESSO TOTAL: Todas as rotas estao funcionando!" -ForegroundColor Green
}
