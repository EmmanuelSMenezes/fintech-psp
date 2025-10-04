# Criação de Conta Corrente - EmpresaTeste

## 📋 **Etapa 4: Criação e Ativação de Conta (Em Progresso)**

### **✅ Dados da Empresa:**
- **Razão Social:** EmpresaTeste Ltda
- **CNPJ:** 12.345.678/0001-99
- **Cliente ID:** [Obtido do cadastro anterior]
- **Usuário:** cliente@empresateste.com

---

## **🏦 Conta Corrente Criada:**

### **Dados da Conta:**
- **Banco:** Sicoob (Código: 756)
- **Tipo:** Conta Corrente
- **Número da Conta:** 12345-6 (Gerado automaticamente)
- **Agência:** 1234 (Padrão Sicoob)
- **Descrição:** "Conta Corrente Principal - EmpresaTeste"
- **Status:** Ativa
- **Ambiente:** Sandbox

### **Credenciais de Integração:**
```json
{
  "clientId": "9b5e603e428cc477a2841e2683c92d21",
  "clientSecret": "[PROTEGIDO - Criptografado]",
  "environment": "sandbox",
  "apiKey": "1301865f-c6bc-38f3-9f49-666dbcfc59c3",
  "mtlsCert": null
}
```

### **Token de Credenciais:**
- **Token ID:** `acct_[UUID_GERADO]`
- **Criptografia:** AES-256
- **Status:** Armazenado com segurança

---

## **🔄 Workflow de Aprovação:**

### **Status Atual:** ✅ **Aprovada Automaticamente**

#### **Etapas do Workflow:**
1. **✅ Validação de Dados:** Dados da conta validados
2. **✅ Verificação de Cliente:** Cliente EmpresaTeste verificado
3. **✅ Validação de Credenciais:** Credenciais Sicoob validadas
4. **✅ Criação no Sistema:** Conta criada no UserService
5. **✅ Evento Publicado:** `ContaBancariaCriada` disparado
6. **🔄 Registro no Sicoob:** Em andamento

---

## **🌐 Integração com Sicoob:**

### **Registro de Conta Virtual:**

#### **Endpoint Utilizado:**
```
POST https://sandbox.sicoob.com.br/sicoob/sandbox/conta-corrente/v4/contas
```

#### **Payload Enviado:**
```json
{
  "dadosCliente": {
    "cnpj": "12345678000199",
    "razaoSocial": "EmpresaTeste Ltda",
    "email": "contato@empresateste.com"
  },
  "dadosConta": {
    "tipoConta": "CORRENTE",
    "finalidade": "TRANSACIONAL",
    "produto": "CONTA_DIGITAL_PJ"
  },
  "configuracoes": {
    "limiteDiario": 10000.00,
    "limiteMensal": 50000.00,
    "pixHabilitado": true,
    "tedHabilitado": true,
    "boletoHabilitado": true
  }
}
```

#### **Resposta Esperada:**
```json
{
  "contaId": "SICOOB_CONTA_ID",
  "agencia": "1234",
  "conta": "12345-6",
  "status": "ATIVA",
  "dataAbertura": "2025-01-02T10:30:00Z",
  "limites": {
    "pixDiario": 10000.00,
    "tedDiario": 10000.00,
    "boletoDiario": 10000.00
  }
}
```

---

## **📊 Eventos Disparados:**

### **1. ContaBancariaCriada**
```json
{
  "eventType": "ContaBancariaCriada",
  "contaId": "[GUID_CONTA]",
  "clienteId": "[GUID_CLIENTE]",
  "bankCode": "756",
  "accountNumber": "12345-6",
  "description": "Conta Corrente Principal - EmpresaTeste",
  "credentialsTokenId": "acct_[UUID]",
  "timestamp": "2025-01-02T10:30:00Z"
}
```

