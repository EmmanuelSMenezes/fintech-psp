# 💰 **TransactionService - Modelo de Dados**

## 📋 **Visão Geral**

O TransactionService processa todas as transações financeiras (PIX, TED, Boleto, Crypto) e gerencia QR Codes no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5002
### **Tecnologias**: PostgreSQL + Dapper + Event Sourcing

---

## 📊 **Entidades Principais**

### **1. Transaction (Aggregate Root)**

**Descrição**: Transação financeira principal

```csharp
public class Transaction : AggregateRoot
{
    public Guid TransactionId { get; private set; }     // PK
    public string ExternalId { get; private set; }      // ID externo único
    public TransactionType Type { get; private set; }   // PIX, TED, BOLETO, CRYPTO
    public TransactionStatus Status { get; private set; } // Status da transação
    public Money Amount { get; private set; }           // Valor + moeda
    public string? BankCode { get; private set; }       // Código do banco
    
    // Campos específicos PIX
    public string? PixKey { get; private set; }         // Chave PIX
    public string? EndToEndId { get; private set; }     // E2E ID
    
    // Campos específicos TED
    public string? AccountBranch { get; private set; }  // Agência
    public string? AccountNumber { get; private set; }  // Conta
    public string? TaxId { get; private set; }          // CPF/CNPJ
    public string? Name { get; private set; }           // Nome do beneficiário
    
    // Campos comuns
    public string? Description { get; private set; }    // Descrição
    public string? WebhookUrl { get; private set; }     // URL webhook
    public DateTime? DueDate { get; private set; }      // Vencimento (boleto)
    public string? PayerTaxId { get; private set; }     // CPF/CNPJ pagador
    public string? PayerName { get; private set; }      // Nome pagador
    public string? Instructions { get; private set; }   // Instruções
    
    // Campos específicos Crypto
    public string? CryptoType { get; private set; }     // BTC, ETH, etc.
    public string? WalletAddress { get; private set; }  // Endereço carteira
    
    public DateTime CreatedAt { get; private set; }     // Data criação
}
```

### **2. QrCode**

**Descrição**: QR Codes PIX (estáticos e dinâmicos)

```csharp
public class QrCode
{
    public Guid Id { get; set; }                        // PK
    public string ExternalId { get; set; }              // ID externo único
    public QrCodeType Type { get; set; }                // STATIC, DYNAMIC
    public string PixKey { get; set; }                  // Chave PIX
    public decimal? Amount { get; set; }                // Valor (dinâmico)
    public string? Description { get; set; }            // Descrição
    public string BankCode { get; set; }                // Código do banco
    public string EmvCode { get; set; }                 // Código EMV
    public string? QrCodeImageUrl { get; set; }         // URL da imagem
    public QrCodeStatus Status { get; set; }            // Status
    public DateTime? ExpiresAt { get; set; }            // Expiração
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **transactions**
```sql
CREATE TABLE transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id VARCHAR(255) UNIQUE NOT NULL,
    type VARCHAR(50) NOT NULL CHECK (type IN ('PIX', 'TED', 'BOLETO', 'CRYPTO')),
    status VARCHAR(50) NOT NULL CHECK (status IN (
        'PENDING', 'PROCESSING', 'CONFIRMED', 'FAILED', 'CANCELLED',
        'REJECTED', 'EXPIRED', 'UNDER_ANALYSIS', 'INITIATED', 'ISSUED'
    )),
    amount DECIMAL(15,2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    bank_code VARCHAR(10),
    
    -- Campos específicos PIX
    pix_key VARCHAR(255),
    end_to_end_id VARCHAR(32),
    
    -- Campos específicos TED
    account_branch VARCHAR(10),
    account_number VARCHAR(20),
    tax_id VARCHAR(20),
    name VARCHAR(255),
    
    -- Campos comuns
    description TEXT,
    webhook_url TEXT,
    due_date TIMESTAMP WITH TIME ZONE,
    payer_tax_id VARCHAR(20),
    payer_name VARCHAR(255),
    instructions TEXT,
    
    -- Campos específicos Crypto
    crypto_type VARCHAR(10),
    wallet_address VARCHAR(255),
    
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
```

### **qr_codes**
```sql
CREATE TABLE qr_codes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id VARCHAR(255) UNIQUE NOT NULL,
    type VARCHAR(20) NOT NULL CHECK (type IN ('STATIC', 'DYNAMIC')),
    pix_key VARCHAR(255) NOT NULL,
    amount DECIMAL(15,2),
    description TEXT,
    bank_code VARCHAR(10) NOT NULL,
    emv_code TEXT NOT NULL,
    qr_code_image_url TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE' CHECK (status IN (
        'ACTIVE', 'EXPIRED', 'USED', 'CANCELLED'
    )),
    expires_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);
