# 🔌 **IntegrationService - Modelo de Dados**

## 📋 **Visão Geral**

O IntegrationService gerencia integrações com bancos externos (Sicoob, Stark Bank, etc.) e processa transações PIX e Boleto no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5004
### **Tecnologias**: PostgreSQL + Dapper + HttpClient

---

## 📊 **Entidades Principais**

### **1. SicoobPixCobranca**

**Descrição**: Cobrança PIX do Sicoob

```csharp
public class SicoobPixCobranca
{
    public Guid Id { get; set; }                        // PK
    public string TxId { get; set; }                    // Transaction ID único
    public decimal Valor { get; set; }                  // Valor da cobrança
    public string ChavePix { get; set; }                // Chave PIX
    public string? SolicitacaoPagador { get; set; }     // Descrição
    public DateTime Vencimento { get; set; }            // Data vencimento
    public string Status { get; set; }                  // Status cobrança
    public string? PixCopiaECola { get; set; }          // EMV Code
    public string? QrCodeUrl { get; set; }              // URL QR Code
    public string? Location { get; set; }               // Location ID
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
    public string? ResponseJson { get; set; }           // Resposta completa
}
```

### **2. SicoobBoleto**

**Descrição**: Boleto do Sicoob

```csharp
public class SicoobBoleto
{
    public Guid Id { get; set; }                        // PK
    public string NossoNumero { get; set; }             // Nosso número
    public decimal Valor { get; set; }                  // Valor boleto
    public DateTime Vencimento { get; set; }            // Data vencimento
    public string CpfCnpjBeneficiario { get; set; }     // CPF/CNPJ beneficiário
    public string NomeBeneficiario { get; set; }        // Nome beneficiário
    public string CpfCnpjPagador { get; set; }          // CPF/CNPJ pagador
    public string NomePagador { get; set; }             // Nome pagador
    public string? Descricao { get; set; }              // Descrição
    public string Status { get; set; }                  // Status boleto
    public string? LinhaDigitavel { get; set; }         // Linha digitável
    public string? CodigoBarras { get; set; }           // Código barras
    public string? UrlBoleto { get; set; }              // URL PDF
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
    public string? ResponseJson { get; set; }           // Resposta completa
}
```

### **3. IntegrationLog**

**Descrição**: Log de integrações

```csharp
public class IntegrationLog
{
    public Guid Id { get; set; }                        // PK
    public string Service { get; set; }                 // Serviço (SICOOB, STARK, etc.)
    public string Operation { get; set; }               // Operação (PIX, BOLETO)
    public string Method { get; set; }                  // Método HTTP
    public string Endpoint { get; set; }                // Endpoint chamado
    public string? RequestBody { get; set; }            // Request body
    public string? ResponseBody { get; set; }           // Response body
    public int? StatusCode { get; set; }                // Status HTTP
    public bool Success { get; set; }                   // Sucesso
    public string? ErrorMessage { get; set; }           // Mensagem erro
    public long DurationMs { get; set; }                // Duração em ms
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **sicoob_pix_cobrancas**
```sql
CREATE TABLE sicoob_pix_cobrancas (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tx_id VARCHAR(35) UNIQUE NOT NULL,
    valor DECIMAL(15,2) NOT NULL CHECK (valor > 0),
    chave_pix VARCHAR(255) NOT NULL,
    solicitacao_pagador TEXT,
    vencimento TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'ATIVA',
    pix_copia_e_cola TEXT,
    qr_code_url TEXT,
    location VARCHAR(255),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    response_json JSONB,
    
    CONSTRAINT chk_sicoob_pix_status CHECK (status IN ('ATIVA', 'CONCLUIDA', 'REMOVIDA_PELO_USUARIO_RECEBEDOR', 'REMOVIDA_PELO_PSP'))
);
```

### **sicoob_boletos**
```sql
CREATE TABLE sicoob_boletos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    nosso_numero VARCHAR(20) UNIQUE NOT NULL,
    valor DECIMAL(15,2) NOT NULL CHECK (valor > 0),
    vencimento DATE NOT NULL,
    cpf_cnpj_beneficiario VARCHAR(18) NOT NULL,
    nome_beneficiario VARCHAR(255) NOT NULL,
    cpf_cnpj_pagador VARCHAR(18) NOT NULL,
    nome_pagador VARCHAR(255) NOT NULL,
    descricao TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'REGISTRADO',
    linha_digitavel VARCHAR(54),
    codigo_barras VARCHAR(44),
    url_boleto TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    response_json JSONB,
    
    CONSTRAINT chk_sicoob_boleto_status CHECK (status IN ('REGISTRADO', 'PAGO', 'VENCIDO', 'CANCELADO'))
);
```

### **integration_logs**
```sql
CREATE TABLE integration_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    service VARCHAR(50) NOT NULL,
    operation VARCHAR(50) NOT NULL,
    method VARCHAR(10) NOT NULL,
    endpoint VARCHAR(500) NOT NULL,
    request_body TEXT,
    response_body TEXT,
    status_code INTEGER,
    success BOOLEAN NOT NULL,
    error_message TEXT,
    duration_ms BIGINT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
