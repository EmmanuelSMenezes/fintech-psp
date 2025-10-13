# TESTE DEBUG DE AUTENTICAÇÃO
# Objetivo: Diagnosticar problemas de login

Write-Host "=== TESTE DEBUG DE AUTENTICAÇÃO ===" -ForegroundColor Green
Write-Host ""

# Configurações
$baseUrl = "http://localhost:5000"
$authUrl = "http://localhost:5001"
$adminEmail = "admin@fintechpsp.com"
$adminPassword = "admin123"

# 1. Testar AuthService direto
Write-Host "1. Testando AuthService direto..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = $adminEmail
        password = $adminPassword
    } | ConvertTo-Json
    
    Write-Host "   Body: $loginBody" -ForegroundColor Gray
    
    $directResponse = Invoke-RestMethod -Uri "$authUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -TimeoutSec 10
    Write-Host "   ✅ Login direto OK!" -ForegroundColor Green
    Write-Host "   Resposta completa: $($directResponse | ConvertTo-Json -Depth 3)" -ForegroundColor Gray

    if ($directResponse.accessToken) {
        Write-Host "   Token: $($directResponse.accessToken.Substring(0, 50))..." -ForegroundColor Gray
        $directToken = $directResponse.accessToken
    } else {
        Write-Host "   ❌ Token não encontrado na resposta" -ForegroundColor Red
    }

    if ($directResponse.user) {
        Write-Host "   User: $($directResponse.user.name)" -ForegroundColor Gray
    }
} catch {
    Write-Host "   ❌ Erro no login direto: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $responseBody = $reader.ReadToEnd()
            Write-Host "   Response: $responseBody" -ForegroundColor Yellow
        } catch {
            Write-Host "   Não foi possível ler o erro" -ForegroundColor Red
        }
    }
    return
}

# 2. Testar via API Gateway
Write-Host "2. Testando via API Gateway..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = $adminEmail
        password = $adminPassword
    } | ConvertTo-Json
    
    $gatewayResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -TimeoutSec 10
    Write-Host "   ✅ Login via Gateway OK!" -ForegroundColor Green
    Write-Host "   Token: $($gatewayResponse.accessToken.Substring(0, 50))..." -ForegroundColor Gray
    Write-Host "   User: $($gatewayResponse.user.name)" -ForegroundColor Gray

    $gatewayToken = $gatewayResponse.accessToken
} catch {
    Write-Host "   ❌ Erro no login via Gateway: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $responseBody = $reader.ReadToEnd()
            Write-Host "   Response: $responseBody" -ForegroundColor Yellow
        } catch {
            Write-Host "   Não foi possível ler o erro" -ForegroundColor Red
        }
    }
    return
}

# 3. Testar token em endpoint protegido
if ($gatewayToken) {
    Write-Host "3. Testando token em endpoint protegido..." -ForegroundColor Cyan
    try {
        $headers = @{ Authorization = "Bearer $gatewayToken" }
        $testResponse = Invoke-RestMethod -Uri "$baseUrl/admin/users" -Headers $headers -TimeoutSec 10
        Write-Host "   ✅ Token válido! Usuários encontrados: $($testResponse.Count)" -ForegroundColor Green
    } catch {
        Write-Host "   ❌ Erro ao usar token: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "   Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "=== FIM TESTE DEBUG ===" -ForegroundColor Green
