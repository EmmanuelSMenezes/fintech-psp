# Teste simples de criação de empresa
Write-Host "=== TESTE CRIAÇÃO DE EMPRESA ===" -ForegroundColor Cyan

# Token admin
$token = $global:authToken
if (-not $token) {
    Write-Host "Obtendo token..." -ForegroundColor Yellow
    $loginBody = '{"email":"admin@fintechpsp.com","password":"admin123"}'
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:5001/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.accessToken
    Write-Host "Token obtido!" -ForegroundColor Green
}

# Headers
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Payload correto
$payload = @{
    Company = @{
        RazaoSocial = "Empresa Teste Simples LTDA"
        Cnpj = "11.222.333/0001-81"
        Email = "contato@testesimples.com"
        Address = @{
            Cep = "01234-567"
            Logradouro = "Rua Teste"
            Numero = "123"
            Bairro = "Centro"
            Cidade = "São Paulo"
            Estado = "SP"
            Pais = "Brasil"
        }
        ContractData = @{}
    }
    Applicant = @{
        NomeCompleto = "João Silva"
        Cpf = "123.456.789-00"
        Email = "joao@testesimples.com"
        Address = @{
            Cep = "01234-567"
            Logradouro = "Rua Applicant"
            Numero = "100"
            Bairro = "Centro"
            Cidade = "São Paulo"
            Estado = "SP"
            Pais = "Brasil"
        }
    }
    LegalRepresentatives = @()
} | ConvertTo-Json -Depth 5

Write-Host "Enviando requisição..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5010/admin/companies" -Method POST -Body $payload -Headers $headers
    Write-Host "✅ SUCESSO!" -ForegroundColor Green
    Write-Host "ID: $($response.id)" -ForegroundColor Green
    Write-Host "Razão Social: $($response.razaoSocial)" -ForegroundColor Green
    Write-Host "CNPJ: $($response.cnpj)" -ForegroundColor Green
    Write-Host "Status: $($response.status)" -ForegroundColor Green
} catch {
    Write-Host "❌ ERRO: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($stream)
        $errorBody = $reader.ReadToEnd()
        Write-Host "Detalhes: $errorBody" -ForegroundColor Red
    }
}
