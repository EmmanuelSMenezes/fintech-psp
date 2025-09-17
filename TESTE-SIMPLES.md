# 🚀 **TESTE SIMPLES - QR CODE PIX**

## 📋 **Passo a Passo Rápido**

### **1️⃣ Iniciar Apenas os Serviços Essenciais**

Vamos testar sem MassTransit para simplificar:

```powershell
# Terminal 1 - AuthService
dotnet run --project src/Services/FintechPSP.AuthService --urls "http://localhost:5001"

# Terminal 2 - TransactionService  
dotnet run --project src/Services/FintechPSP.TransactionService --urls "http://localhost:5002"

# Terminal 3 - IntegrationService
dotnet run --project src/Services/FintechPSP.IntegrationService --urls "http://localhost:5005"

# Terminal 4 - APIGateway
dotnet run --project src/Gateway/FintechPSP.APIGateway --urls "http://localhost:5000"
```

### **2️⃣ Configurar Postman**

1. **Importar**: `postman/FintechPSP-Collection.json`
2. **Variável base_url**: `http://localhost:5000`

### **3️⃣ Testar Sequência**

#### **3.1 Obter Token**
- **Request**: `Obter Token OAuth 2.0`
- **URL**: `{{base_url}}/auth/token`

#### **3.2 QR Code Estático**
- **Request**: `Gerar QR Code Estático`
- **URL**: `{{base_url}}/transacoes/pix/qrcode/estatico`
- **Body**:
```json
{
  "externalId": "QR-STATIC-001",
  "pixKey": "11999887766",
  "bankCode": "001",
  "description": "Teste QR estático"
}
```

#### **3.3 QR Code Dinâmico**
- **Request**: `Gerar QR Code Dinâmico`
- **URL**: `{{base_url}}/transacoes/pix/qrcode/dinamico`
- **Body**:
```json
{
  "externalId": "QR-DYNAMIC-001",
  "amount": 100.00,
  "pixKey": "teste@email.com",
  "bankCode": "237",
  "description": "Teste QR dinâmico",
  "expiresIn": 300
}
```

#### **3.4 Health Check**
- **Request**: `Health Check QR Code`
- **URL**: `{{base_url}}/qrcode/health`

---

## ✅ **Resultados Esperados**

### **Token Response**
```json
{
  "access_token": "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

### **QR Code Estático Response**
```json
{
  "transactionId": "123e4567-e89b-12d3-a456-426614174000",
  "qrcodePayload": "00020126580014br.gov.bcb.pix...",
  "qrcodeImage": "iVBORw0KGgoAAAANSUhEUgAAAAE...",
  "type": "static",
  "expiresAt": null,
  "pixKey": "11999887766",
  "amount": null,
  "description": "Teste QR estático",
  "createdAt": "2025-09-17T13:30:00Z"
}
```

### **QR Code Dinâmico Response**
```json
{
  "transactionId": "456e7890-e89b-12d3-a456-426614174001",
  "qrcodePayload": "00020126580014br.gov.bcb.pix...",
  "qrcodeImage": "iVBORw0KGgoAAAANSUhEUgAAAAE...",
  "type": "dynamic",
  "expiresAt": "2025-09-17T13:35:00Z",
  "pixKey": "teste@email.com",
  "amount": 100.00,
  "description": "Teste QR dinâmico",
  "createdAt": "2025-09-17T13:30:00Z"
}
```

---

## 🎯 **Pontos de Validação**

### **✅ QR Code Estático**
- ✅ `type`: "static"
- ✅ `expiresAt`: null
- ✅ `amount`: null
- ✅ `qrcodePayload`: Começa com "000201"
- ✅ `qrcodeImage`: Base64 válido

### **✅ QR Code Dinâmico**
- ✅ `type`: "dynamic"
- ✅ `expiresAt`: Data futura
- ✅ `amount`: Valor informado
- ✅ `qrcodePayload`: Contém valor
- ✅ `qrcodeImage`: Base64 válido

### **✅ Payload EMV**
- ✅ Inicia com "000201" (Payload Format)
- ✅ Contém "br.gov.bcb.pix"
- ✅ Termina com CRC16 válido
- ✅ PIX Key presente no payload

---

## 🔧 **Troubleshooting**

### **Erro: "Connection refused"**
```bash
# Verificar se os serviços estão rodando
netstat -an | findstr :5001
netstat -an | findstr :5002
netstat -an | findstr :5005
netstat -an | findstr :5000
```

### **Erro: "Unauthorized"**
- ✅ Executar "Obter Token" primeiro
- ✅ Verificar se token foi salvo em `access_token`

### **Erro: "MassTransit"**
- ✅ Ignorar por enquanto (eventos não críticos para teste)
- ✅ QR Code funciona independente do MassTransit

---

## 🎉 **Sucesso!**

Se você conseguiu:
- ✅ Obter token de autenticação
- ✅ Gerar QR Code estático
- ✅ Gerar QR Code dinâmico
- ✅ Receber payloads EMV válidos
- ✅ Receber imagens base64

**🚀 Parabéns! O sistema QR Code PIX está funcionando perfeitamente!**

---

## 📊 **Próximos Passos**

1. **Testar Integrações**: Endpoints dos bancos em `/integrations`
2. **Validar Payloads**: Usar leitor QR Code para validar
3. **Testar Banking**: Endpoints com scope `banking`
4. **Configurar MassTransit**: Para eventos completos
5. **Deploy**: Preparar para produção

**Sistema pronto para uso! 🎯**
