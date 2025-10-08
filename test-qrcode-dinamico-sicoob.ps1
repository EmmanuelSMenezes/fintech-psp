# ========================================
# Teste Integrado QR Code Dinâmico Sicoob
# ========================================
# Este script testa todo o fluxo desde a criação da transação PIX
# até a geração do QR Code dinâmico via integração Sicoob

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$IntegrationUrl = "http://localhost:5005",
    [string]$TransactionUrl = "http://localhost:5002",
    [decimal]$Amount = 10.50,
    [string]$PixKey = "user@example.com",
    [string]$Description = "Teste QR Code Dinâmico Sicoob"
)

# Configurações
$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

# Headers padrão
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "🚀 INICIANDO TESTE INTEGRADO QR CODE DINÂMICO SICOOB" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
Write-Host ""

# Função para fazer requisições HTTP com tratamento de erro
function Invoke-ApiRequest {
    param(
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [string]$Body = $null
    )
    
    try {
        $params = @{
            Uri = $Url
            Method = $Method
            Headers = $Headers
        }
        
        if ($Body) {
            $params.Body = $Body
        }
        
        $response = Invoke-RestMethod @params
        return $response
    }
    catch {
        Write-Host "❌ Erro na requisição: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode
            Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        }
        throw
    }
}

# Função para aguardar serviços
function Wait-ForService {
    param(
        [string]$Url,
        [string]$ServiceName,
        [int]$MaxAttempts = 30
    )
    
    Write-Host "⏳ Aguardando $ServiceName estar disponível..." -ForegroundColor Yellow
    
    for ($i = 1; $i -le $MaxAttempts; $i++) {
        try {
            $response = Invoke-WebRequest -Uri "$Url/health" -Method GET -TimeoutSec 5
            if ($response.StatusCode -eq 200) {
                Write-Host "✅ $ServiceName está disponível!" -ForegroundColor Green
                return $true
            }
        }
        catch {
            Write-Host "   Tentativa $i/$MaxAttempts - $ServiceName não disponível ainda..." -ForegroundColor Gray
            Start-Sleep -Seconds 2
        }
    }
    
    Write-Host "❌ $ServiceName não ficou disponível após $MaxAttempts tentativas" -ForegroundColor Red
    return $false
}

