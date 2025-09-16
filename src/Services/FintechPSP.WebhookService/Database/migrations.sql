-- =====================================================
-- FintechPSP WebhookService - Database Migrations
-- =====================================================

-- Tabela de webhooks
CREATE TABLE IF NOT EXISTS webhooks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL,
    url VARCHAR(2000) NOT NULL,
    events JSONB NOT NULL,
    secret VARCHAR(255),
    active BOOLEAN NOT NULL DEFAULT true,
    description TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_triggered TIMESTAMP WITH TIME ZONE,
    success_count INTEGER NOT NULL DEFAULT 0,
    failure_count INTEGER NOT NULL DEFAULT 0,
    
    CONSTRAINT chk_url_format CHECK (url ~ '^https?://'),
    CONSTRAINT chk_events_not_empty CHECK (jsonb_array_length(events) > 0),
    CONSTRAINT chk_success_count_positive CHECK (success_count >= 0),
    CONSTRAINT chk_failure_count_positive CHECK (failure_count >= 0)
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_webhooks_client_id ON webhooks(client_id);
CREATE INDEX IF NOT EXISTS idx_webhooks_active ON webhooks(active);
CREATE INDEX IF NOT EXISTS idx_webhooks_events ON webhooks USING GIN(events);
CREATE INDEX IF NOT EXISTS idx_webhooks_created_at ON webhooks(created_at);

-- Tabela de entregas de webhook
CREATE TABLE IF NOT EXISTS webhook_deliveries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    webhook_id UUID NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING', -- PENDING, SUCCESS, FAILED, RETRYING
    http_status_code INTEGER,
    response_body TEXT,
    error_message TEXT,
    attempt_count INTEGER NOT NULL DEFAULT 0,
    next_retry_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    delivered_at TIMESTAMP WITH TIME ZONE,
    
    CONSTRAINT fk_webhook_deliveries_webhook 
        FOREIGN KEY (webhook_id) 
        REFERENCES webhooks(id) ON DELETE CASCADE,
    CONSTRAINT chk_status_valid 
        CHECK (status IN ('PENDING', 'SUCCESS', 'FAILED', 'RETRYING')),
    CONSTRAINT chk_attempt_count_positive CHECK (attempt_count >= 0),
    CONSTRAINT chk_http_status_valid 
        CHECK (http_status_code IS NULL OR (http_status_code >= 100 AND http_status_code < 600))
);

-- Índices para consultas de entregas
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_webhook_id ON webhook_deliveries(webhook_id);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_status ON webhook_deliveries(status);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_created_at ON webhook_deliveries(created_at DESC);
CREATE INDEX IF NOT EXISTS idx_webhook_deliveries_retry ON webhook_deliveries(status, next_retry_at) 
    WHERE status = 'FAILED' AND next_retry_at IS NOT NULL;

-- Tabela de eventos de webhook (Event Store)
CREATE TABLE IF NOT EXISTS webhook_events (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    aggregate_id UUID NOT NULL, -- webhook_id
    event_type VARCHAR(100) NOT NULL,
    event_data JSONB NOT NULL,
    event_version INTEGER NOT NULL,
    occurred_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT uk_webhook_events_aggregate_version UNIQUE (aggregate_id, event_version)
);

-- Índices para Event Store
CREATE INDEX IF NOT EXISTS idx_webhook_events_aggregate_id ON webhook_events(aggregate_id, event_version);
CREATE INDEX IF NOT EXISTS idx_webhook_events_occurred_at ON webhook_events(occurred_at);

-- Trigger para auditoria automática (LGPD compliance)
CREATE OR REPLACE FUNCTION audit_webhook_changes()
RETURNS TRIGGER AS $$
BEGIN
    -- Log de alterações de webhook para auditoria
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
DROP TRIGGER IF EXISTS trigger_audit_webhooks ON webhooks;
CREATE TRIGGER trigger_audit_webhooks
    AFTER INSERT OR UPDATE OR DELETE ON webhooks
    FOR EACH ROW EXECUTE FUNCTION audit_webhook_changes();

-- Função para limpar entregas antigas (mais de 30 dias)
CREATE OR REPLACE FUNCTION cleanup_old_webhook_deliveries()
RETURNS INTEGER AS $$
DECLARE
    deleted_count INTEGER;
BEGIN
    DELETE FROM webhook_deliveries 
    WHERE created_at < NOW() - INTERVAL '30 days'
      AND status IN ('SUCCESS', 'FAILED');
    
    GET DIAGNOSTICS deleted_count = ROW_COUNT;
    
    RETURN deleted_count;
