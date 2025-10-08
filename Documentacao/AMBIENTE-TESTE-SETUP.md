# 🛠️ **SETUP AMBIENTE DE TESTES - FINTECHPSP**

## 📋 **Guia Completo de Configuração**

Este documento fornece instruções detalhadas para configurar o ambiente de testes do sistema FintechPSP.

---

## 🎯 **Objetivos do Ambiente**

### **✅ Ambiente Isolado**
- Dados de teste separados da produção
- Configurações específicas para QA
- Reset fácil entre execuções
- Logs detalhados habilitados

### **🔄 Reprodutibilidade**
- Setup automatizado
- Dados consistentes
- Configurações versionadas
- Ambiente idêntico para toda equipe

---

## 📋 **Pré-requisitos**

### **💻 Hardware Mínimo**
```
CPU: 4 cores (Intel i5 ou AMD Ryzen 5)
RAM: 8GB (16GB recomendado)
Disk: 20GB livres (SSD recomendado)
Network: Banda larga estável (10Mbps+)
```

### **🖥️ Sistema Operacional**
```
✅ Windows 10/11 (64-bit)
✅ macOS 10.15+ (Intel/Apple Silicon)
✅ Ubuntu 20.04+ LTS
✅ Docker Desktop compatível
```

### **🛠️ Software Obrigatório**

#### **Docker & Containers**
```bash
# Docker Desktop
- Versão: 4.0+
- Docker Engine: 20.10+
- Docker Compose: 2.0+
- Kubernetes: Opcional

# Verificação
docker --version
docker-compose --version
```

#### **Desenvolvimento**
```bash
# .NET SDK
- Versão: 8.0+
- Runtime: ASP.NET Core 8.0

# Node.js
- Versão: 18.0+
- NPM: 9.0+
- Yarn: Opcional

# Verificação
dotnet --version
node --version
npm --version
```

#### **Banco de Dados**
```bash
# PostgreSQL Client
- psql: 15+
- pgAdmin: 4+ (opcional)
- DBeaver: Última versão (opcional)

# Redis Client
- redis-cli: 7+
- RedisInsight: Opcional
```

#### **Ferramentas de Teste**
```bash
# API Testing
- Postman: Última versão
- Insomnia: Alternativa
- curl: Para scripts

# Git
- Git: 2.30+
- Git LFS: Para arquivos grandes
```

---

## 🚀 **Setup Passo a Passo**

### **1️⃣ Preparação Inicial**

#### **Clone do Repositório**
```bash
# Clone do projeto
git clone https://github.com/seu-org/fintechpsp.git
cd fintechpsp

# Verificar branch
git branch
git status
```

#### **Configuração de Ambiente**
```bash
# Copiar arquivo de configuração
cp .env.example .env.test

# Editar variáveis para teste
nano .env.test
```

#### **Variáveis de Ambiente para Teste**
```bash
# .env.test
ENVIRONMENT=test
LOG_LEVEL=debug

# Database URLs (portas específicas para teste)
DATABASE_URL_AUTH=postgresql://postgres:postgres@localhost:5433/fintechpsp_auth_test
DATABASE_URL_USER=postgresql://postgres:postgres@localhost:5438/fintechpsp_user_test
DATABASE_URL_TRANSACTION=postgresql://postgres:postgres@localhost:5434/fintechpsp_transaction_test
DATABASE_URL_BALANCE=postgresql://postgres:postgres@localhost:5435/fintechpsp_balance_test
DATABASE_URL_INTEGRATION=postgresql://postgres:postgres@localhost:5436/fintechpsp_integration_test
DATABASE_URL_CONFIG=postgresql://postgres:postgres@localhost:5439/fintechpsp_config_test
DATABASE_URL_WEBHOOK=postgresql://postgres:postgres@localhost:5440/fintechpsp_webhook_test
DATABASE_URL_COMPANY=postgresql://postgres:postgres@localhost:5441/fintechpsp_company_test

# Cache & Message Broker
REDIS_URL=redis://localhost:6379/1
RABBITMQ_URL=amqp://guest:guest@localhost:5672/test

# JWT Configuration
JWT_SECRET=test-secret-key-super-secure-for-testing-only
JWT_ISSUER=Mortadela-Test
JWT_AUDIENCE=Mortadela-Test
JWT_EXPIRY_MINUTES=60

# Sicoob Test Configuration
SICOOB_CLIENT_ID=test_client_id
SICOOB_CLIENT_SECRET=test_client_secret
SICOOB_BASE_URL=https://sandbox.sicoob.com.br
SICOOB_CERT_PATH=/certs/sicoob-test.p12
SICOOB_CERT_PASSWORD=test_password

# Frontend URLs
BACKOFFICE_URL=http://localhost:3000
INTERNETBANKING_URL=http://localhost:3001

# API Gateway
GATEWAY_URL=http://localhost:5000

# Webhook Test URL
WEBHOOK_TEST_URL=https://webhook.site/your-test-id
```

