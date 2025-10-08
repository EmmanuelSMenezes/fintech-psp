# 🏗️ **ARQUITETURA GERAL - FINTECHPSP**

## 📋 **Visão Geral**

O FintechPSP é uma plataforma de Payment Service Provider (PSP) construída com arquitetura de microserviços, implementando padrões modernos como DDD, CQRS, Event Sourcing e Event-Driven Architecture.

---

## 🎯 **Objetivos Arquiteturais**

### **Escalabilidade**
- Microserviços independentes
- Scaling horizontal por serviço
- Load balancing automático
- Cache distribuído

### **Resiliência**
- Circuit breakers
- Retry policies com backoff
- Health checks
- Graceful degradation

### **Observabilidade**
- Logs estruturados
- Métricas de performance
- Distributed tracing
- Alertas proativos

### **Segurança**
- OAuth 2.0 + JWT
- mTLS para integrações
- HMAC para webhooks
- Criptografia de dados sensíveis

---

## 🏢 **Arquitetura de Microserviços**

### **API Gateway (Ocelot)**
- **Porta**: 5000
- **Função**: Roteamento, autenticação, rate limiting
- **Tecnologia**: ASP.NET Core + Ocelot

### **Serviços de Negócio**

#### **AuthService** (🔐)
- **Porta**: 5001
- **Responsabilidade**: Autenticação e autorização
- **Padrões**: JWT, OAuth 2.0, BCrypt
- **Banco**: PostgreSQL (porta 5433)

#### **TransactionService** (💰)
- **Porta**: 5002
- **Responsabilidade**: Processamento de transações
- **Padrões**: DDD, Event Sourcing, CQRS
- **Banco**: PostgreSQL (porta 5434)

#### **BalanceService** (💳)
- **Porta**: 5003
- **Responsabilidade**: Gestão de saldos
- **Padrões**: Event Sourcing, CQRS, Marten
- **Banco**: PostgreSQL (porta 5435)

#### **IntegrationService** (🔌)
- **Porta**: 5004
- **Responsabilidade**: Integrações bancárias
- **Padrões**: Circuit Breaker, Retry, Cache
- **Banco**: PostgreSQL (porta 5436)

#### **UserService** (👥)
- **Porta**: 5006
- **Responsabilidade**: Gestão de usuários e RBAC
- **Padrões**: DDD, Event Sourcing
- **Banco**: PostgreSQL (porta 5438)

#### **ConfigService** (⚙️)
- **Porta**: 5007
- **Responsabilidade**: Configurações do sistema
- **Padrões**: Repository, Configuration as Code
- **Banco**: PostgreSQL (porta 5439)

#### **WebhookService** (🔗)
- **Porta**: 5008
- **Responsabilidade**: Webhooks e notificações
- **Padrões**: Event-Driven, Retry, HMAC
- **Banco**: PostgreSQL (porta 5440)

#### **CompanyService** (🏢)
- **Porta**: 5009
- **Responsabilidade**: Gestão de empresas
- **Padrões**: DDD, Aggregate Root
- **Banco**: PostgreSQL (porta 5441)

---

## 🌐 **Frontends**

### **BackofficeWeb** (Admin)
- **Porta**: 3000
- **Tecnologia**: Next.js + React + TypeScript
- **Função**: Administração do sistema
- **Usuários**: Admins, operadores

### **InternetBankingWeb** (Cliente)
- **Porta**: 3001
- **Tecnologia**: Next.js + React + TypeScript
- **Função**: Portal do cliente
- **Usuários**: Clientes finais

---

## 🗄️ **Infraestrutura de Dados**

### **PostgreSQL Cluster**
- **Versão**: 15
- **Configuração**: Multi-database
- **Backup**: Automático
- **Replicação**: Master-Slave

### **Redis Cache**
- **Porta**: 6379
- **Função**: Cache distribuído, sessões
- **Configuração**: Cluster mode

### **RabbitMQ**
- **Porta**: 5672 (AMQP), 15672 (Management)
- **Função**: Message broker, event bus
- **Configuração**: Cluster com HA

---

## 🔄 **Padrões Arquiteturais**

### **Domain-Driven Design (DDD)**
```
📁 Domain/
├── 📁 Entities/          # Entidades de negócio
├── 📁 ValueObjects/      # Objetos de valor
├── 📁 Aggregates/        # Agregados
├── 📁 Events/            # Eventos de domínio
├── 📁 Services/          # Serviços de domínio
└── 📁 Repositories/      # Interfaces de repositório
```

### **CQRS (Command Query Responsibility Segregation)**
```
📁 Application/
├── 📁 Commands/          # Comandos (escrita)
│   ├── 📄 CreateCommand.cs
│   └── 📄 CreateCommandHandler.cs
├── 📁 Queries/           # Consultas (leitura)
│   ├── 📄 GetQuery.cs
│   └── 📄 GetQueryHandler.cs
└── 📁 DTOs/              # Data Transfer Objects
```

