# 📮 **GUIA DE USO - POSTMAN COLLECTIONS FINTECHPSP**

## 🎯 **Visão Geral**

Este guia fornece instruções detalhadas para usar as collections Postman do FintechPSP, cobrindo desde a configuração inicial até a execução de testes avançados.

---

## 📁 **Arquivos Disponíveis**

### **Collections**
- `postman/FintechPSP-Testes-Completos.json` - Collection principal com todos os endpoints
- `postman/FintechPSP-Transacoes-Cliente.json` - Collection original (compatível com InternetBankingWeb)

### **Environments**
- `postman/environments/FintechPSP-Test.postman_environment.json` - Environment de teste

### **Documentação**
- `Documentacao/TESTES-POSTMAN-PASSO-A-PASSO.md` - Guia detalhado de testes
- `Documentacao/POSTMAN-SCRIPTS-AVANCADOS.md` - Scripts de automação

---

## 🚀 **Setup Inicial**

### **1️⃣ Importar Collection e Environment**

#### **No Postman Desktop:**
```bash
1. Abrir Postman
2. File → Import
3. Selecionar arquivos:
   - postman/FintechPSP-Testes-Completos.json
   - postman/environments/FintechPSP-Test.postman_environment.json
4. Confirmar importação
```

#### **Via Postman CLI (Newman):**
```bash
# Instalar Newman
npm install -g newman

# Executar collection
newman run postman/FintechPSP-Testes-Completos.json \
  --environment postman/environments/FintechPSP-Test.postman_environment.json \
  --reporters cli,html \
  --reporter-html-export reports/postman-report.html
```

### **2️⃣ Configurar Environment**

#### **Selecionar Environment:**
1. No canto superior direito do Postman
2. Selecionar "FintechPSP-Test"
3. Verificar se `base_url` está configurado como `http://localhost:5000`

#### **Variáveis Importantes:**
```json
{
  "base_url": "http://localhost:5000",
  "admin_email": "admin@fintechpsp.com",
  "admin_password": "admin123",
  "client_password": "123456",
  "oauth_client_id": "cliente_banking",
  "oauth_client_secret": "cliente_secret_000"
}
```

---

## 🔄 **Fluxos de Teste**

### **📋 Fluxo 1: BackofficeWeb (Administração)**

#### **Sequência Recomendada:**
```
1. 1.1 - Login Admin
2. 1.2 - Listar Empresas
3. 1.3 - Criar Empresa
4. 1.4 - Aprovar Empresa
5. 1.5 - Criar Usuário Cliente
```

#### **Resultado Esperado:**
- ✅ Admin autenticado com token válido
- ✅ Empresa criada e aprovada
- ✅ Cliente criado com senha
- ✅ Todas as variáveis preenchidas automaticamente

### **📋 Fluxo 2: InternetBankingWeb (Cliente)**

#### **Pré-requisitos:**
- Fluxo 1 executado com sucesso
- Cliente criado via endpoint `/admin/users`

#### **Sequência Recomendada:**
```
1. 2.1 - Login Cliente
2. 2.2 - Dados do Cliente (/client-users/me)
3. 2.3 - Gerar QR Code PIX Dinâmico
4. 2.4 - Listar Transações PIX
```

#### **Resultado Esperado:**
- ✅ Cliente autenticado com token válido
- ✅ Dados do cliente retornados corretamente
- ✅ QR Code PIX gerado com EMV válido
- ✅ Lista de transações retornada

### **📋 Fluxo 3: APIs de Cliente Externo**

#### **Sequência Recomendada:**
```
1. 3.1 - OAuth 2.0 Client Credentials
2. 3.2 - Criar Transação PIX (Cliente Externo)
```

#### **Resultado Esperado:**
- ✅ Token OAuth obtido via client_credentials
- ✅ Transação PIX criada via API externa
- ✅ Webhook configurado (se aplicável)

---

## 🧪 **Validações Automáticas**

### **🔐 Validações de Segurança**
- ✅ JWT tokens válidos (estrutura, issuer, audience)
- ✅ Dados sensíveis não expostos (passwords, secrets)
- ✅ Tokens automaticamente extraídos e reutilizados

### **💰 Validações de Negócio**
- ✅ Valores monetários válidos (positivos, máximo 2 casas decimais)
- ✅ Chaves PIX válidas (CPF, CNPJ, email, telefone, UUID)
- ✅ EMV codes válidos (mínimo 200 caracteres, formato correto)

### **📊 Validações de Performance**
- ✅ Tempo de resposta < 5 segundos
- ✅ Métricas coletadas automaticamente
- ✅ Relatórios de performance gerados

---

## 🔧 **Troubleshooting**

### **❌ Erro 401 - Unauthorized**

