# 🚀 **POSTMAN SCRIPTS AVANÇADOS - FINTECHPSP**

## 📋 **Scripts de Automação e Validação**

Este documento contém scripts avançados para automação de testes no Postman, incluindo validações complexas, geração de dados dinâmicos e fluxos automatizados.

---

## 🔧 **PRE-REQUEST SCRIPTS GLOBAIS**

### **🌐 Collection Pre-request Script**
```javascript
// Script executado antes de cada request na collection

// 1. Configurar variáveis globais
pm.globals.set("timestamp", Date.now());
pm.globals.set("uuid", pm.variables.replaceIn('{{$guid}}'));

// 2. Verificar se ambiente está configurado
const baseUrl = pm.environment.get("base_url");
if (!baseUrl) {
    throw new Error("Environment 'base_url' não configurado!");
}

// 3. Função para gerar CPF válido
function generateCPF() {
    const cpf = [];
    for (let i = 0; i < 9; i++) {
        cpf.push(Math.floor(Math.random() * 9));
    }
    
    // Calcular dígitos verificadores
    let sum = 0;
    for (let i = 0; i < 9; i++) {
        sum += cpf[i] * (10 - i);
    }
    cpf.push(11 - (sum % 11) > 9 ? 0 : 11 - (sum % 11));
    
    sum = 0;
    for (let i = 0; i < 10; i++) {
        sum += cpf[i] * (11 - i);
    }
    cpf.push(11 - (sum % 11) > 9 ? 0 : 11 - (sum % 11));
    
    return cpf.join('');
}

// 4. Função para gerar CNPJ válido
function generateCNPJ() {
    const cnpj = [];
    for (let i = 0; i < 12; i++) {
        cnpj.push(Math.floor(Math.random() * 9));
    }
    
    // Calcular dígitos verificadores
    const weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    const weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    
    let sum = 0;
    for (let i = 0; i < 12; i++) {
        sum += cnpj[i] * weights1[i];
    }
    cnpj.push(sum % 11 < 2 ? 0 : 11 - (sum % 11));
    
    sum = 0;
    for (let i = 0; i < 13; i++) {
        sum += cnpj[i] * weights2[i];
    }
    cnpj.push(sum % 11 < 2 ? 0 : 11 - (sum % 11));
    
    return cnpj.join('');
}

// 5. Disponibilizar funções globalmente
pm.globals.set("generateCPF", generateCPF.toString());
pm.globals.set("generateCNPJ", generateCNPJ.toString());

// 6. Configurar headers padrão
pm.request.headers.add({
    key: 'User-Agent',
    value: 'FintechPSP-Postman-Tests/1.0'
});

// 7. Log do request
console.log(`🚀 Executando: ${pm.request.method} ${pm.request.url}`);
```

---

## 🧪 **TEST SCRIPTS AVANÇADOS**

### **🔐 Validação de JWT Token**
```javascript
// Script para validar estrutura e conteúdo do JWT
pm.test("JWT Token Validation", function () {
    const response = pm.response.json();
    
    if (response.accessToken) {
        const token = response.accessToken;
        
        // Verificar estrutura do JWT (3 partes separadas por ponto)
        const parts = token.split('.');
        pm.expect(parts).to.have.lengthOf(3);
        
        // Decodificar header
        const header = JSON.parse(atob(parts[0]));
        pm.expect(header).to.have.property('alg');
        pm.expect(header).to.have.property('typ', 'JWT');
        
        // Decodificar payload
        const payload = JSON.parse(atob(parts[1]));
        pm.expect(payload).to.have.property('iss', 'Mortadela');
        pm.expect(payload).to.have.property('aud', 'Mortadela');
        pm.expect(payload).to.have.property('exp');
        
        // Verificar se token não está expirado
        const now = Math.floor(Date.now() / 1000);
        pm.expect(payload.exp).to.be.above(now);
        
        console.log('✅ JWT Token válido:', {
            issuer: payload.iss,
            audience: payload.aud,
            expiresAt: new Date(payload.exp * 1000),
            subject: payload.sub
        });
    }
});
```