### **2️⃣ Infraestrutura Base**

#### **Iniciar Serviços de Infraestrutura**
```bash
# Subir apenas infraestrutura primeiro
docker-compose -f docker-compose.test.yml up -d postgres redis rabbitmq

# Aguardar serviços ficarem prontos
echo "Aguardando PostgreSQL..."
until docker-compose -f docker-compose.test.yml exec postgres pg_isready; do
  sleep 2
done

echo "Aguardando Redis..."
until docker-compose -f docker-compose.test.yml exec redis redis-cli ping; do
  sleep 2
done

echo "Aguardando RabbitMQ..."
until docker-compose -f docker-compose.test.yml exec rabbitmq rabbitmqctl status; do
  sleep 5
done

echo "✅ Infraestrutura pronta!"
```

#### **Verificar Infraestrutura**
```bash
# PostgreSQL
docker-compose -f docker-compose.test.yml exec postgres psql -U postgres -c "\l"

# Redis
docker-compose -f docker-compose.test.yml exec redis redis-cli ping

# RabbitMQ
curl -u guest:guest http://localhost:15672/api/overview
```

### **3️⃣ Preparação dos Bancos**

#### **Criar Bancos de Teste**
```bash
# Script para criar todos os bancos
cat > create-test-databases.sql << EOF
-- Criar bancos de teste
CREATE DATABASE fintechpsp_auth_test;
CREATE DATABASE fintechpsp_user_test;
CREATE DATABASE fintechpsp_transaction_test;
CREATE DATABASE fintechpsp_balance_test;
CREATE DATABASE fintechpsp_integration_test;
CREATE DATABASE fintechpsp_config_test;
CREATE DATABASE fintechpsp_webhook_test;
CREATE DATABASE fintechpsp_company_test;

-- Verificar bancos criados
\l
EOF

# Executar script
docker-compose -f docker-compose.test.yml exec postgres psql -U postgres -f /tmp/create-test-databases.sql
```

#### **Executar Migrations**
```bash
# AuthService
docker-compose -f docker-compose.test.yml exec auth-service dotnet ef database update

# UserService
docker-compose -f docker-compose.test.yml exec user-service dotnet ef database update

# Outros serviços...
# (Repetir para cada microserviço)
```

### **4️⃣ Dados de Teste**

#### **Inserir Dados Base**
```sql
-- Conectar ao banco auth_test
psql -h localhost -p 5433 -U postgres -d fintechpsp_auth_test

-- Inserir usuário admin de teste
INSERT INTO system_users (id, email, password_hash, name, role, is_active, is_master, created_at) 
VALUES (
  gen_random_uuid(),
  'admin@fintechpsp.com',
  '$2b$10$N9qo8uLOickgx2ZMRZoMye.IjPeGvGzjYwjUxcHjRMA4nAFPiO/Xi', -- admin123
  'Admin Teste',
  'Admin',
  true,
  true,
  NOW()
);

-- Inserir clientes OAuth de teste
INSERT INTO clients (client_id, client_secret, name, allowed_scopes, is_active) VALUES
('test_web_app', 'test_web_secret', 'Test Web App', 'pix,banking', true),
('test_admin', 'test_admin_secret', 'Test Admin', 'admin,pix,banking', true);
```

