# 🔐 **AuthService - Modelo de Dados**

## 📋 **Visão Geral**

O AuthService é responsável pela autenticação e autorização no sistema FintechPSP, implementando OAuth 2.0 com suporte a client_credentials e user authentication.

## 🗄️ **Estrutura do Banco de Dados**

### **Schema**: `public` (default)
### **Porta**: 5001
### **Tecnologias**: PostgreSQL + Dapper

---

## 📊 **Entidades Principais**

### **1. SystemUser**

**Descrição**: Usuários do sistema (admins, operadores)

```csharp
public class SystemUser
{
    public Guid Id { get; set; }                    // PK
    public string Email { get; set; }               // Unique
    public string PasswordHash { get; set; }        // BCrypt hash
    public string Name { get; set; }                // Nome completo
    public string Role { get; set; }                // Admin, SubAdmin, etc.
    public bool IsActive { get; set; }              // Status ativo
    public bool IsMaster { get; set; }              // Super admin
    public DateTime? LastLoginAt { get; set; }      // Último login
    public DateTime CreatedAt { get; set; }         // Data criação
    public DateTime? UpdatedAt { get; set; }        // Data atualização
}
```

**Tabela**: `system_users`
```sql
CREATE TABLE system_users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL DEFAULT 'Admin',
    is_active BOOLEAN NOT NULL DEFAULT true,
    is_master BOOLEAN NOT NULL DEFAULT false,
    last_login_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);
```

### **2. Client**

**Descrição**: Clientes OAuth 2.0 para aplicações

```csharp
public class Client
{
    public Guid Id { get; set; }                    // PK
    public string ClientId { get; set; }            // Unique identifier
    public string ClientSecret { get; set; }        // Secret hash
    public string Name { get; set; }                // Nome da aplicação
    public string[] AllowedScopes { get; set; }     // Escopos permitidos
    public bool IsActive { get; set; }              // Status ativo
    public DateTime CreatedAt { get; set; }         // Data criação
    public DateTime? UpdatedAt { get; set; }        // Data atualização
}
```

**Tabela**: `clients`
```sql
CREATE TABLE clients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id VARCHAR(255) UNIQUE NOT NULL,
    client_secret VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    allowed_scopes TEXT NOT NULL DEFAULT 'pix,banking,admin',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);
```

### **3. AuthAudit**

**Descrição**: Log de auditoria para tentativas de autenticação

```csharp
public class AuthAudit
{
    public Guid Id { get; set; }                    // PK
    public string? ClientId { get; set; }           // Cliente OAuth
    public string? UserId { get; set; }             // ID do usuário
    public string IpAddress { get; set; }           // IP da requisição
    public string UserAgent { get; set; }           // User agent
    public bool Success { get; set; }               // Sucesso/falha
    public string? ErrorMessage { get; set; }       // Mensagem de erro
    public DateTime CreatedAt { get; set; }         // Data da tentativa
}
```

**Tabela**: `auth_audit`
```sql
CREATE TABLE auth_audit (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id VARCHAR(255),
    user_id VARCHAR(255),
    ip_address INET NOT NULL,
    user_agent TEXT,
    success BOOLEAN NOT NULL,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);
```

---

## 🔑 **DTOs e Requests**

### **LoginRequest**
```csharp
public class LoginRequest
{
    public string Email { get; set; }               // Email do usuário
    public string Password { get; set; }            // Senha em texto
}
```

### **LoginResponse**
```csharp
public class LoginResponse
{
    public string AccessToken { get; set; }         // JWT token
    public string TokenType { get; set; }           // "Bearer"
    public int ExpiresIn { get; set; }              // Segundos até expirar
    public UserInfo User { get; set; }              // Dados do usuário
}
```

### **TokenRequest** (OAuth 2.0)
```csharp
public class TokenRequest
{
    public string GrantType { get; set; }           // "client_credentials"
    public string ClientId { get; set; }            // ID do cliente
    public string ClientSecret { get; set; }        // Secret do cliente
    public string? Scope { get; set; }              // Escopos solicitados
}
```

### **TokenResponse** (OAuth 2.0)
```csharp
public class TokenResponse
{
    public string AccessToken { get; set; }         // JWT token
    public string TokenType { get; set; }           // "Bearer"
    public int ExpiresIn { get; set; }              // Segundos até expirar
    public string? Scope { get; set; }              // Escopos concedidos
}
```

