# 📮 **TESTES POSTMAN PASSO A PASSO - FINTECHPSP**

## 📋 **Visão Geral**

Este documento fornece um guia completo para testar todas as APIs do FintechPSP usando Postman, cobrindo:
- 🖥️ **APIs do BackofficeWeb** (Administração)
- 💻 **APIs do InternetBankingWeb** (Cliente)
- 🔌 **APIs para Clientes Externos** (Integração)

---

## 🚀 **Setup Inicial do Postman**

### **1️⃣ Instalação e Configuração**

#### **Instalar Postman**
```bash
# Download do site oficial
https://www.postman.com/downloads/

# Ou via package manager
# Windows (Chocolatey)
choco install postman

# macOS (Homebrew)
brew install --cask postman

# Linux (Snap)
sudo snap install postman
```

#### **Importar Collection Existente**
```bash
# Localizar arquivo da collection
postman/FintechPSP-Transacoes-Cliente.json

# No Postman:
1. File → Import
2. Selecionar arquivo JSON
3. Confirmar importação
```

### **2️⃣ Configuração de Environment**

#### **Criar Environment "FintechPSP-Test"**
```json
{
  "name": "FintechPSP-Test",
  "values": [
    {
      "key": "base_url",
      "value": "http://localhost:5000",
      "enabled": true
    },
    {
      "key": "backoffice_url",
      "value": "http://localhost:3000",
      "enabled": true
    },
    {
      "key": "internetbanking_url",
      "value": "http://localhost:3001",
      "enabled": true
    },
    {
      "key": "admin_token",
      "value": "",
      "enabled": true
    },
    {
      "key": "client_token",
      "value": "",
      "enabled": true
    },
    {
      "key": "oauth_token",
      "value": "",
      "enabled": true
    },
    {
      "key": "company_id",
      "value": "",
      "enabled": true
    },
    {
      "key": "user_id",
      "value": "",
      "enabled": true
    },
    {
      "key": "transaction_id",
      "value": "",
      "enabled": true
    }
  ]
}
```

---

## 🖥️ **APIS DO BACKOFFICE (ADMINISTRAÇÃO)**

### **📁 Folder: "01 - BackofficeWeb APIs"**

#### **🔐 1.1 - Login Admin**
```http
POST {{base_url}}/auth/login
Content-Type: application/json

{
  "email": "admin@fintechpsp.com",
  "password": "admin123"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response has access token", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('accessToken');
    pm.environment.set("admin_token", jsonData.accessToken);
});

pm.test("User is admin", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.user.role).to.eql("Admin");
});
```

#### **🏢 1.2 - Listar Empresas**
```http
GET {{base_url}}/companies
Authorization: Bearer {{admin_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Response is array", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('array');
});

pm.test("Companies have required fields", function () {
    var jsonData = pm.response.json();
    if (jsonData.length > 0) {
        pm.expect(jsonData[0]).to.have.property('id');
        pm.expect(jsonData[0]).to.have.property('razaoSocial');
        pm.expect(jsonData[0]).to.have.property('cnpj');
        pm.expect(jsonData[0]).to.have.property('status');
    }
});
```

#### **🏢 1.3 - Criar Empresa**
```http
POST {{base_url}}/companies
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "razaoSocial": "Empresa Teste Postman Ltda",
  "nomeFantasia": "Teste Postman",
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
  "email": "teste@postman.com",
  "contractData": {
    "capitalSocial": 100000.00,
    "dataConstituicao": "2024-01-01",
    "naturezaJuridica": "Sociedade Limitada",
    "atividadePrincipal": "Desenvolvimento de Software"
  }
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Company created successfully", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.environment.set("company_id", jsonData.id);
    pm.expect(jsonData.razaoSocial).to.eql("Empresa Teste Postman Ltda");
    pm.expect(jsonData.status).to.eql("PendingDocuments");
});
```

