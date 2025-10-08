# ⚙️ **ConfigService - Modelo de Dados**

## 📋 **Visão Geral**

O ConfigService gerencia configurações do sistema, priorização de contas bancárias e configurações bancárias no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5007
### **Tecnologias**: PostgreSQL + Dapper

---

## 📊 **Entidades Principais**

### **1. BankingConfig**

**Descrição**: Configurações bancárias do sistema

```csharp
public class BankingConfig
{
    public Guid Id { get; set; }                        // PK
    public string Name { get; set; }                    // Nome da configuração
    public string Type { get; set; }                    // Tipo (pix, ted, boleto)
    public bool Enabled { get; set; } = true;           // Habilitado
    public string? Settings { get; set; }               // JSON com configurações
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
    public string? CreatedBy { get; set; }              // Criado por
    public string? UpdatedBy { get; set; }              // Atualizado por
}
```

### **2. ConfiguracaoPriorizacao**

**Descrição**: Configuração de priorização de contas por cliente

```csharp
public class ConfiguracaoPriorizacao
{
    public Guid Id { get; set; }                        // PK
    public Guid ClienteId { get; set; }                 // ID do cliente
    public string PrioridadesJson { get; set; }         // JSON com prioridades
    public decimal TotalPercentual { get; set; }        // Total percentual (100%)
    public bool IsValid { get; set; }                   // Configuração válida
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
}
```

### **3. BancoPersonalizado**

**Descrição**: Bancos personalizados configurados pelo cliente

```csharp
public class BancoPersonalizado
{
    public Guid Id { get; set; }                        // PK
    public Guid ClienteId { get; set; }                 // ID do cliente
    public string BankCode { get; set; }                // Código do banco
    public string? Endpoint { get; set; }               // Endpoint da API
    public string? CredentialsTemplate { get; set; }    // Template credenciais
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
}
```

### **4. SystemConfig**

**Descrição**: Configurações gerais do sistema

```csharp
public class SystemConfig
{
    public Guid Id { get; set; }                        // PK
    public string ConfigKey { get; set; }               // Chave única
    public string ConfigValue { get; set; }             // Valor
    public string? Description { get; set; }            // Descrição
    public bool IsActive { get; set; } = true;          // Ativo
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime UpdatedAt { get; set; }             // Data atualização
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **banking_configs**
```sql
CREATE TABLE banking_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT true,
    settings JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),

    CONSTRAINT uk_banking_configs_name UNIQUE (name)
);
```

### **configuracoes_priorizacao**
```sql
CREATE TABLE configuracoes_priorizacao (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID UNIQUE NOT NULL,
    prioridades_json JSONB NOT NULL,
    total_percentual DECIMAL(5,2) NOT NULL,
    is_valid BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT chk_total_percentual CHECK (total_percentual >= 0 AND total_percentual <= 100)
);
```

### **bancos_personalizados**
```sql
CREATE TABLE bancos_personalizados (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(10) NOT NULL,
    endpoint VARCHAR(500),
    credentials_template TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT uk_bancos_unique_client_code UNIQUE (cliente_id, bank_code)
);
```

### **system_configs**
```sql
CREATE TABLE system_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    config_key VARCHAR(255) UNIQUE NOT NULL,
    config_value TEXT NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);
```

---

## 🔑 **DTOs e Requests**

### **CreateBankingConfigRequest**
```csharp
public class CreateBankingConfigRequest
{
    public string Name { get; set; }                    // Nome
    public string Type { get; set; }                    // Tipo
    public bool Enabled { get; set; } = true;           // Habilitado
    public object? Settings { get; set; }               // Configurações
}
```

### **ConfigurarPriorizacaoRequest**
```csharp
public class ConfigurarPriorizacaoRequest
{
    public Guid? ClienteId { get; set; }                // ID cliente (opcional)
    public List<PrioridadeContaRequest> Prioridades { get; set; } = new();
}

public class PrioridadeContaRequest
{
    public string ContaId { get; set; }                 // ID da conta
    public string BankCode { get; set; }                // Código banco
    public decimal Percentual { get; set; }             // Percentual (0-100)
    public int Ordem { get; set; }                      // Ordem prioridade
}
```

### **ConfigurarBancoRequest**
```csharp
public class ConfigurarBancoRequest
{
    public Guid? ClienteId { get; set; }                // ID cliente (opcional)
    public string BankCode { get; set; }                // Código banco
    public string? Endpoint { get; set; }               // Endpoint API
    public string? CredentialsTemplate { get; set; }    // Template credenciais
}
```

### **BankingConfigResponse**
```csharp
public class BankingConfigResponse
{
    public Guid Id { get; set; }                        // ID
    public string Name { get; set; }                    // Nome
    public string Type { get; set; }                    // Tipo
    public bool Enabled { get; set; }                   // Habilitado
    public object? Settings { get; set; }               // Configurações
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? UpdatedAt { get; set; }            // Data atualização
    public string? CreatedBy { get; set; }              // Criado por
}
```

### **PriorizacaoDetalhesResponse**
```csharp
public class PriorizacaoDetalhesResponse
{
    public Guid ConfigId { get; set; }                  // ID configuração
    public Guid ClienteId { get; set; }                 // ID cliente
    public List<PrioridadeContaResponse> Prioridades { get; set; } = new();
    public decimal TotalPercentual { get; set; }        // Total percentual
    public bool IsValid { get; set; }                   // Válida
    public DateTime CreatedAt { get; set; }             // Data criação
    public DateTime? LastUpdated { get; set; }          // Última atualização
}

