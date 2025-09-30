-- =====================================================
-- FintechPSP CompanyService - Database Migrations
-- =====================================================

-- Create schema if not exists
CREATE SCHEMA IF NOT EXISTS company_service;

-- =====================================================
-- Companies Table
-- =====================================================
CREATE TABLE IF NOT EXISTS company_service.companies (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    razao_social VARCHAR(255) NOT NULL,
    nome_fantasia VARCHAR(255),
    cnpj VARCHAR(18) NOT NULL UNIQUE,
    inscricao_estadual VARCHAR(50),
    inscricao_municipal VARCHAR(50),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Contact
    telefone VARCHAR(20),
    email VARCHAR(255),
    website VARCHAR(255),
    
    -- Contract Data
    capital_social DECIMAL(15,2),
    data_constituicao DATE,
    natureza_juridica VARCHAR(100),
    atividade_principal VARCHAR(255),
    regime_tributario VARCHAR(50),
    
    -- Status and Control
    status VARCHAR(50) NOT NULL DEFAULT 'PendingDocuments',
    observacoes TEXT,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    approved_at TIMESTAMP WITH TIME ZONE,
    approved_by UUID,
    
    -- Indexes
    CONSTRAINT chk_companies_status CHECK (status IN ('PendingDocuments', 'UnderReview', 'Approved', 'Rejected', 'Active', 'Inactive', 'Suspended'))
);

-- =====================================================
-- Legal Representatives Table
-- =====================================================
CREATE TABLE IF NOT EXISTS company_service.legal_representatives (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES company_service.companies(id) ON DELETE CASCADE,
    
    -- Personal Data
    nome_completo VARCHAR(255) NOT NULL,
    cpf VARCHAR(14) NOT NULL,
    rg VARCHAR(20),
    orgao_expedidor VARCHAR(20),
    data_nascimento DATE,
    estado_civil VARCHAR(50),
    nacionalidade VARCHAR(100) DEFAULT 'Brasileira',
    profissao VARCHAR(100),
    
    -- Contact
    email VARCHAR(255),
    telefone VARCHAR(20),
    celular VARCHAR(20),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Company Role
    cargo VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    percentual_participacao DECIMAL(5,2),
    poderes_representacao TEXT,
    pode_assinar_sozinho BOOLEAN DEFAULT false,
    limite_alcada DECIMAL(15,2),
    
    -- Status and Control
    is_active BOOLEAN DEFAULT true,
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    -- Constraints
    CONSTRAINT chk_legal_representatives_type CHECK (type IN ('Administrator', 'PartnerAdministrator', 'Director', 'President', 'VicePresident', 'Attorney', 'Partner', 'Other')),
    CONSTRAINT chk_legal_representatives_percentual CHECK (percentual_participacao >= 0 AND percentual_participacao <= 100),
    CONSTRAINT uk_legal_representatives_company_cpf UNIQUE (company_id, cpf)
);

-- =====================================================
-- Applicants Table (for company creation requests)
-- =====================================================
CREATE TABLE IF NOT EXISTS company_service.applicants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES company_service.companies(id) ON DELETE CASCADE,
    
    -- Personal Data
    nome_completo VARCHAR(255) NOT NULL,
    cpf VARCHAR(14) NOT NULL,
    rg VARCHAR(20),
    orgao_expedidor VARCHAR(20),
    data_nascimento DATE,
    estado_civil VARCHAR(50),
    nacionalidade VARCHAR(100) DEFAULT 'Brasileira',
    profissao VARCHAR(100),
    
    -- Contact
    email VARCHAR(255),
    telefone VARCHAR(20),
    celular VARCHAR(20),
    
    -- Address
    cep VARCHAR(10),
    logradouro VARCHAR(255),
    numero VARCHAR(20),
    complemento VARCHAR(100),
    bairro VARCHAR(100),
    cidade VARCHAR(100),
    estado VARCHAR(2),
    pais VARCHAR(50) DEFAULT 'Brasil',
    
    -- Financial
    renda_mensal DECIMAL(10,2),
    cargo VARCHAR(100),
    
    -- Audit
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- =====================================================
-- Indexes for Performance
-- =====================================================

-- Companies indexes
CREATE INDEX IF NOT EXISTS idx_companies_cnpj ON company_service.companies(cnpj);
CREATE INDEX IF NOT EXISTS idx_companies_status ON company_service.companies(status);
CREATE INDEX IF NOT EXISTS idx_companies_created_at ON company_service.companies(created_at);
CREATE INDEX IF NOT EXISTS idx_companies_razao_social ON company_service.companies(razao_social);

