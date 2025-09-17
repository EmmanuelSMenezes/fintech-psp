#!/usr/bin/env pwsh

Write-Host "🚀 INICIANDO TESTE LOCAL DO FINTECH PSP" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green

# Função para verificar se uma porta está em uso
function Test-Port {
    param([int]$Port)
    try {
        $connection = New-Object System.Net.Sockets.TcpClient
        $connection.Connect("localhost", $Port)
        $connection.Close()
        return $true
    }
    catch {
        return $false
    }
}

# Verificar se Docker está rodando
Write-Host "🔍 Verificando Docker..." -ForegroundColor Yellow
try {
    docker --version | Out-Null
    Write-Host "✅ Docker encontrado" -ForegroundColor Green
}
catch {
    Write-Host "❌ Docker não encontrado. Instale o Docker Desktop." -ForegroundColor Red
    exit 1
}

# Parar containers existentes se houver
Write-Host "🛑 Parando containers existentes..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down 2>$null

# Subir apenas a infraestrutura (PostgreSQL, RabbitMQ, Redis)
Write-Host "🐳 Subindo infraestrutura (PostgreSQL, RabbitMQ, Redis)..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

# Aguardar infraestrutura ficar pronta
Write-Host "⏳ Aguardando infraestrutura ficar pronta..." -ForegroundColor Yellow
Start-Sleep -Seconds 15

# Verificar se os serviços de infraestrutura estão rodando
$infraOk = $true
if (!(Test-Port 5433)) {
    Write-Host "❌ PostgreSQL não está respondendo na porta 5433" -ForegroundColor Red
    $infraOk = $false
}
if (!(Test-Port 5673)) {
    Write-Host "❌ RabbitMQ não está respondendo na porta 5673" -ForegroundColor Red
    $infraOk = $false
}
if (!(Test-Port 6380)) {
    Write-Host "❌ Redis não está respondendo na porta 6380" -ForegroundColor Red
    $infraOk = $false
}

if (!$infraOk) {
    Write-Host "❌ Infraestrutura não está funcionando corretamente" -ForegroundColor Red
    Write-Host "Verificando logs..." -ForegroundColor Yellow
    docker-compose -f docker-compose-complete.yml logs postgres rabbitmq redis
    exit 1
}

Write-Host "✅ Infraestrutura pronta!" -ForegroundColor Green

# Inicializar banco de dados
Write-Host "🗄️ Inicializando banco de dados..." -ForegroundColor Yellow
Get-Content init-database.sql | docker exec -i fintech-postgres-new psql -U postgres 2>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Banco de dados inicializado" -ForegroundColor Green
} else {
    Write-Host "⚠️ Banco já estava inicializado" -ForegroundColor Yellow
}

# Verificar se as portas dos serviços estão livres
$portsToCheck = @(5001, 5002, 5003, 5004, 5005, 5006, 5007, 5000)
$portsInUse = @()

foreach ($port in $portsToCheck) {
    if (Test-Port $port) {
        $portsInUse += $port
    }
}

if ($portsInUse.Count -gt 0) {
    Write-Host "⚠️ As seguintes portas estão em uso: $($portsInUse -join ', ')" -ForegroundColor Yellow
    Write-Host "Você pode continuar, mas pode haver conflitos." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 INFRAESTRUTURA PRONTA! AGORA SIGA OS PASSOS:" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 ABRA 4 TERMINAIS POWERSHELL E EXECUTE:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Terminal 1 - AuthService:" -ForegroundColor White
Write-Host "dotnet run --project src/Services/FintechPSP.AuthService --urls `"http://localhost:5001`"" -ForegroundColor Gray
Write-Host ""
Write-Host "Terminal 2 - TransactionService (QR Codes):" -ForegroundColor White
Write-Host "dotnet run --project src/Services/FintechPSP.TransactionService --urls `"http://localhost:5002`"" -ForegroundColor Gray
Write-Host ""
Write-Host "Terminal 3 - IntegrationService:" -ForegroundColor White
Write-Host "dotnet run --project src/Services/FintechPSP.IntegrationService --urls `"http://localhost:5005`"" -ForegroundColor Gray
Write-Host ""
Write-Host "Terminal 4 - APIGateway:" -ForegroundColor White
Write-Host "dotnet run --project src/Gateway/FintechPSP.APIGateway --urls `"http://localhost:5000`"" -ForegroundColor Gray
Write-Host ""
Write-Host "🔗 CONFIGURAÇÃO POSTMAN:" -ForegroundColor Cyan
Write-Host "- Importar: postman/FintechPSP-Collection.json" -ForegroundColor Gray
Write-Host "- Variável base_url: http://localhost:5000" -ForegroundColor Gray
Write-Host ""
Write-Host "🧪 SEQUÊNCIA DE TESTE:" -ForegroundColor Cyan
Write-Host "1. Obter Token: POST {{base_url}}/auth/token" -ForegroundColor Gray
Write-Host "2. QR Estático: POST {{base_url}}/transacoes/pix/qrcode/estatico" -ForegroundColor Gray
Write-Host "3. QR Dinâmico: POST {{base_url}}/transacoes/pix/qrcode/dinamico" -ForegroundColor Gray
Write-Host ""
Write-Host "📊 MONITORAMENTO:" -ForegroundColor Cyan
Write-Host "- PostgreSQL: http://localhost:5433 (postgres/postgres)" -ForegroundColor Gray
Write-Host "- RabbitMQ: http://localhost:15673 (guest/guest)" -ForegroundColor Gray
Write-Host "- Redis: localhost:6380" -ForegroundColor Gray
Write-Host ""
Write-Host "🛑 PARA PARAR TUDO:" -ForegroundColor Red
Write-Host "docker-compose -f docker-compose-complete.yml down" -ForegroundColor Gray
Write-Host ""
Write-Host "✅ PRONTO PARA TESTAR! 🎉" -ForegroundColor Green