#### **Possíveis Causas:**
1. Token expirado ou inválido
2. Credenciais incorretas
3. Usuário não existe ou está inativo

#### **Soluções:**
```bash
# 1. Verificar credenciais
GET {{base_url}}/auth/login
Body: {"email": "admin@fintechpsp.com", "password": "admin123"}

# 2. Reexecutar login
Execute: "1.1 - Login Admin" ou "2.1 - Login Cliente"

# 3. Verificar se usuário existe
GET {{base_url}}/admin/users (com token admin)
```

### **❌ Erro 404 - Not Found**

#### **Possíveis Causas:**
1. Endpoint não existe
2. ID de recurso inválido
3. Serviço não está rodando

#### **Soluções:**
```bash
# 1. Verificar se serviços estão rodando
docker ps

# 2. Verificar health check
GET {{base_url}}/health

# 3. Verificar variáveis
Verificar se {{company_id}}, {{user_id}} estão preenchidas
```

### **❌ Erro 500 - Internal Server Error**

#### **Possíveis Causas:**
1. Erro no banco de dados
2. Serviço indisponível
3. Dados inválidos no request

#### **Soluções:**
```bash
# 1. Verificar logs dos serviços
docker logs fintech-auth-service --tail 20
docker logs fintech-user-service --tail 20

# 2. Verificar banco de dados
docker logs fintech-postgres --tail 20

# 3. Validar dados do request
Verificar se JSON está válido e campos obrigatórios preenchidos
```

---

## 📊 **Relatórios e Métricas**

### **📈 Métricas Coletadas Automaticamente**
```javascript
// Cada request coleta:
{
  "endpoint": "http://localhost:5000/auth/login",
  "method": "POST",
  "responseTime": 245,
  "statusCode": 200,
  "timestamp": "2025-10-08T12:00:00.000Z"
}
```

### **📋 Visualizar Métricas**
```javascript
// No Console do Postman (após executar collection):
const metrics = JSON.parse(pm.environment.get("performance_metrics"));
console.log("Total requests:", metrics.length);

const avgTime = metrics.reduce((sum, m) => sum + m.responseTime, 0) / metrics.length;
console.log("Tempo médio:", avgTime.toFixed(2), "ms");
```

### **📄 Gerar Relatório HTML**
```bash
# Via Newman
newman run postman/FintechPSP-Testes-Completos.json \
  --environment postman/environments/FintechPSP-Test.postman_environment.json \
  --reporters htmlextra \
  --reporter-htmlextra-export reports/detailed-report.html \
  --reporter-htmlextra-template dashboard
```

---

## 🔄 **Automação e CI/CD**

### **🐳 Docker para Testes**
```dockerfile
# Dockerfile.postman-tests
FROM postman/newman:latest

COPY postman/ /etc/newman/postman/
COPY scripts/run-postman-tests.sh /usr/local/bin/

RUN chmod +x /usr/local/bin/run-postman-tests.sh

CMD ["run-postman-tests.sh"]
```

### **🚀 GitHub Actions**
```yaml
# .github/workflows/api-tests.yml
name: API Tests
on: [push, pull_request]

jobs:
  postman-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install Newman
        run: npm install -g newman newman-reporter-htmlextra
      
      - name: Start Services
        run: docker-compose up -d
      
      - name: Wait for Services
        run: ./scripts/wait-for-services.sh
      
      - name: Run Postman Tests
        run: |
          newman run postman/FintechPSP-Testes-Completos.json \
            --environment postman/environments/FintechPSP-Test.postman_environment.json \
            --reporters cli,htmlextra \
            --reporter-htmlextra-export reports/api-test-report.html
      
      - name: Upload Results
        uses: actions/upload-artifact@v3
        with:
          name: postman-test-results
          path: reports/
```

---

## 📚 **Recursos Adicionais**

### **🔗 Links Úteis**
- [Postman Documentation](https://learning.postman.com/)
- [Newman CLI](https://github.com/postmanlabs/newman)
- [Postman Scripts](https://learning.postman.com/docs/writing-scripts/)

### **📖 Documentação Relacionada**
- `Documentacao/TESTES-POSTMAN-PASSO-A-PASSO.md` - Testes detalhados
- `Documentacao/POSTMAN-SCRIPTS-AVANCADOS.md` - Scripts de automação
- `Documentacao/PLANO-TESTES-QA-FUNCIONAL.md` - Estratégia de testes

### **🎯 Próximos Passos**
1. **Executar fluxo básico** (BackofficeWeb → InternetBankingWeb)
2. **Configurar automação** (Newman + CI/CD)
3. **Personalizar validações** (scripts específicos do projeto)
4. **Integrar com monitoramento** (métricas em produção)

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 2.0.0  
**📮 Collections**: Prontas para uso
