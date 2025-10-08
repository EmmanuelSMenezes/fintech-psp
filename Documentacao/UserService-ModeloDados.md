# 👥 **UserService - Modelo de Dados**

## 📋 **Visão Geral**

O UserService gerencia usuários, contas bancárias e controle de acesso (RBAC) no sistema FintechPSP.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5006
### **Tecnologias**: PostgreSQL + Dapper + Marten (Event Store)

---

## 📊 **Entidades Principais**

### **1. SystemUser**

**Descrição**: Usuários do sistema (clientes, admins)

```csharp
public class SystemUser
{
    public Guid Id { get; set; }                    // PK
    public string Email { get; set; }               // Unique
    public string PasswordHash { get; set; }        // BCrypt hash
    public string Name { get; set; }                // Nome completo
    public string Role { get; set; }                // cliente, admin, etc.
    public bool IsActive { get; set; }              // Status ativo
    public bool IsMaster { get; set; }              // Super admin
    public string? Document { get; set; }           // CPF/CNPJ
    public string? Phone { get; set; }              // Telefone
    public string? Address { get; set; }            // Endereço
    public DateTime? LastLoginAt { get; set; }      // Último login
    public DateTime CreatedAt { get; set; }         // Data criação
    public DateTime? UpdatedAt { get; set; }        // Data atualização
}
```

### **2. BankAccount**

**Descrição**: Contas bancárias dos clientes

```csharp
public class BankAccount
{
    public Guid ContaId { get; set; }               // PK
    public Guid ClienteId { get; set; }             // FK para SystemUser
    public string BankCode { get; set; }            // Código do banco
    public string AccountNumber { get; set; }       // Número da conta
    public string? Description { get; set; }        // Descrição
    public string CredentialsTokenId { get; set; }  // Token de credenciais
    public bool IsActive { get; set; }              // Status ativo
    public DateTime CreatedAt { get; set; }         // Data criação
    public DateTime? UpdatedAt { get; set; }        // Data atualização
}
```

### **3. Acesso (RBAC)**

**Descrição**: Controle de acesso baseado em roles

```csharp
public class Acesso
{
    public Guid AcessoId { get; set; }              // PK
    public Guid UserId { get; set; }                // FK para SystemUser
    public Guid? ParentUserId { get; set; }         // Para sub-usuários
    public string Email { get; set; }               // Email do usuário
    public string Role { get; set; }                // Role do usuário
    public List<string> Permissions { get; set; }   // Permissões específicas
    public bool IsActive { get; set; }              // Status ativo
    public DateTime CreatedAt { get; set; }         // Data criação
    public DateTime? UpdatedAt { get; set; }        // Data atualização
    public Guid CreatedBy { get; set; }             // Quem criou
}
```

---

## 🗂️ **Estrutura das Tabelas**

### **system_users**
```sql
CREATE TABLE system_users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'cliente',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_master BOOLEAN NOT NULL DEFAULT false,
    document VARCHAR(20),
    phone VARCHAR(20),
    address TEXT,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);
```

### **bank_accounts**
```sql
CREATE TABLE bank_accounts (
    conta_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(10) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    description TEXT,
    credentials_token_id VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT fk_bank_accounts_cliente 
        FOREIGN KEY (cliente_id) REFERENCES system_users(id)
);
```

### **acessos**
```sql
CREATE TABLE acessos (
    acesso_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    parent_user_id UUID,
    email VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL,
    permissions JSONB NOT NULL DEFAULT '[]',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    created_by UUID NOT NULL,
    
    CONSTRAINT fk_acessos_user 
        FOREIGN KEY (user_id) REFERENCES system_users(id),
    CONSTRAINT fk_acessos_parent 
        FOREIGN KEY (parent_user_id) REFERENCES system_users(id),
    CONSTRAINT fk_acessos_created_by 
        FOREIGN KEY (created_by) REFERENCES system_users(id)
);
```

---

## 🔑 **DTOs e Requests**

### **CreateClientUserRequest**
```csharp
public class CreateClientUserRequest
{
    public string Name { get; set; }                // Nome completo
    public string Email { get; set; }               // Email único
    public string Document { get; set; }            // CPF/CNPJ
    public string? Phone { get; set; }              // Telefone
    public string? Address { get; set; }            // Endereço
}
```