### **Event Sourcing**
```
📁 Events/
├── 📄 DomainEvent.cs     # Evento base
├── 📄 EntityCreated.cs   # Evento específico
└── 📄 EntityUpdated.cs   # Evento específico

📁 EventStore/
├── 📄 EventStore.cs      # Store de eventos
└── 📄 Projections.cs     # Projeções
```

---

## 🔗 **Comunicação Entre Serviços**

### **Síncrona (HTTP/REST)**
```
Client → API Gateway → Service
```
- Consultas em tempo real
- Operações críticas
- Validações imediatas

### **Assíncrona (Events/RabbitMQ)**
```
Service A → RabbitMQ → Service B
```
- Notificações
- Processamento em background
- Integração entre bounded contexts

### **Event-Driven Architecture**
```mermaid
graph LR
    A[TransactionService] -->|TransactionCreated| B[RabbitMQ]
    B -->|Event| C[BalanceService]
    B -->|Event| D[WebhookService]
    B -->|Event| E[NotificationService]
```

---

## 🔐 **Segurança**

### **Autenticação**
- **JWT Tokens**: HS256, 1h expiry
- **OAuth 2.0**: Client credentials flow
- **BCrypt**: Password hashing (work factor 10)

### **Autorização**
- **RBAC**: Role-based access control
- **Claims**: Granular permissions
- **Scopes**: OAuth 2.0 scopes

### **Comunicação**
- **HTTPS**: TLS 1.3
- **mTLS**: Mutual TLS para integrações
- **HMAC**: Webhook signatures

---

## 📊 **Monitoramento e Observabilidade**

### **Health Checks**
```
GET /health → Service status
GET /health/ready → Readiness probe
GET /health/live → Liveness probe
```

### **Logs**
- **Formato**: JSON estruturado
- **Níveis**: Debug, Info, Warning, Error, Critical
- **Correlação**: Correlation IDs

### **Métricas**
- **Performance**: Response time, throughput
- **Business**: Transaction volume, success rate
- **Infrastructure**: CPU, memory, disk

---

## 🚀 **Deploy e DevOps**

### **Containerização**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "Service.dll"]
```

### **Orquestração**
- **Docker Compose**: Desenvolvimento local
- **Kubernetes**: Produção (futuro)
- **Health Checks**: Automáticos
- **Auto Scaling**: Baseado em métricas

---

## 🔄 **Fluxos Principais**

### **1. Criação de Transação PIX**
```
1. Client → API Gateway → TransactionService
2. TransactionService → IntegrationService (Sicoob)
3. IntegrationService → Sicoob API
4. TransactionCreated Event → RabbitMQ
5. BalanceService ← RabbitMQ (atualiza saldo)
6. WebhookService ← RabbitMQ (notifica cliente)
```

### **2. Autenticação de Usuário**
```
1. Client → API Gateway → AuthService
2. AuthService → PostgreSQL (validação)
3. AuthService → JWT Token generation
4. Client ← JWT Token
5. Subsequent requests use JWT
```

### **3. Webhook Delivery**
```
1. Event → WebhookService
2. WebhookService → Client URL (HTTP POST)
3. Retry logic (exponential backoff)
4. Delivery status tracking
```

---

## 📈 **Escalabilidade**

### **Horizontal Scaling**
- **Stateless Services**: Fácil replicação
- **Load Balancer**: Distribuição de carga
- **Database Sharding**: Por tenant/cliente

### **Vertical Scaling**
- **Resource Limits**: CPU/Memory por container
- **Auto Scaling**: Baseado em métricas
- **Performance Tuning**: Query optimization

---

## 🔧 **Configuração**

### **Environment Variables**
```bash
# Database
DATABASE_URL=postgresql://user:pass@host:port/db

# RabbitMQ
RABBITMQ_URL=amqp://user:pass@host:port/

# Redis
REDIS_URL=redis://host:port

# JWT
JWT_SECRET=your-secret-key
JWT_ISSUER=Mortadela
JWT_AUDIENCE=Mortadela
```

### **Configuration Files**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "Redis": "...",
    "RabbitMQ": "..."
  },
  "Jwt": {
    "Key": "...",
    "Issuer": "Mortadela",
    "Audience": "Mortadela"
  }
}
```

---

## 🎯 **Próximos Passos**

### **Melhorias Planejadas**
- [ ] Kubernetes deployment
- [ ] Distributed tracing (Jaeger)
- [ ] API versioning
- [ ] GraphQL gateway
- [ ] Event replay capability

### **Integrações Futuras**
- [ ] Stark Bank
- [ ] Banco Genial
- [ ] Efí (Gerencianet)
- [ ] Celcoin

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**👥 Arquitetos**: Equipe FintechPSP
