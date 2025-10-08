#!/usr/bin/env pwsh

Write-Host "INICIANDO TODOS OS SISTEMAS PSP-SICOOB" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""

# Função para verificar se Docker está pronto
function Test-DockerReady {
    try {
        $null = docker info 2>$null
        return $true
    }
    catch {
        return $false
    }
}

# Aguardar Docker Desktop estar pronto
Write-Host "Verificando se Docker Desktop esta pronto..." -ForegroundColor Yellow
$maxAttempts = 20
$attempt = 0

while (-not (Test-DockerReady) -and $attempt -lt $maxAttempts) {
    $attempt++
    Write-Host "  Tentativa $attempt/$maxAttempts - Aguardando Docker Desktop..." -ForegroundColor Cyan
    Start-Sleep -Seconds 10
}

if (-not (Test-DockerReady)) {
    Write-Host "ERRO: Docker Desktop nao iniciou dentro do tempo esperado!" -ForegroundColor Red
    Write-Host "Por favor, inicie o Docker Desktop manualmente e tente novamente." -ForegroundColor Yellow
    exit 1
}

Write-Host "Docker Desktop esta pronto!" -ForegroundColor Green
Write-Host ""

# Parar containers existentes se houver
Write-Host "Parando containers existentes..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose-complete.yml down 2>$null
    Write-Host "Containers parados." -ForegroundColor Green
}
catch {
    Write-Host "Nenhum container para parar." -ForegroundColor Cyan
}

Write-Host ""

# Subir infraestrutura primeiro
Write-Host "ETAPA 1: Subindo infraestrutura (PostgreSQL, RabbitMQ, Redis)..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis
    Write-Host "Infraestrutura iniciada!" -ForegroundColor Green
}
catch {
    Write-Host "ERRO ao iniciar infraestrutura: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Aguardar infraestrutura estar pronta
Write-Host "Aguardando infraestrutura estar pronta..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

# Subir microserviços
Write-Host ""
Write-Host "ETAPA 2: Subindo microservicos..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose-complete.yml up -d auth-service user-service company-service transaction-service balance-service webhook-service integration-service config-service
    Write-Host "Microservicos iniciados!" -ForegroundColor Green
}
catch {
    Write-Host "ERRO ao iniciar microservicos: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Aguardar microserviços estarem prontos
Write-Host "Aguardando microservicos estarem prontos..." -ForegroundColor Cyan
Start-Sleep -Seconds 45

# Subir frontends
Write-Host ""
Write-Host "ETAPA 3: Subindo frontends..." -ForegroundColor Yellow
try {
    docker-compose -f docker-compose-complete.yml up -d api-gateway backoffice-web internet-banking-web
    Write-Host "Frontends iniciados!" -ForegroundColor Green
}
catch {
    Write-Host "ERRO ao iniciar frontends: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Aguardar tudo estar pronto
Write-Host ""
Write-Host "Aguardando todos os sistemas estarem prontos..." -ForegroundColor Cyan
Start-Sleep -Seconds 30

# Verificar status
Write-Host ""
Write-Host "VERIFICANDO STATUS DOS SISTEMAS:" -ForegroundColor Green
Write-Host "===============================" -ForegroundColor Green

try {
    docker-compose -f docker-compose-complete.yml ps
}
catch {
    Write-Host "ERRO ao verificar status: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "SISTEMAS INICIADOS!" -ForegroundColor Green
Write-Host "==================" -ForegroundColor Green
Write-Host ""
Write-Host "URLs dos sistemas:" -ForegroundColor Cyan
Write-Host "- API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "- BackofficeWeb: http://localhost:3000" -ForegroundColor White
Write-Host "- InternetBankingWeb: http://localhost:3001" -ForegroundColor White
Write-Host "- IntegrationService: http://localhost:5005" -ForegroundColor White
Write-Host "- CompanyService: http://localhost:5009" -ForegroundColor White
Write-Host "- TransactionService: http://localhost:5002" -ForegroundColor White
Write-Host ""
Write-Host "Execute o teste da trilha:" -ForegroundColor Yellow
Write-Host "powershell -ExecutionPolicy Bypass -File .\teste-trilha-simples.ps1" -ForegroundColor White
