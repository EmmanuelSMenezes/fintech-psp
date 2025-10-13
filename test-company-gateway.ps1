Write-Host "Testando criação de empresa via API Gateway..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Criar empresa via API Gateway
$companyHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

$companyData = '{
    "company": {
        "razaoSocial": "Empresa Gateway LTDA",
        "cnpj": "11223344000155",
        "email": "contato@empresagateway.com"
    },
    "applicant": {
        "nomeCompleto": "Maria Santos",
        "cpf": "98765432100",
        "email": "maria@empresagateway.com"
    },
    "legalRepresentatives": []
}'

Write-Host "Criando empresa via API Gateway..." -ForegroundColor Yellow

try {
    $companyResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/companies' -Method POST -Body $companyData -Headers $companyHeaders
    Write-Host "✅ Empresa criada via Gateway: $($companyResponse.razaoSocial)" -ForegroundColor Green
    Write-Host "ID: $($companyResponse.id)" -ForegroundColor Green
    Write-Host "Status: $($companyResponse.status)" -ForegroundColor Green
    
    # Salvar ID para próximos testes
    $global:companyId = $companyResponse.id
    
} catch {
    Write-Host "❌ Erro via Gateway: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
    }
}
