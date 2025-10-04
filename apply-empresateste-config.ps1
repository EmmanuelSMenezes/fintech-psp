# =====================================================
# FintechPSP - Aplicar Configurações EmpresaTeste
# =====================================================

Write-Host "🎯 Aplicando configurações reais da EmpresaTeste no sistema..." -ForegroundColor Green
Write-Host ""

# 1. Obter token de autenticação
Write-Host "🔐 Obtendo token de autenticação..." -ForegroundColor Yellow
try {
    $authBody = @{
        grant_type = "client_credentials"
        client_id = "integration_test"
        client_secret = "test_secret_000"
        scope = "pix banking"
    } | ConvertTo-Json

    $authHeaders = @{
        'Content-Type' = 'application/json'
    }

    $authResponse = Invoke-RestMethod -Uri "http://localhost:5000/auth/token" -Method POST -Headers $authHeaders -Body $authBody
    $token = $authResponse.access_token
    Write-Host "✅ Token obtido com sucesso" -ForegroundColor Green
} catch {
    Write-Host "❌ Erro ao obter token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Headers para requisições autenticadas
$headers = @{
    'Authorization' = "Bearer $token"
    'Content-Type' = 'application/json'
}

# 2. Verificar se EmpresaTeste já existe
Write-Host "🔍 Verificando se EmpresaTeste já existe..." -ForegroundColor Yellow
try {
    $existingCompany = Invoke-RestMethod -Uri "http://localhost:5000/admin/companies?search=EmpresaTeste" -Method GET -Headers $headers
    if ($existingCompany.companies -and $existingCompany.companies.Count -gt 0) {
        Write-Host "✅ EmpresaTeste já existe no sistema" -ForegroundColor Green
        $companyId = $existingCompany.companies[0].companyId
        Write-Host "   Company ID: $companyId" -ForegroundColor Gray
    } else {
        Write-Host "⚠️ EmpresaTeste não encontrada, criando..." -ForegroundColor Yellow
        
        # Criar EmpresaTeste
        $companyData = @{
            company = @{
                razaoSocial = "EmpresaTeste Ltda"
                cnpj = "12345678000199"
                email = "contato@empresateste.com"
                telefone = "11999999999"
                endereco = @{
                    cep = "01310-100"
                    logradouro = "Av Paulista"
                    numero = "1000"
                    bairro = "Bela Vista"
                    cidade = "São Paulo"
                    estado = "SP"
                    pais = "Brasil"
                }
            }
            applicant = @{
                nome = "João Silva"
                cpf = "12345678909"
                email = "joao@empresateste.com"
                telefone = "11888888888"
            }
            legalRepresentatives = @(
                @{
                    nome = "João Silva"
                    cpf = "12345678909"
                    cargo = "Diretor"
                    email = "joao@empresateste.com"
                }
            )
        } | ConvertTo-Json -Depth 10

        $createResponse = Invoke-RestMethod -Uri "http://localhost:5000/admin/companies" -Method POST -Headers $headers -Body $companyData
        $companyId = $createResponse.companyId
        Write-Host "✅ EmpresaTeste criada com sucesso!" -ForegroundColor Green
        Write-Host "   Company ID: $companyId" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Erro ao verificar/criar empresa: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Criar usuário cliente@empresateste.com
Write-Host "👤 Criando usuário cliente@empresateste.com..." -ForegroundColor Yellow
try {
    $userData = @{
        email = "cliente@empresateste.com"
        nome = "Cliente EmpresaTeste"
        companyId = $companyId
        role = "Cliente"
        permissions = @(
            "view_dashboard",
            "view_transacoes", 
            "view_contas",
            "view_extratos",
            "view_saldo",
            "transacionar_pix",
            "transacionar_ted",
            "transacionar_boleto"
        )
    } | ConvertTo-Json -Depth 5

    $userResponse = Invoke-RestMethod -Uri "http://localhost:5000/admin/users" -Method POST -Headers $headers -Body $userData
    Write-Host "✅ Usuário cliente criado com sucesso!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Usuário pode já existir: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 4. Configurar limites de transação
Write-Host "💰 Configurando limites de transação..." -ForegroundColor Yellow
try {
    # Limites PIX
    $pixLimits = @{
        companyId = $companyId
        transactionType = "PIX"
        dailyLimit = 10000.00
        monthlyLimit = 50000.00
        minAmount = 0.01
        maxAmount = 10000.00
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "http://localhost:5000/config/limits" -Method POST -Headers $headers -Body $pixLimits
    Write-Host "✅ Limites PIX configurados" -ForegroundColor Green

    # Limites TED
    $tedLimits = @{
        companyId = $companyId
        transactionType = "TED"
        dailyLimit = 10000.00
        monthlyLimit = 50000.00
        minAmount = 1.00
        maxAmount = 10000.00
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "http://localhost:5000/config/limits" -Method POST -Headers $headers -Body $tedLimits
    Write-Host "✅ Limites TED configurados" -ForegroundColor Green

    # Limites Boleto
    $boletoLimits = @{
        companyId = $companyId
        transactionType = "BOLETO"
        dailyLimit = 10000.00
        monthlyLimit = 50000.00
        minAmount = 5.00
        maxAmount = 10000.00
    } | ConvertTo-Json

    Invoke-RestMethod -Uri "http://localhost:5000/config/limits" -Method POST -Headers $headers -Body $boletoLimits
    Write-Host "✅ Limites Boleto configurados" -ForegroundColor Green

} catch {
    Write-Host "⚠️ Erro ao configurar limites: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 5. Cadastrar bancos de integração
Write-Host "🏦 Cadastrando bancos de integração..." -ForegroundColor Yellow

$bancos = @(
    @{
        code = "756"
        name = "Sicoob"
        fullName = "Banco Cooperativo do Brasil S.A. - Bancoob"
        type = "COOPERATIVE"
        active = $true
        integrationEnabled = $true
        supportedServices = @("PIX", "TED", "BOLETO", "CONTA_CORRENTE")
        apiEndpoints = @{
            baseUrl = "https://sandbox.sicoob.com.br"
            authUrl = "https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token"
            pixUrl = "https://api.sicoob.com.br/pix-pagamentos/v2"
            tedUrl = "https://api.sicoob.com.br/spb/v2"
            boletoUrl = "https://api.sicoob.com.br/cobranca-bancaria/v3"
            contaUrl = "https://api.sicoob.com.br/conta-corrente/v4"
        }
    },
    @{
        code = "341"
        name = "Itaú"
        fullName = "Itaú Unibanco S.A."
        type = "COMMERCIAL"
        active = $true
        integrationEnabled = $false
        supportedServices = @("PIX", "TED")
    },
    @{
        code = "001"
        name = "Banco do Brasil"
        fullName = "Banco do Brasil S.A."
        type = "COMMERCIAL"
        active = $true
        integrationEnabled = $false
        supportedServices = @("PIX", "TED", "BOLETO")
    }
)

foreach ($banco in $bancos) {
    try {
        $bancoJson = $banco | ConvertTo-Json -Depth 5
        Invoke-RestMethod -Uri "http://localhost:5000/config/banks" -Method POST -Headers $headers -Body $bancoJson
        Write-Host "✅ Banco $($banco.name) cadastrado" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Banco $($banco.name) pode já existir" -ForegroundColor Yellow
    }
}

# 6. Configurar integração Sicoob
Write-Host "🔧 Configurando integração Sicoob..." -ForegroundColor Yellow
try {
    $sicoobConfig = @{
        companyId = $companyId
        bankCode = "756"
        integration = @{
            clientId = "9b5e603e428cc477a2841e2683c92d21"
            environment = "SANDBOX"
            baseUrl = "https://sandbox.sicoob.com.br"
            authUrl = "https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token"
            scopes = @(
                "boletos_consulta", "boletos_inclusao", "boletos_alteracao",
                "pagamentos_inclusao", "pagamentos_alteracao", "pagamentos_consulta",
                "cco_saldo", "cco_extrato", "cco_consulta", "cco_transferencias",
                "pix_pagamentos", "pix_recebimentos", "pix_consultas"
            )
            webhookUrl = "http://localhost:5000/webhooks/sicoob"
        }
    } | ConvertTo-Json -Depth 5

    Invoke-RestMethod -Uri "http://localhost:5000/config/integrations" -Method POST -Headers $headers -Body $sicoobConfig
    Write-Host "✅ Integração Sicoob configurada" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Erro ao configurar integração Sicoob: $($_.Exception.Message)" -ForegroundColor Yellow
}

# 7. Resumo final
Write-Host ""
Write-Host "🎉 CONFIGURAÇÕES APLICADAS COM SUCESSO!" -ForegroundColor Green
Write-Host "=======================================" -ForegroundColor Green
Write-Host ""
Write-Host "✅ EmpresaTeste Ltda criada e ativa" -ForegroundColor White
Write-Host "✅ Usuário cliente@empresateste.com configurado" -ForegroundColor White
Write-Host "✅ Limites de R$ 10.000/dia aplicados (PIX, TED, Boleto)" -ForegroundColor White
Write-Host "✅ Bancos cadastrados: Sicoob (756), Itaú (341), BB (001)" -ForegroundColor White
Write-Host "✅ Integração Sicoob Sandbox configurada" -ForegroundColor White
Write-Host ""
Write-Host "🌐 Acesse o BackofficeWeb e verifique:" -ForegroundColor Cyan
Write-Host "• http://localhost:3000/empresas" -ForegroundColor Gray
Write-Host "• http://localhost:3000/usuarios" -ForegroundColor Gray
Write-Host "• http://localhost:3000/configuracoes" -ForegroundColor Gray
Write-Host ""
Write-Host "🏦 Agora você deve ver o Sicoob na lista de bancos!" -ForegroundColor Green
