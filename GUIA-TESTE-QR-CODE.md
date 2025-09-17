# 🚀 Guia Completo para Testar QR Code PIX

## 📋 **Pré-requisitos**

1. **Docker Desktop** rodando
2. **Postman** instalado
3. **PowerShell** (Windows)

---

## 🔧 **Passo 1: Iniciar o Sistema**

### **🐳 Opção A: Docker Completo (Recomendado)**
```powershell
# No diretório do projeto - Sistema completo em containers
.\start-docker-complete.ps1
```

### **🖥️ Opção B: Desenvolvimento Local**
```powershell
# 1. Subir apenas infraestrutura
docker-compose -f docker-compose-complete.yml up -d postgres rabbitmq redis

# 2. Aguardar 30 segundos
Start-Sleep -Seconds 30

# 3. Inicializar banco
Get-Content init-database.sql | docker exec -i fintech-postgres-new psql -U postgres

# 4. Iniciar serviços localmente (em terminais separados)
dotnet run --project src/Gateway/FintechPSP.APIGateway --urls "http://localhost:5000"
dotnet run --project src/Services/FintechPSP.AuthService --urls "http://localhost:5001"
dotnet run --project src/Services/FintechPSP.TransactionService --urls "http://localhost:5002"
dotnet run --project src/Services/FintechPSP.BalanceService --urls "http://localhost:5003"
dotnet run --project src/Services/FintechPSP.IntegrationService --urls "http://localhost:5005"
```

### **⚡ Opção C: Teste Rápido (Sem MassTransit)**
```powershell
# Apenas serviços essenciais para QR Code
dotnet run --project src/Services/FintechPSP.AuthService --urls "http://localhost:5001"
dotnet run --project src/Services/FintechPSP.TransactionService --urls "http://localhost:5002"
dotnet run --project src/Services/FintechPSP.IntegrationService --urls "http://localhost:5005"
dotnet run --project src/Gateway/FintechPSP.APIGateway --urls "http://localhost:5000"
```

---

## 🤔 **Qual Opção Escolher?**

### **🐳 Docker Completo - Recomendado para:**
- ✅ **Teste completo** do sistema
- ✅ **Demonstração** para clientes
- ✅ **Ambiente isolado** (não interfere com outros projetos)
- ✅ **Event Sourcing** completo funcionando
- ✅ **Todos os 8 microservices** rodando
- ✅ **Infraestrutura completa** (PostgreSQL, RabbitMQ, Redis)

### **🖥️ Desenvolvimento Local - Recomendado para:**
- ✅ **Desenvolvimento ativo** (hot reload)
- ✅ **Debug** e troubleshooting
- ✅ **Mudanças frequentes** no código
- ✅ **Menor uso de recursos** do sistema
- ✅ **Flexibilidade** para testar partes específicas

### **⚡ Teste Rápido - Recomendado para:**
- ✅ **Validação rápida** de QR Code
- ✅ **Problemas com MassTransit**
- ✅ **Recursos limitados** do sistema
- ✅ **Foco apenas** em funcionalidade QR Code

---

## 📮 **Passo 2: Configurar Postman**

### **2.1 Importar Collection**
1. Abrir Postman
2. **Import** → **File**
3. Selecionar: `postman/FintechPSP-Collection.json`

### **2.2 Configurar Variáveis**
- **🐳 Docker**: `base_url` = `http://localhost:5000`
- **🖥️ Local**: `base_url` = `http://localhost:5000`
- **access_token**: (será preenchido automaticamente)

### **2.3 Verificar Conectividade**
```powershell
# Testar se API Gateway está respondendo
curl http://localhost:5000/health

# Verificar serviços individuais (Docker)
curl http://localhost:5001/health  # Auth
curl http://localhost:5002/health  # Transaction
curl http://localhost:5005/health  # Integration
```

---

## 🔐 **Passo 3: Obter Token de Autenticação**

### **3.1 Executar Request**
- **Pasta**: `AuthService`
- **Request**: `Obter Token OAuth 2.0`
- **Método**: `POST`
- **URL**: `{{base_url}}/auth/token`

### **3.2 Body (já configurado)**
```json
{
  "grant_type": "client_credentials",
  "client_id": "fintech_psp_client",
  "client_secret": "fintech_psp_secret_2024",
  "scope": "transactions balance webhooks banking"
}
```