END;
$$ LANGUAGE plpgsql;

-- Função para obter estatísticas de webhook
CREATE OR REPLACE FUNCTION get_webhook_stats(p_webhook_id UUID)
RETURNS TABLE (
    total_deliveries BIGINT,
    successful_deliveries BIGINT,
    failed_deliveries BIGINT,
    pending_deliveries BIGINT,
    success_rate DECIMAL(5,2),
    avg_response_time DECIMAL(10,2)
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        COUNT(*) as total_deliveries,
        COUNT(*) FILTER (WHERE status = 'SUCCESS') as successful_deliveries,
        COUNT(*) FILTER (WHERE status = 'FAILED') as failed_deliveries,
        COUNT(*) FILTER (WHERE status IN ('PENDING', 'RETRYING')) as pending_deliveries,
        CASE 
            WHEN COUNT(*) > 0 THEN 
                ROUND((COUNT(*) FILTER (WHERE status = 'SUCCESS') * 100.0 / COUNT(*)), 2)
            ELSE 0.00
        END as success_rate,
        COALESCE(
            AVG(EXTRACT(EPOCH FROM (delivered_at - created_at)) * 1000) 
            FILTER (WHERE status = 'SUCCESS' AND delivered_at IS NOT NULL), 
            0
        ) as avg_response_time
    FROM webhook_deliveries 
    WHERE webhook_id = p_webhook_id;
END;
$$ LANGUAGE plpgsql;

-- View para relatórios de webhook
CREATE OR REPLACE VIEW v_webhook_summary AS
SELECT 
    w.id,
    w.client_id,
    w.url,
    w.events,
    w.active,
    w.description,
    w.created_at,
    w.last_triggered,
    w.success_count,
    w.failure_count,
    COUNT(wd.id) as total_deliveries,
    COUNT(wd.id) FILTER (WHERE wd.status = 'SUCCESS') as successful_deliveries,
    COUNT(wd.id) FILTER (WHERE wd.status = 'FAILED') as failed_deliveries,
    COUNT(wd.id) FILTER (WHERE wd.status IN ('PENDING', 'RETRYING')) as pending_deliveries,
    CASE 
        WHEN COUNT(wd.id) > 0 THEN 
            ROUND((COUNT(wd.id) FILTER (WHERE wd.status = 'SUCCESS') * 100.0 / COUNT(wd.id)), 2)
        ELSE 0.00
    END as success_rate
FROM webhooks w
LEFT JOIN webhook_deliveries wd ON w.id = wd.webhook_id
GROUP BY w.id, w.client_id, w.url, w.events, w.active, w.description, 
         w.created_at, w.last_triggered, w.success_count, w.failure_count;

-- Inserir dados de teste
INSERT INTO webhooks (id, client_id, url, events, active, description) 
VALUES 
    ('550e8400-e29b-41d4-a716-446655440010', '550e8400-e29b-41d4-a716-446655440000', 
     'https://api.exemplo.com/webhook', '["pix.created", "pix.completed"]', true, 'Webhook para PIX'),
    ('550e8400-e29b-41d4-a716-446655440011', '550e8400-e29b-41d4-a716-446655440001', 
     'https://app.cliente.com/notifications', '["transaction.completed", "transaction.failed"]', true, 'Notificações de transação'),
    ('550e8400-e29b-41d4-a716-446655440012', '550e8400-e29b-41d4-a716-446655440002', 
     'https://webhook.site/test', '["boleto.created", "boleto.paid"]', false, 'Webhook de teste')
ON CONFLICT (id) DO NOTHING;

-- Inserir entregas de teste
INSERT INTO webhook_deliveries (webhook_id, event_type, payload, status, http_status_code, delivered_at)
VALUES 
    ('550e8400-e29b-41d4-a716-446655440010', 'pix.created', '{"transactionId": "123", "amount": 100.00}', 'SUCCESS', 200, NOW() - INTERVAL '1 hour'),
    ('550e8400-e29b-41d4-a716-446655440010', 'pix.completed', '{"transactionId": "123", "status": "completed"}', 'SUCCESS', 200, NOW() - INTERVAL '30 minutes'),
    ('550e8400-e29b-41d4-a716-446655440011', 'transaction.completed', '{"transactionId": "456", "amount": 250.00}', 'FAILED', 500, NULL),
    ('550e8400-e29b-41d4-a716-446655440012', 'boleto.created', '{"boletoId": "789", "dueDate": "2024-12-31"}', 'PENDING', NULL, NULL)
ON CONFLICT DO NOTHING;