public class PrioridadeContaResponse
{
    public string ContaId { get; set; }                 // ID conta
    public string BankCode { get; set; }                // Código banco
    public decimal Percentual { get; set; }             // Percentual
    public int Ordem { get; set; }                      // Ordem
}
```

### **BancoDefaultResponse**
```csharp
public class BancoDefaultResponse
{
    public string BankCode { get; set; }                // Código banco
    public string Name { get; set; }                    // Nome banco
    public bool IsSupported { get; set; }               // Suportado
}
```

### **SystemConfigResponse**
```csharp
public class SystemConfigResponse
{
    public bool MaintenanceMode { get; set; }           // Modo manutenção
    public bool PixEnabled { get; set; }                // PIX habilitado
    public bool TedEnabled { get; set; }                // TED habilitado
    public bool BoletoEnabled { get; set; }             // Boleto habilitado
    public bool CryptoEnabled { get; set; }             // Crypto habilitado
    public int MaxRetryAttempts { get; set; }           // Max tentativas
    public int TimeoutSeconds { get; set; }             // Timeout
    public int RateLimitPerMinute { get; set; }         // Rate limit
}
```

---

## 🏦 **Bancos Padrão**

### **Bancos Suportados**
```csharp
public static List<BancoDefault> GetBancosDefault()
{
    return new List<BancoDefault>
    {
        new() { BankCode = "STARK", Name = "Stark Bank", IsSupported = true },
        new() { BankCode = "SICOOB", Name = "Sicoob", IsSupported = true },
        new() { BankCode = "GENIAL", Name = "Banco Genial", IsSupported = true },
        new() { BankCode = "EFI", Name = "Efí (Gerencianet)", IsSupported = true },
        new() { BankCode = "CELCOIN", Name = "Celcoin", IsSupported = true }
    };
}
```

### **Configurações Bancárias Padrão**
```sql
INSERT INTO banking_configs (name, type, enabled, settings, created_by) VALUES
('Stark Bank PIX', 'pix', true, '{"api_key": "sk_test_***", "environment": "sandbox", "timeout": 30}', 'system'),
('Sicoob PIX', 'pix', true, '{"client_id": "sicoob_***", "client_secret": "***", "environment": "sandbox"}', 'system'),
('Banco Genial TED', 'ted', true, '{"agency": "0001", "account": "123456", "environment": "sandbox"}', 'system'),
('Efí Boleto', 'boleto', true, '{"client_id": "efi_***", "client_secret": "***", "certificate_path": "/certs/efi.p12"}', 'system');
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- Banking Configs
CREATE INDEX idx_banking_configs_type ON banking_configs(type);
CREATE INDEX idx_banking_configs_enabled ON banking_configs(enabled);

-- Configurações Priorização
CREATE INDEX idx_priorizacao_cliente_id ON configuracoes_priorizacao(cliente_id);
CREATE INDEX idx_priorizacao_is_valid ON configuracoes_priorizacao(is_valid);

-- Bancos Personalizados
CREATE INDEX idx_bancos_cliente_id ON bancos_personalizados(cliente_id);
CREATE INDEX idx_bancos_bank_code ON bancos_personalizados(bank_code);

-- System Configs
CREATE INDEX idx_system_configs_key ON system_configs(config_key);
CREATE INDEX idx_system_configs_active ON system_configs(is_active);
```

---

## 🔧 **Configurações do Sistema**

### **Configurações Padrão**
```sql
INSERT INTO system_configs (config_key, config_value, description) VALUES
('max_transaction_amount', '1000000.00', 'Valor máximo permitido para transações em reais'),
('pix_timeout_seconds', '300', 'Timeout em segundos para transações PIX'),
('webhook_retry_attempts', '3', 'Número máximo de tentativas para webhooks'),
('qr_code_expiration_minutes', '30', 'Tempo de expiração padrão para QR codes dinâmicos em minutos');
```

---

## 🎯 **Casos de Uso**

### **Configurações Bancárias**
- Gerenciamento de integrações
- Configuração de credenciais
- Habilitação/desabilitação de serviços
- Múltiplos ambientes (sandbox/production)

### **Priorização de Contas**
- Distribuição percentual de transações
- Múltiplas contas por cliente
- Validação de percentuais (total = 100%)
- Ordem de prioridade

### **Bancos Personalizados**
- Integração com bancos não padrão
- Endpoints customizados
- Templates de credenciais
- Configuração por cliente

### **Configurações do Sistema**
- Parâmetros globais
- Limites e timeouts
- Funcionalidades habilitadas
- Rate limiting

---

## 🔗 **Relacionamentos**

### **ConfiguracaoPriorizacao ↔ Cliente**
- **Tipo**: One-to-One
- **Chave**: `cliente_id` (referência externa)
- **Descrição**: Cada cliente tem uma configuração de priorização

### **BancoPersonalizado ↔ Cliente**
- **Tipo**: One-to-Many
- **Chave**: `cliente_id` (referência externa)
- **Descrição**: Cliente pode ter múltiplos bancos personalizados

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
