-- =====================================================
-- Inserir EmpresaTeste no CompanyService Schema
-- =====================================================

-- 1. Inserir empresa na tabela company_service.companies
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
    numero_contrato,
    data_contrato,
    junta_comercial,
    nire,
    capital_social,
    atividade_principal,
    atividades_secundarias,
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
    'CONT-2025-001',
    '2025-01-01',
    'JUCESP',
    '35300000001',
    1000000.00,
    'Atividades de tecnologia da informação',
    '["Desenvolvimento de software", "Consultoria em TI"]',
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

-- 2. Inserir solicitante na tabela company_service.applicants
INSERT INTO company_service.applicants (
    id,
    company_id,
    nome_completo,
    cpf,
    rg,
    orgao_expedidor,
    data_nascimento,
    estado_civil,
    nacionalidade,
    profissao,
    email,
    telefone,
    celular,
    cep,
    logradouro,
    numero,
    complemento,
    bairro,
    cidade,
    estado,
    pais,
    renda_mensal,
    cargo,
    is_main_representative,
    created_at,
    updated_at
) VALUES (
    '87654321-4321-4321-4321-210987654321',
    '12345678-1234-1234-1234-123456789012',
    'João Silva',
    '12345678909',
    '123456789',
    'SSP-SP',
    '1980-01-01',
    'Solteiro',
    'Brasileira',
    'Empresário',
    'joao@empresateste.com',
    '11888888888',
    '11999999999',
    '01310-100',
    'Av Paulista',
    '1000',
    'Apto 101',
    'Bela Vista',
    'São Paulo',
    'SP',
    'Brasil',
    15000.00,
    'Diretor',
    true,
    NOW(),
    NOW()
) ON CONFLICT (cpf) DO UPDATE SET
    nome_completo = EXCLUDED.nome_completo,
    email = EXCLUDED.email,
    telefone = EXCLUDED.telefone,
    updated_at = NOW();

-- 3. Inserir representante legal na tabela company_service.legal_representatives
INSERT INTO company_service.legal_representatives (
    id,
    company_id,
    nome_completo,
    cpf,
    rg,
    orgao_expedidor,
    data_nascimento,
    estado_civil,
    nacionalidade,
    profissao,
    email,
    telefone,
    celular,
    cep,
    logradouro,
    numero,
    complemento,
    bairro,
    cidade,
    estado,
    pais,
    cargo,
    type,
    percentual_participacao,
    poderes_representacao,
    pode_assinar_sozinho,
    limite_alcada,
    created_at,
    updated_at
) VALUES (
    '11111111-1111-1111-1111-111111111111',
    '12345678-1234-1234-1234-123456789012',
    'João Silva',
    '12345678909',
    '123456789',
    'SSP-SP',
    '1980-01-01',
    'Solteiro',
    'Brasileira',
    'Empresário',
    'joao@empresateste.com',
    '11888888888',
    '11999999999',
    '01310-100',
    'Av Paulista',
    '1000',
    'Apto 101',
    'Bela Vista',
    'São Paulo',
    'SP',
    'Brasil',
    'Diretor',
    'SOCIO',
    100.00,
    'Poderes gerais de administração',
    true,
    1000000.00,
    NOW(),
    NOW()
) ON CONFLICT (company_id, cpf) DO UPDATE SET
    nome_completo = EXCLUDED.nome_completo,
    email = EXCLUDED.email,
    telefone = EXCLUDED.telefone,
    updated_at = NOW();

-- 4. Inserir usuário cliente na tabela public.users (se não existir)
INSERT INTO public.users (
    id,
    name,
    email,
    document,
    phone,
    is_active,
    created_at,
    updated_at
) VALUES (
    '22222222-2222-2222-2222-222222222222',
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

-- 5. Inserir conta bancária Sicoob no user_service
INSERT INTO user_service.contas_bancarias (
    conta_id,
    cliente_id,
    bank_code,
    account_number,
    description,
    credentials_token_id,
    is_active,
    created_at,
    updated_at
) VALUES (
    '33333333-3333-3333-3333-333333333333',
    '22222222-2222-2222-2222-222222222222',
    '756',
    '1234/12345-6',
    'Conta Corrente Sicoob - EmpresaTeste',
    'sicoob_cred_empresateste_001',
    true,
    NOW(),
    NOW()
) ON CONFLICT (cliente_id, bank_code, account_number) DO UPDATE SET
    description = EXCLUDED.description,
    is_active = EXCLUDED.is_active,
    updated_at = NOW();

-- 6. Inserir token de credenciais
INSERT INTO user_service.credentials_tokens (
    token_id,
    encrypted_data,
    encryption_key_id,
    created_at,
    updated_at,
    expires_at
) VALUES (
    'sicoob_cred_empresateste_001',
    '{"clientId":"9b5e603e428cc477a2841e2683c92d21","environment":"SANDBOX","baseUrl":"https://sandbox.sicoob.com.br","authUrl":"https://auth.sicoob.com.br/auth/realms/cooperado/protocol/openid-connect/token","scopes":["boletos_consulta","boletos_inclusao","boletos_alteracao","pagamentos_inclusao","pagamentos_alteracao","pagamentos_consulta","cco_saldo","cco_extrato","cco_consulta","cco_transferencias","pix_pagamentos","pix_recebimentos","pix_consultas"]}',
    'master_key_001',
    NOW(),
    NOW(),
    NOW() + INTERVAL '1 year'
) ON CONFLICT (token_id) DO UPDATE SET
    encrypted_data = EXCLUDED.encrypted_data,
    updated_at = NOW();

-- Verificar dados inseridos
SELECT 'COMPANY SERVICE - Empresas:' as info, count(*) as count FROM company_service.companies;
SELECT 'COMPANY SERVICE - Solicitantes:' as info, count(*) as count FROM company_service.applicants;
SELECT 'COMPANY SERVICE - Representantes:' as info, count(*) as count FROM company_service.legal_representatives;
SELECT 'USER SERVICE - Contas:' as info, count(*) as count FROM user_service.contas_bancarias;
SELECT 'USER SERVICE - Tokens:' as info, count(*) as count FROM user_service.credentials_tokens;

-- Mostrar dados da EmpresaTeste
SELECT 'DADOS DA EMPRESATESTE:' as info;
SELECT razao_social, cnpj, email, status FROM company_service.companies WHERE cnpj = '12345678000199';
SELECT bank_code, account_number, description FROM user_service.contas_bancarias WHERE cliente_id = '22222222-2222-2222-2222-222222222222';
