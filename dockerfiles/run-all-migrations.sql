-- =====================================================
-- FintechPSP - Script Completo de Migrações
-- =====================================================

-- Conectar ao database principal
\c fintech_psp;

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =====================================================
-- 1. TABELAS BASE (init-database.sql)
-- =====================================================

-- Criar tabela de audit logs
CREATE TABLE IF NOT EXISTS audit_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    table_name VARCHAR(100),
    operation VARCHAR(10),
    old_values JSONB,
    new_values JSONB,
    user_id VARCHAR(100),
    timestamp TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    ip_address INET
);

-- Criar tabela de contas
CREATE TABLE IF NOT EXISTS accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    available_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    status VARCHAR(20) NOT NULL DEFAULT 'ACTIVE',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(client_id, account_id)
);

-- Criar índices para accounts
CREATE INDEX IF NOT EXISTS idx_accounts_client_id ON accounts(client_id);
CREATE INDEX IF NOT EXISTS idx_accounts_account_id ON accounts(account_id);

-- Criar tabela de histórico de transações
CREATE TABLE IF NOT EXISTS transaction_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    transaction_id UUID NOT NULL,
    external_id VARCHAR(100),
    type VARCHAR(50) NOT NULL,
    amount DECIMAL(18,2) NOT NULL,
    description TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    operation VARCHAR(20) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    sub_type VARCHAR(50),
    currency VARCHAR(3) DEFAULT 'BRL',
    pix_key VARCHAR(255),
    bank_code VARCHAR(10),
    qrcode_payload TEXT,
    expires_at TIMESTAMP WITH TIME ZONE,
    metadata JSONB,
    FOREIGN KEY (client_id, account_id) REFERENCES accounts(client_id, account_id)
);

-- Criar índices para transaction_history
CREATE INDEX IF NOT EXISTS idx_transaction_history_client_id ON transaction_history(client_id);
CREATE INDEX IF NOT EXISTS idx_transaction_history_account_id ON transaction_history(account_id);
CREATE INDEX IF NOT EXISTS idx_transaction_history_transaction_id ON transaction_history(transaction_id);
CREATE INDEX IF NOT EXISTS idx_transaction_history_external_id ON transaction_history(external_id);
CREATE INDEX IF NOT EXISTS idx_transaction_history_type ON transaction_history(type);
CREATE INDEX IF NOT EXISTS idx_transaction_history_created_at ON transaction_history(created_at);

-- =====================================================
-- 2. AUTH SERVICE MIGRATIONS
-- =====================================================

