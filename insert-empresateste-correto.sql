-- =====================================================
-- Inserir EmpresaTeste com estrutura correta
-- =====================================================

-- 1. Inserir empresa na tabela company_service.companies (estrutura correta)
INSERT INTO company_service.companies (
    id,
    razao_social,
    nome_fantasia,
    cnpj,
    inscricao_estadual,
    inscricao_municipal,
    cep,
    logradouro,
    numero,
    complemento,
    bairro,
    cidade,
    estado,
    pais,
    telefone,
    email,
    website,
    capital_social,
    data_constituicao,
    natureza_juridica,
    atividade_principal,
    regime_tributario,
    status,
    observacoes,
    created_at,
    updated_at
) VALUES (
    '12345678-1234-1234-1234-123456789012',
    'EmpresaTeste Ltda',
    'EmpresaTeste',
    '12345678000199',
    '123456789',
    '987654321',
    '01310-100',
    'Av Paulista',
    '1000',
    'Sala 1001',
    'Bela Vista',
    'São Paulo',
    'SP',
    'Brasil',
    '11999999999',
    'contato@empresateste.com',
    'https://empresateste.com.br',
    1000000.00,
    '2025-01-01',
    'Sociedade Empresária Limitada',
    'Atividades de tecnologia da informação',
    'Lucro Presumido',
    'Approved',
    'Empresa criada para testes da trilha integrada PSP-Sicoob',
    NOW(),
    NOW()
) ON CONFLICT (cnpj) DO UPDATE SET
    razao_social = EXCLUDED.razao_social,
    nome_fantasia = EXCLUDED.nome_fantasia,
    email = EXCLUDED.email,
    telefone = EXCLUDED.telefone,
    status = EXCLUDED.status,
    updated_at = NOW();

-- 2. Verificar se a empresa foi inserida
SELECT 'Empresa inserida:' as info, count(*) as count FROM company_service.companies WHERE cnpj = '12345678000199';

-- 3. Mostrar dados da empresa
SELECT razao_social, cnpj, email, status FROM company_service.companies WHERE cnpj = '12345678000199';

-- 4. Inserir configuração de banco Sicoob no config_service (se existir schema)
DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.schemata WHERE schema_name = 'config_service') THEN
        -- Inserir banco Sicoob personalizado
        INSERT INTO bancos_personalizados (
            id,
            cliente_id,
            bank_code,
            endpoint,
            credentials_template,
            created_at,
            updated_at
        ) VALUES (
            '44444444-4444-4444-4444-444444444444',
            '22222222-2222-2222-2222-222222222222',
            '756',
            'https://sandbox.sicoob.com.br',
            '{"clientId":"9b5e603e428cc477a2841e2683c92d21","environment":"SANDBOX","baseUrl":"https://sandbox.sicoob.com.br","authUrl":"https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token","scopes":["boletos_consulta","boletos_inclusao","boletos_alteracao","pagamentos_inclusao","pagamentos_alteracao","pagamentos_consulta","cco_saldo","cco_extrato","cco_consulta","cco_transferencias","pix_pagamentos","pix_recebimentos","pix_consultas"],"webhookUrl":"http://localhost:5000/webhooks/sicoob"}',
            NOW(),
            NOW()
        ) ON CONFLICT (cliente_id, bank_code) DO UPDATE SET
            endpoint = EXCLUDED.endpoint,
            credentials_template = EXCLUDED.credentials_template,
            updated_at = NOW();
    END IF;
END $$;

-- 5. Inserir configurações de sistema para limites
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
    'Limite diário PIX para EmpresaTeste (CNPJ: 12345678000199)',
    NOW(),
    NOW()
),
(
    '66666666-6666-6666-6666-666666666666',
    'ted_daily_limit_empresateste',
    '10000.00',
    'Limite diário TED para EmpresaTeste (CNPJ: 12345678000199)',
    NOW(),
    NOW()
),
(
    '77777777-7777-7777-7777-777777777777',
    'boleto_daily_limit_empresateste',
    '10000.00',
    'Limite diário Boleto para EmpresaTeste (CNPJ: 12345678000199)',
    NOW(),
    NOW()
),
(
    '88888888-8888-8888-8888-888888888888',
    'sicoob_integration_empresateste',
    'enabled',
    'Integração Sicoob habilitada para EmpresaTeste',
    NOW(),
    NOW()
)
ON CONFLICT (config_key) DO UPDATE SET
    config_value = EXCLUDED.config_value,
    description = EXCLUDED.description,
    updated_at = NOW();

-- 6. Verificar dados finais
SELECT 'RESUMO FINAL:' as info;
SELECT 'Empresas no CompanyService:' as info, count(*) as count FROM company_service.companies;
SELECT 'Usuários no sistema:' as info, count(*) as count FROM public.users;
SELECT 'Contas bancárias:' as info, count(*) as count FROM user_service.contas_bancarias;
SELECT 'Bancos personalizados:' as info, count(*) as count FROM public.bancos_personalizados;
SELECT 'Configurações de sistema:' as info, count(*) as count FROM public.system_configs WHERE config_key LIKE '%empresateste%';

-- 7. Mostrar dados específicos da EmpresaTeste
SELECT 'DADOS EMPRESATESTE:' as info;
SELECT id, razao_social, cnpj, email, status FROM company_service.companies WHERE cnpj = '12345678000199';
SELECT name, email, document FROM public.users WHERE email LIKE '%empresateste%';
SELECT bank_code, account_number, description FROM user_service.contas_bancarias WHERE description LIKE '%EmpresaTeste%';
