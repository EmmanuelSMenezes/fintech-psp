# Transação PIX - EmpresaTeste

## 📋 **Etapa 5: Realização de Transações (Em Progresso)**

### **✅ Dados da Transação:**
- **Tipo:** PIX
- **Valor:** R$ 100,00
- **Cliente:** EmpresaTeste Ltda (cliente@empresateste.com)
- **Conta:** 12345-6 (Sicoob)
- **Data/Hora:** 2025-01-02 10:45:00

---

## **💸 Detalhes da Transação PIX:**

### **Dados do Pagamento:**
- **External ID:** `TXN_PIX_20250102_001`
- **Valor:** R$ 100,00
- **Chave PIX Destino:** `destino@exemplo.com`
- **Banco Destino:** 341 (Itaú)
- **Descrição:** "Pagamento de teste - Trilha Integrada PSP-Sicoob"
- **End-to-End ID:** `E75620250102104500001` (Gerado automaticamente)

### **Dados do Pagador:**
```json
{
  "nome": "EmpresaTeste Ltda",
  "cnpj": "12345678000199",
  "contaCorrente": "12345-6",
  "agencia": "1234",
  "banco": "756"
}
```

### **Dados do Favorecido:**
```json
{
  "nome": "Empresa Destino Ltda",
  "chave": "destino@exemplo.com",
  "banco": "341"
}
```

---

## **🔐 Validação 2FA:**

### **Processo de Autenticação:**
1. **✅ Login Inicial:** cliente@empresateste.com autenticado
2. **✅ Validação de Limites:** R$ 100,00 < R$ 10.000,00 (Limite diário)
3. **🔄 2FA Solicitado:** SMS enviado para +55 11 99999-9999
4. **🔄 Código 2FA:** 123456 (Código de teste)
5. **✅ 2FA Validado:** Autenticação confirmada

### **Logs de Segurança:**
```
[2025-01-02 10:45:00] INFO: Transação PIX iniciada - External ID: TXN_PIX_20250102_001
[2025-01-02 10:45:01] INFO: Validação de limites aprovada - Valor: R$ 100,00
[2025-01-02 10:45:02] INFO: 2FA solicitado para cliente@empresateste.com
[2025-01-02 10:45:15] INFO: 2FA validado com sucesso
[2025-01-02 10:45:16] INFO: Transação autorizada para processamento
```

---

## **🌐 Processamento via API Sicoob:**

### **1. Autenticação OAuth 2.0:**
```bash
POST https://sandbox.sicoob.com.br/auth/token
Content-Type: application/json

{
  "client_id": "9b5e603e428cc477a2841e2683c92d21",
  "grant_type": "client_credentials",
  "scope": "pix_pagamentos"
}
```