#### **✅ 1.4 - Aprovar Empresa**
```http
PUT {{base_url}}/companies/{{company_id}}/approve
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "observacoes": "Empresa aprovada via teste Postman"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Company approved", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData.status).to.eql("Approved");
    pm.expect(jsonData).to.have.property('approvedAt');
});
```

#### **👥 1.5 - Criar Usuário Cliente**
```http
POST {{base_url}}/admin/users
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "name": "Cliente Teste Postman",
  "email": "cliente.postman@teste.com",
  "password": "123456",
  "role": "cliente",
  "isActive": true,
  "document": "12345678901",
  "phone": "(11) 98765-4321"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("User created successfully", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.environment.set("user_id", jsonData.id);
    pm.expect(jsonData.email).to.eql("cliente.postman@teste.com");
    pm.expect(jsonData.role).to.eql("cliente");
});
```

#### **📊 1.6 - Dashboard Métricas**
```http
GET {{base_url}}/admin/dashboard/metrics
Authorization: Bearer {{admin_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Metrics have required fields", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('totalCompanies');
    pm.expect(jsonData).to.have.property('totalUsers');
    pm.expect(jsonData).to.have.property('totalTransactions');
    pm.expect(jsonData).to.have.property('totalVolume');
});
```

#### **🔧 1.7 - Configurações do Sistema**
```http
GET {{base_url}}/admin/system/configs
Authorization: Bearer {{admin_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Configs are returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('object');
    pm.expect(jsonData).to.have.property('pixEnabled');
    pm.expect(jsonData).to.have.property('tedEnabled');
    pm.expect(jsonData).to.have.property('boletoEnabled');
});
```

---

## 💻 **APIS DO INTERNET BANKING (CLIENTE)**

### **📁 Folder: "02 - InternetBankingWeb APIs"**

#### **🔐 2.1 - Login Cliente**
```http
POST {{base_url}}/auth/login
Content-Type: application/json

{
  "email": "cliente.postman@teste.com",
  "password": "123456"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Client login successful", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('accessToken');
    pm.environment.set("client_token", jsonData.accessToken);
    pm.expect(jsonData.user.role).to.eql("cliente");
});
```

#### **👤 2.2 - Dados do Cliente Logado**
```http
GET {{base_url}}/client-users/me
Authorization: Bearer {{client_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Client data returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('name');
    pm.expect(jsonData).to.have.property('email');
    pm.expect(jsonData.email).to.eql("cliente.postman@teste.com");
});
```

#### **💳 2.3 - Consultar Saldo**
```http
GET {{base_url}}/balance/accounts/{{user_id}}
Authorization: Bearer {{client_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Balance information returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('balance');
    pm.expect(jsonData).to.have.property('currency');
    pm.expect(jsonData.currency).to.eql("BRL");
});
```

#### **💰 2.4 - Gerar QR Code PIX Dinâmico**
```http
POST {{base_url}}/transacoes/pix/qrcode/dinamico
Authorization: Bearer {{client_token}}
Content-Type: application/json

{
  "externalId": "QR-POSTMAN-{{$timestamp}}",
  "amount": 150.75,
  "pixKey": "12345678901",
  "bankCode": "SICOOB",
  "description": "Teste QR Code Postman",
  "expiresIn": 3600
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("QR Code created successfully", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('emvCode');
    pm.expect(jsonData).to.have.property('qrCodeImageUrl');
    pm.expect(jsonData.amount).to.eql(150.75);
    pm.expect(jsonData.type).to.eql("DYNAMIC");
});
```

#### **💰 2.5 - Gerar QR Code PIX Estático**
```http
POST {{base_url}}/transacoes/pix/qrcode/estatico
Authorization: Bearer {{client_token}}
Content-Type: application/json

{
  "externalId": "QR-STATIC-POSTMAN-{{$timestamp}}",
  "pixKey": "12345678901",
  "bankCode": "SICOOB",
  "description": "QR Code Estático Postman"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Static QR Code created", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('id');
    pm.expect(jsonData).to.have.property('emvCode');
    pm.expect(jsonData.type).to.eql("STATIC");
    pm.expect(jsonData.amount).to.be.null;
});
```

