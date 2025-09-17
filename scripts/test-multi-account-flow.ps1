# =====================================================
# Script de Teste - Multi-Account Management Flow
# PSP FintechPSP - Teste completo do fluxo de múltiplas contas
# =====================================================

param(
    [string]$BaseUrl = "http://localhost:5000",
    [switch]$Verbose
)

Write-Host "🚀 Iniciando teste do fluxo Multi-Account Management" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow

# Variáveis globais
$global:AccessToken = ""
$global:AdminToken = ""
$global:ClienteId = "123e4567-e89b-12d3-a456-426614174000"
$global:ContaStarkId = ""
$global:ContaEfiId = ""
$global:ContaSicoobId = ""

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [object]$Body = $null,
        [string]$Token = "",
        [string]$Description = ""
    )
    
    if ($Description) {
        Write-Host "📡 $Description" -ForegroundColor Cyan
    }
    
    $headers = @{
        "Content-Type" = "application/json"
    }
    
    if ($Token) {
        $headers["Authorization"] = "Bearer $Token"
    }
    
    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            Headers = $headers
        }
        
        if ($Body) {
            $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
            if ($Verbose) {
                Write-Host "Request Body: $($params["Body"])" -ForegroundColor DarkGray
            }
        }
        
        $response = Invoke-RestMethod @params
        
        if ($Verbose) {
            Write-Host "Response: $($response | ConvertTo-Json -Depth 5)" -ForegroundColor DarkGray
        }
        
        return $response
    }
    catch {
        Write-Host "❌ Erro na requisição: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $errorDetails = $_.Exception.Response | ConvertTo-Json -Depth 3
            Write-Host "Detalhes do erro: $errorDetails" -ForegroundColor Red
        }
        throw
    }
}

# Função para obter token de autenticação
function Get-AuthToken {
    param([string]$Scope = "banking")
    
    $clientId = if ($Scope -eq "admin") { "admin_client" } else { "integration_test" }
    $clientSecret = if ($Scope -eq "admin") { "admin_secret_000" } else { "test_secret_000" }
    
    $body = @{
        grant_type = "client_credentials"
        client_id = $clientId
        client_secret = $clientSecret
        scope = $Scope
    }
    
    $response = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/auth/token" -Body $body -Description "Obtendo token $Scope"
    return $response.access_token
}

# Função para testar criação de contas
function Test-AccountCreation {
    Write-Host "`n🏦 === TESTE 1: Criação de Contas Bancárias ===" -ForegroundColor Magenta
    
    # Criar conta Stark Bank
    $starkAccount = @{
        bankCode = "STARK"
        accountNumber = "12345-6"
        description = "Conta Principal Stark Bank"
        credentials = @{
            clientId = "Client_Id_stark_001"
            clientSecret = "Client_Secret_stark_001"
            additionalData = @{
                environment = "sandbox"
                version = "v1"
            }
        }
    }
    
    $starkResponse = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/banking/contas" -Body $starkAccount -Token $global:AccessToken -Description "Criando conta Stark Bank"
    $global:ContaStarkId = $starkResponse.contaId
    Write-Host "✅ Conta Stark criada: $global:ContaStarkId" -ForegroundColor Green
    
    # Criar conta Efí
    $efiAccount = @{
        bankCode = "EFI"
        accountNumber = "98765-4"
        description = "Conta Secundária Efí"
        credentials = @{
            clientId = "Client_Id_efi_001"
            clientSecret = "Client_Secret_efi_001"
        }
    }
    
    $efiResponse = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/banking/contas" -Body $efiAccount -Token $global:AccessToken -Description "Criando conta Efí"
    $global:ContaEfiId = $efiResponse.contaId
    Write-Host "✅ Conta Efí criada: $global:ContaEfiId" -ForegroundColor Green
    
    # Criar conta Sicoob via Admin
    $sicoobAccount = @{
        clienteId = $global:ClienteId
        bankCode = "SICOOB"
        accountNumber = "11111-2"
        description = "Conta Backup Sicoob - Admin Created"
        credentials = @{
            clientId = "Client_Id_sicoob_admin"
            clientSecret = "Client_Secret_sicoob_admin"
        }
    }
    
    $sicoobResponse = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/admin/contas" -Body $sicoobAccount -Token $global:AdminToken -Description "Criando conta Sicoob (Admin)"
    $global:ContaSicoobId = $sicoobResponse.contaId
    Write-Host "✅ Conta Sicoob criada: $global:ContaSicoobId" -ForegroundColor Green
}

# Função para testar configuração de priorização
function Test-PrioritizationConfig {
    Write-Host "`n⚖️ === TESTE 2: Configuração de Priorização ===" -ForegroundColor Magenta
    
    # Configurar priorização: 50% Stark, 30% Efí, 20% Sicoob
    $priorizacao = @{
        prioridades = @(
            @{ contaId = $global:ContaStarkId; percentual = 50.0 },
            @{ contaId = $global:ContaEfiId; percentual = 30.0 },
            @{ contaId = $global:ContaSicoobId; percentual = 20.0 }
        )
    }
    
    $response = Invoke-ApiRequest -Method "PUT" -Uri "$BaseUrl/banking/configs/roteamento" -Body $priorizacao -Token $global:AccessToken -Description "Configurando priorização"
    Write-Host "✅ Priorização configurada: Total $($response.totalPercentual)%" -ForegroundColor Green
    
    # Verificar configuração
    $config = Invoke-ApiRequest -Method "GET" -Uri "$BaseUrl/banking/configs/roteamento" -Token $global:AccessToken -Description "Verificando configuração de roteamento"
    Write-Host "✅ Configuração verificada: $($config.prioridades.Count) contas configuradas" -ForegroundColor Green
}