```

---

## 🔑 **DTOs e Requests**

### **Sicoob PIX DTOs**

#### **SicoobPixCobrancaRequest**
```csharp
public class SicoobPixCobrancaRequest
{
    public string TxId { get; set; }                    // Transaction ID
    public decimal Valor { get; set; }                  // Valor
    public string ChavePix { get; set; }                // Chave PIX
    public string? SolicitacaoPagador { get; set; }     // Descrição
    public int ExpiracaoSegundos { get; set; } = 3600;  // Expiração
}
```

#### **SicoobPixCobrancaResponse**
```csharp
public class SicoobPixCobrancaResponse
{
    public string TxId { get; set; }                    // Transaction ID
    public decimal Valor { get; set; }                  // Valor
    public string ChavePix { get; set; }                // Chave PIX
    public string Status { get; set; }                  // Status
    public string? SolicitacaoPagador { get; set; }     // Descrição
    public DateTime Vencimento { get; set; }            // Vencimento
    public string? PixCopiaECola { get; set; }          // EMV Code
    public string? Location { get; set; }               // Location
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

### **Sicoob Boleto DTOs**

#### **SicoobBoletoRequest**
```csharp
public class SicoobBoletoRequest
{
    public decimal Valor { get; set; }                  // Valor
    public DateTime Vencimento { get; set; }            // Vencimento
    public string CpfCnpjBeneficiario { get; set; }     // CPF/CNPJ beneficiário
    public string NomeBeneficiario { get; set; }        // Nome beneficiário
    public string CpfCnpjPagador { get; set; }          // CPF/CNPJ pagador
    public string NomePagador { get; set; }             // Nome pagador
    public string? Descricao { get; set; }              // Descrição
    public string? Instrucoes { get; set; }             // Instruções
}
```

#### **SicoobBoletoResponse**
```csharp
public class SicoobBoletoResponse
{
    public string NossoNumero { get; set; }             // Nosso número
    public decimal Valor { get; set; }                  // Valor
    public DateTime Vencimento { get; set; }            // Vencimento
    public string Status { get; set; }                  // Status
    public string? LinhaDigitavel { get; set; }         // Linha digitável
    public string? CodigoBarras { get; set; }           // Código barras
    public string? UrlBoleto { get; set; }              // URL PDF
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

### **Integration DTOs**

#### **IntegrationHealthResponse**
```csharp
public class IntegrationHealthResponse
{
    public string Service { get; set; }                 // Nome serviço
    public bool IsHealthy { get; set; }                 // Status saúde
    public string? ErrorMessage { get; set; }           // Mensagem erro
    public long ResponseTimeMs { get; set; }            // Tempo resposta
    public DateTime CheckedAt { get; set; }             // Data verificação
}
```

#### **IntegrationStatsResponse**
```csharp
public class IntegrationStatsResponse
{
    public string Service { get; set; }                 // Serviço
    public int TotalRequests { get; set; }              // Total requests
    public int SuccessfulRequests { get; set; }         // Sucessos
    public int FailedRequests { get; set; }             // Falhas
    public double SuccessRate { get; set; }             // Taxa sucesso
    public long AverageResponseTimeMs { get; set; }     // Tempo médio
    public DateTime PeriodStart { get; set; }           // Início período
    public DateTime PeriodEnd { get; set; }             // Fim período
}
```

---

## 🏦 **Configurações Sicoob**

### **Autenticação OAuth 2.0 + mTLS**
```csharp
public class SicoobConfig
{
    public string ClientId { get; set; }                // Client ID
    public string ClientSecret { get; set; }            // Client Secret
    public string CertificatePath { get; set; }         // Certificado mTLS
    public string CertificatePassword { get; set; }     // Senha certificado
    public string BaseUrl { get; set; }                 // URL base API
    public string TokenUrl { get; set; }                // URL token
    public int TokenExpiryMinutes { get; set; } = 50;   // Expiração token
}
```

### **Endpoints Sicoob**
```csharp
public static class SicoobEndpoints
{
    public const string TOKEN = "/oauth/token";
    public const string PIX_COBRANCA = "/pix/api/v2/cob";
    public const string PIX_CONSULTA = "/pix/api/v2/cob/{txid}";
    public const string PIX_QRCODE = "/pix/api/v2/loc/{id}/qrcode";
    public const string BOLETO_CRIAR = "/boleto/api/v1/boletos";
    public const string BOLETO_CONSULTAR = "/boleto/api/v1/boletos/{nossoNumero}";
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Sicoob PIX Cobranças
CREATE INDEX idx_sicoob_pix_tx_id ON sicoob_pix_cobrancas(tx_id);
CREATE INDEX idx_sicoob_pix_status ON sicoob_pix_cobrancas(status);
CREATE INDEX idx_sicoob_pix_chave ON sicoob_pix_cobrancas(chave_pix);
CREATE INDEX idx_sicoob_pix_vencimento ON sicoob_pix_cobrancas(vencimento);
CREATE INDEX idx_sicoob_pix_created_at ON sicoob_pix_cobrancas(created_at);

-- Sicoob Boletos
CREATE INDEX idx_sicoob_boleto_nosso_numero ON sicoob_boletos(nosso_numero);
CREATE INDEX idx_sicoob_boleto_status ON sicoob_boletos(status);
CREATE INDEX idx_sicoob_boleto_vencimento ON sicoob_boletos(vencimento);
CREATE INDEX idx_sicoob_boleto_pagador ON sicoob_boletos(cpf_cnpj_pagador);
CREATE INDEX idx_sicoob_boleto_created_at ON sicoob_boletos(created_at);

-- Integration Logs
CREATE INDEX idx_integration_logs_service ON integration_logs(service);
CREATE INDEX idx_integration_logs_operation ON integration_logs(operation);
CREATE INDEX idx_integration_logs_success ON integration_logs(success);
CREATE INDEX idx_integration_logs_created_at ON integration_logs(created_at);
CREATE INDEX idx_integration_logs_service_operation ON integration_logs(service, operation);
```

---

## 🔄 **Status e Estados**

### **Status PIX Cobrança**
```csharp
public enum SicoobPixStatus
{
    ATIVA,                              // Cobrança ativa
    CONCLUIDA,                          // Paga
    REMOVIDA_PELO_USUARIO_RECEBEDOR,    // Cancelada pelo recebedor
    REMOVIDA_PELO_PSP                   // Cancelada pelo PSP
}
```

### **Status Boleto**
```csharp
public enum SicoobBoletoStatus
{
    REGISTRADO,     // Registrado
    PAGO,           // Pago
    VENCIDO,        // Vencido
    CANCELADO       // Cancelado
}
```

---

## 🔐 **Segurança e Autenticação**

### **Token Management**
```csharp
public class SicoobTokenManager
{
    private string? _cachedToken;
    private DateTime _tokenExpiry;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    
    public async Task<string> GetValidTokenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_cachedToken == null || DateTime.UtcNow >= _tokenExpiry)
            {
                _cachedToken = await RequestNewTokenAsync();
                _tokenExpiry = DateTime.UtcNow.AddMinutes(50); // 50 min expiry
            }
            return _cachedToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### **mTLS Configuration**
```csharp
public class SicoobHttpClientFactory
{
    public HttpClient CreateClient(SicoobConfig config)
    {
        var handler = new HttpClientHandler();
        
        // Load client certificate for mTLS
        var cert = new X509Certificate2(config.CertificatePath, config.CertificatePassword);
        handler.ClientCertificates.Add(cert);
        
        return new HttpClient(handler)
        {
            BaseAddress = new Uri(config.BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }
}
```

---

## 🎯 **Casos de Uso**

### **Integração PIX Sicoob**
- Criação de cobranças dinâmicas
- Consulta de status de cobrança
- Geração de QR Codes
- Webhook de notificações

### **Integração Boleto Sicoob**
- Emissão de boletos
- Consulta de status
- Download de PDFs
- Notificações de pagamento

### **Monitoramento**
- Health checks de integrações
- Logs detalhados de requests
- Métricas de performance
- Alertas de falhas

### **Cache e Performance**
- Cache de tokens OAuth
- Retry policies
- Circuit breaker
- Rate limiting

---

## 🔗 **Relacionamentos**

### **SicoobPixCobranca → TransactionService**
- **Tipo**: Referência externa
- **Chave**: `tx_id` (usado como external_id)
- **Descrição**: Cobrança PIX vinculada a transação

### **SicoobBoleto → TransactionService**
- **Tipo**: Referência externa
- **Chave**: `nosso_numero` (usado como external_id)
- **Descrição**: Boleto vinculado a transação

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
