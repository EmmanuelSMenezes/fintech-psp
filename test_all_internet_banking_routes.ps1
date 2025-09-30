# Test all Internet Banking routes
Write-Host "=== TESTE COMPLETO INTERNET BANKING ===" -ForegroundColor Yellow

# Login
try {
    $login = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body '{"email":"admin@fintechpsp.com","password":"admin123"}' -ContentType 'application/json'
    $auth = @{Authorization="Bearer $($login.accessToken)"}
    Write-Host "✅ Login: SUCESSO" -ForegroundColor Green
    
    # Use a fixed clientId for testing
    $clientId = "666da775-b844-44e8-9188-61f83891b8f6"
    Write-Host "ClientId: $clientId" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Login: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test all routes
$routes = @(
    @{Name="Contas"; Url="http://localhost:5000/banking/contas"; Method="GET"},
    @{Name="Balance"; Url="http://localhost:5000/banking/balance/$clientId"; Method="GET"},
    @{Name="Extrato"; Url="http://localhost:5000/banking/balance/$clientId/extrato?startDate=2024-01-01&endDate=2024-03-31"; Method="GET"},
    @{Name="Transacoes"; Url="http://localhost:5000/banking/transacoes"; Method="GET"},
    @{Name="PIX QR Code"; Url="http://localhost:5000/banking/pix/qr-code"; Method="GET"},
    @{Name="PIX Chaves"; Url="http://localhost:5000/banking/pix/chaves"; Method="GET"},
    @{Name="Configs Roteamento"; Url="http://localhost:5000/banking/configs/roteamento"; Method="GET"},
    @{Name="Bancos"; Url="http://localhost:5000/banking/bancos"; Method="GET"},
    @{Name="Webhooks"; Url="http://localhost:5000/banking/webhooks"; Method="GET"}
)

foreach ($route in $routes) {
    Write-Host "`n--- $($route.Name) ---" -ForegroundColor Cyan
    try {
        $response = Invoke-RestMethod -Uri $route.Url -Method $route.Method -Headers $auth -TimeoutSec 15
        if ($response -is [array]) {
            Write-Host "✅ $($route.Name): SUCESSO - $($response.Count) itens" -ForegroundColor Green
        } elseif ($response.PSObject.Properties.Count -gt 0) {
            $props = ($response.PSObject.Properties.Name | Select-Object -First 3) -join ", "
            Write-Host "✅ $($route.Name): SUCESSO - Props: $props" -ForegroundColor Green
        } else {
            Write-Host "✅ $($route.Name): SUCESSO - Resposta vazia" -ForegroundColor Green
        }
    } catch {
        $statusCode = ""
        if ($_.Exception.Response) {
            $statusCode = " ($($_.Exception.Response.StatusCode))"
        }
        Write-Host "❌ $($route.Name): $($_.Exception.Message)$statusCode" -ForegroundColor Red
    }
}

Write-Host "`n=== FIM DOS TESTES INTERNET BANKING ===" -ForegroundColor Yellow
