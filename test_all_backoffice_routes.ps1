# Test all Backoffice routes
Write-Host "=== TESTE COMPLETO BACKOFFICE ===" -ForegroundColor Yellow

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

# Test all admin routes
$routes = @(
    @{Name="Admin Users"; Url="http://localhost:5000/admin/users"; Method="GET"},
    @{Name="Admin Transacoes"; Url="http://localhost:5000/admin/transacoes"; Method="GET"},
    @{Name="Admin Reports"; Url="http://localhost:5000/admin/reports"; Method="GET"},
    @{Name="Admin Configs"; Url="http://localhost:5000/admin/configs"; Method="GET"},
    @{Name="Admin Webhooks"; Url="http://localhost:5000/admin/webhooks"; Method="GET"},
    @{Name="Admin Companies"; Url="http://localhost:5000/admin/companies"; Method="GET"},
    @{Name="Admin Contas"; Url="http://localhost:5000/admin/contas"; Method="GET"}
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

Write-Host "`n=== FIM DOS TESTES BACKOFFICE ===" -ForegroundColor Yellow
