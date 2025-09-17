-- Criar database se não existir
SELECT 'CREATE DATABASE fintech_psp'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'fintech_psp')\gexec

-- Conectar ao database
\c fintech_psp;

-- Criar extensões necessárias
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

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

-- Inserir dados de exemplo
INSERT INTO accounts (client_id, account_id, balance, currency, status) 
VALUES ('550e8400-e29b-41d4-a716-446655440000', 'CONTA_PRINCIPAL', 1000.00, 'BRL', 'ACTIVE')
ON CONFLICT (client_id, account_id) DO NOTHING;

-- Criar views para QR Codes
CREATE OR REPLACE VIEW v_active_qrcodes AS
SELECT 
    th.id,
    th.client_id,
    th.account_id,
    th.transaction_id,
    th.external_id,
    th.type,
    th.sub_type,
    th.amount,
    th.description,
    th.pix_key,
    th.bank_code,
    th.qrcode_payload,
    th.expires_at,
    th.created_at,
    th.metadata
FROM transaction_history th
WHERE th.type = 'QR_CODE_GENERATED'
  AND th.status = 'ACTIVE'
  AND (th.expires_at IS NULL OR th.expires_at > NOW());

CREATE OR REPLACE VIEW v_qrcode_stats AS
SELECT 
    DATE(created_at) as date,
    sub_type,
    COUNT(*) as total_generated,
    COUNT(CASE WHEN expires_at IS NULL THEN 1 END) as static_count,
    COUNT(CASE WHEN expires_at IS NOT NULL THEN 1 END) as dynamic_count,
    SUM(CASE WHEN amount IS NOT NULL THEN amount ELSE 0 END) as total_amount
FROM transaction_history
WHERE type = 'QR_CODE_GENERATED'
GROUP BY DATE(created_at), sub_type
ORDER BY date DESC;

-- Função para validar saldo
CREATE OR REPLACE FUNCTION validate_balance()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.balance < 0 THEN
        RAISE EXCEPTION 'Saldo não pode ser negativo: %', NEW.balance;
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Trigger para validar saldo
DROP TRIGGER IF EXISTS trigger_validate_balance ON accounts;
CREATE TRIGGER trigger_validate_balance
    BEFORE UPDATE ON accounts
    FOR EACH ROW
    EXECUTE FUNCTION validate_balance();

-- Função de auditoria
CREATE OR REPLACE FUNCTION audit_balance_changes()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO audit_logs (
        table_name,
        operation,
        old_values,
        new_values,
        user_id,
        timestamp,
        ip_address
    ) VALUES (
        TG_TABLE_NAME,
        TG_OP,
        CASE WHEN TG_OP = 'DELETE' THEN row_to_json(OLD) ELSE NULL END,
        CASE WHEN TG_OP IN ('INSERT', 'UPDATE') THEN row_to_json(NEW) ELSE NULL END,
        current_setting('app.current_user_id', true),
        NOW(),
        current_setting('app.current_ip', true)
    );
    
    RETURN CASE WHEN TG_OP = 'DELETE' THEN OLD ELSE NEW END;
END;
$$ LANGUAGE plpgsql;

-- Trigger de auditoria
DROP TRIGGER IF EXISTS trigger_audit_accounts ON accounts;
CREATE TRIGGER trigger_audit_accounts
    AFTER INSERT OR UPDATE OR DELETE ON accounts
    FOR EACH ROW
    EXECUTE FUNCTION audit_balance_changes();
