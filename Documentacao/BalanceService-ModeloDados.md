# 💳 **BalanceService - Modelo de Dados**

## 📋 **Visão Geral**

O BalanceService gerencia saldos de contas e histórico de transações usando Event Sourcing e CQRS no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `balance_events` (Marten Event Store)
### **Porta**: 5003
### **Tecnologias**: PostgreSQL + Dapper + Marten (Event Store)

---

## 📊 **Entidades Principais**

### **1. Account**

**Descrição**: Conta bancária com saldo

```csharp
public class Account
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public decimal Balance { get; set; }                // Saldo atual
    public string Currency { get; set; } = "BRL";       // Moeda
    public bool IsActive { get; set; } = true;          // Status ativo
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime LastUpdated { get; set; }           // Última atualização
}
```

### **2. TransactionHistory**

**Descrição**: Histórico de transações (Read Model)

```csharp
public class TransactionHistory
{
    public Guid Id { get; set; }                        // PK
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public Guid TransactionId { get; set; }             // ID da transação
    public string ExternalId { get; set; }              // ID externo
    public string Type { get; set; }                    // PIX, TED, BOLETO, CRYPTO
    public decimal Amount { get; set; }                 // Valor
    public string? Description { get; set; }            // Descrição
    public string Status { get; set; }                  // Status
    public string Operation { get; set; }               // DEBIT, CREDIT
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **accounts**
```sql
CREATE TABLE accounts (
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    PRIMARY KEY (client_id, account_id),
    
    CONSTRAINT chk_accounts_balance_positive CHECK (balance >= 0),
    CONSTRAINT chk_accounts_currency_valid CHECK (currency IN ('BRL', 'USD', 'EUR', 'BTC', 'ETH'))
);
```

### **transaction_history**
```sql
CREATE TABLE transaction_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    transaction_id UUID NOT NULL,
    external_id VARCHAR(100) NOT NULL,
    type VARCHAR(20) NOT NULL, -- PIX, TED, BOLETO, CRYPTO
    amount DECIMAL(18,2) NOT NULL,
    description TEXT,
    status VARCHAR(20) NOT NULL, -- PENDING, COMPLETED, FAILED, CANCELLED
    operation VARCHAR(10) NOT NULL, -- DEBIT, CREDIT
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_transaction_history_account 
        FOREIGN KEY (client_id, account_id) 
        REFERENCES accounts(client_id, account_id),
    
    CONSTRAINT chk_transaction_history_type CHECK (type IN ('PIX', 'TED', 'BOLETO', 'CRYPTO')),
    CONSTRAINT chk_transaction_history_status CHECK (status IN ('PENDING', 'COMPLETED', 'FAILED', 'CANCELLED')),
    CONSTRAINT chk_transaction_history_operation CHECK (operation IN ('DEBIT', 'CREDIT')),
    CONSTRAINT chk_transaction_history_amount_positive CHECK (amount > 0)
);
```

---

## 🔄 **Event Sourcing**

### **Domain Events**

#### **AccountCreated**
```csharp
public class AccountCreated : DomainEvent
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public string Currency { get; set; }                // Moeda
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

#### **BalanceUpdated**
```csharp
public class BalanceUpdated : DomainEvent
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public decimal PreviousBalance { get; set; }        // Saldo anterior
    public decimal NewBalance { get; set; }             // Novo saldo
    public decimal Amount { get; set; }                 // Valor da operação
    public string Operation { get; set; }               // DEBIT, CREDIT
    public string Reason { get; set; }                  // Motivo
    public DateTime UpdatedAt { get; set; }             // Data atualização
}
```

#### **TransactionProcessed**
```csharp
public class TransactionProcessed : DomainEvent
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public Guid TransactionId { get; set; }             // ID da transação
    public string ExternalId { get; set; }              // ID externo
    public string Type { get; set; }                    // Tipo transação
    public decimal Amount { get; set; }                 // Valor
    public string Operation { get; set; }               // DEBIT, CREDIT
    public string Status { get; set; }                  // Status
    public string? Description { get; set; }            // Descrição
    public DateTime ProcessedAt { get; set; }           // Data processamento
}
```

---

## 🔑 **DTOs e Requests**

### **CreateAccountRequest**
```csharp
public class CreateAccountRequest
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public string Currency { get; set; } = "BRL";       // Moeda
    public decimal InitialBalance { get; set; } = 0;    // Saldo inicial
}
```

### **UpdateBalanceRequest**
```csharp
public class UpdateBalanceRequest
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public decimal Amount { get; set; }                 // Valor
    public string Operation { get; set; }               // DEBIT, CREDIT
    public string Reason { get; set; }                  // Motivo
    public Guid? TransactionId { get; set; }            // ID transação (opcional)
    public string? ExternalId { get; set; }             // ID externo (opcional)
    public string? Description { get; set; }            // Descrição (opcional)
}
```

