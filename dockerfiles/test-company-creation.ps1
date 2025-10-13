# Script para testar criação de empresa com payload correto
# Estrutura baseada nos modelos C# exatos

Write-Host "🧪 TESTE CRIAÇÃO DE EMPRESA - PAYLOAD CORRETO" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan

# Obter token admin se não existir
if (-not $global:authToken) {
    Write-Host "🔐 Obtendo token admin..." -ForegroundColor Yellow
    $loginBody = '{"email":"admin@fintechpsp.com","password":"admin123"}'
    try {
        $loginResponse = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
        $global:authToken = $loginResponse.accessToken
        Write-Host "✅ Token obtido com sucesso!" -ForegroundColor Green
    } catch {
        Write-Host "❌ Erro ao obter token: $($_.Exception.Message)" -ForegroundColor Red
        exit 1
    }
}

# Headers para requisição
$headers = @{
    "Authorization" = "Bearer $($global:authToken)"
    "Content-Type" = "application/json"
}

# Payload com estrutura EXATA dos modelos C#
$payload = @{
    Company = @{
        RazaoSocial = "Empresa Teste PowerShell Correta LTDA"
        NomeFantasia = "Teste PowerShell Correta"
        Cnpj = "11.222.333/0001-81"
        InscricaoEstadual = "123456789"
        InscricaoMunicipal = "987654321"
        Telefone = "(11) 99999-7777"
        Email = "contato@testepowershellcorreta.com"
        Website = "www.testepowershellcorreta.com"
        Address = @{
            Cep = "01234-567"
            Logradouro = "Rua PowerShell Correta"
            Numero = "789"
            Complemento = "Sala 101"
            Bairro = "Centro"
            Cidade = "São Paulo"
            Estado = "SP"
            Pais = "Brasil"
        }
        ContractData = @{
            CapitalSocial = 100000.00
            AtividadePrincipal = "Desenvolvimento de software"
            AtividadesSecundarias = @("Consultoria em TI", "Suporte técnico")
        }
        Observacoes = "Empresa criada via PowerShell com payload correto"
    }
    Applicant = @{
        NomeCompleto = "João Silva PowerShell"
        Cpf = "123.456.789-00"
        Rg = "12.345.678-9"
        OrgaoExpedidor = "SSP-SP"
        EstadoCivil = "Solteiro"
        Nacionalidade = "Brasileira"
        Profissao = "Desenvolvedor"
        Email = "joao@testepowershellcorreta.com"
        Telefone = "(11) 99999-8888"
        Celular = "(11) 88888-7777"
        Address = @{
            Cep = "01234-567"
            Logradouro = "Rua Applicant Correta"
            Numero = "100"
            Complemento = "Apto 201"
            Bairro = "Centro"
            Cidade = "São Paulo"
            Estado = "SP"
            Pais = "Brasil"
        }
        RendaMensal = 15000.00
        Cargo = "Sócio Administrador"
        IsMainRepresentative = $true
    }
    LegalRepresentatives = @(
        @{
            NomeCompleto = "João Silva PowerShell"
            Cpf = "123.456.789-00"
            Rg = "12.345.678-9"
            OrgaoExpedidor = "SSP-SP"
            EstadoCivil = "Solteiro"
            Nacionalidade = "Brasileira"
            Profissao = "Desenvolvedor"
            Email = "joao@testepowershellcorreta.com"
            Telefone = "(11) 99999-8888"
            Celular = "(11) 88888-7777"
            Address = @{
                Cep = "01234-567"
                Logradouro = "Rua Representative Correta"
                Numero = "200"
                Complemento = "Casa"
                Bairro = "Centro"
                Cidade = "São Paulo"
                Estado = "SP"
                Pais = "Brasil"
            }
            Cargo = "Sócio Administrador"
            Type = 0  # RepresentationType enum
            PercentualParticipacao = 100.00
            PoderesRepresentacao = "Poderes gerais de administração"
            PodeAssinarSozinho = $true
            LimiteAlcada = 1000000.00
        }
    )
} | ConvertTo-Json -Depth 10

Write-Host "📤 Enviando requisição para CompanyService..." -ForegroundColor Yellow
Write-Host "URL: http://localhost:5010/admin/companies" -ForegroundColor White
Write-Host "Payload size: $($payload.Length) characters" -ForegroundColor White

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5010/admin/companies" -Method POST -Body $payload -Headers $headers
    
    Write-Host "✅ SUCESSO! Empresa criada:" -ForegroundColor Green
    Write-Host "ID: $($response.id)" -ForegroundColor Green
    Write-Host "Razão Social: $($response.razaoSocial)" -ForegroundColor Green
    Write-Host "CNPJ: $($response.cnpj)" -ForegroundColor Green
    Write-Host "Status: $($response.status)" -ForegroundColor Green
    Write-Host "Data Criação: $($response.createdAt)" -ForegroundColor Green
    
    # Salvar ID para próximos testes
    $global:newCompanyId = $response.id
    
    Write-Host "`n🎉 TESTE CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ ERRO na criação:" -ForegroundColor Red
    Write-Host "Mensagem: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        try {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = [System.IO.StreamReader]::new($stream)
            $errorBody = $reader.ReadToEnd()
            Write-Host "Detalhes do erro:" -ForegroundColor Red
            Write-Host $errorBody -ForegroundColor Red
        } catch {
            Write-Host "Não foi possível ler detalhes do erro" -ForegroundColor Red
        }
    }
}

Write-Host "`n📊 Verificando empresas cadastradas..." -ForegroundColor Yellow
try {
    $companiesUrl = "http://localhost:5010/admin/companies?page=1`&limit=10"
    $companiesResponse = Invoke-RestMethod -Uri $companiesUrl -Method GET -Headers @{"Authorization" = "Bearer $($global:authToken)"}
    Write-Host "✅ Total de empresas: $($companiesResponse.total)" -ForegroundColor Green
    Write-Host "Empresas na página: $($companiesResponse.companies.Count)" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao consultar empresas: $($_.Exception.Message)" -ForegroundColor Red
}