#### **📋 2.6 - Listar Transações PIX**
```http
GET {{base_url}}/transacoes/pix?page=1&pageSize=10
Authorization: Bearer {{client_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Transactions list returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('array');
    
    if (jsonData.length > 0) {
        pm.expect(jsonData[0]).to.have.property('transactionId');
        pm.expect(jsonData[0]).to.have.property('type');
        pm.expect(jsonData[0]).to.have.property('status');
        pm.expect(jsonData[0]).to.have.property('amount');
    }
});
```

#### **📊 2.7 - Extrato de Transações**
```http
GET {{base_url}}/balance/statement/{{user_id}}?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer {{client_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Statement returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.be.an('array');
    
    if (jsonData.length > 0) {
        pm.expect(jsonData[0]).to.have.property('id');
        pm.expect(jsonData[0]).to.have.property('amount');
        pm.expect(jsonData[0]).to.have.property('operation');
        pm.expect(jsonData[0]).to.have.property('createdAt');
    }
});
```

---

## 🔌 **APIS PARA CLIENTES EXTERNOS (INTEGRAÇÃO)**

### **📁 Folder: "03 - Client Integration APIs"**

#### **🔐 3.1 - OAuth 2.0 Client Credentials**
```http
POST {{base_url}}/auth/token
Content-Type: application/json

{
  "grant_type": "client_credentials",
  "client_id": "cliente_banking",
  "client_secret": "cliente_secret_000",
  "scope": "banking pix"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("OAuth token received", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('access_token');
    pm.expect(jsonData).to.have.property('token_type');
    pm.expect(jsonData.token_type).to.eql("Bearer");
    pm.environment.set("oauth_token", jsonData.access_token);
});
```

#### **💰 3.2 - Criar Transação PIX (Cliente Externo)**
```http
POST {{base_url}}/api/v1/transactions/pix
Authorization: Bearer {{oauth_token}}
Content-Type: application/json

{
  "externalId": "TXN-CLIENT-{{$timestamp}}",
  "amount": 250.00,
  "currency": "BRL",
  "pixKey": "cliente@empresa.com",
  "description": "Pagamento via API externa",
  "webhookUrl": "https://webhook.site/your-webhook-id"
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("PIX transaction created", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('transactionId');
    pm.expect(jsonData).to.have.property('status');
    pm.expect(jsonData.status).to.eql("PENDING");
    pm.environment.set("transaction_id", jsonData.transactionId);
});
```

#### **🔍 3.3 - Consultar Status da Transação**
```http
GET {{base_url}}/api/v1/transactions/{{transaction_id}}
Authorization: Bearer {{oauth_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Transaction details returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('transactionId');
    pm.expect(jsonData).to.have.property('status');
    pm.expect(jsonData).to.have.property('amount');
    pm.expect(jsonData).to.have.property('createdAt');
});
```

#### **💰 3.4 - Gerar QR Code para Cliente Externo**
```http
POST {{base_url}}/api/v1/pix/qrcode
Authorization: Bearer {{oauth_token}}
Content-Type: application/json

{
  "externalId": "QR-API-{{$timestamp}}",
  "amount": 99.90,
  "pixKey": "12345678901",
  "description": "QR Code via API",
  "expiresIn": 1800
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("QR Code generated for external client", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('qrCodeId');
    pm.expect(jsonData).to.have.property('emvCode');
    pm.expect(jsonData).to.have.property('pixCopiaECola');
    pm.expect(jsonData.emvCode).to.have.length.greaterThan(200);
});
```

#### **📋 3.5 - Listar Transações do Cliente**
```http
GET {{base_url}}/api/v1/transactions?startDate=2024-01-01&endDate=2024-12-31&status=CONFIRMED
Authorization: Bearer {{oauth_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Client transactions returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('transactions');
    pm.expect(jsonData).to.have.property('pagination');
    pm.expect(jsonData.transactions).to.be.an('array');
});
```

