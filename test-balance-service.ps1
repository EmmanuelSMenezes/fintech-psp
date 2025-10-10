Write-Host "TESTES DO BALANCE SERVICE" -ForegroundColor Cyan
Write-Host "=========================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5003"

# Primeiro, obter token OAuth
Write-Host "Obtendo token OAuth..." -ForegroundColor Yellow
try {
    $oauthData = @{
        grant_type = "client_credentials"
        client_id = "fintech_admin"
        client_secret = "admin_secret_789"
        scope = "pix banking admin"
    } | ConvertTo-Json

    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5001/auth/token" -Method POST -Body $oauthData -ContentType "application/json" -TimeoutSec 10
    $token = $tokenResponse.access_token
    Write-Host "✅ Token obtido com sucesso" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao obter token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host ""

Write-Host "TC013: Verificar Swagger do Balance Service" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method GET -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ TC013 PASSOU - Swagger do Balance Service acessivel" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ TC013 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC014: Health Check Balance Service" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/health" -Method GET -TimeoutSec 10
    Write-Host "✅ TC014 PASSOU - Health check OK" -ForegroundColor Green
    Write-Host "Status: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
} catch {
    Write-Host "❌ TC014 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC015: Consultar Saldo de Cliente" -ForegroundColor Yellow
try {
    # Usar um ID de cliente de teste
    $clientId = "62ea7d2b-4c54-4aae-9a9b-5c4404d734bf"
    $response = Invoke-RestMethod -Uri "$baseUrl/api/balance/$clientId" -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ TC015 PASSOU - Saldo consultado com sucesso" -ForegroundColor Green
    Write-Host "Saldo: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "⚠️ TC015 - Cliente nao encontrado (esperado para teste)" -ForegroundColor Yellow
    } else {
        Write-Host "❌ TC015 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "TC016: Consultar Extrato de Cliente" -ForegroundColor Yellow
try {
    $clientId = "62ea7d2b-4c54-4aae-9a9b-5c4404d734bf"
    $startDate = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
    $endDate = (Get-Date).ToString("yyyy-MM-dd")
    
    $response = Invoke-RestMethod -Uri "$baseUrl/api/balance/$clientId/extrato?startDate=$startDate&endDate=$endDate" -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ TC016 PASSOU - Extrato consultado com sucesso" -ForegroundColor Green
    Write-Host "Transacoes: $($response.Count) registros" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 404) {
        Write-Host "⚠️ TC016 - Cliente nao encontrado (esperado para teste)" -ForegroundColor Yellow
    } else {
        Write-Host "❌ TC016 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "TC017: Listar Contas Bancarias" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/accounts" -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ TC017 PASSOU - Contas listadas com sucesso" -ForegroundColor Green
    Write-Host "Total de contas: $($response.Count)" -ForegroundColor Gray
} catch {
    Write-Host "❌ TC017 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "RESUMO DOS TESTES DO BALANCE SERVICE" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