### **3.3 Verificar Response**
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "transactions balance webhooks banking"
}
```

✅ **Token será salvo automaticamente na variável `access_token`**

---

## 🎯 **Passo 4: Testar QR Code Estático**

### **4.1 Executar Request**
- **Pasta**: `QR Code PIX`
- **Request**: `Gerar QR Code Estático`
- **Método**: `POST`
- **URL**: `{{base_url}}/transacoes/pix/qrcode/estatico`

### **4.2 Body (já configurado)**
```json
{
  "externalId": "QR-STATIC-{{$randomUUID}}",
  "pixKey": "11999887766",
  "bankCode": "001",
  "description": "QR Code estático para recebimentos"
}
```

### **4.3 Response Esperado**
```json
{
  "transactionId": "123e4567-e89b-12d3-a456-426614174000",
  "qrcodePayload": "00020126580014br.gov.bcb.pix0136123e4567...",
  "qrcodeImage": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJ...",
  "type": "static",
  "expiresAt": null,
  "pixKey": "11999887766",
  "amount": null,
  "description": "QR Code estático para recebimentos",
  "createdAt": "2025-09-17T13:30:00Z"
}
```

---

## 💰 **Passo 5: Testar QR Code Dinâmico**

### **5.1 Executar Request**
- **Pasta**: `QR Code PIX`
- **Request**: `Gerar QR Code Dinâmico`
- **Método**: `POST`
- **URL**: `{{base_url}}/transacoes/pix/qrcode/dinamico`

### **5.2 Body (já configurado)**
```json
{
  "externalId": "QR-DYNAMIC-{{$randomUUID}}",
  "amount": 150.00,
  "pixKey": "usuario@email.com",
  "bankCode": "237",
  "description": "Pagamento de produto XYZ",
  "expiresIn": 300
}
```

### **5.3 Response Esperado**
```json
{
  "transactionId": "456e7890-e89b-12d3-a456-426614174001",
  "qrcodePayload": "00020126580014br.gov.bcb.pix0136456e7890...",
  "qrcodeImage": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJ...",
  "type": "dynamic",
  "expiresAt": "2025-09-17T13:35:00Z",
  "pixKey": "usuario@email.com",
  "amount": 150.00,
  "description": "Pagamento de produto XYZ",
  "createdAt": "2025-09-17T13:30:00Z"
}
```

---

## 🏦 **Passo 6: Testar Internet Banking**

### **6.1 QR Code Banking**
- **Request**: `Gerar QR Code Estático (Banking)`
- **URL**: `{{base_url}}/banking/transacoes/pix/qrcode/estatico`
- **Scope**: Requer `banking` no token

### **6.2 Body**
```json
{
  "externalId": "QR-BANKING-{{$randomUUID}}",
  "pixKey": "12345678901",
  "bankCode": "341",
  "description": "QR Code para internet banking"
}
```

---

## 🔍 **Passo 7: Verificar Health Checks**

### **7.1 QR Code Service**
- **Request**: `Health Check QR Code`
- **URL**: `{{base_url}}/qrcode/health`
- **Método**: `GET` (sem autenticação)

### **7.2 Response Esperado**
```json
{
  "status": "healthy",
  "service": "QrCodeService",
  "timestamp": "2025-09-17T13:30:00Z",
  "features": ["static_qr", "dynamic_qr", "emv_payload", "base64_image"]
}
```

---

## 🧪 **Passo 8: Testes Avançados**

### **8.1 Testar Idempotência**
1. Execute o mesmo QR Code estático 2x com mesmo `externalId`
2. Deve retornar o mesmo `transactionId` e payload

### **8.2 Testar Validações**
```json
// PIX Key inválida
{
  "externalId": "TEST-001",
  "pixKey": "invalid",
  "bankCode": "001"
}
// Esperado: 400 Bad Request
```

### **8.3 Testar Integrações Bancárias**
- **Stark Bank**: `POST {{base_url}}/integrations/stark-bank/brcodes`
- **Sicoob**: `POST {{base_url}}/integrations/sicoob/cobranca/v3/qrcode`
- **Banco Genial**: `POST {{base_url}}/integrations/banco-genial/openfinance/pix/qrcode`

---

## 📊 **Passo 9: Verificar Logs e Eventos**

### **🐳 Docker - Logs dos Serviços**
```powershell
# Ver logs em tempo real
docker-compose -f docker-compose-complete.yml logs -f transaction-service

# Ver logs específicos
docker-compose -f docker-compose-complete.yml logs transaction-service | findstr "QR Code"

# Ver logs de todos os serviços
docker-compose -f docker-compose-complete.yml logs --tail=50
```

### **🖥️ Local - Logs dos Serviços**
Verificar nos terminais dos serviços:
```
[13:30:00] Gerando QR Code estático para externalId QR-STATIC-001
[13:30:01] QR Code estático gerado com sucesso para transactionId 123e4567...
[13:30:02] Evento QrCodeGerado publicado via MassTransit
```

### **9.2 RabbitMQ Management**
- **🐳 Docker**: http://localhost:15673 (guest/guest)
- **🖥️ Local**: http://localhost:15672 (guest/guest)
- **Verificar**: Filas `balance-service-qrcode-events`

### **9.3 PostgreSQL - Verificar Dados**
```sql
-- Docker
docker exec -it fintech-postgres-new psql -U postgres -d fintech_psp