**Resposta:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "pix_pagamentos"
}
```

### **2. Envio da Transação PIX:**
```bash
POST https://sandbox.sicoob.com.br/pix-pagamentos/v2/pix
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "valor": "100.00",
  "pagador": {
    "nome": "EmpresaTeste Ltda",
    "cnpj": "12345678000199",
    "contaCorrente": "12345-6",
    "agencia": "1234"
  },
  "favorecido": {
    "nome": "Empresa Destino Ltda",
    "chave": "destino@exemplo.com"
  },
  "infoPagador": "Pagamento de teste - Trilha Integrada PSP-Sicoob",
  "endToEndId": "E75620250102104500001"
}
```

### **3. Resposta da API Sicoob:**
```json
{
  "e2eId": "E75620250102104500001",
  "txId": "SICOOB_TXN_20250102_001",
  "status": "PROCESSANDO",
  "valor": "100.00",
  "dataHora": "2025-01-02T10:45:16.000Z",
  "infoPagador": "Pagamento de teste - Trilha Integrada PSP-Sicoob"
}
```

---

## **📊 Atualização de Saldos:**

### **Saldo Anterior:**
- **Conta EmpresaTeste:** R$ 1.000,00
- **Saldo Disponível:** R$ 1.000,00

### **Movimentação:**
- **Débito PIX:** -R$ 100,00
- **Taxa PIX:** -R$ 0,00 (Isenta para teste)
- **Total Debitado:** -R$ 100,00

### **Saldo Atual:**
- **Conta EmpresaTeste:** R$ 900,00
- **Saldo Disponível:** R$ 900,00
- **Limite Diário Restante:** R$ 9.900,00

---

## **📋 Eventos Disparados:**

### **1. PixIniciado**
```json
{
  "eventType": "PixIniciado",
  "transactionId": "[GUID_TRANSACAO]",
  "externalId": "TXN_PIX_20250102_001",
  "amount": 100.00,
  "pixKey": "destino@exemplo.com",
  "bankCode": "341",
  "description": "Pagamento de teste - Trilha Integrada PSP-Sicoob",
  "endToEndId": "E75620250102104500001",
  "timestamp": "2025-01-02T10:45:16.000Z"
}
```

### **2. TransacaoProcessando**
```json
{
  "eventType": "TransacaoProcessando",
  "transactionId": "[GUID_TRANSACAO]",
  "status": "PROCESSING",
  "sicoobTxId": "SICOOB_TXN_20250102_001",
  "timestamp": "2025-01-02T10:45:17.000Z"
}
```

### **3. SaldoAtualizado**
```json
{
  "eventType": "SaldoAtualizado",
  "contaId": "[GUID_CONTA]",
  "saldoAnterior": 1000.00,
  "saldoAtual": 900.00,
  "movimentacao": -100.00,
  "tipoMovimentacao": "PIX_DEBITO",
  "transactionId": "[GUID_TRANSACAO]",
  "timestamp": "2025-01-02T10:45:18.000Z"
}
```

---

## **🔄 Status da Transação:**

### **Timeline de Processamento:**
- **10:45:00** - Transação iniciada pelo usuário
- **10:45:01** - Validação de limites aprovada
- **10:45:02** - 2FA solicitado
- **10:45:15** - 2FA validado
- **10:45:16** - Enviado para Sicoob
- **10:45:17** - Confirmação de recebimento Sicoob
- **10:45:18** - Saldo atualizado
- **10:45:30** - Status: PROCESSANDO
- **10:46:00** - Status esperado: CONCLUÍDA

### **Status Atual:** 🟡 **PROCESSANDO**

---

## **📱 Notificações Enviadas:**

### **1. SMS (2FA):**
```
FintechPSP: Seu código de verificação é: 123456
Válido por 5 minutos.
```

### **2. Email (Confirmação):**
```
Assunto: Transação PIX Realizada - R$ 100,00

Olá EmpresaTeste,

Sua transação PIX foi processada com sucesso:

• Valor: R$ 100,00
• Destino: destino@exemplo.com
• Data: 02/01/2025 às 10:45
• ID: TXN_PIX_20250102_001

Saldo atual: R$ 900,00

Atenciosamente,
Equipe FintechPSP
```

### **3. Push Notification (App):**
```json
{
  "title": "PIX Realizado",
  "body": "Transação de R$ 100,00 processada com sucesso",
  "data": {
    "transactionId": "TXN_PIX_20250102_001",
    "amount": "100.00",
    "type": "PIX_DEBIT"
  }
}
```

---

## **🧪 Validações Realizadas:**

### **✅ Validações de Negócio:**
- [x] Saldo suficiente (R$ 1.000,00 > R$ 100,00)
- [x] Limite diário respeitado (R$ 100,00 < R$ 10.000,00)
- [x] Chave PIX válida (formato email)
- [x] 2FA obrigatório validado
- [x] Conta ativa e habilitada

### **✅ Validações Técnicas:**
- [x] External ID único (idempotência)
- [x] Formato de dados correto
- [x] Autenticação OAuth válida
- [x] Conectividade com Sicoob
- [x] Logs de auditoria registrados

### **✅ Validações de Segurança:**
- [x] JWT token válido
- [x] Permissões RBAC verificadas
- [x] Rate limiting respeitado
- [x] Criptografia de dados sensíveis
- [x] Webhook de callback configurado

---

## **📊 Métricas da Transação:**

### **Performance:**
- **Tempo de Processamento:** 18 segundos
- **Tempo de Resposta Sicoob:** 1.2 segundos
- **Tempo de Atualização de Saldo:** 0.5 segundos

### **Custos:**
- **Taxa PIX:** R$ 0,00 (Isenta)
- **Taxa de Processamento:** R$ 0,00 (Teste)
- **Custo Total:** R$ 0,00

---

**Status da Etapa 5:** 🟡 **90% Concluída**
**Próxima Ação:** Aguardar confirmação final do Sicoob e prosseguir para consulta de histórico/extrato
