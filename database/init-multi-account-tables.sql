-- =====================================================
-- Script de inicialização para Multi-Account Management
-- PSP FintechPSP - Suporte a múltiplas contas bancárias
-- =====================================================

-- Conectar ao banco principal
\c fintech_psp;

-- =====================================================
-- USERSERVICE - Tabelas para gerenciamento de contas
-- =====================================================

-- Tabela de contas bancárias dos clientes
CREATE TABLE IF NOT EXISTS user_service.contas_bancarias (
    conta_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    account_number VARCHAR(50) NOT NULL,
    description VARCHAR(200),
    credentials_token_id VARCHAR(100) NOT NULL,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    -- Índices para performance
    CONSTRAINT uk_conta_cliente_bank UNIQUE (cliente_id, bank_code, account_number)
);

-- Índices para otimização de consultas
CREATE INDEX IF NOT EXISTS idx_contas_cliente_id ON user_service.contas_bancarias(cliente_id);
CREATE INDEX IF NOT EXISTS idx_contas_bank_code ON user_service.contas_bancarias(bank_code);
CREATE INDEX IF NOT EXISTS idx_contas_active ON user_service.contas_bancarias(is_active) WHERE is_active = true;

-- Tabela de credenciais tokenizadas (criptografadas)
CREATE TABLE IF NOT EXISTS user_service.credentials_tokens (
    token_id VARCHAR(100) PRIMARY KEY,
    encrypted_data TEXT NOT NULL, -- JSON criptografado com as credenciais
    encryption_key_id VARCHAR(50) NOT NULL, -- ID da chave de criptografia usada
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    expires_at TIMESTAMP WITH TIME ZONE -- Para rotação de credenciais
);

-- Índice para limpeza de tokens expirados
CREATE INDEX IF NOT EXISTS idx_credentials_expires ON user_service.credentials_tokens(expires_at) WHERE expires_at IS NOT NULL;

-- =====================================================
-- CONFIGSERVICE - Tabelas para priorização e roteamento
-- =====================================================

-- Tabela de configurações de priorização por cliente
CREATE TABLE IF NOT EXISTS config_service.priorizacao_configs (
    config_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL UNIQUE, -- Um cliente tem apenas uma configuração ativa
    total_percentual DECIMAL(5,2) NOT NULL DEFAULT 0.00,
    is_valid BOOLEAN DEFAULT false, -- True quando total_percentual = 100.00
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Validação: total deve ser 100% para ser válido
    CONSTRAINT chk_total_percentual CHECK (total_percentual >= 0.00 AND total_percentual <= 100.00)
);