```

---

## 🔄 **Enums e Value Objects**

### **TransactionType**
```csharp
public enum TransactionType
{
    PIX,        // Pagamento instantâneo
    TED,        // Transferência eletrônica
    BOLETO,     // Boleto bancário
    CRYPTO      // Criptomoeda
}
```

### **TransactionStatus**
```csharp
public enum TransactionStatus
{
    PENDING,        // Pendente
    PROCESSING,     // Processando
    CONFIRMED,      // Confirmada
    FAILED,         // Falhou
    CANCELLED,      // Cancelada
    REJECTED,       // Rejeitada
    EXPIRED,        // Expirada
    UNDER_ANALYSIS, // Em análise
    INITIATED,      // Iniciada
    ISSUED          // Emitida
}
```

### **QrCodeType**
```csharp
public enum QrCodeType
{
    STATIC,     // QR Code estático (sem valor)
    DYNAMIC     // QR Code dinâmico (com valor)
}
```

### **QrCodeStatus**
```csharp
public enum QrCodeStatus
{
    ACTIVE,     // Ativo
    EXPIRED,    // Expirado
    USED,       // Usado
    CANCELLED   // Cancelado
}
```

### **Money (Value Object)**
```csharp
public class Money
{
    public decimal Amount { get; private set; }         // Valor
    public string Currency { get; private set; }        // Moeda (BRL, USD, etc.)
    
    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency ?? "BRL";
    }
}
```

---

## 🔑 **DTOs e Requests**

### **QrCodeEstaticoRequest**
```csharp
public class QrCodeEstaticoRequest
{
    public string ExternalId { get; set; }              // ID único
    public string PixKey { get; set; }                  // Chave PIX
    public string BankCode { get; set; }                // Código banco
    public string? Description { get; set; }            // Descrição
}
```

### **QrCodeDinamicoRequest**
```csharp
public class QrCodeDinamicoRequest
{
    public string ExternalId { get; set; }              // ID único
    public decimal Amount { get; set; }                 // Valor
    public string PixKey { get; set; }                  // Chave PIX
    public string BankCode { get; set; }                // Código banco
    public string? Description { get; set; }            // Descrição
    public int? ExpiresIn { get; set; }                 // Expira em (segundos)
}
```

### **CreateTransactionRequest**
```csharp
public class CreateTransactionRequest
{
    public string ExternalId { get; set; }              // ID único
    public string Type { get; set; }                    // Tipo transação
    public decimal Amount { get; set; }                 // Valor
    public string Currency { get; set; } = "BRL";       // Moeda
    public string? BankCode { get; set; }               // Código banco
    public string? PixKey { get; set; }                 // Chave PIX
    public string? Description { get; set; }            // Descrição
    public string? WebhookUrl { get; set; }             // URL webhook
    
    // Campos específicos por tipo
    public string? AccountBranch { get; set; }          // TED
    public string? AccountNumber { get; set; }          // TED
    public string? TaxId { get; set; }                  // TED/Boleto
    public string? Name { get; set; }                   // TED/Boleto
    public DateTime? DueDate { get; set; }              // Boleto
    public string? CryptoType { get; set; }             // Crypto
    public string? WalletAddress { get; set; }          // Crypto
}
```

### **TransactionResponse**
```csharp
public class TransactionResponse
{
    public Guid TransactionId { get; set; }             // ID da transação
    public string ExternalId { get; set; }              // ID externo
    public string Type { get; set; }                    // Tipo
    public string Status { get; set; }                  // Status
    public decimal Amount { get; set; }                 // Valor
    public string Currency { get; set; }                // Moeda
    public string? BankCode { get; set; }               // Código banco
    public string? PixKey { get; set; }                 // Chave PIX
    public string? EndToEndId { get; set; }             // E2E ID
    public string? Description { get; set; }            // Descrição
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Transactions
CREATE INDEX idx_transactions_external_id ON transactions(external_id);
CREATE INDEX idx_transactions_type ON transactions(type);
CREATE INDEX idx_transactions_status ON transactions(status);
CREATE INDEX idx_transactions_bank_code ON transactions(bank_code);
CREATE INDEX idx_transactions_pix_key ON transactions(pix_key);
CREATE INDEX idx_transactions_created_at ON transactions(created_at);
CREATE INDEX idx_transactions_end_to_end_id ON transactions(end_to_end_id);

-- QR Codes
CREATE INDEX idx_qr_codes_external_id ON qr_codes(external_id);
CREATE INDEX idx_qr_codes_type ON qr_codes(type);
CREATE INDEX idx_qr_codes_status ON qr_codes(status);
CREATE INDEX idx_qr_codes_pix_key ON qr_codes(pix_key);
CREATE INDEX idx_qr_codes_bank_code ON qr_codes(bank_code);
CREATE INDEX idx_qr_codes_created_at ON qr_codes(created_at);
CREATE INDEX idx_qr_codes_expires_at ON qr_codes(expires_at);
```

---

## 🎯 **Casos de Uso**

### **Transações PIX**
- Pagamentos instantâneos
- QR Codes estáticos e dinâmicos
- Integração com bancos
- Webhooks de notificação

### **Transações TED**
- Transferências entre bancos
- Validação de dados bancários
- Processamento assíncrono

### **Boletos**
- Geração de boletos
- Controle de vencimento
- Notificações de pagamento

### **Criptomoedas**
- Transações em crypto
- Múltiplas moedas
- Carteiras digitais

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