-- Verificar QR Codes gerados
SELECT * FROM v_active_qrcodes ORDER BY created_at DESC LIMIT 10;

-- Verificar estatísticas
SELECT * FROM v_qrcode_stats ORDER BY date DESC LIMIT 5;

-- Verificar transações
SELECT transaction_id, external_id, type, sub_type, amount, pix_key, created_at
FROM transaction_history
WHERE type = 'QR_CODE_GENERATED'
ORDER BY created_at DESC LIMIT 10;
```

---

## ❌ **Troubleshooting**

### **🐳 Problemas com Docker**

#### **Erro: "Port already in use"**
```powershell
# Verificar containers em conflito
docker ps | findstr "5000\|5001\|5002"

# Parar containers conflitantes
docker-compose -f docker-compose-complete.yml down

# Usar portas alternativas se necessário
# PostgreSQL: 5433, RabbitMQ: 15673, Redis: 6380
```

#### **Erro: "Container not starting"**
```powershell
# Ver logs detalhados
docker-compose -f docker-compose-complete.yml logs transaction-service

# Verificar status
docker-compose -f docker-compose-complete.yml ps

# Reiniciar serviço específico
docker-compose -f docker-compose-complete.yml restart transaction-service
```

#### **Erro: "Database connection"**
```powershell
# Verificar PostgreSQL
docker exec fintech-postgres-new pg_isready -U postgres

# Conectar manualmente
docker exec -it fintech-postgres-new psql -U postgres -d fintech_psp

# Reinicializar banco se necessário
Get-Content init-database.sql | docker exec -i fintech-postgres-new psql -U postgres
```

### **🖥️ Problemas Locais**

#### **Erro: "Unauthorized"**
- ✅ Verificar se o token foi obtido
- ✅ Verificar se o token está na variável `access_token`
- ✅ Verificar se o header `Authorization: Bearer {{access_token}}` está presente

#### **Erro: "Connection refused"**
```powershell
# Verificar se serviços estão rodando
netstat -an | findstr :5000
netstat -an | findstr :5001
netstat -an | findstr :5002

# Verificar logs nos terminais dos serviços
# Aguardar mais tempo para inicialização (até 2 minutos)
```

#### **Erro: "MassTransit configuration"**
```powershell
# Para testes rápidos, use apenas serviços essenciais:
# Auth (5001) + Transaction (5002) + Integration (5005) + Gateway (5000)
# Ignore erros de MassTransit se não precisar de eventos
```

---

## 🎉 **Sucesso!**

Se todos os testes passaram, você tem:
- ✅ QR Code PIX estático funcionando
- ✅ QR Code PIX dinâmico funcionando
- ✅ Integrações bancárias mockadas (5 bancos)
- ✅ Event Sourcing completo (se usando Docker completo)
- ✅ Projeções no BalanceService (se usando Docker completo)
- ✅ Sistema completo e funcional!

### **🐳 Com Docker**
- ✅ 8 microservices em containers
- ✅ Infraestrutura isolada (PostgreSQL, RabbitMQ, Redis)
- ✅ Health checks e monitoramento
- ✅ Volumes persistentes
- ✅ Logs centralizados

### **🖥️ Com Desenvolvimento Local**
- ✅ Hot reload para desenvolvimento
- ✅ Debug direto no código
- ✅ Flexibilidade para mudanças
- ✅ Menor uso de recursos

**🚀 Sistema PSP com QR Code PIX pronto para produção!**

---

## 📝 **Comandos de Referência Rápida**

### **🐳 Docker**
```powershell
# Subir sistema completo
.\start-docker-complete.ps1

# Ver status
docker-compose -f docker-compose-complete.yml ps

# Ver logs
docker-compose -f docker-compose-complete.yml logs -f transaction-service

# Parar tudo
docker-compose -f docker-compose-complete.yml down
```

### **🖥️ Local**
```powershell
# Serviços essenciais para QR Code
dotnet run --project src/Services/FintechPSP.AuthService --urls "http://localhost:5001"
dotnet run --project src/Services/FintechPSP.TransactionService --urls "http://localhost:5002"
dotnet run --project src/Services/FintechPSP.IntegrationService --urls "http://localhost:5005"
dotnet run --project src/Gateway/FintechPSP.APIGateway --urls "http://localhost:5000"
```

### **📮 Postman**
```json
// Configuração
{
  "base_url": "http://localhost:5000",
  "access_token": "{{access_token}}"
}

// Sequência de teste
1. POST {{base_url}}/auth/token
2. POST {{base_url}}/transacoes/pix/qrcode/estatico
3. POST {{base_url}}/transacoes/pix/qrcode/dinamico
4. GET {{base_url}}/qrcode/health
```
