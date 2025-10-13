Write-Host "Testando criação de empresa diretamente no CompanyService..." -ForegroundColor Cyan

# Login admin
$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$token = $loginResponse.accessToken

Write-Host "✅ Login admin OK" -ForegroundColor Green

# Testar endpoint de teste do CompanyService
Write-Host "🔍 Testando endpoint de teste..." -ForegroundColor Yellow
try {
    $testResponse = Invoke-RestMethod -Uri 'http://localhost:5010/admin/companies/test' -Method GET
    Write-Host "✅ CompanyService respondendo: $($testResponse.message)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro no teste: $($_.Exception.Message)" -ForegroundColor Red
}

# Criar empresa diretamente no CompanyService
$companyHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $token"
}

$companyData = @{
    company = @{
        razaoSocial = "Empresa Teste LTDA"
        nomeFantasia = "EmpresaTeste"
        cnpj = "12345678000199"
        inscricaoEstadual = "123456789"
        telefone = "11987654321"
        email = "contato@empresateste.com"
        website = "www.empresateste.com"
        address = @{
            cep = "01234567"
            logradouro = "Rua Teste"
            numero = "123"
            bairro = "Centro"
            cidade = "São Paulo"
            estado = "SP"
            pais = "Brasil"
        }
        contractData = @{
            capitalSocial = 100000.00
            atividadePrincipal = "Desenvolvimento de software"
            atividadesSecundarias = @("Consultoria em TI")
        }
        observacoes = "Empresa criada para testes E2E"
    }
    applicant = @{
        nome = "João Silva"
        cpf = "12345678900"
        email = "joao@empresateste.com"
        telefone = "11999999999"
    }
    legalRepresentatives = @(
        @{
            nome = "João Silva"
            cpf = "12345678900"
            cargo = "Sócio Administrador"
            email = "joao@empresateste.com"
            telefone = "11999999999"
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "🏢 Criando empresa diretamente no CompanyService..." -ForegroundColor Yellow

try {
    $companyResponse = Invoke-RestMethod -Uri 'http://localhost:5010/admin/companies' -Method POST -Body $companyData -Headers $companyHeaders
    Write-Host "✅ Empresa criada: $($companyResponse.razaoSocial)" -ForegroundColor Green
    Write-Host "ID: $($companyResponse.id)" -ForegroundColor Green
    Write-Host "Status: $($companyResponse.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