### **CreateBankAccountRequest**
```csharp
public class CreateBankAccountRequest
{
    public string ClienteId { get; set; }           // ID do cliente
    public string BankCode { get; set; }            // Código do banco
    public string AccountNumber { get; set; }       // Número da conta
    public string? Description { get; set; }        // Descrição
    public Credentials Credentials { get; set; }    // Credenciais
}

public class Credentials
{
    public string ClientId { get; set; }            // Client ID
    public string ClientSecret { get; set; }        // Client Secret
    public string? ApiKey { get; set; }             // API Key
    public string Environment { get; set; }         // sandbox/production
    public string? MtlsCert { get; set; }           // Certificado mTLS
}
```

### **UserResponse**
```csharp
public class UserResponse
{
    public Guid Id { get; set; }                    // ID do usuário
    public string Name { get; set; }                // Nome
    public string Email { get; set; }               // Email
    public string Document { get; set; }            // Documento
    public string? Phone { get; set; }              // Telefone
    public string? Address { get; set; }            // Endereço
    public bool Active { get; set; }                // Status
    public DateTime CreatedAt { get; set; }         // Data criação
    public string? Role { get; set; }               // Role
    public DateTime? LastLoginAt { get; set; }      // Último login
}
```

---

## 🎭 **Sistema de Roles e Permissões**

### **Roles Disponíveis**
```csharp
public enum SystemRole
{
    Admin,          // Administrador geral
    SubAdmin,       // Sub-administrador
    Cliente,        // Cliente principal
    SubCliente      // Sub-usuário do cliente
}
```

### **Permissões do Sistema**
```csharp
public enum SystemPermission
{
    // Visualização
    ViewDashboard, ViewTransacoes, ViewContas, ViewClientes,
    ViewRelatorios, ViewExtratos, ViewSaldo,
    
    // Edição
    EditContas, EditClientes, EditConfiguracoes, EditAcessos,
    
    // Transações
    TransacionarPix, TransacionarTed, TransacionarBoleto,
    TransacionarCripto, GerarQrCode,
    
    // Administração
    ManageUsers, ManagePermissions, ManageSystem, ViewAuditLogs,
    
    // Configurações
    ConfigurarPriorizacao, ConfigurarBancos, ConfigurarIntegracoes
}
```

### **Mapeamento Role → Permissões**
```csharp
public static class RolePermissions
{
    public static List<string> GetPermissionsForRole(string role)
    {
        return role.ToLower() switch
        {
            "admin" => GetAllPermissions(),
            "subadmin" => GetSubAdminPermissions(),
            "cliente" => GetClientePermissions(),
            "subcliente" => GetSubClientePermissions(),
            _ => new List<string>()
        };
    }
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- SystemUsers
CREATE INDEX idx_system_users_email ON system_users(email);
CREATE INDEX idx_system_users_document ON system_users(document);
CREATE INDEX idx_system_users_role ON system_users(role);
CREATE INDEX idx_system_users_is_active ON system_users(is_active);

-- BankAccounts
CREATE INDEX idx_bank_accounts_cliente_id ON bank_accounts(cliente_id);
CREATE INDEX idx_bank_accounts_bank_code ON bank_accounts(bank_code);
CREATE INDEX idx_bank_accounts_is_active ON bank_accounts(is_active);

-- Acessos
CREATE INDEX idx_acessos_user_id ON acessos(user_id);
CREATE INDEX idx_acessos_parent_user_id ON acessos(parent_user_id);
CREATE INDEX idx_acessos_email ON acessos(email);
CREATE INDEX idx_acessos_role ON acessos(role);
CREATE INDEX idx_acessos_is_active ON acessos(is_active);
```

---

## 🔗 **Relacionamentos**

### **SystemUser ↔ BankAccount**
- **Tipo**: One-to-Many
- **Chave**: `cliente_id` → `system_users.id`
- **Descrição**: Um usuário pode ter múltiplas contas bancárias

### **SystemUser ↔ Acesso**
- **Tipo**: One-to-Many
- **Chave**: `user_id` → `system_users.id`
- **Descrição**: Um usuário pode ter múltiplos acessos (hierarquia)

### **Acesso ↔ Acesso (Parent)**
- **Tipo**: Self-referencing
- **Chave**: `parent_user_id` → `acessos.user_id`
- **Descrição**: Sub-usuários vinculados a usuários principais

---

## 🎯 **Casos de Uso**

### **Gestão de Usuários**
- Cadastro de clientes
- Criação de sub-usuários
- Controle de permissões
- Ativação/desativação

### **Gestão de Contas**
- Vinculação de contas bancárias
- Configuração de credenciais
- Múltiplas contas por cliente

### **Controle de Acesso**
- RBAC (Role-Based Access Control)
- Hierarquia de usuários
- Permissões granulares

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0