### **💰 Validação de Valores Monetários**
```javascript
// Script para validar valores monetários e formatação
pm.test("Monetary Values Validation", function () {
    const response = pm.response.json();
    
    function validateMonetaryValue(value, fieldName) {
        // Verificar se é número
        pm.expect(value, `${fieldName} deve ser número`).to.be.a('number');
        
        // Verificar se é positivo
        pm.expect(value, `${fieldName} deve ser positivo`).to.be.above(0);
        
        // Verificar precisão decimal (máximo 2 casas)
        const decimalPlaces = (value.toString().split('.')[1] || '').length;
        pm.expect(decimalPlaces, `${fieldName} deve ter no máximo 2 casas decimais`).to.be.at.most(2);
        
        // Verificar valor máximo (R$ 1.000.000,00)
        pm.expect(value, `${fieldName} não pode exceder R$ 1.000.000,00`).to.be.at.most(1000000);
    }
    
    // Validar campos monetários se existirem
    if (response.amount) validateMonetaryValue(response.amount, 'amount');
    if (response.valor) validateMonetaryValue(response.valor, 'valor');
    if (response.balance) validateMonetaryValue(response.balance, 'balance');
    if (response.capitalSocial) validateMonetaryValue(response.capitalSocial, 'capitalSocial');
});
```

### **📱 Validação de PIX**
```javascript
// Script para validar dados PIX
pm.test("PIX Data Validation", function () {
    const response = pm.response.json();
    
    // Validar chave PIX
    if (response.pixKey || response.chavePix) {
        const pixKey = response.pixKey || response.chavePix;
        
        // Verificar se não está vazia
        pm.expect(pixKey).to.not.be.empty;
        
        // Validar formato baseado no tipo
        if (/^\d{11}$/.test(pixKey)) {
            // CPF
            pm.expect(pixKey).to.match(/^\d{11}$/);
            console.log('✅ Chave PIX: CPF válido');
        } else if (/^\d{14}$/.test(pixKey)) {
            // CNPJ
            pm.expect(pixKey).to.match(/^\d{14}$/);
            console.log('✅ Chave PIX: CNPJ válido');
        } else if (/^[\w\.-]+@[\w\.-]+\.\w+$/.test(pixKey)) {
            // Email
            pm.expect(pixKey).to.match(/^[\w\.-]+@[\w\.-]+\.\w+$/);
            console.log('✅ Chave PIX: Email válido');
        } else if (/^\+55\d{10,11}$/.test(pixKey)) {
            // Telefone
            pm.expect(pixKey).to.match(/^\+55\d{10,11}$/);
            console.log('✅ Chave PIX: Telefone válido');
        } else {
            // Chave aleatória (UUID)
            pm.expect(pixKey).to.match(/^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i);
            console.log('✅ Chave PIX: UUID válido');
        }
    }
    
    // Validar EMV Code
    if (response.emvCode || response.pixCopiaECola) {
        const emvCode = response.emvCode || response.pixCopiaECola;
        
        // Verificar comprimento (mínimo 200 caracteres)
        pm.expect(emvCode.length).to.be.at.least(200);
        
        // Verificar se contém apenas caracteres válidos
        pm.expect(emvCode).to.match(/^[0-9A-Z\s]+$/);
        
        // Verificar se começa com identificador PIX
        pm.expect(emvCode).to.match(/^00020126/);
        
        console.log('✅ EMV Code válido:', emvCode.length, 'caracteres');
    }
});
```

### **🔗 Validação de Webhooks**
```javascript
// Script para validar configuração de webhooks
pm.test("Webhook Configuration Validation", function () {
    const response = pm.response.json();
    
    if (response.url) {
        // Validar URL
        pm.expect(response.url).to.match(/^https?:\/\/.+/);
        
        // Verificar se é HTTPS em produção
        const environment = pm.environment.get("environment") || "test";
        if (environment === "production") {
            pm.expect(response.url).to.match(/^https:\/\/.+/);
        }
    }
    
    if (response.events) {
        // Validar eventos suportados
        const supportedEvents = [
            'pix.received', 'pix.sent', 'pix.failed', 'pix.refunded',
            'qr_code.created', 'qr_code.paid', 'qr_code.expired',
            'boleto.created', 'boleto.paid', 'boleto.expired',
            'transaction.confirmed', 'transaction.failed',
            'balance.updated', 'account.created'
        ];
        
        response.events.forEach(event => {
            pm.expect(supportedEvents).to.include(event);
        });
        
        console.log('✅ Eventos válidos:', response.events);
    }
    
    if (response.secret) {
        // Validar secret (mínimo 16 caracteres)
        pm.expect(response.secret.length).to.be.at.least(16);
        console.log('✅ Secret configurado com', response.secret.length, 'caracteres');
    }
});
```

