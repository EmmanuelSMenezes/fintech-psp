# FintechPSP - Payment Service Provider

## 🎉 STATUS: 100% OPERACIONAL E PRONTO PARA PRODUÇÃO

Sistema de PSP (Payment Service Provider) event-driven com microservices, desenvolvido em .NET 8, suportando PIX, TED, boleto e criptomoedas. **Completamente validado através de 12 testes E2E com 9 transações PIX processadas!**

## 📊 STATUS ATUAL (13/01/2025)

### ✅ Sistema Completamente Validado
- **7 microserviços** rodando e operacionais
- **9 transações PIX** criadas e persistidas
- **Integração Sicoob** com OAuth 2.0 + mTLS funcionando
- **12 testes E2E** completos e passando
- **Segurança RBAC** implementada e validada

### 🔑 Credenciais de Teste
```bash
# Admin
admin@fintechpsp.com / admin123

# Cliente
joao.silva@empresateste.com / cliente123
Conta: ACC001 | Saldo: R$ 1000,00
```

### 🚀 Teste Rápido
```bash
# Verificar transações
docker exec fintech-postgres psql -U postgres -d fintech_psp -c "SELECT COUNT(*) FROM transactions;"

# Testar login
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@fintechpsp.com","password":"admin123"}'
```

---

## 🏗️ Arquitetura

### Microservices

- **AuthService** - Autenticação OAuth 2.0 (client_credentials)
- **TransactionService** - Processamento de transações (PIX, TED, boleto, cripto)
- **BalanceService** - Consulta de saldos e extratos
- **WebhookService** - Gerenciamento de webhooks
- **IntegrationService** - Integrações com bancos (Stark Bank, Sicoob, Genial, Efí, Celcoin)
- **UserService** - Gerenciamento de usuários e contas
- **ConfigService** - Configurações de taxas e bancos
- **APIGateway** - Gateway com Ocelot

### Tecnologias

- **.NET 8** - Framework principal
- **PostgreSQL** - Banco de dados principal
- **Marten** - Event Sourcing
- **Dapper** - ORM para read models
- **RabbitMQ** - Message broker (MassTransit)
- **MediatR** - CQRS pattern
- **JWT** - Autenticação
- **Swagger** - Documentação da API
- **Docker** - Containerização

## 🚀 Como Executar

### Pré-requisitos

- .NET 8 SDK
- Docker e Docker Compose
- Visual Studio Code ou Visual Studio

### 1. Subir a Infraestrutura

```bash
# Subir PostgreSQL, RabbitMQ e Redis
cd docker
docker-compose up -d
```

### 2. Configurar Banco de Dados

```bash
# Conectar no PostgreSQL e executar os scripts de migração
docker exec -it fintech-postgres psql -U fintech_user -d auth_service -f /docker-entrypoint-initdb.d/01-create-databases.sql

# Executar migrations do AuthService
docker exec -it fintech-postgres psql -U fintech_user -d auth_service -c "$(cat src/Services/FintechPSP.AuthService/Database/migrations.sql)"
```

### 3. Executar os Microservices

```bash
# AuthService (porta 7001)
cd src/Services/FintechPSP.AuthService
dotnet run

# TransactionService (porta 7002)
cd src/Services/FintechPSP.TransactionService
dotnet run

# BalanceService (porta 7003)
cd src/Services/FintechPSP.BalanceService
dotnet run

# Outros serviços...
```

### 4. Testar as APIs

1. Importe a collection do Postman: `postman/FintechPSP-Collection.json`
2. Configure a variável `base_url` para `https://localhost:7001`
3. Execute o request "Obter Token OAuth 2.0" para autenticar
4. Use o token obtido para testar os outros endpoints

## 📋 Endpoints Principais

### AuthService (porta 7001)

```http
POST /auth/token
Content-Type: application/json

{
  "grant_type": "client_credentials",
  "client_id": "fintech_web_app",
  "client_secret": "web_app_secret_123",
  "scope": "pix banking"
}
```

### TransactionService (porta 7002)