---

## 📈 **Índices e Performance**

### **Índices Criados**
```sql
-- SystemUsers
CREATE INDEX idx_system_users_email ON system_users(email);
CREATE INDEX idx_system_users_is_active ON system_users(is_active);
CREATE INDEX idx_system_users_role ON system_users(role);

-- Clients
CREATE INDEX idx_clients_client_id ON clients(client_id);
CREATE INDEX idx_clients_is_active ON clients(is_active);

-- AuthAudit
CREATE INDEX idx_auth_audit_client_id ON auth_audit(client_id);
CREATE INDEX idx_auth_audit_created_at ON auth_audit(created_at);
CREATE INDEX idx_auth_audit_success ON auth_audit(success);
```

---

## 🔐 **Segurança**

### **Hash de Senhas**
- **Algoritmo**: BCrypt
- **Work Factor**: 10
- **Salt**: Automático pelo BCrypt

### **JWT Tokens**
- **Algoritmo**: HS256
- **Issuer**: "Mortadela" (configurável)
- **Audience**: "Mortadela" (configurável)
- **Expiração**: 1 hora (configurável)

### **Escopos OAuth 2.0**
- `pix` - Transações PIX
- `banking` - Operações bancárias
- `admin` - Administração do sistema

---

## 🚀 **Dados Iniciais**

### **Usuário Master**
```sql
INSERT INTO system_users (email, password_hash, name, role, is_active, is_master) 
VALUES ('admin@fintechpsp.com', '$2b$10$N9qo8uLOickgx2ZMRZoMye.IjPeGvGzjYwjUxcHjRMA4nAFPiO/Xi', 
        'Administrador Master', 'Admin', true, true);
```
**Credenciais**: `admin@fintechpsp.com` / `admin123`

### **Clientes OAuth Padrão**
```sql
INSERT INTO clients (client_id, client_secret, name, allowed_scopes) VALUES
('fintech_web_app', 'web_app_secret_123', 'Fintech Web Application', 'pix,banking'),
('fintech_admin', 'admin_secret_789', 'Fintech Admin Panel', 'pix,banking,admin'),
('admin_backoffice', 'admin_secret_000', 'Admin Backoffice', 'admin'),
('cliente_banking', 'cliente_secret_000', 'Cliente Banking', 'banking');
```

---

## 🔄 **Fluxos de Autenticação**

### **1. Login de Usuário**
1. `POST /auth/login` com email/senha
2. Validação de credenciais
3. Geração de JWT token
4. Log de auditoria
5. Retorno do token + dados do usuário

### **2. OAuth 2.0 Client Credentials**
1. `POST /auth/token` com client_id/client_secret
2. Validação do cliente
3. Verificação de escopos
4. Geração de JWT token
5. Log de auditoria
6. Retorno do token

---

## 📊 **Métricas e Monitoramento**

### **Endpoints de Health**
- `GET /health` - Status do serviço

### **Logs de Auditoria**
- Todas as tentativas de login são registradas
- IP, User Agent e resultado são armazenados
- Facilita investigação de segurança

---

## 🔧 **Configurações**

### **JWT Settings**
```json
{
  "Jwt": {
    "Key": "sua-chave-secreta-super-segura-aqui",
    "Issuer": "Mortadela",
    "Audience": "Mortadela",
    "ExpiryMinutes": 60
  }
}
```

### **Database Connection**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=fintechpsp;Username=postgres;Password=postgres"
  }
}
```

---

## 🔗 **Relacionamentos**

### **SystemUser ↔ AuthAudit**
- **Tipo**: One-to-Many
- **Chave**: `user_id` (string, não FK formal)
- **Descrição**: Um usuário pode ter múltiplos logs de auditoria

### **Client ↔ AuthAudit**
- **Tipo**: One-to-Many
- **Chave**: `client_id` (string, não FK formal)
- **Descrição**: Um cliente pode ter múltiplos logs de auditoria

---

## 🎯 **Casos de Uso**

### **Autenticação de Usuário**
- Login de administradores
- Login de operadores
- Validação de permissões

### **Autenticação de Aplicação**
- Integração entre microserviços
- Autenticação de frontends
- APIs externas

### **Auditoria e Segurança**
- Rastreamento de acessos
- Detecção de tentativas maliciosas
- Compliance e logs

---

**📝 Última atualização**: 2025-10-08
**🔄 Versão**: 1.0.0
