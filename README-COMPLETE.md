# 🏦 FintechPSP - Payment Service Provider Completo

## 📋 Visão Geral

Sistema PSP (Payment Service Provider) completo desenvolvido em .NET 9 com arquitetura de microservices event-driven, incluindo frontends React para backoffice administrativo e internet banking.

## 🏗️ Arquitetura

### Backend (.NET 9)
- **8 Microservices** com CQRS + Event Sourcing
- **API Gateway** com Ocelot para roteamento
- **PostgreSQL** com Dapper ORM para persistência
- **RabbitMQ + MassTransit** para messaging
- **Marten** para Event Sourcing
- **Redis** para cache
- **OAuth 2.0** com JWT Bearer tokens

### Frontend (React + Next.js)
- **BackofficeWeb**: Painel administrativo para gestão completa
- **InternetBankingWeb**: Portal do cliente para transações e gestão

### Integrações Bancárias
- **Stark Bank** (PIX, TED, Boleto)
- **Sicoob** (PIX, TED)
- **Banco Genial** (PIX, TED)
- **Efí** (PIX, QR Code)
- **Celcoin** (PIX, Boleto)

## 🚀 Funcionalidades Principais

### 💼 Backoffice (Admin/Sub-Admin)
- **Dashboard** com métricas em tempo real
- **Gestão de Clientes** (CRUD completo)
- **Gestão de Contas Bancárias** (múltiplas contas por cliente)
- **Configuração de Priorização** (roteamento ponderado por percentuais)
- **Histórico de Transações** (filtros avançados)
- **Relatórios Financeiros** (extratos, demonstrativos)
- **Gestão de Acessos** (RBAC com permissões granulares)

### 🏦 Internet Banking (Cliente/Sub-Cliente)
- **Dashboard Pessoal** com resumo financeiro
- **Gerenciamento de Contas** (próprias contas bancárias)
- **Transações PIX/TED** (criação e acompanhamento)
- **Histórico Completo** (todas as transações)
- **Configuração de Priorização** (percentuais de roteamento)
- **Gestão de Sub-usuários** (criação e permissões)

### 🔐 Sistema RBAC (Role-Based Access Control)
- **Roles**: Admin, Sub-Admin, Cliente, Sub-Cliente
- **Permissões Granulares**: 15+ permissões específicas
- **Hierarquia de Usuários**: Sub-usuários com permissões limitadas
- **Validação de Acesso**: Middleware de autorização

## 🛠️ Tecnologias Utilizadas

### Backend
```
- .NET 9
- ASP.NET Core Web API
- PostgreSQL 15
- Dapper ORM
- RabbitMQ + MassTransit
- Marten (Event Sourcing)
- Redis
- Ocelot (API Gateway)
- MediatR (CQRS)
- FluentValidation
- Serilog
- Docker & Docker Compose
```

### Frontend
```
- React 18
- Next.js 15
- TypeScript
- Tailwind CSS
- Axios
- React Hook Form + Yup
- React Hot Toast
- Recharts
- Jest + Testing Library
```

## 📦 Estrutura do Projeto

```
fintech/
├── src/
│   ├── Gateway/
│   │   └── FintechPSP.APIGateway/          # API Gateway com Ocelot
│   ├── Services/
│   │   ├── FintechPSP.AuthService/         # Autenticação OAuth 2.0
│   │   ├── FintechPSP.UserService/         # Gestão de usuários e RBAC
│   │   ├── FintechPSP.TransactionService/  # Transações PIX/TED/Boleto
│   │   ├── FintechPSP.BalanceService/      # Saldos e extratos
│   │   ├── FintechPSP.WebhookService/      # Webhooks bancários
│   │   ├── FintechPSP.IntegrationService/  # Integrações bancárias
│   │   └── FintechPSP.ConfigService/       # Configurações e priorização
│   └── Shared/
│       └── FintechPSP.Shared.Domain/       # Eventos e DTOs compartilhados
├── frontends/
│   ├── BackofficeWeb/                      # React Admin Panel
│   └── InternetBankingWeb/                 # React Client Portal
├── database/                               # Scripts SQL
├── postman/                               # Coleção Postman
├── scripts/                               # Scripts de automação
└── docker-compose-complete.yml           # Orquestração completa
```

## 🚀 Como Executar

### 1. Pré-requisitos
```bash
- Docker & Docker Compose
- .NET 9 SDK
- Node.js 18+
- PostgreSQL 15
- RabbitMQ
- Redis
```

