# 🚀 FintechPSP - Collection Transações Cliente

Collection completa do Postman para transacionar como cliente através das APIs do FintechPSP.

## 📋 **Pré-requisitos**

1. **Sistemas rodando**: Todos os containers Docker devem estar ativos
2. **Postman instalado**: Versão 9.0 ou superior
3. **Collection importada**: `FintechPSP-Transacoes-Cliente.json`

## 🔧 **Configuração Inicial**

### **1. Importar Collection**
1. Abra o Postman
2. Clique em "Import"
3. Selecione o arquivo `FintechPSP-Transacoes-Cliente.json`
4. Confirme a importação

### **2. Configurar Variáveis**
A collection já vem com as variáveis configuradas:
- `base_url`: `http://localhost:5000` (API Gateway)
- `admin_token`: Será preenchido automaticamente após login admin
- `client_token`: Será preenchido automaticamente após login cliente
- `company_id`: Será preenchido automaticamente após criar empresa
- `user_id`: Será preenchido automaticamente após criar usuário
- `qr_code_id`: Será preenchido automaticamente após gerar QR Code

## 🎯 **Fluxo de Execução**

### **ETAPA 1: Autenticação Admin**
Execute na ordem:
1. **1.1 Login Admin**
   - Faz login com `admin@fintechpsp.com` / `admin123`
   - Salva automaticamente o `admin_token`

### **ETAPA 2: Cadastro da Empresa (Como Admin)**
2. **2.1 Criar Empresa**
   - Cria a empresa "EmpresaTeste Ltda"
   - Salva automaticamente o `company_id`

3. **2.2 Aprovar Empresa**
   - Aprova a empresa para permitir operações
   - Usa o `company_id` automaticamente

### **ETAPA 3: Criação do Usuário Cliente (Como Admin)**
4. **3.1 Criar Usuário Cliente**
   - Cria usuário `cliente@empresateste.com`
   - Salva automaticamente o `user_id`

### **ETAPA 4: Login como Cliente**
5. **1.2 Login Cliente**
   - Faz login com `cliente@empresateste.com` / `123456`
   - Salva automaticamente o `client_token`

### **ETAPA 5: Transações PIX (Como Cliente)**
6. **4.1 Gerar QR Code PIX Dinâmico**
   - Cria QR Code com valor R$ 100,50
   - Salva automaticamente o `qr_code_id`

7. **4.2 Gerar QR Code PIX Estático**
   - Cria QR Code sem valor fixo

8. **4.3 Consultar QR Code**
   - Consulta detalhes do QR Code criado

9. **4.4 Listar Transações PIX**
   - Lista todas as transações PIX do cliente

### **ETAPA 6: Integração Sicoob (Como Cliente)**
10. **5.1 Teste Conectividade Sicoob**
    - Testa conexão com APIs Sicoob

11. **5.2 Criar Cobrança PIX Sicoob**
    - Cria cobrança real no Sicoob

12. **5.3 Consultar Cobrança Sicoob**
    - Consulta status da cobrança no Sicoob

### **ETAPA 7: Health Checks**
13. **6.1-6.3 Health Checks**
    - Verifica status de todos os serviços

## 🔑 **Credenciais Padrão**

### **Admin (BackofficeWeb)**
- **Email**: `admin@fintechpsp.com`
- **Senha**: `admin123`
- **Role**: `Admin`

### **Cliente (InternetBankingWeb)**
- **Email**: `cliente@empresateste.com`
- **Senha**: `123456` (padrão do sistema)
- **Role**: `COMPANY_ADMIN`

## 📊 **Dados de Teste**

### **Empresa**
- **Razão Social**: EmpresaTeste Ltda
- **CNPJ**: 12.345.678/0001-99
- **Email**: contato@empresateste.com

### **PIX**
- **Chave PIX**: cliente@empresateste.com
- **Valor Teste**: R$ 100,50
- **Descrição**: Teste PIX - Cobrança dinâmica

## ✅ **Validações Automáticas**

A collection inclui testes automáticos que:
1. **Verificam status HTTP** (200, 201, 202)
2. **Extraem tokens automaticamente** dos responses
3. **Salvam IDs** para uso em requests subsequentes
4. **Exibem logs** no console do Postman

## 🚨 **Troubleshooting**

### **Erro 401 - Unauthorized**
- Verifique se executou o login correto (admin ou cliente)
- Confirme se o token foi salvo nas variáveis

### **Erro 404 - Not Found**
- Verifique se todos os containers estão rodando
- Confirme se a URL base está correta: `http://localhost:5000`

### **Erro 500 - Internal Server Error**
- Verifique logs dos containers: `docker-compose logs`
- Confirme se o banco PostgreSQL está acessível

### **Variáveis não preenchidas**
- Execute os requests na ordem correta
- Verifique se os testes automáticos estão funcionando
- Confirme se os responses contêm os campos esperados

## 🎉 **Resultado Esperado**

Após executar toda a sequência, você terá:
- ✅ Empresa cadastrada e aprovada
- ✅ Usuário cliente criado e logado
- ✅ QR Codes PIX gerados (estático e dinâmico)
- ✅ Integração Sicoob testada
- ✅ Cobrança PIX criada no Sicoob
- ✅ Transações listadas e consultadas

**Agora você pode transacionar como cliente através das APIs do FintechPSP!** 🚀
