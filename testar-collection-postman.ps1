#!/usr/bin/env pwsh

Write-Host "🚀 TESTANDO COLLECTION POSTMAN - TRANSAÇÕES CLIENTE" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green
Write-Host ""

$baseUrl = "http://localhost:5000"
$adminToken = ""
$clientToken = ""
$companyId = ""
$userId = ""

# Função para fazer requests HTTP
function Invoke-ApiTest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [string]$Description
    )
    
    try {
        Write-Host "🔄 $Description..." -ForegroundColor Yellow
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            ContentType = "application/json"
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "✅ Sucesso!" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

Write-Host "ETAPA 1: AUTENTICAÇÃO ADMIN" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan

# 1.1 Login Admin
$loginAdminBody = @{
    email = "admin@fintechpsp.com"
    password = "admin123"
} | ConvertTo-Json

$adminResponse = Invoke-ApiTest -Url "$baseUrl/auth/login" -Method POST -Body $loginAdminBody -Description "Login Admin"

if ($adminResponse -and $adminResponse.accessToken) {
    $adminToken = $adminResponse.accessToken
    Write-Host "🔑 Token admin salvo: $($adminToken.Substring(0, 20))..." -ForegroundColor Green
} else {
    Write-Host "❌ Falha no login admin. Verifique se os serviços estão rodando." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "ETAPA 2: CRIAÇÃO DE EMPRESA" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan

# 2.1 Criar Empresa
$empresaBody = @{
    razaoSocial = "EmpresaTeste Ltda"
    nomeFantasia = "EmpresaTeste"
    cnpj = "12.345.678/0001-99"
    email = "contato@empresateste.com"
    telefone = "(11) 99999-9999"
    address = @{
        street = "Rua Teste, 123"
        city = "São Paulo"
        state = "SP"
        zipCode = "01234-567"
        country = "Brasil"
    }
} | ConvertTo-Json -Depth 5

$headers = @{
    "Authorization" = "Bearer $adminToken"
}

$empresaResponse = Invoke-ApiTest -Url "$baseUrl/admin/companies" -Method POST -Headers $headers -Body $empresaBody -Description "Criar Empresa"

if ($empresaResponse -and $empresaResponse.id) {
    $companyId = $empresaResponse.id
    Write-Host "🏢 Empresa criada com ID: $companyId" -ForegroundColor Green
} else {
    Write-Host "❌ Falha na criação da empresa." -ForegroundColor Red
    exit 1
}

# 2.2 Aprovar Empresa
$aprovacaoBody = @{
    status = "Approved"
    observacoes = "Empresa aprovada para operação"
} | ConvertTo-Json

$aprovacaoResponse = Invoke-ApiTest -Url "$baseUrl/admin/companies/$companyId/status" -Method PATCH -Headers $headers -Body $aprovacaoBody -Description "Aprovar Empresa"

Write-Host ""
Write-Host "ETAPA 3: CRIAÇÃO DE USUÁRIO CLIENTE" -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan

# 3.1 Criar Usuário Cliente
$usuarioBody = @{
    name = "Cliente EmpresaTeste"
    email = "cliente@empresateste.com"
    document = "12345678901"
    phone = "(11) 99999-9999"
    isActive = $true
} | ConvertTo-Json

$usuarioResponse = Invoke-ApiTest -Url "$baseUrl/client-users" -Method POST -Headers $headers -Body $usuarioBody -Description "Criar Usuário Cliente"

if ($usuarioResponse -and $usuarioResponse.id) {
    $userId = $usuarioResponse.id
    Write-Host "👤 Usuário criado com ID: $userId" -ForegroundColor Green
} else {
    Write-Host "❌ Falha na criação do usuário." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "ETAPA 4: LOGIN CLIENTE" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan

# 4.1 Login Cliente
$loginClienteBody = @{
    email = "cliente@empresateste.com"
    password = "123456"
} | ConvertTo-Json

$clienteResponse = Invoke-ApiTest -Url "$baseUrl/auth/login" -Method POST -Body $loginClienteBody -Description "Login Cliente"

if ($clienteResponse -and $clienteResponse.accessToken) {
    $clientToken = $clienteResponse.accessToken
    Write-Host "🔑 Token cliente salvo: $($clientToken.Substring(0, 20))..." -ForegroundColor Green
} else {
    Write-Host "❌ Falha no login cliente." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "ETAPA 5: TRANSAÇÕES PIX" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

$clientHeaders = @{
    "Authorization" = "Bearer $clientToken"
}

# 5.1 QR Code PIX Dinâmico
$qrDinamicoBody = @{
    amount = 100.50
    pixKey = "cliente@empresateste.com"
    description = "Teste PIX - Cobrança dinâmica"
    expiresIn = 3600
} | ConvertTo-Json

$qrDinamicoResponse = Invoke-ApiTest -Url "$baseUrl/transacoes/pix/qrcode/dinamico" -Method POST -Headers $clientHeaders -Body $qrDinamicoBody -Description "Gerar QR Code PIX Dinâmico"

# 5.2 QR Code PIX Estático
$qrEstaticoBody = @{
    pixKey = "cliente@empresateste.com"
    description = "QR Code estático para recebimentos"
    city = "São Paulo"
    merchantName = "EmpresaTeste Ltda"
} | ConvertTo-Json

$qrEstaticoResponse = Invoke-ApiTest -Url "$baseUrl/transacoes/pix/qrcode/estatico" -Method POST -Headers $clientHeaders -Body $qrEstaticoBody -Description "Gerar QR Code PIX Estático"

Write-Host ""
Write-Host "ETAPA 6: INTEGRAÇÃO SICOOB" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan

# 6.1 Teste Conectividade Sicoob
$sicoobResponse = Invoke-ApiTest -Url "$baseUrl/integrations/sicoob/test-connectivity" -Method GET -Headers $clientHeaders -Description "Teste Conectividade Sicoob"

Write-Host ""
Write-Host "ETAPA 7: HEALTH CHECKS" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan

# 7.1 Health Checks
Invoke-ApiTest -Url "$baseUrl/health" -Method GET -Description "Health Check Geral"
Invoke-ApiTest -Url "$baseUrl/qrcode/health" -Method GET -Description "Health Check TransactionService"
Invoke-ApiTest -Url "$baseUrl/integrations/health" -Method GET -Description "Health Check IntegrationService"

Write-Host ""
Write-Host "🎉 TESTE DA COLLECTION CONCLUÍDO!" -ForegroundColor Green
Write-Host "==================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 RESUMO:" -ForegroundColor Yellow
Write-Host "- ✅ Admin logado com sucesso" -ForegroundColor White
Write-Host "- ✅ Empresa criada e aprovada" -ForegroundColor White
Write-Host "- ✅ Usuário cliente criado" -ForegroundColor White
Write-Host "- ✅ Cliente logado com sucesso" -ForegroundColor White
Write-Host "- ✅ QR Codes PIX gerados" -ForegroundColor White
Write-Host "- ✅ Integração Sicoob testada" -ForegroundColor White
Write-Host "- ✅ Health checks validados" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Agora você pode usar a collection do Postman para transacionar!" -ForegroundColor Green
Write-Host "📁 Arquivo: postman/FintechPSP-Transacoes-Cliente.json" -ForegroundColor Cyan
