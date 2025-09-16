-- Criar databases para cada microservice
CREATE DATABASE auth_service;
CREATE DATABASE transaction_service;
CREATE DATABASE balance_service;
CREATE DATABASE webhook_service;
CREATE DATABASE user_service;
CREATE DATABASE config_service;

-- Criar usuários específicos para cada serviço (opcional, para maior segurança)
CREATE USER auth_user WITH PASSWORD 'auth_pass';
CREATE USER transaction_user WITH PASSWORD 'transaction_pass';
CREATE USER balance_user WITH PASSWORD 'balance_pass';
CREATE USER webhook_user WITH PASSWORD 'webhook_pass';
CREATE USER user_service_user WITH PASSWORD 'user_pass';
CREATE USER config_user WITH PASSWORD 'config_pass';

-- Conceder permissões
GRANT ALL PRIVILEGES ON DATABASE auth_service TO auth_user;
GRANT ALL PRIVILEGES ON DATABASE transaction_service TO transaction_user;
GRANT ALL PRIVILEGES ON DATABASE balance_service TO balance_user;
GRANT ALL PRIVILEGES ON DATABASE webhook_service TO webhook_user;
GRANT ALL PRIVILEGES ON DATABASE user_service TO user_service_user;
GRANT ALL PRIVILEGES ON DATABASE config_service TO config_user;

-- Conceder permissões ao usuário principal também
GRANT ALL PRIVILEGES ON DATABASE auth_service TO fintech_user;
GRANT ALL PRIVILEGES ON DATABASE transaction_service TO fintech_user;
GRANT ALL PRIVILEGES ON DATABASE balance_service TO fintech_user;
GRANT ALL PRIVILEGES ON DATABASE webhook_service TO fintech_user;
GRANT ALL PRIVILEGES ON DATABASE user_service TO fintech_user;
GRANT ALL PRIVILEGES ON DATABASE config_service TO fintech_user;
