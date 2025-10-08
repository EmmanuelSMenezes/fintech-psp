# ========================================
# Executor da Trilha Integrada PSP-Sicoob
# ========================================
# Script para executar e validar cada etapa da trilha integrada

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$BackofficeUrl = "http://localhost:3000",
    [string]$InternetBankingUrl = "http://localhost:3001",
    [string]$CompanyServiceUrl = "http://localhost:5004",
    [string]$UserServiceUrl = "http://localhost:5003",
    [string]$ConfigServiceUrl = "http://localhost:5006",
    [string]$TransactionServiceUrl = "http://localhost:5002",
    [string]$IntegrationServiceUrl = "http://localhost:5005",
    [int]$EtapaInicial = 1,
    [int]$EtapaFinal = 6
)

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Dados consistentes para toda a trilha
$EmpresaTeste = @{
    razaoSocial = "EmpresaTeste Ltda"
    nomeFantasia = "EmpresaTeste"
    cnpj = "12345678000199"
    inscricaoEstadual = "123456789"
    inscricaoMunicipal = "987654321"
    email = "contato@empresateste.com"
    telefone = "(11) 99999-9999"
}

$UsuarioTeste = @{
    email = "cliente@empresateste.com"
    name = "Gerente EmpresaTeste"
    role = "COMPANY_ADMIN"
}

$ContaTeste = @{
    accountType = "CHECKING"
    bankCode = "756"
    initialBalance = 0.00
    description = "Conta Corrente Principal"
}

$TransacaoTeste = @{
    amount = 100.00
    pixKey = "a59b3ad1-c78a-4382-9216-01376298b153"
    description = "Teste integração Sicoob"
    expiresIn = 3600
}

# Headers padrão
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "🚀 EXECUTANDO TRILHA INTEGRADA PSP-SICOOB" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Etapas: $EtapaInicial a $EtapaFinal" -ForegroundColor Cyan
Write-Host ""

# Função para fazer requisições HTTP
function Invoke-ApiRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null,
        [string]$Description = ""
    )
    
    try {
        Write-Host "  📤 $Description" -ForegroundColor Yellow
        Write-Host "     $Method $Url" -ForegroundColor Gray
        
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
            TimeoutSec = 30
        }
        
        if ($Body) {
            $params.Body = $Body
            Write-Host "     Body: $($Body.Length) chars" -ForegroundColor Gray
        }
        
        $response = Invoke-RestMethod @params
        Write-Host "  ✅ Sucesso" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "  ❌ Erro: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Função para validar etapa