#### **Script de Reset de Dados**
```bash
# reset-test-data.sh
#!/bin/bash

echo "🔄 Resetando dados de teste..."

# Parar serviços
docker-compose -f docker-compose.test.yml down

# Limpar volumes
docker volume prune -f

# Subir infraestrutura
docker-compose -f docker-compose.test.yml up -d postgres redis rabbitmq

# Aguardar serviços
sleep 30

# Recriar bancos
docker-compose -f docker-compose.test.yml exec postgres psql -U postgres -f /tmp/create-test-databases.sql

# Executar migrations
./run-migrations.sh

# Inserir dados base
./insert-test-data.sh

echo "✅ Dados de teste resetados!"
```

### **5️⃣ Microserviços**

#### **Subir Todos os Serviços**
```bash
# Subir todos os microserviços
docker-compose -f docker-compose.test.yml up -d

# Verificar status
docker-compose -f docker-compose.test.yml ps

# Aguardar todos ficarem healthy
./wait-for-services.sh
```

#### **Script de Health Check**
```bash
# wait-for-services.sh
#!/bin/bash

services=(
  "http://localhost:5000/health"  # Gateway
  "http://localhost:5001/health"  # AuthService
  "http://localhost:5002/health"  # TransactionService
  "http://localhost:5003/health"  # BalanceService
  "http://localhost:5004/health"  # IntegrationService
  "http://localhost:5006/health"  # UserService
  "http://localhost:5007/health"  # ConfigService
  "http://localhost:5008/health"  # WebhookService
  "http://localhost:5009/health"  # CompanyService
)

echo "🔍 Verificando health dos serviços..."

for service in "${services[@]}"; do
  echo "Verificando $service..."
  
  for i in {1..30}; do
    if curl -s "$service" > /dev/null; then
      echo "✅ $service OK"
      break
    fi
    
    if [ $i -eq 30 ]; then
      echo "❌ $service FALHOU"
      exit 1
    fi
    
    sleep 2
  done
done

echo "🎉 Todos os serviços estão funcionando!"
```

### **6️⃣ Frontends**

#### **BackofficeWeb**
```bash
# Navegar para pasta
cd src/Web/FintechPSP.BackofficeWeb

# Instalar dependências
npm install

# Configurar ambiente
cp .env.example .env.test
echo "NEXT_PUBLIC_API_URL=http://localhost:5000" > .env.test

# Iniciar em modo desenvolvimento
npm run dev

# Verificar
curl http://localhost:3000
```

#### **InternetBankingWeb**
```bash
# Navegar para pasta
cd src/Web/FintechPSP.InternetBankingWeb

# Instalar dependências
npm install

# Configurar ambiente
cp .env.example .env.test
echo "NEXT_PUBLIC_API_URL=http://localhost:5000" > .env.test

# Iniciar em modo desenvolvimento
npm run dev

# Verificar
curl http://localhost:3001
```

---

## 🧪 **Validação do Setup**

### **✅ Checklist de Validação**

#### **Infraestrutura**
```bash
# PostgreSQL
□ Conecta em localhost:5433
□ Bancos de teste criados
□ Migrations executadas
□ Dados base inseridos

# Redis
□ Conecta em localhost:6379
□ Responde ao ping
□ Database 1 configurado

# RabbitMQ
□ Conecta em localhost:5672
□ Management UI em localhost:15672
□ Vhost /test configurado
```

#### **Microserviços**
```bash
# Health Checks
□ Gateway: http://localhost:5000/health
□ AuthService: http://localhost:5001/health
□ TransactionService: http://localhost:5002/health
□ BalanceService: http://localhost:5003/health
□ IntegrationService: http://localhost:5004/health
□ UserService: http://localhost:5006/health
□ ConfigService: http://localhost:5007/health
□ WebhookService: http://localhost:5008/health
□ CompanyService: http://localhost:5009/health
```