-- Tabela de clientes
CREATE TABLE IF NOT EXISTS clients (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    document VARCHAR(20) UNIQUE NOT NULL,
    phone VARCHAR(20),
    api_key VARCHAR(255) UNIQUE NOT NULL,
    secret_key VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    webhook_url TEXT,
    allowed_ips TEXT[],
    rate_limit_per_minute INTEGER DEFAULT 60,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Tabela de tokens ativos
CREATE TABLE IF NOT EXISTS active_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL REFERENCES clients(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL,
    expires_at TIMESTAMP WITH TIME ZONE NOT NULL,
    scopes TEXT[] DEFAULT '{}',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_used_at TIMESTAMP WITH TIME ZONE
);

-- Tabela de auditoria de autenticação
CREATE TABLE IF NOT EXISTS auth_audit (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID REFERENCES clients(id) ON DELETE SET NULL,
    ip_address INET,
    user_agent TEXT,
    endpoint VARCHAR(255),
    method VARCHAR(10),
    success BOOLEAN NOT NULL,
    error_message TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

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

-- Índices para auth service
CREATE INDEX IF NOT EXISTS idx_clients_email ON clients(email);
CREATE INDEX IF NOT EXISTS idx_clients_document ON clients(document);
CREATE INDEX IF NOT EXISTS idx_clients_api_key ON clients(api_key);
CREATE INDEX IF NOT EXISTS idx_clients_is_active ON clients(is_active);

CREATE INDEX IF NOT EXISTS idx_active_tokens_client_id ON active_tokens(client_id);
CREATE INDEX IF NOT EXISTS idx_active_tokens_token_hash ON active_tokens(token_hash);
CREATE INDEX IF NOT EXISTS idx_active_tokens_expires_at ON active_tokens(expires_at);

CREATE INDEX IF NOT EXISTS idx_auth_audit_client_id ON auth_audit(client_id);
CREATE INDEX IF NOT EXISTS idx_auth_audit_created_at ON auth_audit(created_at);
CREATE INDEX IF NOT EXISTS idx_auth_audit_success ON auth_audit(success);

CREATE INDEX IF NOT EXISTS idx_system_users_email ON system_users(email);
CREATE INDEX IF NOT EXISTS idx_system_users_is_active ON system_users(is_active);
CREATE INDEX IF NOT EXISTS idx_system_users_role ON system_users(role);

-- Inserir usuário master padrão
-- Senha: admin123 (texto plano para desenvolvimento - BCrypt configurado como fallback)
INSERT INTO system_users (email, password_hash, name, role, is_active, is_master) VALUES
('admin@fintechpsp.com', 'admin123', 'Administrador Master', 'Admin', true, true)
ON CONFLICT (email) DO NOTHING;

-- Tabela de API Keys para autenticação de clientes externos
CREATE TABLE IF NOT EXISTS api_keys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    public_key VARCHAR(255) UNIQUE NOT NULL,
    secret_hash VARCHAR(255) NOT NULL,
    company_id UUID NOT NULL,
    name VARCHAR(255) NOT NULL,
    scopes TEXT NOT NULL DEFAULT '[]',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMP WITH TIME ZONE,
    last_used_at TIMESTAMP WITH TIME ZONE,
    allowed_ip VARCHAR(45),
    rate_limit_per_minute INTEGER NOT NULL DEFAULT 100,
    created_by UUID NOT NULL
);

-- Índices para API Keys
CREATE INDEX IF NOT EXISTS idx_api_keys_public_key ON api_keys(public_key);
CREATE INDEX IF NOT EXISTS idx_api_keys_company_id ON api_keys(company_id);
CREATE INDEX IF NOT EXISTS idx_api_keys_is_active ON api_keys(is_active);

-- =====================================================
-- 3. USER SERVICE MIGRATIONS
-- =====================================================

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

-- Índices para user service
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_document ON users(document);
CREATE INDEX IF NOT EXISTS idx_users_is_active ON users(is_active);

CREATE INDEX IF NOT EXISTS idx_contas_cliente_id ON contas_bancarias(cliente_id);
CREATE INDEX IF NOT EXISTS idx_contas_bank_code ON contas_bancarias(bank_code);
CREATE INDEX IF NOT EXISTS idx_contas_is_active ON contas_bancarias(is_active);
CREATE UNIQUE INDEX IF NOT EXISTS idx_contas_unique_account ON contas_bancarias(cliente_id, bank_code, account_number) WHERE is_active = true;

CREATE INDEX IF NOT EXISTS idx_credentials_conta_id ON conta_credentials_tokens(conta_id);

-- =====================================================
-- 4. INSERIR DADOS DE EXEMPLO
-- =====================================================

-- Inserir conta de exemplo
INSERT INTO accounts (client_id, account_id, balance, available_balance, currency, status) 
VALUES ('550e8400-e29b-41d4-a716-446655440000', 'CONTA_PRINCIPAL', 1000.00, 1000.00, 'BRL', 'ACTIVE')
ON CONFLICT (client_id, account_id) DO NOTHING;

-- Inserir cliente de exemplo para auth
INSERT INTO clients (id, name, email, document, phone, api_key, secret_key, is_active, webhook_url) VALUES
('550e8400-e29b-41d4-a716-446655440000', 'Cliente Teste', 'cliente@teste.com', '12345678901', '11999999999', 'test_api_key_123', 'test_secret_key_456', true, 'https://webhook.teste.com')
ON CONFLICT (email) DO NOTHING;

-- Inserir usuário de exemplo
INSERT INTO users (id, name, email, document, phone, is_active) VALUES
('550e8400-e29b-41d4-a716-446655440000', 'João Silva', 'joao.silva@teste.com', '12345678901', '11999999999', true)
ON CONFLICT (email) DO NOTHING;

-- =====================================================
-- COMPANY SERVICE SCHEMA E TABELAS
-- =====================================================

-- Criar schema para CompanyService
CREATE SCHEMA IF NOT EXISTS company_service;

-- Tabela de empresas
CREATE TABLE IF NOT EXISTS company_service.companies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    razao_social VARCHAR(255) NOT NULL,
    nome_fantasia VARCHAR(255),
    cnpj VARCHAR(20) UNIQUE NOT NULL,
    inscricao_estadual VARCHAR(50),
    inscricao_municipal VARCHAR(50),
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    telefone VARCHAR(20),
    email VARCHAR(255),
    website VARCHAR(255),
    capital_social DECIMAL(18,2),
    atividade_principal VARCHAR(255),
    status VARCHAR(50) NOT NULL DEFAULT 'PendingDocuments',
    observacoes TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE,
    approved_at TIMESTAMP WITH TIME ZONE,
    approved_by UUID
);

-- Inserir empresas de exemplo
INSERT INTO company_service.companies (
    razao_social, nome_fantasia, cnpj, inscricao_estadual,
    cep, logradouro, numero, bairro, cidade, estado,
    telefone, email, status
) VALUES
('Empresa Teste Ltda', 'Teste Corp', '12345678000195', '123456789',
 '01234567', 'Rua das Flores', '123', 'Centro', 'São Paulo', 'SP',
 '11999999999', 'contato@teste.com', 'Active'),
('Inovação Digital S.A.', 'InnovaDigital', '98765432000187', '987654321',
 '87654321', 'Av. Tecnologia', '456', 'Tech Park', 'São Paulo', 'SP',
 '11888888888', 'info@innovadigital.com', 'Active'),
('Soluções Empresariais ME', 'SolEmp', '11122233000144', '111222333',
 '11223344', 'Praça dos Negócios', '789', 'Comercial', 'Rio de Janeiro', 'RJ',
 '21777777777', 'vendas@solemp.com', 'PendingDocuments')
ON CONFLICT (cnpj) DO NOTHING;

\echo 'Migrações executadas com sucesso!'
