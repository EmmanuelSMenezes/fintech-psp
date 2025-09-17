-- Tabela de usuários
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    document VARCHAR(20) UNIQUE NOT NULL,
    phone VARCHAR(20),
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de contas bancárias
CREATE TABLE IF NOT EXISTS contas_bancarias (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(10) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    description VARCHAR(255) DEFAULT '',
    credentials_token_id VARCHAR(255) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Tabela de tokens de credenciais (tokenização)
CREATE TABLE IF NOT EXISTS conta_credentials_tokens (
    token_id VARCHAR(255) PRIMARY KEY,
    conta_id UUID NOT NULL,
    encrypted_credentials TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_document ON users(document);
CREATE INDEX IF NOT EXISTS idx_users_is_active ON users(is_active);

CREATE INDEX IF NOT EXISTS idx_contas_cliente_id ON contas_bancarias(cliente_id);
CREATE INDEX IF NOT EXISTS idx_contas_bank_code ON contas_bancarias(bank_code);
CREATE INDEX IF NOT EXISTS idx_contas_is_active ON contas_bancarias(is_active);
CREATE UNIQUE INDEX IF NOT EXISTS idx_contas_unique_account ON contas_bancarias(cliente_id, bank_code, account_number) WHERE is_active = true;

CREATE INDEX IF NOT EXISTS idx_credentials_conta_id ON conta_credentials_tokens(conta_id);

-- Foreign keys
ALTER TABLE contas_bancarias ADD CONSTRAINT IF NOT EXISTS fk_contas_cliente 
    FOREIGN KEY (cliente_id) REFERENCES users(id) ON DELETE CASCADE;

ALTER TABLE conta_credentials_tokens ADD CONSTRAINT IF NOT EXISTS fk_credentials_conta
    FOREIGN KEY (conta_id) REFERENCES contas_bancarias(id) ON DELETE CASCADE;

-- =====================================================
-- TABELA DE ACESSOS RBAC
-- =====================================================

-- Tabela para controle de acessos e permissões
CREATE TABLE IF NOT EXISTS acessos (
    acesso_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    parent_user_id UUID NULL, -- Para sub-usuários
    email VARCHAR(255) NOT NULL,
    role VARCHAR(50) NOT NULL CHECK (role IN ('admin', 'sub-admin', 'cliente', 'sub-cliente')),
    permissions JSONB NOT NULL DEFAULT '[]'::jsonb,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NULL,
    created_by UUID NOT NULL,

    -- Constraints
    CONSTRAINT unique_user_id UNIQUE (user_id),
    CONSTRAINT unique_email UNIQUE (email),
    CONSTRAINT valid_email CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$'),
    CONSTRAINT parent_user_constraint CHECK (
        (role IN ('admin', 'cliente') AND parent_user_id IS NULL) OR
        (role IN ('sub-admin', 'sub-cliente') AND parent_user_id IS NOT NULL)
    )
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_acessos_user_id ON acessos (user_id);
CREATE INDEX IF NOT EXISTS idx_acessos_parent_user_id ON acessos (parent_user_id) WHERE parent_user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_acessos_email ON acessos (email);
CREATE INDEX IF NOT EXISTS idx_acessos_role ON acessos (role);
CREATE INDEX IF NOT EXISTS idx_acessos_is_active ON acessos (is_active);
CREATE INDEX IF NOT EXISTS idx_acessos_created_at ON acessos (created_at);

-- Índice GIN para busca em permissões JSON
CREATE INDEX IF NOT EXISTS idx_acessos_permissions ON acessos USING GIN (permissions);

-- Função para atualizar updated_at automaticamente
CREATE OR REPLACE FUNCTION update_acessos_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = NOW();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger para atualizar updated_at
CREATE TRIGGER update_acessos_updated_at_trigger
    BEFORE UPDATE ON acessos
    FOR EACH ROW
    EXECUTE FUNCTION update_acessos_updated_at();

-- Inserir dados de exemplo para acessos
INSERT INTO acessos (user_id, email, role, permissions, created_by) VALUES
(
    '11111111-1111-1111-1111-111111111111'::uuid,
    'admin@fintech.com',
    'admin',
    '["view_dashboard", "view_transacoes", "view_contas", "view_clientes", "view_relatorios", "view_extratos", "edit_contas", "edit_clientes", "edit_configuracoes", "edit_acessos", "manage_users", "manage_permissions", "manage_system", "view_audit_logs", "configurar_priorizacao", "configurar_bancos", "configurar_integracoes"]'::jsonb,
    '11111111-1111-1111-1111-111111111111'::uuid
) ON CONFLICT (user_id) DO NOTHING;

INSERT INTO acessos (user_id, email, role, permissions, created_by) VALUES
(
    '22222222-2222-2222-2222-222222222222'::uuid,
    'cliente@fintech.com',
    'cliente',
    '["view_dashboard", "view_transacoes", "view_contas", "view_extratos", "edit_contas", "configurar_priorizacao", "manage_sub_users"]'::jsonb,
    '11111111-1111-1111-1111-111111111111'::uuid
) ON CONFLICT (user_id) DO NOTHING;
