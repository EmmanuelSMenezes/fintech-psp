-- TransactionService Database Schema

-- Tabela principal de transações
CREATE TABLE IF NOT EXISTS transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id VARCHAR(255) UNIQUE NOT NULL,
    type VARCHAR(50) NOT NULL CHECK (type IN ('PIX', 'TED', 'BOLETO', 'CRYPTO')),
    status VARCHAR(50) NOT NULL CHECK (status IN ('PENDING', 'PROCESSING', 'CONFIRMED', 'FAILED', 'CANCELLED')),
    amount DECIMAL(15,2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    bank_code VARCHAR(10),
    
    -- Campos específicos PIX
    pix_key VARCHAR(255),
    end_to_end_id VARCHAR(32),
    
    -- Campos específicos TED
    account_branch VARCHAR(10),
    account_number VARCHAR(20),
    tax_id VARCHAR(20),
    name VARCHAR(255),
    
    -- Campos comuns
    description TEXT,
    webhook_url TEXT,
    
    -- Campos específicos Boleto
    due_date TIMESTAMP WITH TIME ZONE,
    payer_tax_id VARCHAR(20),
    payer_name VARCHAR(255),
    instructions TEXT,
    boleto_barcode VARCHAR(255),
    boleto_url TEXT,
    
    -- Campos específicos Crypto
    crypto_type VARCHAR(10),
    wallet_address VARCHAR(255),
    crypto_tx_hash VARCHAR(255),
    
    -- Auditoria
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_transactions_external_id ON transactions(external_id);
CREATE INDEX IF NOT EXISTS idx_transactions_status ON transactions(status);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON transactions(type);
CREATE INDEX IF NOT EXISTS idx_transactions_bank_code ON transactions(bank_code);
CREATE INDEX IF NOT EXISTS idx_transactions_end_to_end_id ON transactions(end_to_end_id);
CREATE INDEX IF NOT EXISTS idx_transactions_created_at ON transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_transactions_pix_key ON transactions(pix_key);
CREATE INDEX IF NOT EXISTS idx_transactions_tax_id ON transactions(tax_id);

-- Tabela de histórico de status
CREATE TABLE IF NOT EXISTS transaction_status_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    transaction_id UUID NOT NULL REFERENCES transactions(transaction_id),
    old_status VARCHAR(50),
    new_status VARCHAR(50) NOT NULL,
    message TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Índices para histórico
CREATE INDEX IF NOT EXISTS idx_transaction_status_history_transaction_id ON transaction_status_history(transaction_id);
CREATE INDEX IF NOT EXISTS idx_transaction_status_history_created_at ON transaction_status_history(created_at);

-- Tabela de taxas por banco
CREATE TABLE IF NOT EXISTS bank_fees (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bank_code VARCHAR(10) NOT NULL,
    transaction_type VARCHAR(50) NOT NULL,
    fee_percentage DECIMAL(5,4) NOT NULL DEFAULT 0,
    fee_fixed DECIMAL(10,2) NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices para taxas
CREATE INDEX IF NOT EXISTS idx_bank_fees_bank_code ON bank_fees(bank_code);
CREATE INDEX IF NOT EXISTS idx_bank_fees_transaction_type ON bank_fees(transaction_type);
CREATE INDEX IF NOT EXISTS idx_bank_fees_is_active ON bank_fees(is_active);

-- Inserir taxas padrão
INSERT INTO bank_fees (bank_code, transaction_type, fee_percentage, fee_fixed) VALUES
('001', 'PIX', 0.0050, 0.00),      -- Banco do Brasil - 0.5%
('341', 'PIX', 0.0070, 0.00),      -- Itaú - 0.7%
('756', 'PIX', 0.0040, 0.00),      -- Sicoob - 0.4%
('364', 'PIX', 0.0060, 0.00),      -- Genial - 0.6%
('001', 'TED', 0.0100, 5.00),      -- Banco do Brasil TED - 1% + R$ 5
('341', 'TED', 0.0120, 7.50),      -- Itaú TED - 1.2% + R$ 7.50
('756', 'TED', 0.0080, 4.00),      -- Sicoob TED - 0.8% + R$ 4
('001', 'BOLETO', 0.0200, 2.50),   -- Banco do Brasil Boleto - 2% + R$ 2.50
('341', 'BOLETO', 0.0250, 3.00),   -- Itaú Boleto - 2.5% + R$ 3
('756', 'BOLETO', 0.0180, 2.00),   -- Sicoob Boleto - 1.8% + R$ 2
('CRYPTO', 'CRYPTO', 0.0300, 0.00) -- Crypto - 3%
ON CONFLICT DO NOTHING;

-- Tabela de auditoria de transações
CREATE TABLE IF NOT EXISTS transaction_audit (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    transaction_id UUID NOT NULL,
    external_id VARCHAR(255) NOT NULL,
    action VARCHAR(50) NOT NULL,
    old_data JSONB,
    new_data JSONB,
    user_id VARCHAR(255),
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Índices para auditoria
CREATE INDEX IF NOT EXISTS idx_transaction_audit_transaction_id ON transaction_audit(transaction_id);
CREATE INDEX IF NOT EXISTS idx_transaction_audit_external_id ON transaction_audit(external_id);
CREATE INDEX IF NOT EXISTS idx_transaction_audit_action ON transaction_audit(action);
CREATE INDEX IF NOT EXISTS idx_transaction_audit_created_at ON transaction_audit(created_at);

-- Função para trigger de auditoria
CREATE OR REPLACE FUNCTION audit_transaction_changes()
RETURNS TRIGGER AS $$
BEGIN
    IF TG_OP = 'INSERT' THEN
        INSERT INTO transaction_audit (transaction_id, external_id, action, new_data)
        VALUES (NEW.transaction_id, NEW.external_id, 'INSERT', row_to_json(NEW));
        RETURN NEW;
    ELSIF TG_OP = 'UPDATE' THEN
        INSERT INTO transaction_audit (transaction_id, external_id, action, old_data, new_data)
        VALUES (NEW.transaction_id, NEW.external_id, 'UPDATE', row_to_json(OLD), row_to_json(NEW));
        RETURN NEW;
    ELSIF TG_OP = 'DELETE' THEN
        INSERT INTO transaction_audit (transaction_id, external_id, action, old_data)
        VALUES (OLD.transaction_id, OLD.external_id, 'DELETE', row_to_json(OLD));
        RETURN OLD;
    END IF;
    RETURN NULL;
END;
$$ LANGUAGE plpgsql;

-- Trigger para auditoria automática
CREATE TRIGGER transaction_audit_trigger
    AFTER INSERT OR UPDATE OR DELETE ON transactions
    FOR EACH ROW EXECUTE FUNCTION audit_transaction_changes();
