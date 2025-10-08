# 🎯 **SEQUÊNCIA DE TESTES DE NEGÓCIO - FINTECHPSP**

## 📋 **Fluxo Completo Sequenciado para Testes**

Esta é a sequência **exata** que você deve seguir para testar o sistema FintechPSP de forma lógica e sequenciada, simulando o fluxo real de negócio.

---

## 🚀 **FASE 1: PREPARAÇÃO DO AMBIENTE**

### **1️⃣ Verificar Infraestrutura**
```bash
# Verificar se todos os serviços estão rodando
docker ps

# Deve mostrar:
✅ fintech-postgres (porta 5432)
✅ fintech-redis (porta 6379) 
✅ fintech-rabbitmq (porta 5672)
✅ fintech-auth-service (porta 5001)
✅ fintech-user-service (porta 5006)
✅ fintech-transaction-service (porta 5002)
✅ fintech-company-service (porta 5009)
✅ fintech-integration-service (porta 5004)
✅ fintech-api-gateway (porta 5000)
✅ fintech-backoffice-web (porta 3000)
✅ fintech-internetbanking-web (porta 3001)
```

### **2️⃣ Health Check Geral**
```bash
# Testar se API Gateway está respondendo
curl http://localhost:5000/health

# Resultado esperado: {"status": "healthy"}
```

---

## 🏢 **FASE 2: ADMINISTRAÇÃO (BACKOFFICE)**

### **3️⃣ Login do Administrador**
**Endpoint:** `POST /auth/login`
```json
{
  "email": "admin@fintechpsp.com",
  "password": "admin123"
}
```
**✅ Resultado esperado:** Token JWT válido, role "Admin"

### **4️⃣ Criar Empresa (Onboarding)**
**Endpoint:** `POST /companies`
```json
{
  "razaoSocial": "Empresa Teste Ltda",
  "nomeFantasia": "Teste Corp",
  "cnpj": "12.345.678/0001-99",
  "inscricaoEstadual": "123456789",
  "address": {
    "cep": "01234-567",
    "logradouro": "Rua Teste",
    "numero": "123",
    "bairro": "Centro",
    "cidade": "São Paulo",
    "estado": "SP",
    "pais": "Brasil"
  },
  "telefone": "(11) 99999-9999",
  "email": "empresa@teste.com",
  "contractData": {
    "capitalSocial": 100000.00,
    "dataConstituicao": "2024-01-01",
    "naturezaJuridica": "Sociedade Limitada",
    "atividadePrincipal": "Desenvolvimento de Software"
  }
}
```
**✅ Resultado esperado:** Empresa criada com status "PendingDocuments"

### **5️⃣ Aprovar Empresa**
**Endpoint:** `PUT /companies/{id}/approve`
```json
{
  "observacoes": "Empresa aprovada para testes"
}
```
**✅ Resultado esperado:** Status alterado para "Approved"

### **6️⃣ Criar Usuário Cliente**
**Endpoint:** `POST /admin/users`
```json
{
  "name": "Cliente Teste",
  "email": "cliente@teste.com",
  "password": "123456",
  "role": "cliente",
  "isActive": true,
  "document": "12345678901",
  "phone": "(11) 98765-4321"
}
```
**✅ Resultado esperado:** Cliente criado com senha definida

### **7️⃣ Verificar Dashboard Admin**
**Endpoint:** `GET /admin/dashboard/metrics`
**✅ Resultado esperado:** Métricas atualizadas (1 empresa, 1 cliente)

---

## 💻 **FASE 3: CLIENTE (INTERNET BANKING)**

### **8️⃣ Login do Cliente**
**Endpoint:** `POST /auth/login`
```json
{
  "email": "cliente@teste.com",
  "password": "123456"
}
```
**✅ Resultado esperado:** Token JWT válido, role "cliente"

### **9️⃣ Verificar Dados do Cliente**
**Endpoint:** `GET /client-users/me`
**✅ Resultado esperado:** Dados do cliente logado (sem senha exposta)

### **🔟 Consultar Saldo Inicial**
**Endpoint:** `GET /balance/accounts/{userId}`
**✅ Resultado esperado:** Saldo inicial (provavelmente R$ 0,00)

---

## 💰 **FASE 4: TRANSAÇÕES PIX**

### **1️⃣1️⃣ Gerar QR Code PIX Dinâmico**
**Endpoint:** `POST /transacoes/pix/qrcode/dinamico`
```json
{
  "externalId": "QR-TESTE-001",
  "amount": 150.75,
  "pixKey": "12345678901",
  "bankCode": "SICOOB",
  "description": "Teste QR Code Dinâmico",
  "expiresIn": 3600
}
```
**✅ Resultado esperado:** QR Code gerado com EMV válido (248 caracteres)

### **1️⃣2️⃣ Gerar QR Code PIX Estático**
**Endpoint:** `POST /transacoes/pix/qrcode/estatico`
```json
{
  "externalId": "QR-STATIC-001",
  "pixKey": "12345678901",
  "bankCode": "SICOOB",
  "description": "QR Code Estático para Testes"
}
```
**✅ Resultado esperado:** QR Code estático (sem valor fixo)

### **1️⃣3️⃣ Listar Transações PIX**
**Endpoint:** `GET /transacoes/pix?page=1&pageSize=10`
**✅ Resultado esperado:** Lista com os QR Codes criados

---

## 🏦 **FASE 5: INTEGRAÇÃO SICOOB**

### **1️⃣4️⃣ Health Check Sicoob**
**Endpoint:** `GET /integrations/health`
**✅ Resultado esperado:** Sicoob integration healthy