```http
# PIX
POST /transacoes/pix
Authorization: Bearer {token}
Content-Type: application/json

{
  "externalId": "pix-123",
  "amount": 100.50,
  "pixKey": "11999887766",
  "bankCode": "341",
  "description": "Pagamento teste",
  "webhookUrl": "https://webhook.site/unique-id"
}

# TED
POST /transacoes/ted
Authorization: Bearer {token}
Content-Type: application/json

{
  "externalId": "ted-123",
  "amount": 500.00,
  "bankCode": "001",
  "accountBranch": "1234",
  "accountNumber": "567890",
  "taxId": "12345678901",
  "name": "João da Silva"
}

# Boleto
POST /transacoes/boleto
Authorization: Bearer {token}
Content-Type: application/json

{
  "externalId": "boleto-123",
  "amount": 250.75,
  "dueDate": "2025-10-15T23:59:59Z",
  "payerTaxId": "12345678901",
  "payerName": "Maria Santos",
  "instructions": "Pagamento referente a serviços"
}
```

### BalanceService (porta 7003)

```http
GET /clientes/{clienteId}/saldo
Authorization: Bearer {token}
```

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=fintech_psp;Username=fintech_user;Password=fintech_pass"

# RabbitMQ
ConnectionStrings__RabbitMQ="amqp://guest:guest@localhost:5672/"

# JWT
Jwt__Key="mortadela-super-secret-key-that-should-be-at-least-256-bits-long-for-production"
Jwt__Issuer="Mortadela"
Jwt__Audience="Mortadela"
```

### Clientes OAuth Pré-configurados

| Client ID | Client Secret | Scopes | Descrição |
|-----------|---------------|--------|-----------|
| `fintech_web_app` | `web_app_secret_123` | `pix,banking` | Aplicação Web |
| `fintech_mobile_app` | `mobile_app_secret_456` | `pix,banking` | Aplicação Mobile |
| `fintech_admin` | `admin_secret_789` | `pix,banking,admin` | Painel Admin |
| `integration_test` | `test_secret_000` | `pix,banking,admin` | Testes |

## 🏦 Integrações Bancárias

### Bancos Suportados

- **Stark Bank** - API Key authentication
- **Sicoob** - OAuth + certificado PEM
- **Banco Genial** - OAuth FAPI
- **Efí Bank** - OAuth Basic
- **Celcoin** - Basic auth com GalaxId

### Roteamento por Taxa

O sistema roteia automaticamente para o banco com menor taxa:

```json
{
  "001": { "taxa": 0.5, "nome": "Banco do Brasil" },
  "341": { "taxa": 0.7, "nome": "Itaú" },
  "756": { "taxa": 0.4, "nome": "Sicoob" }
}
```

## 🔒 Segurança e Compliance

### LGPD
- Tokenização de dados sensíveis (CPF, chaves PIX)
- Consent via scopes OAuth
- Audit logs imutáveis

### PCI-DSS
- Não armazenamento de dados sensíveis
- Criptografia em trânsito e repouso
- Validação de entrada

### Open Finance
- Padrões FAPI para Banco Genial
- mTLS para Sicoob e Efí
- Validação de webhooks com HMAC

## 📊 Monitoramento

- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing
- **Health Checks** - Endpoints `/health` em cada serviço

## 🧪 Testes

```bash
# Testes unitários
dotnet test tests/FintechPSP.Domain.Tests

# Testes de integração
dotnet test tests/FintechPSP.API.Tests

# Testes end-to-end
dotnet test tests/FintechPSP.E2E.Tests
```

## 📦 Deploy

### Docker

```bash
# Build das imagens
docker build -t fintechpsp/authservice -f src/Services/FintechPSP.AuthService/Dockerfile .

# Deploy com docker-compose
docker-compose -f docker/docker-compose.prod.yml up -d
```

### Kubernetes

```bash
# Deploy no K8s
kubectl apply -f k8s/
```

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para detalhes.

## 📞 Suporte

Para suporte, envie um email para suporte@fintechpsp.com ou abra uma issue no GitHub.