#### **🔗 3.6 - Configurar Webhook**
```http
POST {{base_url}}/api/v1/webhooks
Authorization: Bearer {{oauth_token}}
Content-Type: application/json

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

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Webhook configured successfully", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('webhookId');
    pm.expect(jsonData).to.have.property('url');
    pm.expect(jsonData.events).to.include('pix.received');
    pm.expect(jsonData.active).to.be.true;
});
```

---

## 🔌 **INTEGRAÇÃO SICOOB (TESTES AVANÇADOS)**

### **📁 Folder: "04 - Sicoob Integration"**

#### **🏦 4.1 - Health Check Sicoob**
```http
GET {{base_url}}/integrations/health
Authorization: Bearer {{admin_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Sicoob integration healthy", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('sicoob');
    pm.expect(jsonData.sicoob.isHealthy).to.be.true;
    pm.expect(jsonData.sicoob.responseTimeMs).to.be.below(5000);
});
```

#### **💰 4.2 - Criar Cobrança PIX Sicoob**
```http
POST {{base_url}}/integrations/sicoob/pix/cobranca
Authorization: Bearer {{admin_token}}
Content-Type: application/json

{
  "txId": "SICOOB-{{$timestamp}}",
  "valor": 199.99,
  "chavePix": "12345678901",
  "solicitacaoPagador": "Cobrança teste Sicoob via Postman",
  "expiracaoSegundos": 3600
}
```

**Tests Script:**
```javascript
pm.test("Status code is 201", function () {
    pm.response.to.have.status(201);
});

pm.test("Sicoob charge created", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('txId');
    pm.expect(jsonData).to.have.property('status');
    pm.expect(jsonData).to.have.property('pixCopiaECola');
    pm.expect(jsonData.status).to.eql("ATIVA");
    pm.expect(jsonData.pixCopiaECola).to.have.length(248);
});
```

#### **🔍 4.3 - Consultar Cobrança Sicoob**
```http
GET {{base_url}}/integrations/sicoob/pix/cobranca/SICOOB-{{$timestamp}}
Authorization: Bearer {{admin_token}}
```

**Tests Script:**
```javascript
pm.test("Status code is 200", function () {
    pm.response.to.have.status(200);
});

pm.test("Sicoob charge details returned", function () {
    var jsonData = pm.response.json();
    pm.expect(jsonData).to.have.property('txId');
    pm.expect(jsonData).to.have.property('valor');
    pm.expect(jsonData).to.have.property('status');
    pm.expect(jsonData).to.have.property('vencimento');
});
```

---

## 🧪 **TESTES DE FLUXO COMPLETO**

### **📁 Folder: "05 - End-to-End Flows"**

#### **🔄 5.1 - Fluxo Completo: Admin → Cliente → Transação**

**Pre-request Script:**
```javascript
// Gerar IDs únicos para o teste
const timestamp = Date.now();
pm.environment.set("test_timestamp", timestamp);
pm.environment.set("test_company_cnpj", `12.345.678/000${timestamp.toString().slice(-1)}-99`);
pm.environment.set("test_client_email", `cliente.${timestamp}@teste.com`);
```

**Sequência de Requests:**
1. Login Admin
2. Criar Empresa
3. Aprovar Empresa  
4. Criar Cliente
5. Login Cliente
6. Gerar QR Code
7. Consultar Transação

#### **📊 5.2 - Teste de Performance (Collection Runner)**

**Collection Variables:**
```json
{
  "iterations": 10,
  "delay": 1000,
  "data": "test-data.csv"
}
```

**test-data.csv:**
```csv
email,password,amount,description
cliente1@teste.com,123456,100.00,Teste 1
cliente2@teste.com,123456,200.00,Teste 2
cliente3@teste.com,123456,300.00,Teste 3
```

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**📮 Postman**: Collection completa