### 2. Executar com Docker (Recomendado)
```bash
# Clonar o repositório
git clone <repository-url>
cd fintech

# Executar todos os serviços
docker-compose -f docker-compose-complete.yml up -d

# Aguardar inicialização (2-3 minutos)
# Verificar logs
docker-compose -f docker-compose-complete.yml logs -f
```

### 3. Acessar as Aplicações

#### 🌐 URLs dos Serviços
- **API Gateway**: http://localhost:5000
- **BackofficeWeb**: http://localhost:3000
- **InternetBankingWeb**: http://localhost:3001
- **PostgreSQL**: localhost:5432
- **RabbitMQ Management**: http://localhost:15672
- **Redis**: localhost:6379

#### 🔑 Credenciais de Teste

**Backoffice (Admin)**
```
Client ID: admin_backoffice
Client Secret: admin_secret_000
Scope: admin
```

**Backoffice (Sub-Admin)**
```
Client ID: sub_admin_backoffice
Client Secret: sub_admin_secret_000
Scope: sub-admin
```

**Internet Banking (Cliente)**
```
Client ID: cliente_banking
Client Secret: cliente_secret_000
Scope: banking
```

**Internet Banking (Sub-Cliente)**
```
Client ID: sub_cliente_banking
Client Secret: sub_cliente_secret_000
Scope: sub-banking
```

## 🧪 Executar Testes

### Backend (.NET)
```bash
# Executar todos os testes
dotnet test

# Testes específicos por serviço
dotnet test src/Services/FintechPSP.UserService.Tests/
dotnet test src/Services/FintechPSP.ConfigService.Tests/
dotnet test src/Services/FintechPSP.IntegrationService.Tests/
```

### Frontend (React)
```bash
# Windows
.\scripts\run-frontend-tests.ps1

# Linux/macOS
./scripts/run-frontend-tests.sh

# Ou manualmente
cd frontends/BackofficeWeb && npm test
cd frontends/InternetBankingWeb && npm test
```

## 📊 Monitoramento

### Logs
```bash
# Logs de todos os serviços
docker-compose -f docker-compose-complete.yml logs -f

# Logs de um serviço específico
docker-compose -f docker-compose-complete.yml logs -f user-service
```

### Health Checks
```bash
# Verificar saúde dos serviços
curl http://localhost:5000/health
curl http://localhost:3000/api/health
curl http://localhost:3001/api/health
```

## 🔧 Configuração

### Variáveis de Ambiente
```env
# Database
ConnectionStrings__DefaultConnection=Host=postgres:5432;Database=fintech_psp;Username=postgres;Password=postgres

# RabbitMQ
ConnectionStrings__RabbitMQ=amqp://guest:guest@rabbitmq:5672/

# Redis
Redis__ConnectionString=redis:6379

# Frontend
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
```

## 📈 Métricas e KPIs

### Dashboard Backoffice
- Total de clientes cadastrados
- Total de contas bancárias
- Volume de transações (diário/mensal)
- Status das integrações bancárias
- Distribuição por banco (percentuais)

### Dashboard Internet Banking
- Saldo total disponível
- Transações do dia
- Histórico de movimentações
- Status das contas bancárias
- Sub-usuários ativos

## 🔒 Segurança

### Autenticação
- **OAuth 2.0** Client Credentials Flow
- **JWT Bearer Tokens** com expiração
- **Scopes** para separação de contextos (admin/banking)

### Autorização
- **RBAC** com 4 roles principais
- **15+ permissões** granulares
- **Hierarquia** de usuários (parent/child)
- **Middleware** de validação em todas as rotas

### Dados Sensíveis
- **Credenciais tokenizadas** com AES-256
- **Senhas hasheadas** com BCrypt
- **Logs sanitizados** (sem dados sensíveis)

## 🚀 Deploy em Produção

### Recomendações
1. **Kubernetes** para orquestração
2. **Azure Key Vault** para secrets
3. **Application Insights** para monitoramento
4. **Azure SQL Database** para produção
5. **Azure Service Bus** para messaging
6. **CDN** para assets estáticos
7. **Load Balancer** para alta disponibilidade

## 📞 Suporte

Para dúvidas ou problemas:
1. Verificar logs dos containers
2. Consultar documentação da API (Postman)
3. Executar health checks
4. Verificar conectividade de rede

---

**FintechPSP v1.0.0** - Sistema PSP completo com microservices e frontends React 🚀
