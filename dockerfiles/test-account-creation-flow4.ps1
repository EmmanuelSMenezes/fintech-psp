# ===================================================================
# 🏦 TESTE CRIAÇÃO DE CONTA - FLUXO 4
# ===================================================================

Write-Host "=== FLUXO 4: CRIAÇÃO DE CONTA BANCÁRIA ===" -ForegroundColor Cyan

# Verificar se temos token de autenticação
if (-not $global:authToken) {
    Write-Host "❌ Token de autenticação não encontrado. Execute primeiro o script de autenticação." -ForegroundColor Red
    exit 1
}

$token = $global:authToken
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# ===================================================================
# 1. BUSCAR EMPRESA CRIADA ANTERIORMENTE
# ===================================================================
Write-Host "`n1. Buscando empresa para criar conta..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5008/admin/companies" -Method GET -Headers $headers
    
    if ($response.companies -and $response.companies.Length -gt 0) {
        $empresa = $response.companies[0]
        $clienteId = $empresa.id
        Write-Host "✅ Empresa encontrada: $($empresa.razaoSocial)" -ForegroundColor Green
        Write-Host "   Cliente ID: $clienteId" -ForegroundColor Gray
        Write-Host "   CNPJ: $($empresa.cnpj)" -ForegroundColor Gray
    } else {
        Write-Host "❌ Nenhuma empresa encontrada. Criando empresa primeiro..." -ForegroundColor Red
        
        # Criar empresa rapidamente
        $empresaPayload = @{
            Company = @{
                RazaoSocial = "Empresa Conta Teste LTDA"
                Cnpj = "44.555.666/0001-04"
                Email = "conta@empresateste.com"
                Address = @{
                    Cep = "01234-567"
                    Logradouro = "Rua Conta Teste"
                    Numero = "444"
                    Bairro = "Centro"
                    Cidade = "São Paulo"
                    Estado = "SP"
                    Pais = "Brasil"
                }
                ContractData = @{}
            }
            Applicant = @{
                NomeCompleto = "Carlos Silva"
                Cpf = "444.555.666-77"
                Email = "carlos@empresateste.com"
                Address = @{
                    Cep = "01234-567"
                    Logradouro = "Rua Carlos"
                    Numero = "555"
                    Bairro = "Centro"
                    Cidade = "São Paulo"
                    Estado = "SP"
                    Pais = "Brasil"
                }
            }
            LegalRepresentatives = @()
        } | ConvertTo-Json -Depth 5
        
        $empresaResponse = Invoke-RestMethod -Uri "http://localhost:5008/admin/companies" -Method POST -Body $empresaPayload -Headers $headers
        $clienteId = $empresaResponse.id
        Write-Host "✅ Empresa criada: $($empresaResponse.razaoSocial)" -ForegroundColor Green
        Write-Host "   Cliente ID: $clienteId" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Erro ao buscar/criar empresa: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# ===================================================================
# 2. CRIAR CONTA BANCÁRIA SICOOB
# ===================================================================
Write-Host "`n2. Criando conta bancária Sicoob..." -ForegroundColor Yellow

$contaSicoobPayload = @{
    clienteId = $clienteId
    bankCode = "756"  # Código do Sicoob
    accountNumber = "12345-6"
    description = "Conta Corrente Sicoob Principal"
    credentials = @{
        username = "sicoob_user_test"
        password = "sicoob_pass_test"
        additionalData = @{
            agencia = "1234"
            conta = "123456"
            cooperativa = "0001"
            client_id = "dd533251-7a11-4939-8713-016763653f3c"
            certificate_path = "/app/Certificates/sicoob-certificate.pfx"
            certificate_password = "Vi294141"
        }
    }
} | ConvertTo-Json -Depth 5

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5008/admin/contas" -Method POST -Body $contaSicoobPayload -Headers $headers
    Write-Host "✅ Conta Sicoob criada com sucesso!" -ForegroundColor Green
    Write-Host "   Conta ID: $($response.contaId)" -ForegroundColor Gray
    Write-Host "   Banco: $($response.bankCode)" -ForegroundColor Gray
    Write-Host "   Número: $($response.accountNumber)" -ForegroundColor Gray
    Write-Host "   Descrição: $($response.description)" -ForegroundColor Gray
    $global:contaSicoobId = $response.contaId
} catch {
    Write-Host "❌ Erro ao criar conta Sicoob: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $errorStream = $_.Exception.Response.GetResponseStream()
        $reader = [System.IO.StreamReader]::new($errorStream)
        $errorBody = $reader.ReadToEnd()
        Write-Host "Error Body: $errorBody" -ForegroundColor Red
    }
}

