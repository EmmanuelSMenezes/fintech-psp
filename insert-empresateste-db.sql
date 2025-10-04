-- =====================================================
-- FintechPSP - Inserir EmpresaTeste diretamente no banco
-- =====================================================

-- 1. Inserir EmpresaTeste
INSERT INTO companies (
    id, 
    razao_social, 
    nome_fantasia, 
    cnpj, 
    email, 
    telefone,
    cep,
    logradouro,
    numero,
    bairro,
    cidade,
    estado,
    pais,
    status,
    created_at,
    updated_at
) VALUES (
    '12345678-1234-1234-1234-123456789012',
    'EmpresaTeste Ltda',
    'EmpresaTeste',
    '12345678000199',
    'contato@empresateste.com',
    '11999999999',
    '01310-100',
    'Av Paulista',
    '1000',
    'Bela Vista',
    'São Paulo',
    'SP',
    'Brasil',
    'Approved',
    NOW(),
    NOW()
) ON CONFLICT (cnpj) DO NOTHING;

-- 2. Inserir usuário cliente@empresateste.com
INSERT INTO users (
    id,
    email,
    nome,
    company_id,
    role,
    status,
    created_at,
    updated_at
) VALUES (
    '87654321-4321-4321-4321-210987654321',
    'cliente@empresateste.com',
    'Cliente EmpresaTeste',
    '12345678-1234-1234-1234-123456789012',
    'Cliente',
    'Active',
    NOW(),
    NOW()
) ON CONFLICT (email) DO NOTHING;

-- 3. Inserir bancos de integração
INSERT INTO banks (
    id,
    code,
    name,
    full_name,
    type,
    active,
    integration_enabled,
    created_at,
    updated_at
) VALUES 
(
    '11111111-1111-1111-1111-111111111111',
    '756',
    'Sicoob',
    'Banco Cooperativo do Brasil S.A. - Bancoob',
    'COOPERATIVE',
    true,
    true,
    NOW(),
    NOW()
),
(
    '22222222-2222-2222-2222-222222222222',
    '341',
    'Itaú',
    'Itaú Unibanco S.A.',
    'COMMERCIAL',
    true,
    false,
    NOW(),
    NOW()
),
(
    '33333333-3333-3333-3333-333333333333',
    '001',
    'Banco do Brasil',
    'Banco do Brasil S.A.',
    'COMMERCIAL',
    true,
    false,
    NOW(),
    NOW()
)
ON CONFLICT (code) DO NOTHING;

-- 4. Inserir limites de transação para EmpresaTeste
INSERT INTO transaction_limits (
    id,
    company_id,
    transaction_type,
    daily_limit,
    monthly_limit,
    min_amount,
    max_amount,
    active,
    created_at,
    updated_at
) VALUES 
(
    '44444444-4444-4444-4444-444444444444',
    '12345678-1234-1234-1234-123456789012',
    'PIX',
    10000.00,
    50000.00,
    0.01,
    10000.00,
    true,
    NOW(),
    NOW()
),
(
    '55555555-5555-5555-5555-555555555555',
    '12345678-1234-1234-1234-123456789012',
    'TED',
    10000.00,
    50000.00,
    1.00,
    10000.00,
    true,
    NOW(),
    NOW()
),
(
    '66666666-6666-6666-6666-666666666666',
    '12345678-1234-1234-1234-123456789012',
    'BOLETO',
    10000.00,
    50000.00,
    5.00,
    10000.00,
    true,
    NOW(),
    NOW()
)
ON CONFLICT (company_id, transaction_type) DO NOTHING;

-- 5. Inserir configuração de integração Sicoob
INSERT INTO integrations (
    id,
    company_id,
    bank_code,
    integration_type,
    client_id,
    environment,
    base_url,
    auth_url,
    webhook_url,
    scopes,
    active,
    created_at,
    updated_at
) VALUES (
    '77777777-7777-7777-7777-777777777777',
    '12345678-1234-1234-1234-123456789012',
    '756',
    'SICOOB_OAUTH',
    '9b5e603e428cc477a2841e2683c92d21',
    'SANDBOX',
    'https://sandbox.sicoob.com.br',
    'https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token',
    'http://localhost:5000/webhooks/sicoob',
    '["boletos_consulta","boletos_inclusao","boletos_alteracao","pagamentos_inclusao","pagamentos_alteracao","pagamentos_consulta","cco_saldo","cco_extrato","cco_consulta","cco_transferencias","pix_pagamentos","pix_recebimentos","pix_consultas"]',
    true,
    NOW(),
    NOW()
) ON CONFLICT (company_id, bank_code) DO NOTHING;

-- 6. Inserir permissões do usuário cliente
INSERT INTO user_permissions (
    id,
    user_id,
    permission,
    granted,
    created_at,
    updated_at
) VALUES 
('88888888-8888-8888-8888-888888888888', '87654321-4321-4321-4321-210987654321', 'view_dashboard', true, NOW(), NOW()),
('99999999-9999-9999-9999-999999999999', '87654321-4321-4321-4321-210987654321', 'view_transacoes', true, NOW(), NOW()),
('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '87654321-4321-4321-4321-210987654321', 'view_contas', true, NOW(), NOW()),
('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '87654321-4321-4321-4321-210987654321', 'view_extratos', true, NOW(), NOW()),
('cccccccc-cccc-cccc-cccc-cccccccccccc', '87654321-4321-4321-4321-210987654321', 'view_saldo', true, NOW(), NOW()),
('dddddddd-dddd-dddd-dddd-dddddddddddd', '87654321-4321-4321-4321-210987654321', 'transacionar_pix', true, NOW(), NOW()),
('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '87654321-4321-4321-4321-210987654321', 'transacionar_ted', true, NOW(), NOW()),
('ffffffff-ffff-ffff-ffff-ffffffffffff', '87654321-4321-4321-4321-210987654321', 'transacionar_boleto', true, NOW(), NOW())
ON CONFLICT (user_id, permission) DO NOTHING;

-- Verificar se os dados foram inseridos
SELECT 'EmpresaTeste criada:' as status, count(*) as count FROM companies WHERE cnpj = '12345678000199';
SELECT 'Usuário cliente criado:' as status, count(*) as count FROM users WHERE email = 'cliente@empresateste.com';
SELECT 'Bancos cadastrados:' as status, count(*) as count FROM banks WHERE code IN ('756', '341', '001');
SELECT 'Limites configurados:' as status, count(*) as count FROM transaction_limits WHERE company_id = '12345678-1234-1234-1234-123456789012';
SELECT 'Integração Sicoob:' as status, count(*) as count FROM integrations WHERE bank_code = '756';
SELECT 'Permissões do cliente:' as status, count(*) as count FROM user_permissions WHERE user_id = '87654321-4321-4321-4321-210987654321';
