INSERT INTO acessos (user_id, email, role, permissions, created_by, created_at, updated_at)
VALUES (
    '22222222-2222-2222-2222-222222222222',
    'cliente@empresateste.com',
    'cliente',
    '["view_dashboard", "view_transacoes", "view_contas", "edit_contas"]',
    '11111111-1111-1111-1111-111111111111',
    NOW(),
    NOW()
);