### **BalanceResponse**
```csharp
public class BalanceResponse
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public decimal Balance { get; set; }                // Saldo atual
    public string Currency { get; set; }                // Moeda
    public bool IsActive { get; set; }                  // Status ativo
    public DateTime LastUpdated { get; set; }           // Última atualização
}
```

### **TransactionHistoryResponse**
```csharp
public class TransactionHistoryResponse
{
    public Guid Id { get; set; }                        // ID do histórico
    public Guid TransactionId { get; set; }             // ID da transação
    public string ExternalId { get; set; }              // ID externo
    public string Type { get; set; }                    // Tipo
    public decimal Amount { get; set; }                 // Valor
    public string? Description { get; set; }            // Descrição
    public string Status { get; set; }                  // Status
    public string Operation { get; set; }               // DEBIT, CREDIT
    public DateTime CreatedAt { get; set; }             // Data criação
}
```

### **StatementRequest**
```csharp
public class StatementRequest
{
    public Guid ClientId { get; set; }                  // ID do cliente
    public string AccountId { get; set; }               // ID da conta
    public DateTime? StartDate { get; set; }            // Data início
    public DateTime? EndDate { get; set; }              // Data fim
    public string? Type { get; set; }                   // Filtro por tipo
    public string? Operation { get; set; }              // Filtro por operação
    public int Page { get; set; } = 1;                  // Página
    public int PageSize { get; set; } = 50;             // Tamanho página
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Accounts
CREATE INDEX idx_accounts_client_id ON accounts(client_id);
CREATE INDEX idx_accounts_is_active ON accounts(is_active);
CREATE INDEX idx_accounts_last_updated ON accounts(last_updated);

-- Transaction History
CREATE INDEX idx_transaction_history_client_id ON transaction_history(client_id);
CREATE INDEX idx_transaction_history_account_id ON transaction_history(account_id);
CREATE INDEX idx_transaction_history_transaction_id ON transaction_history(transaction_id);
CREATE INDEX idx_transaction_history_external_id ON transaction_history(external_id);
CREATE INDEX idx_transaction_history_type ON transaction_history(type);
CREATE INDEX idx_transaction_history_status ON transaction_history(status);
CREATE INDEX idx_transaction_history_operation ON transaction_history(operation);
CREATE INDEX idx_transaction_history_created_at ON transaction_history(created_at);

-- Composite indexes for common queries
CREATE INDEX idx_transaction_history_client_account_date 
    ON transaction_history(client_id, account_id, created_at DESC);
CREATE INDEX idx_transaction_history_client_type_date 
    ON transaction_history(client_id, type, created_at DESC);
```

---

## 🔄 **CQRS Pattern**

### **Commands**
- `CreateAccountCommand` - Criar nova conta
- `UpdateBalanceCommand` - Atualizar saldo
- `ProcessTransactionCommand` - Processar transação
- `DeactivateAccountCommand` - Desativar conta

### **Queries**
- `GetBalanceQuery` - Obter saldo atual
- `GetTransactionHistoryQuery` - Obter histórico
- `GetStatementQuery` - Obter extrato
- `GetAccountsQuery` - Listar contas

### **Handlers**
- `CreateAccountHandler` - Processa criação de conta
- `UpdateBalanceHandler` - Processa atualização de saldo
- `GetBalanceHandler` - Consulta saldo
- `GetTransactionHistoryHandler` - Consulta histórico

---

## 🔗 **Relacionamentos**

### **Account ↔ TransactionHistory**
- **Tipo**: One-to-Many
- **Chave**: `(client_id, account_id)` → `accounts.(client_id, account_id)`
- **Descrição**: Uma conta pode ter múltiplas transações no histórico

---

## 🎯 **Casos de Uso**

### **Gestão de Saldos**
- Consulta de saldo em tempo real
- Atualização de saldo por transação
- Múltiplas moedas
- Validação de saldo suficiente

### **Histórico de Transações**
- Registro de todas as operações
- Extratos por período
- Filtros por tipo e operação
- Paginação para performance

### **Event Sourcing**
- Rastreabilidade completa
- Replay de eventos
- Auditoria de mudanças
- Recuperação de estado

---

## 🔧 **Configurações**

### **Marten Event Store**
```csharp
builder.Services.AddMarten(options =>
{
    options.Connection(connectionString);
    options.DatabaseSchemaName = "balance_events";
});
```

### **Moedas Suportadas**
- `BRL` - Real Brasileiro
- `USD` - Dólar Americano
- `EUR` - Euro
- `BTC` - Bitcoin
- `ETH` - Ethereum

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
