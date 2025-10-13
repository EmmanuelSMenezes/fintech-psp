Write-Host "Testando criação de empresa..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Testar endpoint de teste
try {
    $testResponse = Invoke-RestMethod -Uri 'http://localhost:5010/admin/companies/test' -Method GET
    Write-Host "✅ CompanyService: $($testResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "❌ CompanyService erro: $($_.Exception.Message)" -ForegroundColor Red
}

# Criar empresa
$companyHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

$companyData = @{
    company = @{
        razaoSocial = "Empresa Teste LTDA"
        nomeFantasia = "EmpresaTeste"
        cnpj = "12345678000199"
        email = "contato@empresateste.com"
        telefone = "11987654321"
        address = @{
            cep = "01234567"
            logradouro = "Rua Teste"
            numero = "123"
            bairro = "Centro"
            cidade = "São Paulo"
            estado = "SP"
            pais = "Brasil"
        }
    }
    applicant = @{
        nome = "João Silva"
        cpf = "12345678900"
        email = "joao@empresateste.com"
    }
    legalRepresentatives = @()
} | ConvertTo-Json -Depth 5

Write-Host "Criando empresa..." -ForegroundColor Yellow

try {
    $companyResponse = Invoke-RestMethod -Uri 'http://localhost:5010/admin/companies' -Method POST -Body $companyData -Headers $companyHeaders
    Write-Host "✅ Empresa criada: $($companyResponse.razaoSocial)" -ForegroundColor Green
    Write-Host "ID: $($companyResponse.id)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
}
