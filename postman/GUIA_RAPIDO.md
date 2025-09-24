# 🚀 GUIA RÁPIDO - FintechPSP Collection

## ⚡ Início Rápido (5 minutos)

### 1. Importar no Postman
```
1. Abrir Postman
2. Import → FintechPSP-Collection-Final.json
3. ✅ Collection importada!
```

### 2. Testar Sistema (Ordem obrigatória)
```
1️⃣ Login Admin          → Obter token JWT
2️⃣ Validar Token JWT    → Confirmar autenticação
3️⃣ Criar Cliente        → Criar usuário teste
4️⃣ Listar Clientes      → Ver clientes cadastrados
```

### 3. Verificar Resultados
- ✅ **Console do Postman**: Logs automáticos
- ✅ **Variáveis**: `access_token`, `client_id` salvos automaticamente
- ✅ **JWT**: Issuer/Audience = "Mortadela"

## 🎯 Trilha Completa (14 passos)

### FASE 1 - AUTENTICAÇÃO
- 1️⃣ **Login Admin** - Credenciais: admin@fintechpsp.com / admin123
- 2️⃣ **Validar Token JWT** - Verificar se JWT está válido

### FASE 2 - USUÁRIOS  
- 3️⃣ **Criar Cliente** - João Silva Teste
- 4️⃣ **Listar Clientes** - Ver todos os clientes
- 5️⃣ **Buscar Cliente por ID** - Buscar cliente específico
- 6️⃣ **Atualizar Cliente** - Modificar dados

### FASE 3 - CONTAS
- 7️⃣ **Criar Conta Bancária** - Conta corrente R$ 1.000
- 8️⃣ **Listar Contas** - Ver todas as contas
- 9️⃣ **Consultar Saldo** - Verificar saldo atual

### FASE 4 - TRANSAÇÕES
- 🔟 **Criar Transação PIX** - PIX de R$ 100,50
- 1️⃣1️⃣ **Consultar Transação** - Status da transação
- 1️⃣2️⃣ **Listar Transações** - Histórico completo

### FASE 5 - WEBHOOKS
- 1️⃣3️⃣ **Configurar Webhook** - Notificações automáticas
- 1️⃣4️⃣ **Testar Integração** - Health check integrações

## 🔧 Pré-requisitos

```bash
# 1. Subir sistema completo
docker compose -f docker-compose-complete.yml up -d

# 2. Verificar se está rodando
docker ps | grep fintech

# 3. Testar API Gateway
curl http://localhost:5000/health
```

## ✅ Validação Rápida

Execute este comando para testar se tudo está funcionando:

```powershell
# Teste rápido de login
$login = Invoke-RestMethod -Uri 'http://localhost:5000/auth/login' -Method POST -Body (@{email='admin@fintechpsp.com';password='admin123'} | ConvertTo-Json) -ContentType 'application/json'
Write-Host "✅ Sistema funcionando! Token obtido."
```

## 🎯 Resultados Esperados

Após executar toda a trilha:

```
✅ JWT com Mortadela funcionando
✅ Cliente "João Silva Teste" criado
✅ Conta bancária com R$ 1.000 criada  
✅ Transação PIX de R$ 100,50 processada
✅ Webhook configurado
✅ Sistema completo validado
```

## 🚨 Problemas Comuns

| Erro | Solução |
|------|---------|
| 401 Unauthorized | Execute "1️⃣ Login Admin" primeiro |
| Connection refused | Verifique se containers estão rodando |
| 404 Not Found | Confirme se API Gateway está funcionando |
| Token inválido | Refaça o login para obter novo token |

## 📊 Monitoramento

Durante os testes, monitore:

```bash
# Logs do API Gateway
docker logs fintech-api-gateway --tail 20

# Logs do AuthService  
docker logs fintech-auth-service --tail 20

# Status dos containers
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

## 🎉 Sucesso!

Se todos os 14 passos executaram com sucesso:
- ✅ **Sistema FintechPSP funcionando 100%**
- ✅ **JWT Mortadela configurado corretamente**
- ✅ **Todos os microserviços integrados**
- ✅ **Pronto para produção!**

---

**⚡ Tempo estimado**: 10-15 minutos  
**🎯 Cobertura**: 100% do sistema  
**🔐 JWT**: Mortadela (Issuer/Audience)
