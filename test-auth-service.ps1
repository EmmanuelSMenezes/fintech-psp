#!/usr/bin/env pwsh

Write-Host "🔐 TESTES DE AUTENTICACAO - AUTHSERVICE" -ForegroundColor Cyan
Write-Host "=======================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5001"
$testResults = @()

# Função para executar teste de API
function Test-AuthAPI {
    param(
        [string]$TestCase,
        [string]$Description,
        [string]$Method,
        [string]$Endpoint,
        [hashtable]$Body = @{},
        [int]$ExpectedStatus = 200
    )
    
    Write-Host "🧪 $TestCase - $Description" -ForegroundColor Yellow
    
    try {
        $headers = @{
            "Content-Type" = "application/json"
        }
        
        $jsonBody = $Body | ConvertTo-Json -Depth 3
        
        if ($Method -eq "GET") {
            $response = Invoke-WebRequest -Uri "$baseUrl$Endpoint" -Method $Method -Headers $headers -UseBasicParsing -TimeoutSec 10
        } else {
            $response = Invoke-WebRequest -Uri "$baseUrl$Endpoint" -Method $Method -Headers $headers -Body $jsonBody -UseBasicParsing -TimeoutSec 10
        }
        
        $success = $response.StatusCode -eq $ExpectedStatus
        $statusColor = if ($success) { "Green" } else { "Red" }
        $statusIcon = if ($success) { "✅" } else { "❌" }
        
        Write-Host "   $statusIcon Status: $($response.StatusCode) - Expected: $ExpectedStatus" -ForegroundColor $statusColor
        
        if ($response.Content) {
            $content = $response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue
            if ($content) {
                Write-Host "   Response: $($content | ConvertTo-Json -Compress)" -ForegroundColor Gray
            }
        }
        
        $testResults += @{
            TestCase = $TestCase
            Description = $Description
            Success = $success
            StatusCode = $response.StatusCode
            ExpectedStatus = $ExpectedStatus
        }
        
    } catch {
        Write-Host "   ERROR: $($_.Exception.Message)" -ForegroundColor Red
        $testResults += @{
            TestCase = $TestCase
            Description = $Description
            Success = $false
            StatusCode = "ERRO"
            ExpectedStatus = $ExpectedStatus
            Error = $_.Exception.Message
        }
    }
    
    Write-Host ""
    return $testResults[-1]
}

Write-Host "🚀 Iniciando testes do AuthService..." -ForegroundColor Green
Write-Host ""

# TC001: Login de Usuário Válido
$loginBody = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
}

Test-AuthAPI -TestCase "TC001" -Description "Login com credenciais válidas" -Method "POST" -Endpoint "/auth/login" -Body $loginBody -ExpectedStatus 200

# TC002: Login com Credenciais Inválidas
$invalidLoginBody = @{
    email = "admin@fintechpsp.com"
    password = "senhaerrada"
}

Test-AuthAPI -TestCase "TC002" -Description "Login com senha inválida" -Method "POST" -Endpoint "/auth/login" -Body $invalidLoginBody -ExpectedStatus 401

# TC003: OAuth 2.0 Client Credentials
$oauthBody = @{
    grant_type = "client_credentials"
    client_id = "fintech_admin"
    client_secret = "admin_secret_789"
    scope = "pix,banking,admin"
}

Test-AuthAPI -TestCase "TC003" -Description "OAuth 2.0 Client Credentials" -Method "POST" -Endpoint "/auth/token" -Body $oauthBody -ExpectedStatus 200

# Teste adicional: Verificar endpoints disponíveis
Test-AuthAPI -TestCase "TC004" -Description "Health Check" -Method "GET" -Endpoint "/health" -ExpectedStatus 200

Write-Host "📊 RESUMO DOS TESTES" -ForegroundColor Cyan
Write-Host "====================" -ForegroundColor Cyan

$successCount = ($testResults | Where-Object { $_.Success }).Count
$totalCount = $testResults.Count

foreach ($result in $testResults) {
    $icon = if ($result.Success) { "✅" } else { "❌" }
    $color = if ($result.Success) { "Green" } else { "Red" }
    Write-Host "$icon $($result.TestCase): $($result.Description)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Sucessos: $successCount/$totalCount" -ForegroundColor Green
Write-Host "Falhas: $($totalCount - $successCount)/$totalCount" -ForegroundColor Red

if ($successCount -eq $totalCount) {
    Write-Host ""
    Write-Host "🎉 TODOS OS TESTES DE AUTENTICACAO PASSARAM!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "⚠️ ALGUNS TESTES FALHARAM - VERIFICAR LOGS ACIMA" -ForegroundColor Yellow
}

Write-Host ""
