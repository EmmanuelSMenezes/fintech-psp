# 🐳 DOCKERFILES FINTECHPSP

Esta pasta contém todos os Dockerfiles necessários para subir o ambiente completo do FintechPSP.

## 📁 Estrutura

```
dockerfiles/
├── README.md                    # Este arquivo
├── Dockerfile.APIGateway        # API Gateway (Ocelot)
├── Dockerfile.AuthService       # Serviço de Autenticação
├── Dockerfile.BalanceService    # Serviço de Saldos
├── Dockerfile.TransactionService # Serviço de Transações
├── Dockerfile.IntegrationService # Serviço de Integrações (Sicoob)
├── Dockerfile.UserService       # Serviço de Usuários
├── Dockerfile.ConfigService     # Serviço de Configurações
├── Dockerfile.WebhookService    # Serviço de Webhooks
├── Dockerfile.CompanyService    # Serviço de Empresas
├── Dockerfile.BackofficeWeb     # Frontend Backoffice
├── Dockerfile.InternetBankingWeb # Frontend Internet Banking
└── docker-compose-complete.yml  # Docker Compose completo
```

## 🚀 Como usar

### 1. Subir apenas a infraestrutura
```bash
cd dockerfiles
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis
```

### 2. Subir todos os serviços
```bash
cd dockerfiles
docker-compose -f docker-compose-complete.yml up -d
```

### 3. Subir serviços específicos
```bash
cd dockerfiles
docker-compose -f docker-compose-complete.yml up -d auth-service balance-service
```

### 4. Verificar status
```bash
docker ps
docker-compose -f docker-compose-complete.yml logs -f
```

## 🔧 Configurações

- **PostgreSQL**: porta 5433
- **RabbitMQ**: porta 5673 (management: 15673)
- **Redis**: porta 6380
- **API Gateway**: porta 5000
- **Serviços**: portas 5001-5010
- **Frontends**: portas 3000-3001

## 📊 Monitoramento

Após subir os serviços, acesse:
- **API Gateway**: http://localhost:5000
- **RabbitMQ Management**: http://localhost:15673 (guest/guest)
- **Backoffice**: http://localhost:3000
- **Internet Banking**: http://localhost:3001

## 🧪 Testes

Execute os scripts de teste após subir o ambiente:
```bash
# Teste simples
./test-simple.ps1

# Teste completo
./test-final-simple.ps1
```
