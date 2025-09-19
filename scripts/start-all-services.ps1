#!/usr/bin/env pwsh

Write-Host "🚀 Iniciando todos os serviços do FintechPSP..." -ForegroundColor Green

# Função para iniciar serviço em nova janela
function Start-ServiceInNewWindow {
    param(
        [string]$ServiceName,
        [string]$ServicePath,
        [string]$Port,
        [string]$Color = "Green"
    )
    
    Write-Host "🔄 Iniciando $ServiceName na porta $Port..." -ForegroundColor $Color
    
    $scriptBlock = {
        param($path, $port, $name)
        Set-Location $path
        $env:ASPNETCORE_URLS = "http://localhost:$port"
        Write-Host "🚀 $name rodando em http://localhost:$port" -ForegroundColor Green
        dotnet run
    }
    
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$($scriptBlock.ToString())} '$ServicePath' '$Port' '$ServiceName'"
}

# Verificar se estamos no diretório raiz do projeto
if (-not (Test-Path "src/Services")) {
    Write-Host "❌ Execute este script a partir do diretório raiz do projeto!" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Serviços que serão iniciados:" -ForegroundColor Yellow
Write-Host "  • API Gateway (porta 5000)" -ForegroundColor Cyan
Write-Host "  • Auth Service (porta 5001)" -ForegroundColor Cyan
Write-Host "  • Config Service (porta 6002)" -ForegroundColor Cyan
Write-Host "  • Transaction Service (porta 6003)" -ForegroundColor Cyan
Write-Host "  • Balance Service (porta 6004)" -ForegroundColor Cyan
Write-Host "  • Webhook Service (porta 6005)" -ForegroundColor Cyan
Write-Host "  • Integration Service (porta 6006)" -ForegroundColor Cyan
Write-Host "  • User Service (porta 6007)" -ForegroundColor Cyan
Write-Host "  • Company Service (porta 5004)" -ForegroundColor Cyan
Write-Host ""

# Iniciar serviços
Start-ServiceInNewWindow "API Gateway" "src/Gateway/FintechPSP.APIGateway" "5000" "Blue"
Start-Sleep 2

Start-ServiceInNewWindow "Auth Service" "src/Services/FintechPSP.AuthService" "5001" "Green"
Start-Sleep 2

Start-ServiceInNewWindow "Config Service" "src/Services/FintechPSP.ConfigService" "6002" "Yellow"
Start-Sleep 2

Start-ServiceInNewWindow "Transaction Service" "src/Services/FintechPSP.TransactionService" "6003" "Magenta"
Start-Sleep 2

Start-ServiceInNewWindow "Balance Service" "src/Services/FintechPSP.BalanceService" "6004" "Cyan"
Start-Sleep 2

Start-ServiceInNewWindow "Webhook Service" "src/Services/FintechPSP.WebhookService" "6005" "Red"
Start-Sleep 2

Start-ServiceInNewWindow "Integration Service" "src/Services/FintechPSP.IntegrationService" "6006" "DarkGreen"
Start-Sleep 2

Start-ServiceInNewWindow "User Service" "src/Services/FintechPSP.UserService" "6007" "DarkYellow"
Start-Sleep 2

Start-ServiceInNewWindow "Company Service" "src/Services/FintechPSP.CompanyService" "5004" "DarkCyan"

Write-Host ""
Write-Host "✅ Todos os serviços foram iniciados!" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URLs dos serviços:" -ForegroundColor Yellow
Write-Host "  • API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "  • Auth Service: http://localhost:5001" -ForegroundColor White
Write-Host "  • Config Service: http://localhost:6002" -ForegroundColor White
Write-Host "  • Transaction Service: http://localhost:6003" -ForegroundColor White
Write-Host "  • Balance Service: http://localhost:6004" -ForegroundColor White
Write-Host "  • Webhook Service: http://localhost:6005" -ForegroundColor White
Write-Host "  • Integration Service: http://localhost:6006" -ForegroundColor White
Write-Host "  • User Service: http://localhost:6007" -ForegroundColor White
Write-Host "  • Company Service: http://localhost:5004" -ForegroundColor White
Write-Host ""
Write-Host "🎯 Para testar os serviços:" -ForegroundColor Yellow
Write-Host "  • Health Check: http://localhost:5000/health" -ForegroundColor White
Write-Host "  • Swagger Company: http://localhost:5004" -ForegroundColor White
Write-Host ""
Write-Host "⚠️  Aguarde alguns segundos para todos os serviços iniciarem completamente." -ForegroundColor Yellow
Write-Host "📝 Pressione qualquer tecla para sair..." -ForegroundColor Gray

$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