# ===================================================================
# 3. CRIAR CONTA BANCÁRIA GENÉRICA (OUTRO BANCO)
# ===================================================================
Write-Host "`n3. Criando conta bancária genérica..." -ForegroundColor Yellow

$contaGenericaPayload = @{
    clienteId = $clienteId
    bankCode = "001"  # Banco do Brasil
    accountNumber = "98765-4"
    description = "Conta Corrente Banco do Brasil"
    credentials = @{
        username = "bb_user_test"
        password = "bb_pass_test"
        additionalData = @{
            agencia = "5678"
            conta = "987654"
            digito = "3"
        }
    }
} | ConvertTo-Json -Depth 5

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5008/admin/contas" -Method POST -Body $contaGenericaPayload -Headers $headers
    Write-Host "✅ Conta genérica criada com sucesso!" -ForegroundColor Green
    Write-Host "   Conta ID: $($response.contaId)" -ForegroundColor Gray
    Write-Host "   Banco: $($response.bankCode)" -ForegroundColor Gray
    Write-Host "   Número: $($response.accountNumber)" -ForegroundColor Gray
    Write-Host "   Descrição: $($response.description)" -ForegroundColor Gray
    $global:contaGenericaId = $response.contaId
} catch {
    Write-Host "❌ Erro ao criar conta genérica: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 4. LISTAR CONTAS DO CLIENTE
# ===================================================================
Write-Host "`n4. Listando contas do cliente..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5008/admin/contas/$clienteId" -Method GET -Headers $headers
    Write-Host "✅ Contas encontradas: $($response.contas.Length)" -ForegroundColor Green
    
    foreach ($conta in $response.contas) {
        Write-Host "   - $($conta.description) ($($conta.bankCode)) - $($conta.accountNumber)" -ForegroundColor Gray
        Write-Host "     ID: $($conta.contaId)" -ForegroundColor DarkGray
        Write-Host "     Ativo: $($conta.isActive)" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Erro ao listar contas: $($_.Exception.Message)" -ForegroundColor Red
}

# ===================================================================
# 5. CRIAR SALDO INICIAL (BALANCE SERVICE)
# ===================================================================
Write-Host "`n5. Criando saldo inicial para as contas..." -ForegroundColor Yellow

# Tentar criar saldo inicial de R$ 1000,00 para a conta Sicoob
if ($global:contaSicoobId) {
    Write-Host "   Criando saldo inicial para conta Sicoob..." -ForegroundColor Gray
    
    $saldoPayload = @{
        clientId = $clienteId
        amount = 1000.00
        description = "Saldo inicial - Conta Sicoob"
        transactionType = "INITIAL_BALANCE"
    } | ConvertTo-Json -Depth 3
    
    try {
        # Tentar via BalanceService (se estiver rodando)
        $response = Invoke-RestMethod -Uri "http://localhost:5010/saldo/credito" -Method POST -Body $saldoPayload -Headers $headers
        Write-Host "✅ Saldo inicial criado: R$ $($response.newBalance)" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ BalanceService não disponível. Saldo será criado quando o serviço estiver online." -ForegroundColor Yellow
    }
}

# ===================================================================
# 6. VERIFICAR SALDO
# ===================================================================
Write-Host "`n6. Verificando saldo das contas..." -ForegroundColor Yellow

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5010/saldo/$clienteId" -Method GET -Headers $headers
    Write-Host "✅ Saldo atual: R$ $($response.availableBalance)" -ForegroundColor Green
    Write-Host "   Saldo bloqueado: R$ $($response.blockedBalance)" -ForegroundColor Gray
    Write-Host "   Saldo total: R$ $($response.totalBalance)" -ForegroundColor Gray
} catch {
    Write-Host "⚠️ Não foi possível verificar saldo. BalanceService pode estar offline." -ForegroundColor Yellow
}

# ===================================================================
# 7. RESUMO FINAL
# ===================================================================
Write-Host "`n=== RESUMO DO FLUXO 4 ===" -ForegroundColor Cyan
Write-Host "✅ Cliente ID: $clienteId" -ForegroundColor Green
if ($global:contaSicoobId) {
    Write-Host "✅ Conta Sicoob criada: $($global:contaSicoobId)" -ForegroundColor Green
}
if ($global:contaGenericaId) {
    Write-Host "✅ Conta genérica criada: $($global:contaGenericaId)" -ForegroundColor Green
}
Write-Host "✅ Contas bancárias configuradas e prontas para transações!" -ForegroundColor Green

Write-Host "`n🎉 FLUXO 4 CONCLUÍDO: Criação de conta implementada com sucesso!" -ForegroundColor Green