#### **APIs Funcionais**
```bash
# Teste de Login
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fintechpsp.com","password":"admin123"}'

# Resposta esperada: 200 OK com token JWT
```

#### **Frontends**
```bash
# BackofficeWeb
□ Carrega em http://localhost:3000
□ Página de login visível
□ Login funciona com admin@fintechpsp.com

# InternetBankingWeb
□ Carrega em http://localhost:3001
□ Página de login visível
□ Conecta com API Gateway
```

### **🔧 Troubleshooting**

#### **Problemas Comuns**

**Docker não inicia**
```bash
# Verificar Docker Desktop
docker info

# Reiniciar Docker
sudo systemctl restart docker  # Linux
# Ou reiniciar Docker Desktop
```

**Portas ocupadas**
```bash
# Verificar portas em uso
netstat -tulpn | grep :5000

# Matar processo
sudo kill -9 <PID>
```

**Banco não conecta**
```bash
# Verificar logs do PostgreSQL
docker-compose -f docker-compose.test.yml logs postgres

# Testar conexão manual
psql -h localhost -p 5433 -U postgres
```

**Serviços não sobem**
```bash
# Verificar logs
docker-compose -f docker-compose.test.yml logs auth-service

# Verificar recursos
docker stats
```

---

## 📊 **Monitoramento do Ambiente**

### **🔍 Logs Centralizados**
```bash
# Ver logs de todos os serviços
docker-compose -f docker-compose.test.yml logs -f

# Logs de serviço específico
docker-compose -f docker-compose.test.yml logs -f auth-service

# Filtrar por nível
docker-compose -f docker-compose.test.yml logs | grep ERROR
```

### **📈 Métricas de Saúde**
```bash
# Script de monitoramento
#!/bin/bash
while true; do
  echo "=== $(date) ==="
  
  # CPU e Memória
  docker stats --no-stream --format "table {{.Name}}\t{{.CPUPerc}}\t{{.MemUsage}}"
  
  # Health checks
  curl -s http://localhost:5000/health | jq .
  
  sleep 30
done
```

---

## 🚀 **Automação do Setup**

### **📜 Script Completo**
```bash
# setup-test-environment.sh
#!/bin/bash

set -e

echo "🚀 Iniciando setup do ambiente de testes..."

# 1. Verificar pré-requisitos
echo "1️⃣ Verificando pré-requisitos..."
command -v docker >/dev/null 2>&1 || { echo "Docker não encontrado"; exit 1; }
command -v docker-compose >/dev/null 2>&1 || { echo "Docker Compose não encontrado"; exit 1; }

# 2. Configurar ambiente
echo "2️⃣ Configurando ambiente..."
cp .env.example .env.test

# 3. Subir infraestrutura
echo "3️⃣ Subindo infraestrutura..."
docker-compose -f docker-compose.test.yml up -d postgres redis rabbitmq

# 4. Aguardar serviços
echo "4️⃣ Aguardando serviços ficarem prontos..."
./wait-for-infrastructure.sh

# 5. Preparar bancos
echo "5️⃣ Preparando bancos de dados..."
./create-test-databases.sh

# 6. Subir microserviços
echo "6️⃣ Subindo microserviços..."
docker-compose -f docker-compose.test.yml up -d

# 7. Aguardar health checks
echo "7️⃣ Verificando health dos serviços..."
./wait-for-services.sh

# 8. Inserir dados de teste
echo "8️⃣ Inserindo dados de teste..."
./insert-test-data.sh

# 9. Subir frontends
echo "9️⃣ Subindo frontends..."
./start-frontends.sh

echo "✅ Ambiente de testes configurado com sucesso!"
echo "🌐 BackofficeWeb: http://localhost:3000"
echo "🌐 InternetBankingWeb: http://localhost:3001"
echo "🔗 API Gateway: http://localhost:5000"
```

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**⚙️ Ambiente**: Teste/QA
