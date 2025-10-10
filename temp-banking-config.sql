INSERT INTO banking_configs (name, type, enabled, settings, created_at, created_by) 
VALUES (
    'Sicoob_EmpresaTeste', 
    'SICOOB', 
    true, 
    '{
        "client_id": "12345678-1234-1234-1234-123456789012",
        "bank_code": "SICOOB",
        "bank_name": "Sicoob",
        "api_base_url": "https://sandbox.sicoob.com.br",
        "client_id_oauth": "dd533251-7a11-4939-8713-016763653f3c",
        "certificate_path": "/app/Certificates/sicoob-certificate.pfx",
        "certificate_password": "Vi294141",
        "supports_pix": true,
        "supports_ted": true,
        "supports_boleto": true,
        "environment": "SANDBOX"
    }', 
    NOW(), 
    'system'
) ON CONFLICT (name) DO UPDATE SET 
    enabled = true, 
    settings = EXCLUDED.settings, 
    updated_at = NOW(),
    updated_by = 'system';
