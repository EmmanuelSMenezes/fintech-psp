# 🐳 **Guia Docker - FintechPSP Completo**

## 🚀 **Início Rápido**

### **1️⃣ Subir Sistema Completo**
```powershell
# Executar script automático
.\start-docker-complete.ps1
```

### **2️⃣ Ou Manual**
```powershell
# Subir todos os serviços
docker-compose -f docker-compose-complete.yml up --build -d

# Aguardar 30 segundos
Start-Sleep -Seconds 30

# Inicializar banco
Get-Content init-database.sql | docker exec -i fintech-postgres-new psql -U postgres
```

---

## 📋 **Serviços e Portas**

| Serviço | Porta | URL | Container |
|---------|-------|-----|-----------|
| **API Gateway** | 5000 | http://localhost:5000 | fintech-api-gateway |
| **Auth Service** | 5001 | http://localhost:5001 | fintech-auth-service |
| **Transaction Service** | 5002 | http://localhost:5002 | fintech-transaction-service |
| **Balance Service** | 5003 | http://localhost:5003 | fintech-balance-service |
| **Webhook Service** | 5004 | http://localhost:5004 | fintech-webhook-service |
| **Integration Service** | 5005 | http://localhost:5005 | fintech-integration-service |
| **User Service** | 5006 | http://localhost:5006 | fintech-user-service |
| **Config Service** | 5007 | http://localhost:5007 | fintech-config-service |

### **Infraestrutura**
| Serviço | Porta | URL | Container |
|---------|-------|-----|-----------|
| **PostgreSQL** | 5433 | localhost:5433 | fintech-postgres-new |
| **RabbitMQ** | 15673 | http://localhost:15673 | fintech-rabbitmq-new |
| **Redis** | 6380 | localhost:6380 | fintech-redis-new |

---

## 🔧 **Comandos Úteis**

### **Gerenciamento**
```powershell
# Ver status de todos os containers
docker-compose -f docker-compose-complete.yml ps

# Ver logs de um serviço específico
docker-compose -f docker-compose-complete.yml logs transaction-service

# Ver logs em tempo real
docker-compose -f docker-compose-complete.yml logs -f transaction-service

# Parar todos os serviços
docker-compose -f docker-compose-complete.yml down

# Parar e remover volumes
docker-compose -f docker-compose-complete.yml down -v

# Reconstruir um serviço específico
docker-compose -f docker-compose-complete.yml up --build -d transaction-service
```

### **Debug**
```powershell
# Entrar no container PostgreSQL
docker exec -it fintech-postgres-new psql -U postgres -d fintech_psp

# Entrar no container de um serviço
docker exec -it fintech-transaction-service bash

# Ver logs de inicialização
docker-compose -f docker-compose-complete.yml logs --tail=50 transaction-service
```

---

## 📮 **Testando com Postman**

### **1️⃣ Configuração**
- **Collection**: `postman/FintechPSP-Collection.json`
- **base_url**: `http://localhost:5000`

### **2️⃣ Sequência de Teste**
1. **Obter Token**: `POST {{base_url}}/auth/token`
2. **QR Estático**: `POST {{base_url}}/transacoes/pix/qrcode/estatico`
3. **QR Dinâmico**: `POST {{base_url}}/transacoes/pix/qrcode/dinamico`
4. **Health Check**: `GET {{base_url}}/qrcode/health`

---

## 🔍 **Monitoramento**

### **Health Checks**
```powershell
# Verificar saúde de todos os serviços
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
curl http://localhost:5004/health
curl http://localhost:5005/health
curl http://localhost:5006/health
curl http://localhost:5007/health
```

### **RabbitMQ Management**
- **URL**: http://localhost:15673
- **Login**: guest / guest
- **Filas**: Verificar filas de eventos

### **PostgreSQL**
```sql
-- Conectar ao banco
docker exec -it fintech-postgres-new psql -U postgres -d fintech_psp

-- Verificar QR Codes gerados
SELECT * FROM v_active_qrcodes;

-- Verificar estatísticas
SELECT * FROM v_qrcode_stats;

-- Verificar transações
SELECT * FROM transaction_history WHERE type = 'QR_CODE_GENERATED';
```

---

## ❌ **Troubleshooting**

### **Problema: Porta já em uso**
```powershell
# Verificar quais portas estão em uso
netstat -an | findstr :5000
netstat -an | findstr :5433

# Parar containers conflitantes
docker stop $(docker ps -q --filter "publish=5000")
```

### **Problema: Serviço não inicia**
```powershell
# Ver logs detalhados
docker-compose -f docker-compose-complete.yml logs transaction-service

# Verificar dependências
docker-compose -f docker-compose-complete.yml ps

# Reiniciar serviço específico
docker-compose -f docker-compose-complete.yml restart transaction-service
```

### **Problema: Banco não conecta**
```powershell
# Verificar se PostgreSQL está rodando
docker exec fintech-postgres-new pg_isready -U postgres

# Verificar logs do PostgreSQL
docker-compose -f docker-compose-complete.yml logs postgres

# Recriar banco
docker-compose -f docker-compose-complete.yml down
docker volume rm fintech_postgres_data
docker-compose -f docker-compose-complete.yml up -d postgres
```

### **Problema: Build falha**
```powershell
# Limpar cache do Docker
docker system prune -a

# Rebuild completo
docker-compose -f docker-compose-complete.yml build --no-cache

# Verificar se .NET SDK está disponível
docker run --rm mcr.microsoft.com/dotnet/sdk:9.0 dotnet --version
```

---

## 🔄 **Desenvolvimento**

### **Hot Reload (Desenvolvimento Local)**
Para desenvolvimento, é melhor rodar localmente:
```powershell
# Subir apenas infraestrutura
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

# Rodar serviços localmente
dotnet run --project src/Services/FintechPSP.TransactionService --urls "http://localhost:5002"
```

### **Rebuild Após Mudanças**
```powershell
# Rebuild serviço específico
docker-compose -f docker-compose-complete.yml build transaction-service
docker-compose -f docker-compose-complete.yml up -d transaction-service

# Rebuild todos
docker-compose -f docker-compose-complete.yml build
docker-compose -f docker-compose-complete.yml up -d
```

---

## 📊 **Recursos do Sistema**

### **Volumes Persistentes**
- `fintech_postgres_data`: Dados do PostgreSQL
- `fintech_rabbitmq_data`: Dados do RabbitMQ  
- `fintech_redis_data`: Dados do Redis

### **Network**
- `fintech-network`: Rede interna para comunicação entre serviços

### **Environment Variables**
Cada serviço tem configurações específicas para:
- Connection strings do PostgreSQL
- Configurações do RabbitMQ
- Configurações do Redis
- URLs dos serviços

---

## 🎉 **Sucesso!**

Se tudo funcionou:
- ✅ 8 microservices rodando
- ✅ Infraestrutura completa
- ✅ QR Code PIX funcionando
- ✅ Event Sourcing ativo
- ✅ APIs documentadas no Swagger

**🚀 Sistema PSP completo rodando em Docker!**

---

## 📝 **Próximos Passos**

1. **Configurar CI/CD**: Pipeline para deploy automático
2. **Monitoring**: Prometheus + Grafana
3. **Logging**: ELK Stack ou similar
4. **Security**: HTTPS, secrets management
5. **Scaling**: Kubernetes para produção

**Sistema pronto para produção! 🎯**