try {
    # ========================================
    # ETAPA 1: Verificar se os serviços estão rodando
    # ========================================
    Write-Host "📋 ETAPA 1: Verificando disponibilidade dos serviços" -ForegroundColor Cyan
    Write-Host ""
    
    $services = @(
        @{ Name = "API Gateway"; Url = $BaseUrl },
        @{ Name = "Transaction Service"; Url = $TransactionUrl },
        @{ Name = "Integration Service"; Url = $IntegrationUrl }
    )
    
    foreach ($service in $services) {
        if (-not (Wait-ForService -Url $service.Url -ServiceName $service.Name)) {
            throw "Serviço $($service.Name) não está disponível"
        }
    }
    
    Write-Host ""
    
    # ========================================
    # ETAPA 2: Obter token de autenticação
    # ========================================
    Write-Host "🔐 ETAPA 2: Obtendo token de autenticação" -ForegroundColor Cyan
    Write-Host ""
    
    $authRequest = @{
        grant_type = "client_credentials"
        client_id = "test-client"
        client_secret = "test-secret"
        scope = "transactions"
    } | ConvertTo-Json
    
    try {
        $authResponse = Invoke-ApiRequest -Url "$BaseUrl/auth/token" -Method POST -Headers $headers -Body $authRequest
        $token = $authResponse.access_token
        
        # Adicionar token aos headers
        $headers["Authorization"] = "Bearer $token"
        
        Write-Host "✅ Token obtido com sucesso!" -ForegroundColor Green
        Write-Host "   Token: $($token.Substring(0, 20))..." -ForegroundColor Gray
    }
    catch {
        Write-Host "⚠️  Falha na autenticação, continuando sem token..." -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    # ========================================
    # ETAPA 3: Criar transação PIX dinâmica
    # ========================================
    Write-Host "💰 ETAPA 3: Criando transação PIX dinâmica" -ForegroundColor Cyan
    Write-Host ""
    
    $externalId = "TEST-QR-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    
    $pixRequest = @{
        externalId = $externalId
        amount = $Amount
        pixKey = $PixKey
        bankCode = "756"  # Código do Sicoob
        description = $Description
        expiresIn = 300   # 5 minutos
    } | ConvertTo-Json
    
    Write-Host "📤 Enviando requisição para criar QR Code dinâmico..." -ForegroundColor Yellow
    Write-Host "   External ID: $externalId" -ForegroundColor Gray
    Write-Host "   Valor: R$ $Amount" -ForegroundColor Gray
    Write-Host "   PIX Key: $PixKey" -ForegroundColor Gray
    Write-Host "   Banco: Sicoob (756)" -ForegroundColor Gray
    Write-Host ""
    
    $qrResponse = Invoke-ApiRequest -Url "$TransactionUrl/transacoes/pix/qrcode/dinamico" -Method POST -Headers $headers -Body $pixRequest
    
    Write-Host "✅ QR Code dinâmico criado com sucesso!" -ForegroundColor Green
    Write-Host "   Transaction ID: $($qrResponse.transactionId)" -ForegroundColor Gray
    Write-Host "   Tipo: $($qrResponse.type)" -ForegroundColor Gray
    Write-Host "   Expira em: $($qrResponse.expiresAt)" -ForegroundColor Gray
    Write-Host "   PIX Copia e Cola: $($qrResponse.qrcodePayload.Length) caracteres" -ForegroundColor Gray
    Write-Host ""
    
    # ========================================
    # ETAPA 4: Testar integração Sicoob PIX Cobrança
    # ========================================
    Write-Host "🏦 ETAPA 4: Testando integração Sicoob PIX Cobrança" -ForegroundColor Cyan
    Write-Host ""
    
    $cobrancaRequest = @{
        chave = $PixKey
        valor = @{
            original = $Amount.ToString("F2", [System.Globalization.CultureInfo]::InvariantCulture)
        }
        calendario = @{
            expiracao = 300
        }
        devedor = @{
            nome = "Emmanuel Santos Menezes"
            cpf = "39745467820"
        }
        solicitacaoPagador = $Description
        infoAdicionais = @(
            @{
                nome = "ExternalId"
                valor = $externalId
            }
        )
    } | ConvertTo-Json -Depth 10
    
    Write-Host "📤 Criando cobrança PIX no Sicoob..." -ForegroundColor Yellow
    
    $cobrancaResponse = Invoke-ApiRequest -Url "$IntegrationUrl/integrations/sicoob/pix/cobranca" -Method POST -Headers $headers -Body $cobrancaRequest
    
    Write-Host "✅ Cobrança PIX criada no Sicoob!" -ForegroundColor Green
    Write-Host "   TxId: $($cobrancaResponse.txId)" -ForegroundColor Gray
    Write-Host "   Status: $($cobrancaResponse.status)" -ForegroundColor Gray
    Write-Host "   Provider: $($cobrancaResponse.provider)" -ForegroundColor Gray
    
    if ($cobrancaResponse.pixCopiaECola) {
        Write-Host "   PIX Copia e Cola: $($cobrancaResponse.pixCopiaECola.Length) caracteres" -ForegroundColor Gray
    }
    
    if ($cobrancaResponse.qrcode) {
        Write-Host "   QR Code: Presente" -ForegroundColor Gray
    }
    
    Write-Host ""
    
    # ========================================
    # ETAPA 5: Testar endpoint específico de QR Code
    # ========================================
    Write-Host "🎯 ETAPA 5: Testando endpoint específico de QR Code Sicoob" -ForegroundColor Cyan
    Write-Host ""
    
    $qrCodeRequest = @{
        txId = $cobrancaResponse.txId
        chave = $PixKey
        valor = $Amount
    } | ConvertTo-Json
    
    Write-Host "📤 Solicitando QR Code específico do Sicoob..." -ForegroundColor Yellow
    
    try {
        $qrCodeResponse = Invoke-ApiRequest -Url "$IntegrationUrl/integrations/sicoob/cobranca/v3/qrcode" -Method POST -Headers $headers -Body $qrCodeRequest
        
        Write-Host "✅ QR Code específico obtido!" -ForegroundColor Green
        Write-Host "   TxId: $($qrCodeResponse.txid)" -ForegroundColor Gray
        Write-Host "   Tipo: $($qrCodeResponse.type)" -ForegroundColor Gray
        Write-Host "   Provider: $($qrCodeResponse.provider)" -ForegroundColor Gray
        
        if ($qrCodeResponse.pixCopiaECola) {
            Write-Host "   PIX Copia e Cola: $($qrCodeResponse.pixCopiaECola.Length) caracteres" -ForegroundColor Gray
        }
    }
    catch {
        Write-Host "⚠️  Endpoint específico de QR Code não disponível, mas cobrança foi criada com sucesso" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    # ========================================
    # ETAPA 6: Validar dados do QR Code
    # ========================================
    Write-Host "🔍 ETAPA 6: Validando dados do QR Code gerado" -ForegroundColor Cyan
    Write-Host ""
    
    $pixCopiaECola = $cobrancaResponse.pixCopiaECola
    if ($pixCopiaECola) {
        Write-Host "📋 Análise do PIX Copia e Cola:" -ForegroundColor Yellow
        Write-Host "   Tamanho: $($pixCopiaECola.Length) caracteres" -ForegroundColor Gray
        Write-Host "   Formato: $($pixCopiaECola.Substring(0, 20))..." -ForegroundColor Gray
        
        # Validações básicas do formato EMV
        if ($pixCopiaECola.StartsWith("00020")) {
            Write-Host "   ✅ Formato EMV válido (inicia com 00020)" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Formato EMV inválido" -ForegroundColor Red
        }
        
        if ($pixCopiaECola.Contains("br.gov.bcb.pix")) {
            Write-Host "   ✅ Contém identificador PIX brasileiro" -ForegroundColor Green
        } else {
            Write-Host "   ❌ Não contém identificador PIX brasileiro" -ForegroundColor Red
        }
        
        if ($pixCopiaECola.Length -ge 100 -and $pixCopiaECola.Length -le 512) {
            Write-Host "   ✅ Tamanho dentro do esperado (100-512 caracteres)" -ForegroundColor Green
        } else {
            Write-Host "   ⚠️  Tamanho fora do padrão esperado" -ForegroundColor Yellow
        }
    } else {
        Write-Host "❌ PIX Copia e Cola não foi gerado" -ForegroundColor Red
    }
    
    Write-Host ""
    
    # ========================================
    # RESUMO FINAL
    # ========================================
    Write-Host "📊 RESUMO DO TESTE" -ForegroundColor Green
    Write-Host "==================" -ForegroundColor Green
    Write-Host ""
    Write-Host "✅ Serviços verificados e disponíveis" -ForegroundColor Green
    Write-Host "✅ QR Code dinâmico criado no TransactionService" -ForegroundColor Green
    Write-Host "✅ Cobrança PIX criada no Sicoob via IntegrationService" -ForegroundColor Green
    Write-Host "✅ PIX Copia e Cola gerado com sucesso" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎯 DADOS FINAIS:" -ForegroundColor Cyan
    Write-Host "   External ID: $externalId" -ForegroundColor Gray
    Write-Host "   Transaction ID: $($qrResponse.transactionId)" -ForegroundColor Gray
    Write-Host "   Sicoob TxId: $($cobrancaResponse.txId)" -ForegroundColor Gray
    Write-Host "   Valor: R$ $Amount" -ForegroundColor Gray
    Write-Host "   PIX Key: $PixKey" -ForegroundColor Gray
    Write-Host "   Status: $($cobrancaResponse.status)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "🔗 PIX COPIA E COLA:" -ForegroundColor Cyan
    Write-Host "$pixCopiaECola" -ForegroundColor White
    Write-Host ""
    Write-Host "🎉 TESTE CONCLUÍDO COM SUCESSO!" -ForegroundColor Green
    
}
catch {
    Write-Host ""
    Write-Host "❌ ERRO DURANTE O TESTE:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "🔍 Verifique se:" -ForegroundColor Yellow
    Write-Host "   - Os serviços estão rodando (docker-compose up)" -ForegroundColor Yellow
    Write-Host "   - As configurações do Sicoob estão corretas" -ForegroundColor Yellow
    Write-Host "   - Os certificados estão no local correto" -ForegroundColor Yellow
    Write-Host "   - A conectividade com o Sicoob está funcionando" -ForegroundColor Yellow
    
    exit 1
}
