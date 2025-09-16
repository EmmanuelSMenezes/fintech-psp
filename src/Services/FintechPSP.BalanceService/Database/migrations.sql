-- =====================================================
-- FintechPSP BalanceService - Database Migrations
-- =====================================================

-- Tabela de contas
CREATE TABLE IF NOT EXISTS accounts (
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    available_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    blocked_balance DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT pk_accounts PRIMARY KEY (client_id, account_id),
    CONSTRAINT chk_available_balance_positive CHECK (available_balance >= 0),
    CONSTRAINT chk_blocked_balance_positive CHECK (blocked_balance >= 0)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_accounts_client_id ON accounts(client_id);
CREATE INDEX IF NOT EXISTS idx_accounts_last_updated ON accounts(last_updated);

-- Tabela de histórico de transações (read model)
CREATE TABLE IF NOT EXISTS transaction_history (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    account_id VARCHAR(50) NOT NULL,
    transaction_id UUID NOT NULL,
    external_id VARCHAR(100) NOT NULL,
    type VARCHAR(20) NOT NULL, -- PIX, TED, BOLETO, CRYPTO
    amount DECIMAL(18,2) NOT NULL,
    description TEXT,
    status VARCHAR(20) NOT NULL, -- PENDING, COMPLETED, FAILED, CANCELLED
    operation VARCHAR(10) NOT NULL, -- DEBIT, CREDIT
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT fk_transaction_history_account 
        FOREIGN KEY (client_id, account_id) 
        REFERENCES accounts(client_id, account_id)
);

-- Índices para consultas de extrato
CREATE INDEX IF NOT EXISTS idx_transaction_history_client_date ON transaction_history(client_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_transaction_history_account_date ON transaction_history(client_id, account_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_transaction_history_external_id ON transaction_history(external_id);
CREATE INDEX IF NOT EXISTS idx_transaction_history_transaction_id ON transaction_history(transaction_id);

-- Tabela de eventos de saldo (Event Store)
CREATE TABLE IF NOT EXISTS balance_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL, -- client_id
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    event_version INTEGER NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uk_balance_events_aggregate_version UNIQUE (aggregate_id, event_version)
);

-- Índices para Event Store
CREATE INDEX IF NOT EXISTS idx_balance_events_aggregate_id ON balance_events(aggregate_id, event_version);
CREATE INDEX IF NOT EXISTS idx_balance_events_occurred_at ON balance_events(occurred_at);

-- Trigger para auditoria automática (LGPD compliance)
CREATE OR REPLACE FUNCTION audit_balance_changes()
RETURNS TRIGGER AS $$
BEGIN
    -- Log de alterações de saldo para auditoria
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
    
    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;

-- Aplicar trigger de auditoria
DROP TRIGGER IF EXISTS trigger_audit_accounts ON accounts;
CREATE TRIGGER trigger_audit_accounts
    AFTER INSERT OR UPDATE OR DELETE ON accounts
    FOR EACH ROW EXECUTE FUNCTION audit_balance_changes();

-- Função para calcular saldo total
CREATE OR REPLACE FUNCTION get_total_balance(p_client_id UUID, p_account_id VARCHAR(50) DEFAULT NULL)
RETURNS DECIMAL(18,2) AS $$
DECLARE
    total_balance DECIMAL(18,2);
BEGIN
    SELECT (available_balance + blocked_balance)
    INTO total_balance
    FROM accounts
    WHERE client_id = p_client_id
      AND (p_account_id IS NULL OR account_id = p_account_id)
    LIMIT 1;
    
    RETURN COALESCE(total_balance, 0.00);
END;
$$ LANGUAGE plpgsql;

-- Função para validar operações de saldo
CREATE OR REPLACE FUNCTION validate_balance_operation()
RETURNS TRIGGER AS $$
BEGIN
    -- Validar que saldo disponível não fica negativo
    IF NEW.available_balance < 0 THEN
        RAISE EXCEPTION 'Saldo disponível não pode ser negativo: %', NEW.available_balance;
    END IF;
    
    -- Validar que saldo bloqueado não fica negativo
    IF NEW.blocked_balance < 0 THEN
        RAISE EXCEPTION 'Saldo bloqueado não pode ser negativo: %', NEW.blocked_balance;
    END IF;
    
    -- Atualizar timestamp de última modificação
    NEW.last_updated = NOW();
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Aplicar trigger de validação
DROP TRIGGER IF EXISTS trigger_validate_balance ON accounts;
CREATE TRIGGER trigger_validate_balance
    BEFORE UPDATE ON accounts
    FOR EACH ROW EXECUTE FUNCTION validate_balance_operation();

-- View para relatórios de saldo consolidado
CREATE OR REPLACE VIEW v_balance_summary AS
SELECT 
    a.client_id,
    a.account_id,
    a.available_balance,
    a.blocked_balance,
    (a.available_balance + a.blocked_balance) as total_balance,
    a.currency,
    a.created_at,
    a.last_updated,
    COUNT(th.id) as transaction_count,
    COALESCE(SUM(CASE WHEN th.operation = 'CREDIT' THEN th.amount ELSE 0 END), 0) as total_credits,
    COALESCE(SUM(CASE WHEN th.operation = 'DEBIT' THEN th.amount ELSE 0 END), 0) as total_debits
FROM accounts a
LEFT JOIN transaction_history th ON a.client_id = th.client_id AND a.account_id = th.account_id
GROUP BY a.client_id, a.account_id, a.available_balance, a.blocked_balance, 
         a.currency, a.created_at, a.last_updated;

-- Inserir dados de teste
INSERT INTO accounts (client_id, account_id, available_balance, blocked_balance, currency) 
VALUES 
    ('550e8400-e29b-41d4-a716-446655440000', 'CONTA_PRINCIPAL', 1000.00, 0.00, 'BRL'),
    ('550e8400-e29b-41d4-a716-446655440001', 'CONTA_PRINCIPAL', 2500.50, 100.00, 'BRL'),
    ('550e8400-e29b-41d4-a716-446655440002', 'CONTA_PRINCIPAL', 500.75, 0.00, 'BRL')
ON CONFLICT (client_id, account_id) DO NOTHING;

-- Inserir histórico de teste
INSERT INTO transaction_history (client_id, account_id, transaction_id, external_id, type, amount, description, status, operation)
VALUES 
    ('550e8400-e29b-41d4-a716-446655440000', 'CONTA_PRINCIPAL', gen_random_uuid(), 'PIX-001', 'PIX', 100.00, 'Recebimento PIX', 'COMPLETED', 'CREDIT'),
    ('550e8400-e29b-41d4-a716-446655440000', 'CONTA_PRINCIPAL', gen_random_uuid(), 'TED-001', 'TED', 50.00, 'Transferência TED', 'COMPLETED', 'DEBIT'),
    ('550e8400-e29b-41d4-a716-446655440001', 'CONTA_PRINCIPAL', gen_random_uuid(), 'PIX-002', 'PIX', 200.00, 'Pagamento PIX', 'COMPLETED', 'DEBIT'),
    ('550e8400-e29b-41d4-a716-446655440001', 'CONTA_PRINCIPAL', gen_random_uuid(), 'BOLETO-001', 'BOLETO', 300.00, 'Pagamento boleto', 'COMPLETED', 'CREDIT')
ON CONFLICT DO NOTHING;
