-- Tabela de configurações do sistema (existente)
CREATE TABLE IF NOT EXISTS system_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    config_key VARCHAR(255) UNIQUE NOT NULL,
    config_value TEXT NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Tabela de configurações de priorização
CREATE TABLE IF NOT EXISTS configuracoes_priorizacao (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID UNIQUE NOT NULL,
    prioridades_json JSONB NOT NULL,
    total_percentual DECIMAL(5,2) NOT NULL,
    is_valid BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Tabela de bancos personalizados
CREATE TABLE IF NOT EXISTS bancos_personalizados (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    cliente_id UUID NOT NULL,
    bank_code VARCHAR(10) NOT NULL,
    endpoint VARCHAR(500),
    credentials_template TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices
CREATE INDEX IF NOT EXISTS idx_system_configs_key ON system_configs(config_key);
CREATE INDEX IF NOT EXISTS idx_system_configs_active ON system_configs(is_active);

CREATE INDEX IF NOT EXISTS idx_priorizacao_cliente_id ON configuracoes_priorizacao(cliente_id);
CREATE INDEX IF NOT EXISTS idx_priorizacao_is_valid ON configuracoes_priorizacao(is_valid);

CREATE INDEX IF NOT EXISTS idx_bancos_cliente_id ON bancos_personalizados(cliente_id);
CREATE INDEX IF NOT EXISTS idx_bancos_bank_code ON bancos_personalizados(bank_code);
CREATE UNIQUE INDEX IF NOT EXISTS idx_bancos_unique_client_code ON bancos_personalizados(cliente_id, bank_code);

-- Inserir configurações padrão do sistema
INSERT INTO system_configs (config_key, config_value, description) VALUES
('max_transaction_amount', '1000000.00', 'Valor máximo permitido para transações em reais')
ON CONFLICT (config_key) DO NOTHING;

INSERT INTO system_configs (config_key, config_value, description) VALUES
('pix_timeout_seconds', '300', 'Timeout em segundos para transações PIX')
ON CONFLICT (config_key) DO NOTHING;

INSERT INTO system_configs (config_key, config_value, description) VALUES
('webhook_retry_attempts', '3', 'Número máximo de tentativas para webhooks')
ON CONFLICT (config_key) DO NOTHING;

INSERT INTO system_configs (config_key, config_value, description) VALUES
('qr_code_expiration_minutes', '30', 'Tempo de expiração padrão para QR codes dinâmicos em minutos')
ON CONFLICT (config_key) DO NOTHING;

-- =====================================================
-- Tabela: banking_configs
-- Descrição: Configurações bancárias do sistema
-- =====================================================
CREATE TABLE IF NOT EXISTS banking_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    enabled BOOLEAN NOT NULL DEFAULT true,
    settings JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE,
    created_by VARCHAR(100),
    updated_by VARCHAR(100),

    -- Constraints
    CONSTRAINT uk_banking_configs_name UNIQUE (name)
);

-- Índices para banking_configs
CREATE INDEX IF NOT EXISTS idx_banking_configs_type ON banking_configs(type);
CREATE INDEX IF NOT EXISTS idx_banking_configs_enabled ON banking_configs(enabled);
CREATE INDEX IF NOT EXISTS idx_banking_configs_created_at ON banking_configs(created_at);

-- Dados iniciais para banking_configs
INSERT INTO banking_configs (id, name, type, enabled, settings, created_by) VALUES
(gen_random_uuid(), 'Stark Bank PIX', 'pix', true, '{"api_key": "sk_test_***", "environment": "sandbox", "timeout": 30}', 'system'),
(gen_random_uuid(), 'Sicoob PIX', 'pix', true, '{"client_id": "sicoob_***", "client_secret": "***", "environment": "sandbox"}', 'system'),
(gen_random_uuid(), 'Banco Genial TED', 'ted', true, '{"agency": "0001", "account": "123456", "environment": "sandbox"}', 'system'),
(gen_random_uuid(), 'Efí Boleto', 'boleto', true, '{"client_id": "efi_***", "client_secret": "***", "certificate_path": "/certs/efi.p12"}', 'system'),
(gen_random_uuid(), 'Celcoin Crypto', 'crypto', false, '{"api_key": "celcoin_***", "webhook_url": "https://api.fintech.com/webhooks/celcoin"}', 'system')
ON CONFLICT (name) DO NOTHING;