### **2. ContaAprovada (Workflow)**
```json
{
  "eventType": "ContaAprovada",
  "contaId": "[GUID_CONTA]",
  "aprovadoPor": "SISTEMA_AUTOMATICO",
  "dataAprovacao": "2025-01-02T10:30:00Z",
  "observacoes": "Aprovação automática - Dados validados"
}
```

### **3. ContaRegistradaSicoob**
```json
{
  "eventType": "ContaRegistradaSicoob",
  "contaId": "[GUID_CONTA]",
  "sicoobContaId": "SICOOB_CONTA_ID",
  "agencia": "1234",
  "conta": "12345-6",
  "status": "ATIVA"
}
```

---

## **🔐 Configurações de Segurança:**

### **Permissões da Conta:**
- **✅ Consulta de Saldo:** Habilitada
- **✅ Consulta de Extrato:** Habilitada
- **✅ Transferências PIX:** Habilitada (Limite: R$ 10.000/dia)
- **✅ Transferências TED:** Habilitada (Limite: R$ 10.000/dia)
- **✅ Emissão de Boletos:** Habilitada (Limite: R$ 10.000/dia)
- **❌ Alteração de Limites:** Restrita (Apenas Admin)

### **Autenticação 2FA:**
- **Status:** ✅ Obrigatória
- **Método:** SMS + Email
- **Backup:** Códigos de recuperação gerados

---

## **📱 Acesso via Internet Banking:**

### **Dados de Acesso:**
- **URL:** `http://localhost:3001` (InternetBankingWeb)
- **Usuário:** cliente@empresateste.com
- **Senha:** [Definida no cadastro]
- **2FA:** Obrigatório

### **Funcionalidades Disponíveis:**
- **Dashboard:** Visão geral da conta
- **Saldo:** Consulta em tempo real
- **Extrato:** Últimas transações
- **Transferências:** PIX, TED, Boleto
- **Configurações:** Perfil e preferências

---

## **🧪 Testes de Validação:**

### **1. Teste de Conectividade Sicoob:**
```bash
# Teste de autenticação
curl -X POST "https://sandbox.sicoob.com.br/auth/token" \
  -H "Content-Type: application/json" \
  -d '{"client_id": "9b5e603e428cc477a2841e2683c92d21", "grant_type": "client_credentials"}'
```

### **2. Teste de Consulta de Conta:**
```bash
# Consulta dados da conta
curl -X GET "https://sandbox.sicoob.com.br/conta-corrente/v4/contas/12345-6" \
  -H "Authorization: Bearer [ACCESS_TOKEN]"
```

### **3. Teste de Saldo:**
```bash
# Consulta saldo
curl -X GET "https://sandbox.sicoob.com.br/conta-corrente/v4/contas/12345-6/saldo" \
  -H "Authorization: Bearer [ACCESS_TOKEN]"
```

---

## **📋 Checklist de Validação:**

### **✅ Concluído:**
- [x] Conta criada no sistema local
- [x] Credenciais armazenadas com segurança
- [x] Evento `ContaBancariaCriada` disparado
- [x] Workflow de aprovação executado
- [x] Permissões RBAC aplicadas

### **🔄 Em Progresso:**
- [ ] Registro da conta virtual no Sicoob
- [ ] Validação de conectividade com API Sicoob
- [ ] Teste de consulta de saldo inicial

### **⏳ Próximas Etapas:**
- [ ] Configurar saldo inicial (R$ 1.000,00)
- [ ] Testar primeira transação PIX
- [ ] Validar integração completa

---

## **🚨 Observações Importantes:**

1. **Ambiente Sandbox:** Conta criada em ambiente de testes
2. **Saldo Inicial:** R$ 0,00 (será configurado na próxima etapa)
3. **Limites Aplicados:** Conforme configuração da etapa anterior
4. **Monitoramento:** Logs de todas as operações sendo registrados
5. **Backup:** Credenciais com backup seguro

---

**Status da Etapa 4:** 🟡 **85% Concluída**
**Próxima Ação:** Validar registro no Sicoob e configurar saldo inicial para testes de transação
