# ========================================
# Executor Simples da Trilha PSP-Sicoob
# ========================================

param(
    [string]$BaseUrl = "http://localhost:5000",
    [int]$EtapaInicial = 1,
    [int]$EtapaFinal = 6
)

$ErrorActionPreference = "Continue"

# Dados de teste
$EmpresaTeste = @{
    razaoSocial = "EmpresaTeste Ltda"
    nomeFantasia = "EmpresaTeste"
    cnpj = "12345678000199"
    email = "contato@empresateste.com"
    telefone = "(11) 99999-9999"
}

$UsuarioTeste = @{
    email = "cliente@empresateste.com"
    name = "Gerente EmpresaTeste"
    role = "COMPANY_ADMIN"
}

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

function Invoke-ApiRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [string]$Description = ""
    )
    
    try {
        Write-Host "  -> $Description" -ForegroundColor Yellow
        Write-Host "     $Method $Url" -ForegroundColor Gray
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 30
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "  OK Sucesso" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "  ERRO: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

Write-Host "EXECUTANDO TRILHA INTEGRADA PSP-SICOOB" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green
Write-Host "Etapas: $EtapaInicial a $EtapaFinal" -ForegroundColor Cyan
Write-Host ""

# ETAPA 1: CADASTRO DA EMPRESA
if ($EtapaInicial -le 1 -and $EtapaFinal -ge 1) {
    Write-Host ""
    Write-Host "ETAPA 1: CADASTRO DO CLIENTE (EMPRESA)" -ForegroundColor Magenta
    Write-Host "======================================" -ForegroundColor Magenta
    
    $empresaRequest = $EmpresaTeste | ConvertTo-Json -Depth 10
    $empresaResponse = Invoke-ApiRequest -Url "$BaseUrl/companies" -Method POST -Headers $headers -Body $empresaRequest -Description "Criando empresa"
    
    if ($empresaResponse) {
        $global:EmpresaId = $empresaResponse.id
        Write-Host "  Empresa criada com ID: $global:EmpresaId" -ForegroundColor Green
        
        # Validar CNPJ via Receita Federal
        $cnpjRequest = @{ cnpj = $EmpresaTeste.cnpj } | ConvertTo-Json
        $cnpjResponse = Invoke-ApiRequest -Url "$BaseUrl/integrations/receita-federal/cnpj/validate" -Method POST -Headers $headers -Body $cnpjRequest -Description "Validando CNPJ"
        
        if ($cnpjResponse -and $cnpjResponse.isValid) {
            Write-Host "  CNPJ validado: $($cnpjResponse.companyName)" -ForegroundColor Green
        }
        
        Write-Host "ETAPA 1 CONCLUIDA - Habilita Etapa 2" -ForegroundColor Green
    }
}

# ETAPA 2: GERACAO DE USUARIO
if ($EtapaInicial -le 2 -and $EtapaFinal -ge 2) {
    Write-Host ""
    Write-Host "ETAPA 2: GERACAO DE USUARIO PARA O CLIENTE" -ForegroundColor Magenta
    Write-Host "==========================================" -ForegroundColor Magenta
    
    if (-not $global:EmpresaId) {
        Write-Host "  EmpresaId nao disponivel. Execute Etapa 1 primeiro." -ForegroundColor Yellow
    } else {
        # Aprovar empresa
        $aprovacaoRequest = @{
            status = "Approved"
            approvedBy = "admin@fintech.com"
            approvalNotes = "Documentacao validada - Teste automatizado"
        } | ConvertTo-Json
        
        $aprovacaoResponse = Invoke-ApiRequest -Url "$BaseUrl/companies/$global:EmpresaId/status" -Method PUT -Headers $headers -Body $aprovacaoRequest -Description "Aprovando empresa"
        
        # Criar usuario
        $usuarioData = $UsuarioTeste.Clone()
        $usuarioData.companyId = $global:EmpresaId
        $usuarioRequest = $usuarioData | ConvertTo-Json -Depth 10
        
        $usuarioResponse = Invoke-ApiRequest -Url "$BaseUrl/users" -Method POST -Headers $headers -Body $usuarioRequest -Description "Criando usuario"
        
        if ($usuarioResponse) {
            $global:UsuarioId = $usuarioResponse.id
            Write-Host "  Usuario criado com ID: $global:UsuarioId" -ForegroundColor Green
            Write-Host "ETAPA 2 CONCLUIDA - Habilita Etapa 3" -ForegroundColor Green
        }
    }
}

# ETAPA 3: CONFIGURACAO INICIAL
if ($EtapaInicial -le 3 -and $EtapaFinal -ge 3) {
    Write-Host ""
    Write-Host "ETAPA 3: CONFIGURACAO INICIAL" -ForegroundColor Magenta
    Write-Host "=============================" -ForegroundColor Magenta
    
    # Configurar limites
    $limitesRequest = @{
        dailyLimits = @{
            pix = 10000.00
            ted = 50000.00
            boleto = 25000.00
        }
        monthlyLimits = @{
            pix = 100000.00
            ted = 500000.00
            boleto = 250000.00
        }
    } | ConvertTo-Json -Depth 10
    
    $limitesResponse = Invoke-ApiRequest -Url "$BaseUrl/config/limits" -Method POST -Headers $headers -Body $limitesRequest -Description "Configurando limites"
    
    # Testar conectividade Sicoob
    $conectividadeResponse = Invoke-ApiRequest -Url "$BaseUrl/integrations/sicoob/test-connectivity" -Method GET -Headers $headers -Description "Testando conectividade Sicoob"
    
    if ($conectividadeResponse) {
        Write-Host "  Conectividade Sicoob: $($conectividadeResponse.status)" -ForegroundColor Green
        Write-Host "ETAPA 3 CONCLUIDA - Habilita Etapa 4" -ForegroundColor Green
    }
}

# ETAPA 4: CRIACAO DE CONTA
if ($EtapaInicial -le 4 -and $EtapaFinal -ge 4) {
    Write-Host ""
    Write-Host "ETAPA 4: CRIACAO E ATIVACAO DE CONTA" -ForegroundColor Magenta
    Write-Host "====================================" -ForegroundColor Magenta
    
    if (-not $global:UsuarioId) {
        Write-Host "  UsuarioId nao disponivel. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        $contaRequest = @{
            accountType = "CHECKING"
            bankCode = "756"
            initialBalance = 0.00
            description = "Conta Corrente Principal"
            userId = $global:UsuarioId
        } | ConvertTo-Json -Depth 10
        
        $contaResponse = Invoke-ApiRequest -Url "$BaseUrl/accounts" -Method POST -Headers $headers -Body $contaRequest -Description "Criando conta corrente"
        
        if ($contaResponse) {
            $global:ContaId = $contaResponse.id
            Write-Host "  Conta criada com ID: $global:ContaId" -ForegroundColor Green
            Write-Host "ETAPA 4 CONCLUIDA - Habilita Etapa 5" -ForegroundColor Green
        }
    }
}

