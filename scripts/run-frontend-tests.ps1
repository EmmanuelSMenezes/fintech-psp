# Script para executar testes dos frontends React
# PowerShell script para Windows

Write-Host "🧪 Executando testes dos frontends React..." -ForegroundColor Green

# Função para verificar se o comando foi executado com sucesso
function Test-Command {
    param($ExitCode, $TestName)
    if ($ExitCode -eq 0) {
        Write-Host "✅ $TestName - PASSOU" -ForegroundColor Green
        return $true
    } else {
        Write-Host "❌ $TestName - FALHOU" -ForegroundColor Red
        return $false
    }
}

$TestResults = @()

Write-Host "`n📋 Instalando dependências..." -ForegroundColor Yellow

# Instalar dependências do BackofficeWeb
Write-Host "`n🔧 Instalando dependências do BackofficeWeb..." -ForegroundColor Cyan
Set-Location "frontends/BackofficeWeb"
npm install
$TestResults += Test-Command $LASTEXITCODE "BackofficeWeb - Instalação de dependências"

# Executar testes do BackofficeWeb
Write-Host "`n🧪 Executando testes do BackofficeWeb..." -ForegroundColor Cyan
npm test -- --passWithNoTests --watchAll=false
$TestResults += Test-Command $LASTEXITCODE "BackofficeWeb - Testes unitários"

# Voltar para o diretório raiz
Set-Location "../.."

# Instalar dependências do InternetBankingWeb
Write-Host "`n🔧 Instalando dependências do InternetBankingWeb..." -ForegroundColor Cyan
Set-Location "frontends/InternetBankingWeb"
npm install
$TestResults += Test-Command $LASTEXITCODE "InternetBankingWeb - Instalação de dependências"

# Executar testes do InternetBankingWeb
Write-Host "`n🧪 Executando testes do InternetBankingWeb..." -ForegroundColor Cyan
npm test -- --passWithNoTests --watchAll=false
$TestResults += Test-Command $LASTEXITCODE "InternetBankingWeb - Testes unitários"

# Voltar para o diretório raiz
Set-Location "../.."

# Resumo dos resultados
Write-Host "`n📊 RESUMO DOS TESTES:" -ForegroundColor Yellow
Write-Host "===================" -ForegroundColor Yellow

$PassedTests = ($TestResults | Where-Object { $_ -eq $true }).Count
$TotalTests = $TestResults.Count

if ($PassedTests -eq $TotalTests) {
    Write-Host "🎉 TODOS OS TESTES PASSARAM! ($PassedTests/$TotalTests)" -ForegroundColor Green
    Write-Host "`n✅ Frontends React estão funcionando corretamente!" -ForegroundColor Green
    exit 0
} else {
    $FailedTests = $TotalTests - $PassedTests
    Write-Host "⚠️  ALGUNS TESTES FALHARAM! ($PassedTests/$TotalTests passaram, $FailedTests falharam)" -ForegroundColor Red
    Write-Host "`n❌ Verifique os logs acima para mais detalhes." -ForegroundColor Red
    exit 1
}

Write-Host "`n🏁 Script de testes concluído." -ForegroundColor Blue
