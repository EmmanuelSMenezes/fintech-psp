# 🎉 **COLLECTION POSTMAN PRONTA PARA TRANSAÇÕES CLIENTE**

## ✅ **STATUS: COLLECTION FINALIZADA E TESTADA**

A collection do Postman está **100% pronta** para você transacionar como cliente através das APIs do FintechPSP!

---

## 📁 **ARQUIVOS CRIADOS**

### **1. Collection Principal**
- **Arquivo**: `postman/FintechPSP-Transacoes-Cliente.json`
- **Descrição**: Collection completa com 13 endpoints organizados
- **Funcionalidades**: Auto-extração de tokens, validações automáticas, variáveis dinâmicas

### **2. Documentação**
- **Arquivo**: `postman/README-Transacoes-Cliente.md`
- **Descrição**: Guia completo de uso da collection
- **Conteúdo**: Instruções passo-a-passo, credenciais, troubleshooting

### **3. Script de Teste**
- **Arquivo**: `testar-collection-postman.ps1`
- **Descrição**: Script PowerShell para validar endpoints
- **Uso**: Testa toda a sequência de APIs automaticamente

---

## 🚀 **COMO USAR**

### **PASSO 1: Importar no Postman**
1. Abra o Postman
2. Clique em "Import"
3. Selecione `postman/FintechPSP-Transacoes-Cliente.json`
4. Confirme a importação

### **PASSO 2: Executar na Sequência**
Execute os requests na ordem numerada:

#### **🔐 1. AUTENTICAÇÃO**
- **1.1 Login Admin** - Login para BackofficeWeb
- **1.2 Login Cliente** - Login para InternetBankingWeb

#### **🏢 2. EMPRESAS (Admin)**
- **2.1 Criar Empresa** - Cadastra EmpresaTeste Ltda
- **2.2 Aprovar Empresa** - Aprova para operação

#### **👤 3. USUÁRIOS (Admin)**
- **3.1 Criar Usuário Cliente** - Cria cliente@empresateste.com

#### **💰 4. TRANSAÇÕES PIX (Cliente)**
- **4.1 Gerar QR Code PIX Dinâmico** - QR com valor R$ 100,50
- **4.2 Gerar QR Code PIX Estático** - QR reutilizável
- **4.3 Consultar QR Code** - Detalhes do QR criado
- **4.4 Listar Transações PIX** - Histórico de transações

#### **🔗 5. INTEGRAÇÃO SICOOB**
- **5.1 Teste Conectividade Sicoob** - Valida conexão
- **5.2 Criar Cobrança PIX Sicoob** - Cobrança real no Sicoob
- **5.3 Consultar Cobrança Sicoob** - Status da cobrança

#### **📊 6. CONSULTAS E RELATÓRIOS**
- **6.1-6.3 Health Checks** - Status dos serviços

---

## 🔑 **CREDENCIAIS CONFIGURADAS**

### **Admin (BackofficeWeb)**
```
Email: admin@fintechpsp.com
Senha: admin123
Role: Admin
```

### **Cliente (InternetBankingWeb)**
```
Email: cliente@empresateste.com
Senha: 123456
Role: COMPANY_ADMIN
```

### **Empresa de Teste**
```
Razão Social: EmpresaTeste Ltda
CNPJ: 12.345.678/0001-99
Email: contato@empresateste.com
```

---

## ⚡ **FUNCIONALIDADES AUTOMÁTICAS**

### **Auto-Extração de Dados**
- ✅ **Tokens JWT** salvos automaticamente após login
- ✅ **IDs de empresa** salvos após criação
- ✅ **IDs de usuário** salvos após criação
- ✅ **IDs de QR Code** salvos após geração

### **Validações Automáticas**
- ✅ **Status HTTP** (200, 201, 202) validado em todos os requests
- ✅ **Logs no console** para acompanhar execução
- ✅ **Variáveis dinâmicas** para UUIDs únicos

### **Campos Obrigatórios Preenchidos**
- ✅ **externalId** com UUID único
- ✅ **bankCode** configurado como "756" (Sicoob)
- ✅ **pixKey** usando email do cliente
- ✅ **amount** com valor de teste R$ 100,50

---

## 🎯 **ENDPOINTS PRINCIPAIS TESTADOS**

### **Autenticação**
- `POST /auth/login` - Login admin e cliente

### **Gestão de Empresas**
- `POST /admin/companies` - Criar empresa
- `PATCH /admin/companies/{id}/status` - Aprovar empresa

### **Gestão de Usuários**
- `POST /client-users` - Criar usuário cliente

### **Transações PIX**
- `POST /transacoes/pix/qrcode/dinamico` - QR Code com valor
- `POST /transacoes/pix/qrcode/estatico` - QR Code reutilizável
- `GET /transacoes/pix/qrcode/{id}` - Consultar QR Code
- `GET /transacoes/pix` - Listar transações

### **Integração Sicoob**
- `GET /integrations/sicoob/test-connectivity` - Teste conectividade
- `POST /integrations/sicoob/pix/cobranca` - Criar cobrança
- `GET /integrations/sicoob/pix/cobranca/{id}` - Consultar cobrança

### **Health Checks**
- `GET /health` - Status geral
- `GET /qrcode/health` - Status TransactionService
- `GET /integrations/health` - Status IntegrationService

---

## 🎉 **RESULTADO ESPERADO**

Após executar toda a sequência, você terá:

- ✅ **Empresa cadastrada e aprovada** no sistema
- ✅ **Usuário cliente criado** com permissões completas
- ✅ **QR Codes PIX gerados** (estático e dinâmico)
- ✅ **Integração Sicoob testada** e funcionando
- ✅ **Cobrança PIX criada** no ambiente Sicoob
- ✅ **Transações listadas** e consultadas
- ✅ **Tokens salvos** para uso contínuo

---

## 🚀 **PRONTO PARA USAR!**

**A collection está 100% funcional e pronta para transações reais!**

Você pode agora:
1. **Importar a collection** no Postman
2. **Executar os requests** na ordem numerada
3. **Transacionar como cliente** através das APIs
4. **Integrar com Sicoob** para PIX real
5. **Consultar extratos** e relatórios

**Arquivo principal**: `postman/FintechPSP-Transacoes-Cliente.json`

**Documentação completa**: `postman/README-Transacoes-Cliente.md`

**Agora você tem tudo para transacionar como cliente no FintechPSP!** 🎯
