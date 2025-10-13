#!/usr/bin/env pwsh

# Script para subir o ambiente Docker completo do FintechPSP
Write-Host "INICIANDO AMBIENTE DOCKER FINTECHPSP" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan

# Verificar se Docker está rodando
Write-Host "Verificando Docker..." -ForegroundColor Yellow
try {
    docker version | Out-Null
    Write-Host "Docker esta rodando!" -ForegroundColor Green
}
catch {
    Write-Host "Docker nao esta rodando. Inicie o Docker Desktop primeiro." -ForegroundColor Red
    exit 1
}

# Parar containers existentes
Write-Host "Parando containers existentes..." -ForegroundColor Yellow
docker-compose -f docker-compose-complete.yml down

# Opcao 1: Subir apenas infraestrutura
Write-Host ""
Write-Host "Escolha uma opcao:" -ForegroundColor Cyan
Write-Host "1. Subir apenas infraestrutura (PostgreSQL, RabbitMQ, Redis)" -ForegroundColor White
Write-Host "2. Subir infraestrutura + microservicos" -ForegroundColor White
Write-Host "3. Subir tudo (infraestrutura + microservicos + frontends)" -ForegroundColor White
Write-Host "4. Subir em etapas (recomendado para teste)" -ForegroundColor Green

$choice = Read-Host "Digite sua escolha (1-4)"

switch ($choice) {
    "1" {
        Write-Host "Subindo apenas infraestrutura..." -ForegroundColor Cyan
        docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis
    }
    "2" {
        Write-Host "Subindo infraestrutura + microservicos..." -ForegroundColor Cyan
        docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis auth-service balance-service transaction-service integration-service user-service config-service webhook-service company-service api-gateway
    }
    "3" {
        Write-Host "Subindo tudo..." -ForegroundColor Cyan
        docker-compose -f docker-compose-complete.yml up -d
    }
    "4" {
        Write-Host "Subindo em etapas..." -ForegroundColor Cyan
        
        Write-Host "Etapa 1: Infraestrutura..." -ForegroundColor Yellow
        docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis
        Write-Host "Aguardando 30 segundos..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        Write-Host "Etapa 2: Microserviços básicos..." -ForegroundColor Yellow
        docker-compose -f docker-compose-complete.yml up -d auth-service user-service config-service company-service
        Write-Host "Aguardando 45 segundos..." -ForegroundColor Yellow
        Start-Sleep -Seconds 45
        
        Write-Host "Etapa 3: Serviços de negócio..." -ForegroundColor Yellow
        docker-compose -f docker-compose-complete.yml up -d balance-service transaction-service integration-service webhook-service
        Write-Host "Aguardando 45 segundos..." -ForegroundColor Yellow
        Start-Sleep -Seconds 45
        
        Write-Host "Etapa 4: API Gateway..." -ForegroundColor Yellow
        docker-compose -f docker-compose-complete.yml up -d api-gateway
        Write-Host "Aguardando 30 segundos..." -ForegroundColor Yellow
        Start-Sleep -Seconds 30
        
        Write-Host "Etapa 5: Frontends..." -ForegroundColor Yellow
        docker-compose -f docker-compose-complete.yml up -d backoffice-web internet-banking-web
    }
    default {
        Write-Host "❌ Opção inválida. Saindo..." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "⏳ Aguardando serviços ficarem prontos..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Mostrar status
Write-Host ""
Write-Host "📊 Status dos containers:" -ForegroundColor Cyan
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

Write-Host ""
Write-Host "🔧 Comandos úteis:" -ForegroundColor Cyan
Write-Host "• Ver logs: docker-compose -f docker-compose-complete.yml logs -f [service-name]" -ForegroundColor Yellow
Write-Host "• Parar tudo: docker-compose -f docker-compose-complete.yml down" -ForegroundColor Yellow
Write-Host "• Reiniciar serviço: docker-compose -f docker-compose-complete.yml restart [service-name]" -ForegroundColor Yellow
Write-Host "• Executar teste: ./test-docker-environment.ps1" -ForegroundColor Yellow

Write-Host ""
Write-Host "🌐 URLs dos serviços:" -ForegroundColor Cyan
Write-Host "• API Gateway: http://localhost:5000" -ForegroundColor White
Write-Host "• RabbitMQ Management: http://localhost:15673 (guest/guest)" -ForegroundColor White
Write-Host "• BackofficeWeb: http://localhost:3000" -ForegroundColor White
Write-Host "• InternetBankingWeb: http://localhost:3001" -ForegroundColor White

Write-Host ""
Write-Host "✅ Ambiente iniciado!" -ForegroundColor Green
