Write-Host "Testando criacao de conta..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$adminToken = $loginResponse.accessToken
Write-Host "Login admin OK" -ForegroundColor Green

$authHeaders = @{
    'Authorization' = "Bearer $adminToken"
    'Content-Type' = 'application/json'
}

# Login cliente
$clientLoginBody = @{email = "joao.silva@empresateste.com"; password = "cliente123"} | ConvertTo-Json
$clientLoginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $clientLoginBody -Headers $loginHeaders
$clientToken = $clientLoginResponse.accessToken
$clientId = "ec8f0a2c-1347-4160-bbe0-c39448f1c1cb"
Write-Host "Login cliente OK" -ForegroundColor Green

$clientHeaders = @{
    'Authorization' = "Bearer $clientToken"
    'Content-Type' = 'application/json'
}

# Criar conta bancaria
Write-Host "Criando conta bancaria..." -ForegroundColor Yellow
$accountRequest = @{
    clienteId = $clientId
    bankCode = "756"
    accountNumber = "$(Get-Random -Minimum 10000 -Maximum 99999)-$(Get-Random -Minimum 1 -Maximum 9)"
    description = "Conta Corrente Principal"
    credentials = @{
        clientId = "client_test_123"
        clientSecret = "secret_test_456"
        apiKey = "api_key_789"
        environment = "sandbox"
        mtlsCert = ""
    }
} | ConvertTo-Json -Depth 5

try {
    $bankAccountResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/contas' -Method POST -Body $accountRequest -Headers $authHeaders
    Write-Host "Conta bancaria criada!" -ForegroundColor Green
    Write-Host "ContaId: $($bankAccountResponse.contaId)" -ForegroundColor Green
    Write-Host "BankCode: $($bankAccountResponse.bankCode)" -ForegroundColor Green
} catch {
    Write-Host "Erro ao criar conta: $($_.Exception.Message)" -ForegroundColor Red
}

# Verificar no BalanceService
Write-Host "Verificando BalanceService..." -ForegroundColor Yellow
try {
    $balanceResponse = Invoke-RestMethod -Uri "http://localhost:5003/saldo/$clientId" -Method GET -Headers $clientHeaders
    Write-Host "Conta no BalanceService!" -ForegroundColor Green
    Write-Host "Saldo disponivel: R$ $($balanceResponse.availableBalance)" -ForegroundColor Green
} catch {
    Write-Host "Conta nao encontrada no BalanceService: $($_.Exception.Message)" -ForegroundColor Red
}

# Listar contas
Write-Host "Listando contas..." -ForegroundColor Yellow
try {
    $accountsList = Invoke-RestMethod -Uri "http://localhost:5000/admin/contas" -Method GET -Headers $authHeaders
    Write-Host "Total de contas: $($accountsList.contas.Count)" -ForegroundColor Green
} catch {
    Write-Host "Erro ao listar contas: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Teste de criacao de conta concluido" -ForegroundColor Green