### **1️⃣5️⃣ Criar Cobrança PIX Sicoob**
**Endpoint:** `POST /integrations/sicoob/pix/cobranca`
```json
{
  "txId": "SICOOB-TESTE-001",
  "valor": 199.99,
  "chavePix": "12345678901",
  "solicitacaoPagador": "Cobrança teste via Sicoob",
  "expiracaoSegundos": 3600
}
```
**✅ Resultado esperado:** Cobrança ATIVA com PIX Copia e Cola (248 chars)

### **1️⃣6️⃣ Consultar Cobrança Sicoob**
**Endpoint:** `GET /integrations/sicoob/pix/cobranca/SICOOB-TESTE-001`
**✅ Resultado esperado:** Detalhes da cobrança criada

---

## 🔌 **FASE 6: APIS EXTERNAS (CLIENTE INTEGRAÇÃO)**

### **1️⃣7️⃣ Autenticação OAuth 2.0**
**Endpoint:** `POST /auth/token`
```json
{
  "grant_type": "client_credentials",
  "client_id": "cliente_banking",
  "client_secret": "cliente_secret_000",
  "scope": "banking pix"
}
```
**✅ Resultado esperado:** Access token OAuth válido

### **1️⃣8️⃣ Criar Transação PIX via API Externa**
**Endpoint:** `POST /api/v1/transactions/pix`
```json
{
  "externalId": "TXN-EXT-001",
  "amount": 250.00,
  "currency": "BRL",
  "pixKey": "cliente@empresa.com",
  "description": "Pagamento via API externa",
  "webhookUrl": "https://webhook.site/your-webhook-id"
}
```
**✅ Resultado esperado:** Transação criada com status PENDING

### **1️⃣9️⃣ Consultar Status da Transação**
**Endpoint:** `GET /api/v1/transactions/{transactionId}`
**✅ Resultado esperado:** Detalhes da transação criada

### **2️⃣0️⃣ Configurar Webhook**
**Endpoint:** `POST /api/v1/webhooks`
```json
{
  "url": "https://webhook.site/your-webhook-id",
  "events": [
    "pix.received",
    "pix.sent",
    "qr_code.paid",
    "transaction.confirmed"
  ],
  "secret": "webhook-secret-key-123",
  "description": "Webhook para notificações PIX"
}
```
**✅ Resultado esperado:** Webhook configurado e ativo

---

## 🔄 **FASE 7: VALIDAÇÃO FINAL**

### **2️⃣1️⃣ Verificar Saldo Atualizado**
**Endpoint:** `GET /balance/accounts/{userId}`
**✅ Resultado esperado:** Saldo refletindo as transações

### **2️⃣2️⃣ Extrato de Transações**
**Endpoint:** `GET /balance/statement/{userId}?startDate=2024-01-01&endDate=2024-12-31`
**✅ Resultado esperado:** Histórico completo de transações

### **2️⃣3️⃣ Dashboard Admin Final**
**Endpoint:** `GET /admin/dashboard/metrics`
**✅ Resultado esperado:** Métricas atualizadas com todas as transações

---

## 📊 **CHECKLIST DE VALIDAÇÃO**

### **✅ Funcionalidades Básicas**
- [ ] Admin consegue fazer login
- [ ] Empresa pode ser criada e aprovada
- [ ] Cliente pode ser criado com senha
- [ ] Cliente consegue fazer login
- [ ] Dados do cliente são retornados corretamente

### **✅ Transações PIX**
- [ ] QR Code dinâmico é gerado com EMV válido
- [ ] QR Code estático é gerado corretamente
- [ ] Lista de transações é retornada
- [ ] Saldo é consultado corretamente

### **✅ Integração Sicoob**
- [ ] Health check retorna sucesso
- [ ] Cobrança PIX é criada com status ATIVA
- [ ] PIX Copia e Cola tem 248 caracteres
- [ ] Consulta de cobrança funciona

### **✅ APIs Externas**
- [ ] OAuth 2.0 retorna token válido
- [ ] Transação PIX é criada via API
- [ ] Status da transação é consultado
- [ ] Webhook é configurado corretamente

### **✅ Validações de Segurança**
- [ ] Tokens JWT são válidos
- [ ] Dados sensíveis não são expostos
- [ ] Autenticação é obrigatória nos endpoints protegidos

---

## 🚨 **PROBLEMAS COMUNS E SOLUÇÕES**

### **❌ Erro 401 - Unauthorized**
```bash
# Solução: Reexecutar login e verificar token
POST /auth/login com credenciais corretas
```

### **❌ Erro 404 - Not Found**
```bash
# Solução: Verificar se IDs foram capturados corretamente
Verificar variáveis {{company_id}}, {{user_id}}, etc.
```

### **❌ Erro 500 - Internal Server Error**
```bash
# Solução: Verificar logs dos serviços
docker logs fintech-auth-service --tail 20
docker logs fintech-user-service --tail 20
```

### **❌ Sicoob Integration Error**
```bash
# Solução: Verificar configuração mTLS e certificados
Verificar se certificados Sicoob estão configurados
```

---

## 🎯 **TEMPO ESTIMADO**

- **Setup inicial**: 10 minutos
- **Fase 1-2 (Admin)**: 15 minutos
- **Fase 3-4 (Cliente)**: 15 minutos
- **Fase 5 (Sicoob)**: 10 minutos
- **Fase 6 (APIs)**: 15 minutos
- **Validação final**: 10 minutos

**⏱️ Total: ~75 minutos para teste completo**

---

**📝 Execute esta sequência exatamente nesta ordem para garantir que todos os dados necessários sejam criados antes de serem utilizados!**

**🎯 Cada passo depende do anterior - não pule etapas!**