---

## 🔄 **SCRIPTS DE FLUXO AUTOMATIZADO**

### **🎯 Setup Automático de Dados de Teste**
```javascript
// Pre-request script para setup automático
const setupTestData = {
    async createCompany() {
        const timestamp = Date.now();
        const cnpj = eval(pm.globals.get("generateCNPJ"))();
        
        const companyData = {
            razaoSocial: `Empresa Teste ${timestamp}`,
            cnpj: cnpj,
            email: `empresa${timestamp}@teste.com`,
            address: {
                cep: "01234-567",
                logradouro: "Rua Teste",
                numero: "123",
                bairro: "Centro",
                cidade: "São Paulo",
                estado: "SP"
            }
        };
        
        pm.environment.set("test_company_data", JSON.stringify(companyData));
        return companyData;
    },
    
    async createUser() {
        const timestamp = Date.now();
        const cpf = eval(pm.globals.get("generateCPF"))();
        
        const userData = {
            name: `Cliente Teste ${timestamp}`,
            email: `cliente${timestamp}@teste.com`,
            password: "123456",
            document: cpf,
            role: "cliente"
        };
        
        pm.environment.set("test_user_data", JSON.stringify(userData));
        return userData;
    }
};

// Executar setup se necessário
if (pm.request.name.includes("Setup")) {
    setupTestData.createCompany();
    setupTestData.createUser();
}
```

### **📊 Coleta de Métricas de Performance**
```javascript
// Test script para coletar métricas
pm.test("Performance Metrics", function () {
    const responseTime = pm.response.responseTime;
    const responseSize = pm.response.responseSize;
    
    // Definir SLAs
    const SLA_RESPONSE_TIME = 2000; // 2 segundos
    const SLA_RESPONSE_SIZE = 1048576; // 1MB
    
    // Validar SLAs
    pm.expect(responseTime).to.be.below(SLA_RESPONSE_TIME);
    pm.expect(responseSize).to.be.below(SLA_RESPONSE_SIZE);
    
    // Coletar métricas
    const metrics = {
        endpoint: pm.request.url.toString(),
        method: pm.request.method,
        responseTime: responseTime,
        responseSize: responseSize,
        statusCode: pm.response.code,
        timestamp: new Date().toISOString()
    };
    
    // Armazenar métricas
    const existingMetrics = JSON.parse(pm.environment.get("performance_metrics") || "[]");
    existingMetrics.push(metrics);
    pm.environment.set("performance_metrics", JSON.stringify(existingMetrics));
    
    console.log('📊 Métricas coletadas:', metrics);
});
```

### **🔍 Validação de Dados Sensíveis**
```javascript
// Script para verificar se dados sensíveis não estão expostos
pm.test("Sensitive Data Protection", function () {
    const responseText = pm.response.text();
    const response = pm.response.json();
    
    // Lista de campos sensíveis que não devem aparecer na resposta
    const sensitiveFields = [
        'password', 'senha', 'secret', 'secreto',
        'private_key', 'chave_privada', 'token_secret',
        'client_secret', 'api_secret', 'webhook_secret'
    ];
    
    // Verificar se campos sensíveis não estão presentes
    sensitiveFields.forEach(field => {
        pm.expect(response).to.not.have.property(field);
        pm.expect(responseText.toLowerCase()).to.not.include(field);
    });
    
    // Verificar se senhas não estão em texto plano
    if (response.passwordHash) {
        pm.expect(response.passwordHash).to.match(/^\$2[aby]\$\d+\$.{53}$/); // BCrypt format
    }
    
    // Verificar se CPF/CNPJ estão mascarados (se aplicável)
    if (response.document && response.document.length > 11) {
        // CNPJ deve estar mascarado: XX.XXX.XXX/XXXX-XX
        pm.expect(response.document).to.match(/^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$/);
    }
    
    console.log('🔒 Dados sensíveis protegidos');
});
```

---

## 🚀 **AUTOMAÇÃO COM NEWMAN (CLI)**

