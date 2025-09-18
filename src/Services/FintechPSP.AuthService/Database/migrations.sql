-- AuthService Database Schema

-- Tabela de clientes OAuth
CREATE TABLE IF NOT EXISTS clients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id VARCHAR(255) UNIQUE NOT NULL,
    client_secret VARCHAR(255) NOT NULL,
    name VARCHAR(255) NOT NULL,
    allowed_scopes TEXT NOT NULL DEFAULT 'pix,banking,admin',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_clients_client_id ON clients(client_id);
CREATE INDEX IF NOT EXISTS idx_clients_is_active ON clients(is_active);

-- Tabela de tokens ativos (para revogação)
CREATE TABLE IF NOT EXISTS active_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id VARCHAR(255) NOT NULL,
    token_hash VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    scopes TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    FOREIGN KEY (client_id) REFERENCES clients(client_id)
);

-- Índices para tokens
CREATE INDEX IF NOT EXISTS idx_active_tokens_client_id ON active_tokens(client_id);
CREATE INDEX IF NOT EXISTS idx_active_tokens_expires_at ON active_tokens(expires_at);
CREATE INDEX IF NOT EXISTS idx_active_tokens_token_hash ON active_tokens(token_hash);

-- Tabela de auditoria de autenticação
CREATE TABLE IF NOT EXISTS auth_audit (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id VARCHAR(255) NOT NULL,
    ip_address INET,
    user_agent TEXT,
    scopes_requested TEXT,
    scopes_granted TEXT,
    success BOOLEAN NOT NULL,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Índices para auditoria
CREATE INDEX IF NOT EXISTS idx_auth_audit_client_id ON auth_audit(client_id);
CREATE INDEX IF NOT EXISTS idx_auth_audit_created_at ON auth_audit(created_at);
CREATE INDEX IF NOT EXISTS idx_auth_audit_success ON auth_audit(success);

-- Tabela de usuários do sistema
CREATE TABLE IF NOT EXISTS system_users (
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

-- Índices para usuários
CREATE INDEX IF NOT EXISTS idx_system_users_email ON system_users(email);
CREATE INDEX IF NOT EXISTS idx_system_users_is_active ON system_users(is_active);
CREATE INDEX IF NOT EXISTS idx_system_users_role ON system_users(role);

-- Inserir usuário master padrão
-- Senha: admin123 (hash bcrypt com workfactor 10)
INSERT INTO system_users (email, password_hash, name, role, is_active, is_master) VALUES
('admin@fintechpsp.com', '$2b$10$N9qo8uLOickgx2ZMRZoMye.IjPeGvGzjYwjUxcHjRMA4nAFPiO/Xi', 'Administrador Master', 'Admin', true, true)
ON CONFLICT (email) DO NOTHING;

-- Inserir clientes de exemplo
INSERT INTO clients (client_id, client_secret, name, allowed_scopes) VALUES
('fintech_web_app', 'web_app_secret_123', 'Fintech Web Application', 'pix,banking'),
('fintech_mobile_app', 'mobile_app_secret_456', 'Fintech Mobile Application', 'pix,banking'),
('fintech_admin', 'admin_secret_789', 'Fintech Admin Panel', 'pix,banking,admin'),
('integration_test', 'test_secret_000', 'Integration Test Client', 'pix,banking,admin'),
('admin_backoffice', 'admin_secret_000', 'Admin Backoffice', 'admin'),
('cliente_banking', 'cliente_secret_000', 'Cliente Banking', 'banking')
ON CONFLICT (client_id) DO NOTHING;
