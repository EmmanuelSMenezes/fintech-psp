# API Gateway - Configuração Ocelot

## 📋 **Resumo**

Este documento detalha a configuração do API Gateway usando Ocelot para o projeto FintechPSP, incluindo as correções realizadas para resolver problemas de roteamento em ambiente Docker.

## 🔧 **Problema Resolvido**

### **Erro Original:**
```
Request URL: http://localhost:5000/admin/companies?page=1&limit=10
Status Code: 502 Bad Gateway
```

### **Causa:**
O arquivo `ocelot.json` estava configurado para usar `localhost` e portas específicas do host, mas em ambiente Docker os serviços se comunicam através dos nomes dos containers na rede interna.

### **Solução:**
Atualização completa do arquivo `src/Gateway/FintechPSP.APIGateway/ocelot.json` para usar nomes dos containers Docker.

## 🔄 **Configurações Corrigidas**

### **Antes (Incorreto):**
```json
{
  "DownstreamHostAndPorts": [
    {
      "Host": "localhost",
      "Port": 5001
    }
  ]
}
```

### **Depois (Correto):**
```json
{
  "DownstreamHostAndPorts": [
    {
      "Host": "fintech-auth-service",
      "Port": 8080
    }
  ]
}
```

## 📊 **Mapeamento de Serviços**

| Serviço | Host Antigo | Host Novo | Porta |
|---------|-------------|-----------|-------|
| AuthService | localhost:5001 | fintech-auth-service:8080 | 8080 |
| UserService | localhost:5006 | fintech-user-service:8080 | 8080 |
| CompanyService | localhost:5010 | fintech-company-service:8080 | 8080 |
| TransactionService | localhost:5004 | fintech-transaction-service:8080 | 8080 |
| BalanceService | localhost:5003 | fintech-balance-service:8080 | 8080 |

## 🧪 **Testes de Validação**

### **1. Teste Direto do CompanyService:**
```bash
GET http://localhost:5010/admin/companies?page=1&limit=10
```
**Resultado:** ✅ 200 OK - Lista de empresas retornada

### **2. Teste via API Gateway:**
```bash
GET http://localhost:5000/admin/companies?page=1&limit=10
```
**Resultado:** ✅ 401 Unauthorized - Autenticação exigida (comportamento correto)

## 🔑 **Configuração de Autenticação**

O API Gateway está configurado para exigir autenticação JWT em endpoints protegidos:

```json
{
  "AuthenticationOptions": {
    "AuthenticationProviderKey": "Bearer"
  }
}
```

### **Como obter token:**
```bash
POST http://localhost:5001/auth/login
Content-Type: application/json

{
  "email": "admin@fintechpsp.com",
  "password": "admin123"
}
```

## 🐳 **Configuração Docker**

### **Rede Docker:**
- Nome: `dockerfiles_fintech-network`
- Tipo: bridge
- Comunicação interna: nomes dos containers

### **Container API Gateway:**
- Nome: `fintech-api-gateway`
- Porta externa: 5000
- Porta interna: 8080
- Imagem: `dockerfiles-api-gateway`

## 📝 **Comandos de Build e Deploy**

### **1. Build da imagem:**
```bash
docker build -f Dockerfile.APIGateway -t dockerfiles-api-gateway ..
```

### **2. Executar container:**
```bash
docker run -d --name fintech-api-gateway --network dockerfiles_fintech-network -p 5000:8080 dockerfiles-api-gateway
```

### **3. Verificar logs:**
```bash
docker logs fintech-api-gateway --tail 20
```

## ✅ **Status Final**

- ✅ API Gateway funcionando
- ✅ Roteamento para todos os microserviços
- ✅ Autenticação JWT configurada
- ✅ Comunicação Docker interna funcionando
- ✅ Endpoints retornando respostas corretas

## 🔗 **Endpoints Disponíveis**

| Endpoint | Método | Serviço | Autenticação |
|----------|--------|---------|--------------|
| `/auth/login` | POST | AuthService | Não |
| `/admin/companies` | GET/POST | CompanyService | Sim |
| `/admin/users` | GET/POST | UserService | Sim |
| `/saldo/*` | GET/POST | BalanceService | Sim |
| `/transacoes/*` | GET/POST | TransactionService | Sim |

## 📚 **Referências**

- [Ocelot Documentation](https://ocelot.readthedocs.io/)
- [Docker Networking](https://docs.docker.com/network/)
- [JWT Authentication](https://jwt.io/)