### **📜 Script de Execução Automatizada**
```bash
#!/bin/bash
# run-postman-tests.sh

echo "🚀 Iniciando testes automatizados do FintechPSP"

# Configurações
COLLECTION_FILE="postman/FintechPSP-Complete-Tests.json"
ENVIRONMENT_FILE="postman/environments/FintechPSP-Test.json"
REPORT_DIR="reports/postman"

# Criar diretório de relatórios
mkdir -p $REPORT_DIR

# Executar testes com Newman
newman run $COLLECTION_FILE \
  --environment $ENVIRONMENT_FILE \
  --reporters cli,html,json \
  --reporter-html-export $REPORT_DIR/report.html \
  --reporter-json-export $REPORT_DIR/results.json \
  --delay-request 1000 \
  --timeout-request 30000 \
  --insecure \
  --color on

# Verificar resultado
if [ $? -eq 0 ]; then
    echo "✅ Todos os testes passaram!"
    echo "📊 Relatório: $REPORT_DIR/report.html"
else
    echo "❌ Alguns testes falharam!"
    exit 1
fi
```

### **🐳 Docker para Testes**
```dockerfile
# Dockerfile.newman
FROM postman/newman:latest

# Copiar arquivos de teste
COPY postman/ /etc/newman/

# Script de execução
COPY scripts/run-tests.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/run-tests.sh

# Executar testes
CMD ["run-tests.sh"]
```

### **🔄 GitHub Actions Integration**
```yaml
# .github/workflows/postman-tests.yml
name: Postman API Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  api-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
    
    - name: Install Newman
      run: npm install -g newman newman-reporter-htmlextra
    
    - name: Start Test Environment
      run: |
        docker-compose -f docker-compose.test.yml up -d
        ./wait-for-services.sh
    
    - name: Run Postman Tests
      run: |
        newman run postman/FintechPSP-Complete-Tests.json \
          --environment postman/environments/FintechPSP-Test.json \
          --reporters cli,htmlextra \
          --reporter-htmlextra-export reports/postman-report.html
    
    - name: Upload Test Results
      uses: actions/upload-artifact@v3
      if: always()
      with:
        name: postman-test-results
        path: reports/
```

---

## 📊 **RELATÓRIOS E ANÁLISES**

### **📈 Script de Análise de Métricas**
```javascript
// Script para analisar métricas coletadas
const analyzeMetrics = () => {
    const metrics = JSON.parse(pm.environment.get("performance_metrics") || "[]");
    
    if (metrics.length === 0) {
        console.log("📊 Nenhuma métrica coletada");
        return;
    }
    
    // Calcular estatísticas
    const responseTimes = metrics.map(m => m.responseTime);
    const avgResponseTime = responseTimes.reduce((a, b) => a + b, 0) / responseTimes.length;
    const maxResponseTime = Math.max(...responseTimes);
    const minResponseTime = Math.min(...responseTimes);
    
    // Agrupar por endpoint
    const endpointStats = {};
    metrics.forEach(metric => {
        const endpoint = metric.endpoint;
        if (!endpointStats[endpoint]) {
            endpointStats[endpoint] = {
                count: 0,
                totalTime: 0,
                errors: 0
            };
        }
        
        endpointStats[endpoint].count++;
        endpointStats[endpoint].totalTime += metric.responseTime;
        if (metric.statusCode >= 400) {
            endpointStats[endpoint].errors++;
        }
    });
    
    // Gerar relatório
    console.log("📊 RELATÓRIO DE PERFORMANCE");
    console.log("============================");
    console.log(`Total de requests: ${metrics.length}`);
    console.log(`Tempo médio: ${avgResponseTime.toFixed(2)}ms`);
    console.log(`Tempo mínimo: ${minResponseTime}ms`);
    console.log(`Tempo máximo: ${maxResponseTime}ms`);
    console.log("");
    
    console.log("📈 POR ENDPOINT:");
    Object.entries(endpointStats).forEach(([endpoint, stats]) => {
        const avgTime = (stats.totalTime / stats.count).toFixed(2);
        const errorRate = ((stats.errors / stats.count) * 100).toFixed(1);
        console.log(`${endpoint}: ${avgTime}ms avg, ${errorRate}% errors`);
    });
};

// Executar análise no final da collection
if (pm.request.name === "Analyze Metrics") {
    analyzeMetrics();
}
```

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**🚀 Scripts**: Automação avançada
