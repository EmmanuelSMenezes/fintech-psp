# 📚 **DOCUMENTAÇÃO FINTECHPSP**

Esta pasta contém a documentação completa do modelo de dados de todos os microserviços do sistema FintechPSP.

## 📁 **Estrutura da Documentação**

### **🏗️ Arquitetura Geral**
- `ARQUITETURA-GERAL.md` - Visão geral da arquitetura do sistema

### **🔐 Serviços de Autenticação**
- `AuthService-ModeloDados.md` - Modelo de dados do AuthService
- `UserService-ModeloDados.md` - Modelo de dados do UserService

### **💰 Serviços de Transações**
- `TransactionService-ModeloDados.md` - Modelo de dados do TransactionService
- `BalanceService-ModeloDados.md` - Modelo de dados do BalanceService

### **🏢 Serviços de Gestão**
- `CompanyService-ModeloDados.md` - Modelo de dados do CompanyService
- `ConfigService-ModeloDados.md` - Modelo de dados do ConfigService

### **🔗 Serviços de Integração**
- `IntegrationService-ModeloDados.md` - Modelo de dados do IntegrationService
- `WebhookService-ModeloDados.md` - Modelo de dados do WebhookService

### **🧪 Qualidade e Testes**
- `PLANO-TESTES-QA-FUNCIONAL.md` - Plano completo de testes QA
- `CASOS-TESTE-DETALHADOS.md` - Casos de teste detalhados por módulo
- `AMBIENTE-TESTE-SETUP.md` - Setup completo do ambiente de testes
- `AUTOMACAO-TESTES.md` - Estratégia e implementação de automação

### **📮 Testes com Postman**
- `TESTES-POSTMAN-PASSO-A-PASSO.md` - Guia completo de testes com Postman
- `POSTMAN-SCRIPTS-AVANCADOS.md` - Scripts de automação e validação avançada
- `POSTMAN-GUIA-USO.md` - Guia de uso das collections Postman

## 🎯 **Como Usar Esta Documentação**

### **Para Desenvolvedores**
1. **Novos no projeto**: Comece com `ARQUITETURA-GERAL.md`
2. **Implementando features**: Consulte o modelo de dados do serviço específico
3. **Integrações**: Veja `FLUXO-DADOS.md` e os eventos compartilhados

### **Para Arquitetos**
1. **Visão geral**: `ARQUITETURA-GERAL.md`
2. **Padrões**: Cada serviço segue CQRS, Event Sourcing e DDD
3. **Comunicação**: Event-driven via RabbitMQ

### **Para DBAs**
1. **Schemas**: Cada arquivo contém o schema completo do banco
2. **Relacionamentos**: Documentados em cada modelo
3. **Índices**: Especificados para performance

## 🔧 **Tecnologias Documentadas**

- **.NET 8** - Microserviços
- **PostgreSQL** - Banco de dados principal
- **RabbitMQ** - Message broker
- **Redis** - Cache
- **Dapper** - ORM para acesso a dados
- **MediatR** - CQRS pattern
- **MassTransit** - Event-driven communication
- **Marten** - Event Store

## 📊 **Padrões Arquiteturais**

### **Domain-Driven Design (DDD)**
- Aggregate Roots
- Value Objects
- Domain Events
- Repositories

### **CQRS (Command Query Responsibility Segregation)**
- Commands para escrita
- Queries para leitura
- Handlers separados

### **Event Sourcing**
- Event Store com Marten
- Projeções para read models
- Replay de eventos

### **Microserviços**
- Bounded contexts bem definidos
- Comunicação assíncrona
- Independência de deploy

## 🚀 **Status da Documentação**

### **📊 Modelo de Dados**
- ✅ **ARQUITETURA-GERAL** - Completo
- ✅ **AuthService** - Completo
- ✅ **UserService** - Completo
- ✅ **TransactionService** - Completo
- ✅ **BalanceService** - Completo
- ✅ **CompanyService** - Completo
- ✅ **ConfigService** - Completo
- ✅ **IntegrationService** - Completo
- ✅ **WebhookService** - Completo

### **🧪 Testes QA**
- ✅ **PLANO-TESTES-QA** - Completo
- ✅ **CASOS-TESTE-DETALHADOS** - Completo
- ✅ **AMBIENTE-TESTE-SETUP** - Completo
- ✅ **AUTOMACAO-TESTES** - Completo

### **📮 Testes Postman**
- ✅ **TESTES-POSTMAN-PASSO-A-PASSO** - Completo
- ✅ **POSTMAN-SCRIPTS-AVANCADOS** - Completo
- ✅ **POSTMAN-GUIA-USO** - Completo

## 📝 **Convenções**

### **Nomenclatura**
- **Entidades**: PascalCase (ex: `SystemUser`)
- **Propriedades**: PascalCase (ex: `UserId`)
- **Tabelas**: snake_case (ex: `system_users`)
- **Colunas**: snake_case (ex: `user_id`)

### **Identificadores**
- **Primary Keys**: UUID (Guid)
- **Foreign Keys**: Sufixo `_id`
- **Timestamps**: `created_at`, `updated_at`

### **Status e Enums**
- **Strings**: Para flexibilidade
- **Check constraints**: Para validação
- **Valores padrão**: Sempre definidos

## 🔄 **Versionamento**

Esta documentação é atualizada automaticamente conforme o código evolui.

**Última atualização**: 2025-10-08
**Versão do sistema**: 1.0.0
**Status**: Produção

---

**📧 Contato**: Para dúvidas sobre a documentação, consulte a equipe de arquitetura.