function Test-EtapaCompleta {
    param(
        [string]$Nome,
        [scriptblock]$Validacao
    )
    
    Write-Host "🔍 Validando $Nome..." -ForegroundColor Cyan
    
    try {
        $resultado = & $Validacao
        if ($resultado) {
            Write-Host "✅ $Nome: VÁLIDA" -ForegroundColor Green
            return $true
        } else {
            Write-Host "❌ $Nome: INVÁLIDA" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "❌ $Nome: ERRO - $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# ========================================
# ETAPA 1: CADASTRO DO CLIENTE (EMPRESA)
# ========================================
if ($EtapaInicial -le 1 -and $EtapaFinal -ge 1) {
    Write-Host ""
    Write-Host "📋 ETAPA 1: CADASTRO DO CLIENTE (EMPRESA)" -ForegroundColor Magenta
    Write-Host "=========================================" -ForegroundColor Magenta
    
    # 1.1 Criar empresa
    $empresaRequest = $EmpresaTeste | ConvertTo-Json -Depth 10
    $empresaResponse = Invoke-ApiRequest -Url "$CompanyServiceUrl/companies" -Method POST -Headers $headers -Body $empresaRequest -Description "Criando empresa"
    
    if ($empresaResponse) {
        $global:EmpresaId = $empresaResponse.id
        Write-Host "  📝 Empresa criada com ID: $global:EmpresaId" -ForegroundColor Green
        
        # 1.2 Validar criação
        $validacao = Test-EtapaCompleta -Nome "Cadastro da Empresa" -Validacao {
            $empresa = Invoke-ApiRequest -Url "$CompanyServiceUrl/companies/$global:EmpresaId" -Method GET -Headers $headers -Description "Validando empresa criada"
            return ($empresa -and $empresa.cnpj -eq $EmpresaTeste.cnpj)
        }
        
        if ($validacao) {
            Write-Host "🔗 Etapa 1 CONCLUÍDA - Habilita Etapa 2 (Geração de Usuário)" -ForegroundColor Green
        }
    }
}

# ========================================
# ETAPA 2: GERAÇÃO DE USUÁRIO
# ========================================
if ($EtapaInicial -le 2 -and $EtapaFinal -ge 2) {
    Write-Host ""
    Write-Host "📋 ETAPA 2: GERAÇÃO DE USUÁRIO PARA O CLIENTE" -ForegroundColor Magenta
    Write-Host "=============================================" -ForegroundColor Magenta
    
    if (-not $global:EmpresaId) {
        Write-Host "⚠️  EmpresaId não disponível. Execute Etapa 1 primeiro." -ForegroundColor Yellow
    } else {
        # 2.1 Aprovar empresa (simulado)
        $aprovacaoRequest = @{
            status = "Approved"
            approvedBy = "admin@fintech.com"
            approvalNotes = "Documentação validada - Teste automatizado"
        } | ConvertTo-Json
        
        $aprovacaoResponse = Invoke-ApiRequest -Url "$CompanyServiceUrl/companies/$global:EmpresaId/status" -Method PUT -Headers $headers -Body $aprovacaoRequest -Description "Aprovando empresa"
        
        # 2.2 Criar usuário
        $usuarioData = $UsuarioTeste.Clone()
        $usuarioData.companyId = $global:EmpresaId
        $usuarioRequest = $usuarioData | ConvertTo-Json -Depth 10
        
        $usuarioResponse = Invoke-ApiRequest -Url "$UserServiceUrl/users" -Method POST -Headers $headers -Body $usuarioRequest -Description "Criando usuário"
        
        if ($usuarioResponse) {
            $global:UsuarioId = $usuarioResponse.id
            Write-Host "  👤 Usuário criado com ID: $global:UsuarioId" -ForegroundColor Green
            
            # 2.3 Validar criação
            $validacao = Test-EtapaCompleta -Nome "Geração de Usuário" -Validacao {
                $usuario = Invoke-ApiRequest -Url "$UserServiceUrl/users/$global:UsuarioId" -Method GET -Headers $headers -Description "Validando usuário criado"
                return ($usuario -and $usuario.email -eq $UsuarioTeste.email)
            }
            
            if ($validacao) {
                Write-Host "🔗 Etapa 2 CONCLUÍDA - Habilita Etapa 3 (Configuração Inicial)" -ForegroundColor Green
            }
        }
    }
}

# ========================================
# ETAPA 3: CONFIGURAÇÃO INICIAL
# ========================================
if ($EtapaInicial -le 3 -and $EtapaFinal -ge 3) {
    Write-Host ""
    Write-Host "📋 ETAPA 3: CONFIGURAÇÃO INICIAL" -ForegroundColor Magenta
    Write-Host "================================" -ForegroundColor Magenta
    
    # 3.1 Configurar limites
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
        transactionLimits = @{
            pixMax = 5000.00
            tedMax = 100000.00
            boletoMax = 50000.00
        }
    } | ConvertTo-Json -Depth 10
    
    $limitesResponse = Invoke-ApiRequest -Url "$ConfigServiceUrl/config/limits" -Method POST -Headers $headers -Body $limitesRequest -Description "Configurando limites"
    
    # 3.2 Testar conectividade Sicoob
    $conectividadeResponse = Invoke-ApiRequest -Url "$IntegrationServiceUrl/integrations/sicoob/test-connectivity" -Method GET -Headers $headers -Description "Testando conectividade Sicoob"
    
    if ($conectividadeResponse) {
        Write-Host "  🏦 Conectividade Sicoob: $($conectividadeResponse.status)" -ForegroundColor Green
        
        $validacao = Test-EtapaCompleta -Nome "Configuração Inicial" -Validacao {
            return ($limitesResponse -and $conectividadeResponse.status -eq "success")
        }
        
        if ($validacao) {
            Write-Host "🔗 Etapa 3 CONCLUÍDA - Habilita Etapa 4 (Criação de Conta)" -ForegroundColor Green
        }
    }
}

# ========================================
# ETAPA 4: CRIAÇÃO E ATIVAÇÃO DE CONTA
# ========================================
if ($EtapaInicial -le 4 -and $EtapaFinal -ge 4) {
    Write-Host ""
    Write-Host "📋 ETAPA 4: CRIAÇÃO E ATIVAÇÃO DE CONTA" -ForegroundColor Magenta
    Write-Host "=======================================" -ForegroundColor Magenta
    
    if (-not $global:UsuarioId) {
        Write-Host "⚠️  UsuarioId não disponível. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        # 4.1 Criar conta
        $contaData = $ContaTeste.Clone()
        $contaData.userId = $global:UsuarioId
        $contaRequest = $contaData | ConvertTo-Json -Depth 10
        
        $contaResponse = Invoke-ApiRequest -Url "$UserServiceUrl/accounts" -Method POST -Headers $headers -Body $contaRequest -Description "Criando conta corrente"
        
        if ($contaResponse) {
            $global:ContaId = $contaResponse.id
            Write-Host "  🏦 Conta criada com ID: $global:ContaId" -ForegroundColor Green
            
            # 4.2 Aprovar conta (simulado)
            $aprovacaoContaRequest = @{
                status = "ACTIVE"
                approvedBy = "admin@fintech.com"
            } | ConvertTo-Json
            
            $aprovacaoContaResponse = Invoke-ApiRequest -Url "$UserServiceUrl/accounts/$global:ContaId/status" -Method PUT -Headers $headers -Body $aprovacaoContaRequest -Description "Aprovando conta"
            
            $validacao = Test-EtapaCompleta -Nome "Criação de Conta" -Validacao {
                $conta = Invoke-ApiRequest -Url "$UserServiceUrl/accounts/$global:ContaId" -Method GET -Headers $headers -Description "Validando conta criada"
                return ($conta -and $conta.status -eq "ACTIVE")
            }
            
            if ($validacao) {
                Write-Host "🔗 Etapa 4 CONCLUÍDA - Habilita Etapa 5 (Transações PIX)" -ForegroundColor Green
            }
        }
    }
}

# ========================================
# ETAPA 5: REALIZAÇÃO DE TRANSAÇÕES PIX
# ========================================
if ($EtapaInicial -le 5 -and $EtapaFinal -ge 5) {
    Write-Host ""
    Write-Host "📋 ETAPA 5: REALIZAÇÃO DE TRANSAÇÕES PIX" -ForegroundColor Magenta
    Write-Host "========================================" -ForegroundColor Magenta
    
    if (-not $global:ContaId) {
        Write-Host "⚠️  ContaId não disponível. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        # 5.1 Criar QR Code dinâmico
        $qrCodeData = $TransacaoTeste.Clone()
        $qrCodeData.contaId = $global:ContaId
        $qrCodeData.externalId = "TEST-QR-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        $qrCodeRequest = $qrCodeData | ConvertTo-Json -Depth 10
        
        $qrCodeResponse = Invoke-ApiRequest -Url "$TransactionServiceUrl/transacoes/pix/qrcode/dinamico" -Method POST -Headers $headers -Body $qrCodeRequest -Description "Criando QR Code dinâmico"
        
        if ($qrCodeResponse) {
            $global:TransacaoId = $qrCodeResponse.transactionId
            Write-Host "  💰 QR Code criado - Transação ID: $global:TransacaoId" -ForegroundColor Green
            Write-Host "  📱 PIX Copia e Cola: $($qrCodeResponse.qrcodePayload.Length) caracteres" -ForegroundColor Gray
            
            # 5.2 Testar integração Sicoob
            $cobrancaRequest = @{
                chave = $TransacaoTeste.pixKey
                valor = @{
                    original = $TransacaoTeste.amount.ToString("F2")
                }
                calendario = @{
                    expiracao = $TransacaoTeste.expiresIn
                }
                solicitacaoPagador = $TransacaoTeste.description
            } | ConvertTo-Json -Depth 10
            
            $cobrancaResponse = Invoke-ApiRequest -Url "$IntegrationServiceUrl/integrations/sicoob/pix/cobranca" -Method POST -Headers $headers -Body $cobrancaRequest -Description "Criando cobrança Sicoob"
            
            if ($cobrancaResponse) {
                $global:SicoobTxId = $cobrancaResponse.txId
                Write-Host "  🏦 Cobrança Sicoob criada - TxId: $global:SicoobTxId" -ForegroundColor Green
                
                $validacao = Test-EtapaCompleta -Nome "Transações PIX" -Validacao {
                    return ($qrCodeResponse -and $cobrancaResponse -and $cobrancaResponse.status -eq "ATIVA")
                }
                
                if ($validacao) {
                    Write-Host "🔗 Etapa 5 CONCLUÍDA - Habilita Etapa 6 (Consulta de Extrato)" -ForegroundColor Green
                }
            }
        }
    }
}

# ========================================
# ETAPA 6: CONSULTA DE HISTÓRICO E EXTRATO
# ========================================
if ($EtapaInicial -le 6 -and $EtapaFinal -ge 6) {
    Write-Host ""
    Write-Host "📋 ETAPA 6: CONSULTA DE HISTÓRICO E EXTRATO SICOOB" -ForegroundColor Magenta
    Write-Host "==================================================" -ForegroundColor Magenta
    
    if (-not $global:ContaId) {
        Write-Host "⚠️  ContaId não disponível. Execute Etapas anteriores primeiro." -ForegroundColor Yellow
    } else {
        # 6.1 Consultar histórico interno
        $historicoResponse = Invoke-ApiRequest -Url "$UserServiceUrl/accounts/$global:ContaId/history?days=30" -Method GET -Headers $headers -Description "Consultando histórico interno"
        
        # 6.2 Consultar extrato Sicoob
        $extratoResponse = Invoke-ApiRequest -Url "$IntegrationServiceUrl/integrations/sicoob/conta/12345/extrato" -Method GET -Headers $headers -Description "Consultando extrato Sicoob"
        
        # 6.3 Testar conciliação
        $conciliacaoResponse = Invoke-ApiRequest -Url "$UserServiceUrl/accounts/$global:ContaId/reconciliation" -Method POST -Headers $headers -Description "Executando conciliação"
        
        $validacao = Test-EtapaCompleta -Nome "Consulta de Extrato" -Validacao {
            return ($historicoResponse -or $extratoResponse)
        }
        
        if ($validacao) {
            Write-Host "🔗 Etapa 6 CONCLUÍDA - Trilha Integrada Finalizada!" -ForegroundColor Green
        }
    }
}

# ========================================
# RESUMO FINAL
# ========================================
Write-Host ""
Write-Host "📊 RESUMO DA TRILHA INTEGRADA" -ForegroundColor Green
Write-Host "=============================" -ForegroundColor Green
Write-Host ""

if ($global:EmpresaId) { Write-Host "✅ Empresa ID: $global:EmpresaId" -ForegroundColor Green }
if ($global:UsuarioId) { Write-Host "✅ Usuário ID: $global:UsuarioId" -ForegroundColor Green }
if ($global:ContaId) { Write-Host "✅ Conta ID: $global:ContaId" -ForegroundColor Green }
if ($global:TransacaoId) { Write-Host "✅ Transação ID: $global:TransacaoId" -ForegroundColor Green }
if ($global:SicoobTxId) { Write-Host "✅ Sicoob TxId: $global:SicoobTxId" -ForegroundColor Green }

Write-Host ""
Write-Host "🎉 TRILHA INTEGRADA PSP-SICOOB EXECUTADA!" -ForegroundColor Green
Write-Host "Verifique os logs detalhados acima para validar cada etapa." -ForegroundColor Cyan
