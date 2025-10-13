Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  3. CADASTRO DE CLIENTE (EMPRESA)      " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Login admin para obter token
Write-Host "🔐 Fazendo login como admin..." -ForegroundColor Yellow

$loginHeaders = @{'Content-Type' = 'application/json'}
$loginBody = @{email = "admin@fintechpsp.com"; password = "admin123"} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri 'http://localhost:5001/auth/login' -Method POST -Body $loginBody -Headers $loginHeaders
$adminToken = $loginResponse.accessToken

Write-Host "✅ Login admin realizado com sucesso!" -ForegroundColor Green
Write-Host ""

# Criar empresa com dados consistentes
Write-Host "🏢 Criando empresa cliente..." -ForegroundColor Yellow

$companyHeaders = @{
    'Content-Type' = 'application/json'
    'Authorization' = "Bearer $adminToken"
}

$companyData = @{
    company = @{
        razaoSocial = "Empresa Teste LTDA"
        nomeFantasia = "EmpresaTeste"
        cnpj = "12.345.678/0001-99"
        inscricaoEstadual = "123456789"
        telefone = "(11) 98765-4321"
        email = "contato@empresateste.com"
        website = "www.empresateste.com"
        address = @{
            cep = "01234-567"
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
        cpf = "123.456.789-00"
        email = "joao@empresateste.com"
        telefone = "(11) 99999-9999"
    }
    legalRepresentatives = @(
        @{
            nome = "João Silva"
            cpf = "123.456.789-00"
            cargo = "Sócio Administrador"
            email = "joao@empresateste.com"
            telefone = "(11) 99999-9999"
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "📤 REQUEST - Criar Empresa:" -ForegroundColor Cyan
Write-Host "POST http://localhost:5000/admin/companies" -ForegroundColor White
Write-Host "Headers: Authorization: Bearer [token]" -ForegroundColor White
Write-Host ""

try {
    $companyResponse = Invoke-RestMethod -Uri 'http://localhost:5000/admin/companies' -Method POST -Body $companyData -Headers $companyHeaders -TimeoutSec 20
    
    Write-Host "📥 RESPONSE - Empresa Criada:" -ForegroundColor Green
    Write-Host "✅ Status: 201 Created" -ForegroundColor Green
    Write-Host "🏢 Company ID: $($companyResponse.id)" -ForegroundColor Green
    Write-Host "📋 Razão Social: $($companyResponse.razaoSocial)" -ForegroundColor Green
    Write-Host "🆔 CNPJ: $($companyResponse.cnpj)" -ForegroundColor Green
    Write-Host "📊 Status: $($companyResponse.status)" -ForegroundColor Green
    Write-Host "📅 Criado em: $($companyResponse.createdAt)" -ForegroundColor Green
    Write-Host ""
    
    # Salvar ID da empresa para próximos testes
    $global:companyId = $companyResponse.id
    
    Write-Host "🎉 CADASTRO DE EMPRESA - SUCESSO!" -ForegroundColor Green
    Write-Host "🔍 Empresa criada com status: $($companyResponse.status)" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Erro ao criar empresa: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