# ETAPA 5: TRANSACOES PIX
if ($EtapaInicial -le 5 -and $EtapaFinal -ge 5) {
    Write-Host ""
    Write-Host "ETAPA 5: REALIZACAO DE TRANSACOES PIX" -ForegroundColor Magenta
    Write-Host "====================================" -ForegroundColor Magenta
    
    if (-not $global:ContaId) {
        Write-Host "  ContaId nao disponivel. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        $qrCodeRequest = @{
            amount = 100.00
            pixKey = "a59b3ad1-c78a-4382-9216-01376298b153"
            description = "Teste integracao Sicoob"
            expiresIn = 3600
            contaId = $global:ContaId
            externalId = "TEST-QR-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        } | ConvertTo-Json -Depth 10
        
        $qrCodeResponse = Invoke-ApiRequest -Url "$BaseUrl/transacoes/pix/qrcode/dinamico" -Method POST -Headers $headers -Body $qrCodeRequest -Description "Criando QR Code dinamico"
        
        if ($qrCodeResponse) {
            $global:TransacaoId = $qrCodeResponse.transactionId
            Write-Host "  QR Code criado - Transacao ID: $global:TransacaoId" -ForegroundColor Green
            Write-Host "  PIX Copia e Cola: $($qrCodeResponse.qrcodePayload.Length) caracteres" -ForegroundColor Gray
            Write-Host "ETAPA 5 CONCLUIDA - Habilita Etapa 6" -ForegroundColor Green
        }
    }
}

# ETAPA 6: CONSULTA DE EXTRATO
if ($EtapaInicial -le 6 -and $EtapaFinal -ge 6) {
    Write-Host ""
    Write-Host "ETAPA 6: CONSULTA DE HISTORICO E EXTRATO SICOOB" -ForegroundColor Magenta
    Write-Host "===============================================" -ForegroundColor Magenta
    
    if (-not $global:ContaId) {
        Write-Host "  ContaId nao disponivel. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        # Consultar historico interno
        $historicoResponse = Invoke-ApiRequest -Url "$BaseUrl/accounts/$global:ContaId/history?days=30" -Method GET -Headers $headers -Description "Consultando historico interno"
        
        # Executar conciliacao
        $conciliacaoResponse = Invoke-ApiRequest -Url "$BaseUrl/reconciliation/sicoob/auto" -Method POST -Headers $headers -Description "Executando conciliacao"
        
        if ($conciliacaoResponse) {
            Write-Host "  Taxa de conciliacao: $($conciliacaoResponse.reconciliationRate)%" -ForegroundColor Green
            Write-Host "ETAPA 6 CONCLUIDA - Trilha Integrada Finalizada!" -ForegroundColor Green
        }
    }
}

# RESUMO FINAL
Write-Host ""
Write-Host "RESUMO DA TRILHA INTEGRADA" -ForegroundColor Green
Write-Host "==========================" -ForegroundColor Green
Write-Host ""

if ($global:EmpresaId) { Write-Host "OK Empresa ID: $global:EmpresaId" -ForegroundColor Green }
if ($global:UsuarioId) { Write-Host "OK Usuario ID: $global:UsuarioId" -ForegroundColor Green }
if ($global:ContaId) { Write-Host "OK Conta ID: $global:ContaId" -ForegroundColor Green }
if ($global:TransacaoId) { Write-Host "OK Transacao ID: $global:TransacaoId" -ForegroundColor Green }

Write-Host ""
Write-Host "TRILHA INTEGRADA PSP-SICOOB EXECUTADA!" -ForegroundColor Green
Write-Host "Verifique os logs detalhados acima para validar cada etapa." -ForegroundColor Cyan
