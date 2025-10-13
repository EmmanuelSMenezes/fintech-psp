Write-Host "Testando criação de empresa com dados mínimos..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Criar empresa com dados mínimos
$companyHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

# JSON mais simples
$companyData = '{
    "company": {
        "razaoSocial": "Empresa Teste LTDA",
        "cnpj": "98765432000188",
        "email": "contato@empresateste.com"
    },
    "applicant": {
        "nomeCompleto": "João Silva",
        "cpf": "12345678900",
        "email": "joao@empresateste.com"
    },
    "legalRepresentatives": []
}'

Write-Host "Criando empresa com dados mínimos..." -ForegroundColor Yellow
Write-Host "JSON: $companyData" -ForegroundColor Gray

try {
    $companyResponse = Invoke-RestMethod -Uri 'http://localhost:5010/admin/companies' -Method POST -Body $companyData -Headers $companyHeaders
    Write-Host "✅ Empresa criada: $($companyResponse.razaoSocial)" -ForegroundColor Green
    Write-Host "ID: $($companyResponse.id)" -ForegroundColor Green
    Write-Host "Status: $($companyResponse.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
    
    # Tentar obter mais detalhes do erro
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
    }
}