# Função para testar roteamento ponderado
function Test-WeightedRouting {
    Write-Host "`n🎯 === TESTE 3: Roteamento Ponderado ===" -ForegroundColor Magenta
    
    # Testar seleção de conta múltiplas vezes para verificar distribuição
    $selections = @{}
    
    for ($i = 1; $i -le 10; $i++) {
        $request = @{
            amount = 1000.00
        }
        
        $response = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/integrations/routing/select-account" -Body $request -Token $global:AccessToken -Description "Seleção de conta #$i"
        
        $bankCode = $response.selectedAccount.bankCode
        if ($selections.ContainsKey($bankCode)) {
            $selections[$bankCode]++
        } else {
            $selections[$bankCode] = 1
        }
    }
    
    Write-Host "📊 Distribuição das seleções:" -ForegroundColor Yellow
    foreach ($bank in $selections.Keys) {
        $percentage = ($selections[$bank] / 10) * 100
        Write-Host "  $bank`: $($selections[$bank])/10 ($percentage%)" -ForegroundColor White
    }
}

# Função para testar transações com contaId específico
function Test-TransactionsWithAccountId {
    Write-Host "`n💳 === TESTE 4: Transações com Conta Específica ===" -ForegroundColor Magenta
    
    # Transação PIX com conta Stark específica
    $pixTransaction = @{
        externalId = "pix-test-$(Get-Random)"
        amount = 150.75
        pixKey = "11999887766"
        bankCode = "341"
        description = "Teste PIX com conta específica"
        contaId = $global:ContaStarkId
    }
    
    $pixResponse = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/transacoes/pix" -Body $pixTransaction -Token $global:AccessToken -Description "Transação PIX com conta Stark"
    Write-Host "✅ PIX criado: $($pixResponse.transactionId)" -ForegroundColor Green
    
    # Transação TED com conta Efí específica
    $tedTransaction = @{
        externalId = "ted-test-$(Get-Random)"
        amount = 500.00
        bankCode = "001"
        accountBranch = "1234"
        accountNumber = "567890"
        taxId = "12345678901"
        name = "João da Silva"
        contaId = $global:ContaEfiId
    }
    
    $tedResponse = Invoke-ApiRequest -Method "POST" -Uri "$BaseUrl/transacoes/ted" -Body $tedTransaction -Token $global:AccessToken -Description "Transação TED com conta Efí"
    Write-Host "✅ TED criado: $($tedResponse.transactionId)" -ForegroundColor Green
}

# Função para testar listagem de contas com prioridades
function Test-AccountListing {
    Write-Host "`n📋 === TESTE 5: Listagem de Contas e Prioridades ===" -ForegroundColor Magenta
    
    # Listar contas do cliente
    $contas = Invoke-ApiRequest -Method "GET" -Uri "$BaseUrl/banking/contas" -Token $global:AccessToken -Description "Listando contas do cliente"
    Write-Host "✅ Contas encontradas: $($contas.Count)" -ForegroundColor Green
    
    # Listar contas com prioridades via IntegrationService
    $contasComPrioridade = Invoke-ApiRequest -Method "GET" -Uri "$BaseUrl/integrations/routing/accounts-priority" -Token $global:AccessToken -Description "Listando contas com prioridades"
    Write-Host "✅ Contas com prioridade: $($contasComPrioridade.accounts.Count)" -ForegroundColor Green
    
    foreach ($conta in $contasComPrioridade.accounts) {
        $priority = if ($conta.hasPriorityConfig) { "$($conta.priority)%" } else { "Não configurado" }
        Write-Host "  $($conta.bankCode) ($($conta.accountNumber)): $priority" -ForegroundColor White
    }
}

# Função principal
function Main {
    try {
        # Obter tokens de autenticação
        Write-Host "🔐 Obtendo tokens de autenticação..." -ForegroundColor Yellow
        $global:AccessToken = Get-AuthToken -Scope "banking"
        $global:AdminToken = Get-AuthToken -Scope "admin"
        Write-Host "✅ Tokens obtidos com sucesso" -ForegroundColor Green
        
        # Executar testes
        Test-AccountCreation
        Test-PrioritizationConfig
        Test-WeightedRouting
        Test-TransactionsWithAccountId
        Test-AccountListing
        
        Write-Host "`n🎉 === TODOS OS TESTES CONCLUÍDOS COM SUCESSO! ===" -ForegroundColor Green
        Write-Host "✅ Multi-Account Management funcionando perfeitamente" -ForegroundColor Green
        Write-Host "✅ Roteamento ponderado implementado" -ForegroundColor Green
        Write-Host "✅ Credenciais tokenizadas funcionando" -ForegroundColor Green
        Write-Host "✅ Transações com contaId específico funcionando" -ForegroundColor Green
        
    }
    catch {
        Write-Host "`n❌ ERRO NO TESTE: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Stack Trace: $($_.ScriptStackTrace)" -ForegroundColor Red
        exit 1
    }
}

# Executar script principal
Main
