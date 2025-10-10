-- Criar transação PIX de R$ 100,00
INSERT INTO transaction_history (
    client_id, 
    account_id, 
    transaction_id, 
    external_id, 
    type, 
    amount, 
    description, 
    status, 
    operation, 
    created_at, 
    sub_type, 
    currency, 
    pix_key
) VALUES (
    '12345678-1234-1234-1234-123456789012',
    'CONTA_EMPRESATESTE',
    gen_random_uuid(),
    'PIX_SICOOB_' || EXTRACT(EPOCH FROM NOW())::text,
    'PIX',
    100.00,
    'Pagamento PIX via Sicoob - Teste EmpresaTeste',
    'COMPLETED',
    'DEBIT',
    NOW(),
    'INSTANT',
    'BRL',
    'cliente@empresateste.com'
);

-- Atualizar saldo da conta (debitar R$ 100,00)
UPDATE accounts 
SET available_balance = available_balance - 100.00,
    last_updated = NOW()
WHERE client_id = '12345678-1234-1234-1234-123456789012' 
  AND account_id = 'CONTA_EMPRESATESTE';
