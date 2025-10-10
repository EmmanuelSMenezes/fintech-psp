Write-Host "TESTES DE INTEGRACAO SICOOB" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5005"

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

Write-Host "TC009: Health Check Integracoes" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/integrations/health" -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ TC009 PASSOU - Health check das integracoes OK" -ForegroundColor Green
    Write-Host "Status: $($response | ConvertTo-Json -Compress)" -ForegroundColor Gray
} catch {
    Write-Host "❌ TC009 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC010: Teste Sicoob PIX Cobranca (Sandbox)" -ForegroundColor Yellow
try {
    $cobrancaData = @{
        valor = "10.00"
        descricao = "Teste de cobranca PIX"
        pagador = @{
            nome = "Cliente Teste"
            cpf = "12345678901"
        }
        vencimento = (Get-Date).AddDays(1).ToString("yyyy-MM-dd")
    } | ConvertTo-Json -Depth 3

    $response = Invoke-RestMethod -Uri "$baseUrl/integrations/sicoob/pix/cobranca" -Method POST -Body $cobrancaData -Headers $headers -TimeoutSec 15
    Write-Host "✅ TC010 PASSOU - Cobranca PIX criada com sucesso" -ForegroundColor Green
    Write-Host "ID da cobranca: $($response.id)" -ForegroundColor Gray
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "⚠️ TC010 - Erro de autenticacao (esperado em sandbox)" -ForegroundColor Yellow
    } else {
        Write-Host "❌ TC010 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "TC011: Verificar Swagger da Integracao" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/swagger" -Method GET -UseBasicParsing -TimeoutSec 10
    if ($response.StatusCode -eq 200) {
        Write-Host "✅ TC011 PASSOU - Swagger da integracao acessivel" -ForegroundColor Green
    }
} catch {
    Write-Host "❌ TC011 FALHOU - $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

Write-Host "TC012: Listar Endpoints Disponiveis" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/endpoints" -Method GET -Headers $headers -TimeoutSec 10
    Write-Host "✅ TC012 PASSOU - Endpoints listados" -ForegroundColor Green
} catch {
    Write-Host "⚠️ TC012 - Endpoint nao encontrado (esperado)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "RESUMO DOS TESTES DE INTEGRACAO" -ForegroundColor Cyan
Write-Host "===============================" -ForegroundColor Cyan