-- Legal Representatives indexes
CREATE INDEX IF NOT EXISTS idx_legal_representatives_company_id ON company_service.legal_representatives(company_id);
CREATE INDEX IF NOT EXISTS idx_legal_representatives_cpf ON company_service.legal_representatives(cpf);
CREATE INDEX IF NOT EXISTS idx_legal_representatives_type ON company_service.legal_representatives(type);
CREATE INDEX IF NOT EXISTS idx_legal_representatives_active ON company_service.legal_representatives(is_active);

-- Applicants indexes
CREATE INDEX IF NOT EXISTS idx_applicants_company_id ON company_service.applicants(company_id);
CREATE INDEX IF NOT EXISTS idx_applicants_cpf ON company_service.applicants(cpf);

-- =====================================================
-- Sample Data for Testing
-- =====================================================

-- Insert sample company
INSERT INTO company_service.companies (
    id, razao_social, nome_fantasia, cnpj, inscricao_estadual, 
    cep, logradouro, numero, bairro, cidade, estado,
    telefone, email, status, created_at
) VALUES (
    'b79fda6d-1642-4c05-b81f-7d065a2e28a1',
    'Tech Solutions Ltda',
    'TechSol',
    '12.345.678/0001-90',
    '123.456.789.012',
    '01310-100',
    'Av. Paulista',
    '1000',
    'Bela Vista',
    'São Paulo',
    'SP',
    '(11) 3000-0000',
    'contato@techsol.com.br',
    'Active',
    CURRENT_TIMESTAMP - INTERVAL '30 days'
) ON CONFLICT (id) DO NOTHING;

-- Insert sample legal representatives
INSERT INTO company_service.legal_representatives (
    company_id, nome_completo, cpf, rg, orgao_expedidor, data_nascimento,
    estado_civil, nacionalidade, profissao, email, telefone, cargo, type,
    percentual_participacao, pode_assinar_sozinho, limite_alcada,
    cep, logradouro, numero, bairro, cidade, estado,
    created_at
) VALUES 
(
    'b79fda6d-1642-4c05-b81f-7d065a2e28a1',
    'João Silva Santos',
    '123.456.789-01',
    '12.345.678-9',
    'SSP/SP',
    '1980-05-15',
    'Casado',
    'Brasileira',
    'Empresário',
    'joao@techsol.com.br',
    '(11) 99999-9999',
    'Diretor Presidente',
    'President',
    60.0,
    true,
    1000000.00,
    '01310-100',
    'Rua das Palmeiras',
    '123',
    'Jardins',
    'São Paulo',
    'SP',
    CURRENT_TIMESTAMP - INTERVAL '30 days'
),
(
    'b79fda6d-1642-4c05-b81f-7d065a2e28a1',
    'Maria Oliveira Costa',
    '987.654.321-09',
    '98.765.432-1',
    'SSP/SP',
    '1985-08-22',
    'Solteira',
    'Brasileira',
    'Administradora',
    'maria@techsol.com.br',
    '(11) 88888-8888',
    'Diretora Financeira',
    'Director',
    40.0,
    false,
    500000.00,
    '04567-890',
    'Av. Brigadeiro Faria Lima',
    '456',
    'Itaim Bibi',
    'São Paulo',
    'SP',
    CURRENT_TIMESTAMP - INTERVAL '30 days'
) ON CONFLICT (company_id, cpf) DO NOTHING;

-- =====================================================
-- Audit Table for LGPD Compliance
-- =====================================================
CREATE TABLE IF NOT EXISTS company_service.audit_log (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    table_name VARCHAR(100) NOT NULL,
    record_id UUID NOT NULL,
    operation VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    old_values JSONB,
    new_values JSONB,
    user_id UUID,
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT chk_audit_operation CHECK (operation IN ('INSERT', 'UPDATE', 'DELETE'))
);

CREATE INDEX IF NOT EXISTS idx_audit_log_table_record ON company_service.audit_log(table_name, record_id);
CREATE INDEX IF NOT EXISTS idx_audit_log_created_at ON company_service.audit_log(created_at);
CREATE INDEX IF NOT EXISTS idx_audit_log_user_id ON company_service.audit_log(user_id);