-- Tabela de prioridades por conta
CREATE TABLE IF NOT EXISTS config_service.priorizacao_contas (
    prioridade_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    config_id UUID NOT NULL REFERENCES config_service.priorizacao_configs(config_id) ON DELETE CASCADE,
    conta_id UUID NOT NULL, -- Referência para a conta no UserService
    bank_code VARCHAR(20) NOT NULL, -- Desnormalizado para performance
    percentual DECIMAL(5,2) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Validações
    CONSTRAINT chk_percentual CHECK (percentual > 0.00 AND percentual <= 100.00),
    CONSTRAINT uk_config_conta UNIQUE (config_id, conta_id)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_priorizacao_cliente ON config_service.priorizacao_configs(cliente_id);
CREATE INDEX IF NOT EXISTS idx_priorizacao_contas_config ON config_service.priorizacao_contas(config_id);
CREATE INDEX IF NOT EXISTS idx_priorizacao_contas_conta ON config_service.priorizacao_contas(conta_id);

-- Tabela de bancos personalizados (além dos padrão)
CREATE TABLE IF NOT EXISTS config_service.bancos_personalizados (
    banco_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(20) NOT NULL,
    bank_name VARCHAR(100) NOT NULL,
    api_base_url VARCHAR(500),
    supports_pix BOOLEAN DEFAULT true,
    supports_ted BOOLEAN DEFAULT true,
    supports_boleto BOOLEAN DEFAULT true,
    supports_qrcode BOOLEAN DEFAULT true,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT uk_cliente_bank_code UNIQUE (cliente_id, bank_code)
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_bancos_cliente ON config_service.bancos_personalizados(cliente_id);
CREATE INDEX IF NOT EXISTS idx_bancos_code ON config_service.bancos_personalizados(bank_code);

-- =====================================================
-- DADOS INICIAIS - Bancos padrão suportados
-- =====================================================

-- Inserir bancos padrão se não existirem
INSERT INTO config_service.bancos_personalizados (
    banco_id, cliente_id, bank_code, bank_name, api_base_url, 
    supports_pix, supports_ted, supports_boleto, supports_qrcode, is_active
) VALUES 
    -- Bancos padrão disponíveis para todos os clientes (cliente_id = '00000000-0000-0000-0000-000000000000')
    ('10000000-0000-0000-0000-000000000001', '00000000-0000-0000-0000-000000000000', 'STARK', 'Stark Bank', 'https://api.starkbank.com', true, true, true, true, true),
    ('10000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000000', 'SICOOB', 'Sicoob', 'https://api.sicoob.com.br', true, true, true, true, true),
    ('10000000-0000-0000-0000-000000000003', '00000000-0000-0000-0000-000000000000', 'GENIAL', 'Banco Genial', 'https://api.genial.com.br', true, true, true, true, true),
    ('10000000-0000-0000-0000-000000000004', '00000000-0000-0000-0000-000000000000', 'EFI', 'Efí (Gerencianet)', 'https://api.gerencianet.com.br', true, true, true, true, true),
    ('10000000-0000-0000-0000-000000000005', '00000000-0000-0000-0000-000000000000', 'CELCOIN', 'Celcoin', 'https://api.celcoin.com.br', true, true, true, true, true)
ON CONFLICT (cliente_id, bank_code) DO NOTHING;

-- =====================================================
-- FUNÇÕES E TRIGGERS para validação automática
-- =====================================================

-- Função para validar total de percentuais
CREATE OR REPLACE FUNCTION config_service.validate_priorizacao_total()
RETURNS TRIGGER AS $$
BEGIN
    -- Atualizar o total na tabela de configuração
    UPDATE config_service.priorizacao_configs 
    SET 
        total_percentual = (
            SELECT COALESCE(SUM(percentual), 0.00) 
            FROM config_service.priorizacao_contas 
            WHERE config_id = COALESCE(NEW.config_id, OLD.config_id)
        ),
        is_valid = (
            SELECT COALESCE(SUM(percentual), 0.00) = 100.00
            FROM config_service.priorizacao_contas 
            WHERE config_id = COALESCE(NEW.config_id, OLD.config_id)
        ),
        updated_at = CURRENT_TIMESTAMP
    WHERE config_id = COALESCE(NEW.config_id, OLD.config_id);
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Triggers para validação automática
DROP TRIGGER IF EXISTS trg_validate_priorizacao_insert ON config_service.priorizacao_contas;
CREATE TRIGGER trg_validate_priorizacao_insert
    AFTER INSERT ON config_service.priorizacao_contas
    FOR EACH ROW EXECUTE FUNCTION config_service.validate_priorizacao_total();

DROP TRIGGER IF EXISTS trg_validate_priorizacao_update ON config_service.priorizacao_contas;
CREATE TRIGGER trg_validate_priorizacao_update
    AFTER UPDATE ON config_service.priorizacao_contas
    FOR EACH ROW EXECUTE FUNCTION config_service.validate_priorizacao_total();

DROP TRIGGER IF EXISTS trg_validate_priorizacao_delete ON config_service.priorizacao_contas;
CREATE TRIGGER trg_validate_priorizacao_delete
    AFTER DELETE ON config_service.priorizacao_contas
    FOR EACH ROW EXECUTE FUNCTION config_service.validate_priorizacao_total();

-- =====================================================
-- VIEWS para consultas otimizadas
-- =====================================================

-- View para contas com prioridades
CREATE OR REPLACE VIEW config_service.v_contas_com_prioridade AS
SELECT 
    cb.conta_id,
    cb.cliente_id,
    cb.bank_code,
    cb.account_number,
    cb.description,
    cb.is_active,
    COALESCE(pc.percentual, 0.00) as priority_percentual,
    CASE WHEN pc.percentual IS NOT NULL THEN true ELSE false END as has_priority_config,
    pc.config_id,
    cfg.is_valid as config_is_valid,
    cfg.total_percentual as config_total_percentual
FROM user_service.contas_bancarias cb
LEFT JOIN config_service.priorizacao_contas pc ON cb.conta_id = pc.conta_id
LEFT JOIN config_service.priorizacao_configs cfg ON pc.config_id = cfg.config_id
WHERE cb.is_active = true;

-- =====================================================
-- COMENTÁRIOS E DOCUMENTAÇÃO
-- =====================================================

COMMENT ON TABLE user_service.contas_bancarias IS 'Contas bancárias dos clientes com credenciais tokenizadas';
COMMENT ON TABLE user_service.credentials_tokens IS 'Credenciais criptografadas das contas bancárias';
COMMENT ON TABLE config_service.priorizacao_configs IS 'Configurações de priorização por cliente';
COMMENT ON TABLE config_service.priorizacao_contas IS 'Percentuais de priorização por conta';
COMMENT ON TABLE config_service.bancos_personalizados IS 'Bancos disponíveis (padrão + personalizados por cliente)';

COMMENT ON COLUMN user_service.contas_bancarias.credentials_token_id IS 'ID do token de credenciais criptografadas';
COMMENT ON COLUMN config_service.priorizacao_configs.is_valid IS 'True quando total_percentual = 100.00';
COMMENT ON COLUMN config_service.priorizacao_contas.percentual IS 'Percentual de priorização (0.01 a 100.00)';

-- =====================================================
-- GRANTS de permissão (ajustar conforme necessário)
-- =====================================================

-- Conceder permissões aos usuários dos serviços
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA user_service TO user_service_user;
-- GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA config_service TO config_service_user;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA user_service TO user_service_user;
-- GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA config_service TO config_service_user;

PRINT 'Multi-Account Management tables created successfully!';
