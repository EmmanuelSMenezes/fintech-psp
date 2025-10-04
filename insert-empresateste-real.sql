-- =====================================================
-- FintechPSP - Inserir EmpresaTeste nas tabelas reais
-- =====================================================

-- 1. Inserir usuário EmpresaTeste (representa a empresa)
INSERT INTO users (
    id,
    name,
    email,
    document,
    phone,
    is_active,
    created_at,
    updated_at
) VALUES (
    '12345678-1234-1234-1234-123456789012',
    'EmpresaTeste Ltda',
    'contato@empresateste.com',
    '12345678000199',
    '11999999999',
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    name = EXCLUDED.name,
    phone = EXCLUDED.phone,
    updated_at = NOW();

-- 2. Inserir usuário cliente@empresateste.com
INSERT INTO users (
    id,
    name,
    email,
    document,
    phone,
    is_active,
    created_at,
    updated_at
) VALUES (
    '87654321-4321-4321-4321-210987654321',
    'Cliente EmpresaTeste',
    'cliente@empresateste.com',
    '12345678909',
    '11888888888',
    true,
    NOW(),
    NOW()
) ON CONFLICT (email) DO UPDATE SET
    name = EXCLUDED.name,
    phone = EXCLUDED.phone,
    updated_at = NOW();

-- 3. Inserir bancos personalizados (Sicoob)
INSERT INTO bancos_personalizados (
    id,
    cliente_id,
    bank_code,
    endpoint,
    credentials_template,
    created_at,
    updated_at
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    '12345678-1234-1234-1234-123456789012',
    '756',
    'https://sandbox.sicoob.com.br',
    '{"clientId":"9b5e603e428cc477a2841e2683c92d21","environment":"SANDBOX","baseUrl":"https://sandbox.sicoob.com.br","authUrl":"https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token","scopes":["boletos_consulta","boletos_inclusao","boletos_alteracao","pagamentos_inclusao","pagamentos_alteracao","pagamentos_consulta","cco_saldo","cco_extrato","cco_consulta","cco_transferencias","pix_pagamentos","pix_recebimentos","pix_consultas"]}',
    NOW(),
    NOW()
) ON CONFLICT (cliente_id, bank_code) DO UPDATE SET
    endpoint = EXCLUDED.endpoint,
    credentials_template = EXCLUDED.credentials_template,
    updated_at = NOW();

-- 4. Inserir outros bancos para referência
INSERT INTO bancos_personalizados (
    id,
    cliente_id,
    bank_code,
    endpoint,
    credentials_template,
    created_at,
    updated_at
) VALUES 
(
    '22222222-2222-2222-2222-222222222222',
    '12345678-1234-1234-1234-123456789012',
    '341',
    'https://api.itau.com.br',
    '{"integration":"disabled","name":"Itaú Unibanco S.A."}',
    NOW(),
    NOW()
),
(
    '33333333-3333-3333-3333-333333333333',
    '12345678-1234-1234-1234-123456789012',
    '001',
    'https://api.bb.com.br',
    '{"integration":"disabled","name":"Banco do Brasil S.A."}',
    NOW(),
    NOW()
)
ON CONFLICT (cliente_id, bank_code) DO UPDATE SET
    endpoint = EXCLUDED.endpoint,
    credentials_template = EXCLUDED.credentials_template,
    updated_at = NOW();

-- 5. Inserir conta bancária Sicoob para EmpresaTeste
INSERT INTO contas_bancarias (
    id,
    cliente_id,
    bank_code,
    account_number,
    description,
    credentials_token_id,
    created_at,
    updated_at
) VALUES (
    '44444444-4444-4444-4444-444444444444',
    '12345678-1234-1234-1234-123456789012',
    '756',
    '1234/12345-6',
    'Conta Corrente Sicoob - EmpresaTeste',
    '77777777-7777-7777-7777-777777777777',
    NOW(),
    NOW()
) ON CONFLICT (cliente_id, bank_code, account_number) DO UPDATE SET
    description = EXCLUDED.description,
    updated_at = NOW();

-- 6. Inserir token de credenciais
INSERT INTO conta_credentials_tokens (
    id,
    encrypted_credentials,
    created_at,
    updated_at
) VALUES (
    '77777777-7777-7777-7777-777777777777',
    'encrypted_sicoob_credentials_for_empresateste',
    NOW(),
    NOW()
) ON CONFLICT (id) DO UPDATE SET
    updated_at = NOW();

-- 7. Inserir configurações do sistema para limites
INSERT INTO system_configs (
    id,
    config_key,
    config_value,
    description,
    created_at,
    updated_at
) VALUES 
(
    '55555555-5555-5555-5555-555555555555',
    'pix_daily_limit_empresateste',
    '10000.00',
    'Limite diário PIX para EmpresaTeste',
    NOW(),
    NOW()
),
(
    '66666666-6666-6666-6666-666666666666',
    'ted_daily_limit_empresateste',
    '10000.00',
    'Limite diário TED para EmpresaTeste',
    NOW(),
    NOW()
),
(
    '88888888-8888-8888-8888-888888888888',
    'boleto_daily_limit_empresateste',
    '10000.00',
    'Limite diário Boleto para EmpresaTeste',
    NOW(),
    NOW()
)
ON CONFLICT (config_key) DO UPDATE SET
    config_value = EXCLUDED.config_value,
    description = EXCLUDED.description,
    updated_at = NOW();

-- 8. Inserir webhook para Sicoob
INSERT INTO webhooks (
    id,
    cliente_id,
    url,
    events,
    is_active,
    created_at,
    updated_at
) VALUES (
    '99999999-9999-9999-9999-999999999999',
    '12345678-1234-1234-1234-123456789012',
    'http://localhost:5000/webhooks/sicoob',
    '["transaction.completed","balance.updated","pix.received","boleto.paid"]',
    true,
    NOW(),
    NOW()
) ON CONFLICT (cliente_id, url) DO UPDATE SET
    events = EXCLUDED.events,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- Verificar se os dados foram inseridos corretamente
SELECT 'EmpresaTeste (empresa):' as status, count(*) as count 
FROM users WHERE email = 'contato@empresateste.com';

SELECT 'Cliente EmpresaTeste:' as status, count(*) as count 
FROM users WHERE email = 'cliente@empresateste.com';

SELECT 'Bancos configurados:' as status, count(*) as count 
FROM bancos_personalizados WHERE cliente_id = '12345678-1234-1234-1234-123456789012';

SELECT 'Conta Sicoob:' as status, count(*) as count 
FROM contas_bancarias WHERE cliente_id = '12345678-1234-1234-1234-123456789012' AND bank_code = '756';

SELECT 'Configurações de limite:' as status, count(*) as count 
FROM system_configs WHERE config_key LIKE '%empresateste%';

SELECT 'Webhook Sicoob:' as status, count(*) as count 
FROM webhooks WHERE cliente_id = '12345678-1234-1234-1234-123456789012';

-- Mostrar dados inseridos
SELECT 'DADOS INSERIDOS:' as info;
SELECT name, email, document FROM users WHERE email IN ('contato@empresateste.com', 'cliente@empresateste.com');
SELECT bank_code, endpoint FROM bancos_personalizados WHERE cliente_id = '12345678-1234-1234-1234-123456789012';
SELECT bank_code, account_number, description FROM contas_bancarias WHERE cliente_id = '12345678-1234-1234-1234-123456789012';
