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
