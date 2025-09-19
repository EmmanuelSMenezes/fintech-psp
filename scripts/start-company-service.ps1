#!/usr/bin/env pwsh

Write-Host "🏢 Iniciando FintechPSP Company Service..." -ForegroundColor Green

# Navegar para o diretório do serviço
Set-Location "src/Services/FintechPSP.CompanyService"

# Verificar se o projeto existe
if (-not (Test-Path "FintechPSP.CompanyService.csproj")) {
    Write-Host "❌ Projeto FintechPSP.CompanyService.csproj não encontrado!" -ForegroundColor Red
    exit 1
}

# Restaurar dependências
Write-Host "📦 Restaurando dependências..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao restaurar dependências!" -ForegroundColor Red
    exit 1
}

# Compilar o projeto
Write-Host "🔨 Compilando projeto..." -ForegroundColor Yellow
dotnet build

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Erro ao compilar projeto!" -ForegroundColor Red
    exit 1
}

# Iniciar o serviço
Write-Host "🚀 Iniciando Company Service na porta 5004..." -ForegroundColor Green
$env:ASPNETCORE_URLS = "http://localhost:5004"
dotnet run
