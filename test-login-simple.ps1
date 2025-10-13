# Teste simples de login para diagnosticar problemas

Write-Host "=== TESTE SIMPLES DE LOGIN ===" -ForegroundColor Green

$baseUrl = "http://localhost:5000"

# Teste 1: Verificar se API Gateway responde
Write-Host "1. Testando API Gateway..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "$baseUrl" -TimeoutSec 5
    Write-Host "   ✅ API Gateway respondendo (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ❌ API Gateway não responde: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Teste 2: Testar AuthService direto
Write-Host "2. Testando AuthService direto..." -ForegroundColor Cyan
try {
    $authUrl = "http://localhost:5001"
    $response = Invoke-WebRequest -Uri "$authUrl" -TimeoutSec 5
    Write-Host "   ✅ AuthService respondendo (Status: $($response.StatusCode))" -ForegroundColor Green
} catch {
    Write-Host "   ❌ AuthService não responde: $($_.Exception.Message)" -ForegroundColor Red
}

# Teste 3: Login via AuthService direto
Write-Host "3. Testando login via AuthService direto..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = "admin@fintechpsp.com"
        password = "admin123"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -TimeoutSec 10
    Write-Host "   ✅ Login direto OK" -ForegroundColor Green
    Write-Host "      Token: $($loginResponse.token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro no login direto: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "      Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Teste 4: Login via API Gateway
Write-Host "4. Testando login via API Gateway..." -ForegroundColor Cyan
try {
    $loginBody = @{
        email = "admin@fintechpsp.com"
        password = "admin123"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method POST -Body $loginBody -ContentType "application/json" -TimeoutSec 10
    Write-Host "   ✅ Login via Gateway OK" -ForegroundColor Green
    Write-Host "      Token: $($loginResponse.token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "   ❌ Erro no login via Gateway: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "      Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        try {
            $errorContent = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($errorContent)
            $errorText = $reader.ReadToEnd()
            Write-Host "      Erro: $errorText" -ForegroundColor Red
        } catch {
            Write-Host "      Não foi possível ler o erro" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "=== FIM TESTE ===" -ForegroundColor Green
