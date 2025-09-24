# 🚀 FintechPSP - Collection Postman Final

Esta é a collection completa e final para testar todo o sistema FintechPSP com as configurações JWT "Mortadela".

## 📋 Como Usar

### 1. Importar a Collection
1. Abra o Postman
2. Clique em "Import"
3. Selecione o arquivo `FintechPSP-Collection-Final.json`
4. A collection será importada com todas as variáveis configuradas

### 2. Configuração Inicial
- **Base URL**: `http://localhost:5000` (já configurado)
- **Credenciais Admin**: `admin@fintechpsp.com` / `admin123`
- **JWT**: Configurado para usar "Mortadela" como Issuer/Audience

### 3. Variáveis Automáticas
A collection gerencia automaticamente as seguintes variáveis:
- `access_token` - Token JWT obtido no login
- `client_id` - ID do cliente criado
- `account_id` - ID da conta bancária criada
- `transaction_id` - ID da transação criada

## 🎯 Trilha de Testes - SIGA ESTA ORDEM

### 📋 FASE 1 - AUTENTICAÇÃO
1. **1️⃣ Login Admin** - Fazer login e obter token JWT
2. **2️⃣ Validar Token JWT** - Verificar se o token está válido

### 👥 FASE 2 - GESTÃO DE USUÁRIOS
3. **3️⃣ Criar Cliente** - Criar um novo cliente no sistema
4. **4️⃣ Listar Clientes** - Ver todos os clientes cadastrados
5. **5️⃣ Buscar Cliente por ID** - Buscar cliente específico
6. **6️⃣ Atualizar Cliente** - Atualizar dados do cliente

### 🏦 FASE 3 - GESTÃO DE CONTAS
7. **7️⃣ Criar Conta Bancária** - Criar conta para o cliente
8. **8️⃣ Listar Contas** - Ver todas as contas
9. **9️⃣ Consultar Saldo** - Verificar saldo da conta

### 💸 FASE 4 - TRANSAÇÕES
10. **🔟 Criar Transação PIX** - Processar uma transação PIX
11. **1️⃣1️⃣ Consultar Transação** - Verificar status da transação
12. **1️⃣2️⃣ Listar Transações** - Ver histórico de transações

### 🔗 FASE 5 - WEBHOOKS & INTEGRAÇÕES
13. **1️⃣3️⃣ Configurar Webhook** - Configurar notificações
14. **1️⃣4️⃣ Testar Integração** - Verificar integrações

### 🛠️ UTILITÁRIOS
- **🔍 Health Check** - Verificar se API Gateway está funcionando
- **📊 Status dos Serviços** - Status de todos os microserviços

## ✅ Testes Automáticos

Cada endpoint inclui testes automáticos que:
- ✅ Verificam status codes corretos
- ✅ Validam estrutura das respostas
- ✅ Salvam IDs automaticamente nas variáveis
- ✅ Verificam JWT com Issuer/Audience "Mortadela"
- ✅ Mostram logs informativos no console

## 🔧 Pré-requisitos

Certifique-se de que o sistema está rodando:

```bash
# Subir toda a stack
docker compose -f docker-compose-complete.yml up -d

# Verificar se os serviços estão rodando
docker ps | grep fintech
```

## 🎯 Endpoints Principais

| Serviço | Porta | Endpoint |
|---------|-------|----------|
| API Gateway | 5000 | http://localhost:5000 |
| AuthService | 5001 | http://localhost:5001 |
| UserService | 5002 | http://localhost:5002 |
| TransactionService | 5003 | http://localhost:5003 |
| BalanceService | 5004 | http://localhost:5004 |

## 🔐 Configuração JWT

A collection está configurada para usar:
- **Issuer**: "Mortadela"
- **Audience**: "Mortadela"
- **Key**: "mortadela-super-secret-key-that-should-be-at-least-256-bits-long-for-production"

## 📝 Logs e Debugging

Durante os testes, verifique:
1. **Console do Postman** - Logs automáticos dos testes
2. **Logs dos containers** - `docker logs fintech-api-gateway`
3. **Variáveis da collection** - Valores salvos automaticamente

## 🚨 Troubleshooting

### Erro 401 Unauthorized
- Execute primeiro o "1️⃣ Login Admin"
- Verifique se o token foi salvo na variável `access_token`

### Erro de conexão
- Verifique se os containers estão rodando
- Teste o Health Check primeiro

### Erro 404 Not Found
- Verifique se a rota existe no ocelot.json
- Confirme se o serviço de destino está funcionando

## 🎉 Resultado Esperado

Ao seguir toda a trilha, você terá:
- ✅ Autenticação funcionando com JWT Mortadela
- ✅ Cliente criado e gerenciado
- ✅ Conta bancária criada
- ✅ Transação PIX processada
- ✅ Webhook configurado
- ✅ Sistema completo testado e validado

---

**Versão**: 2.1.0  
**Última atualização**: 2025-09-24  
**JWT**: Mortadela (Issuer/Audience)
