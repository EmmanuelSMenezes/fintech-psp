# 🤖 **AUTOMAÇÃO DE TESTES - FINTECHPSP**

## 📋 **Estratégia de Automação**

### **🎯 Objetivos**
- ✅ Reduzir tempo de execução de testes
- ✅ Aumentar cobertura de testes
- ✅ Detectar regressões rapidamente
- ✅ Integrar com CI/CD pipeline
- ✅ Gerar relatórios automáticos

### **📊 Pirâmide de Testes**
```
        🔺 E2E Tests (10%)
       🔺🔺 Integration Tests (20%)
    🔺🔺🔺🔺 Unit Tests (70%)
```

---

## 🛠️ **Ferramentas de Automação**

### **🔧 Stack Tecnológico**

#### **API Testing**
```yaml
Postman/Newman:
  - Coleções de testes
  - Variáveis de ambiente
  - Scripts de validação
  - Relatórios HTML

RestAssured (Java):
  - Testes de API robustos
  - Validação de schema
  - Autenticação automática
  - Integração com TestNG

Playwright (Node.js):
  - Testes de API modernos
  - Suporte a TypeScript
  - Paralelização nativa
  - Relatórios detalhados
```

#### **UI Testing**
```yaml
Cypress:
  - Testes end-to-end
  - Real-time reloading
  - Time travel debugging
  - Screenshots/videos

Selenium WebDriver:
  - Cross-browser testing
  - Grid para paralelização
  - Múltiplas linguagens
  - Integração CI/CD

Playwright:
  - Multi-browser support
  - Auto-wait capabilities
  - Mobile testing
  - Visual comparisons
```

#### **Performance Testing**
```yaml
K6:
  - Load testing moderno
  - Scripts em JavaScript
  - Métricas detalhadas
  - Integração CI/CD

JMeter:
  - GUI e linha de comando
  - Protocolos múltiplos
  - Relatórios gráficos
  - Distribuído
```

---

## 📁 **Estrutura de Automação**

### **🗂️ Organização de Arquivos**
```
tests/
├── 📁 api/
│   ├── 📁 auth/
│   │   ├── 📄 login.spec.js
│   │   ├── 📄 oauth.spec.js
│   │   └── 📄 logout.spec.js
│   ├── 📁 users/
│   │   ├── 📄 crud.spec.js
│   │   └── 📄 permissions.spec.js
│   ├── 📁 transactions/
│   │   ├── 📄 pix.spec.js
│   │   ├── 📄 qrcode.spec.js
│   │   └── 📄 boleto.spec.js
│   └── 📁 integrations/
│       ├── 📄 sicoob.spec.js
│       └── 📄 health.spec.js
├── 📁 ui/
│   ├── 📁 backoffice/
│   │   ├── 📄 login.spec.js
│   │   ├── 📄 dashboard.spec.js
│   │   └── 📄 companies.spec.js
│   └── 📁 internetbanking/
│       ├── 📄 login.spec.js
│       ├── 📄 pix.spec.js
│       └── 📄 transactions.spec.js
├── 📁 e2e/
│   ├── 📄 complete-pix-flow.spec.js
│   ├── 📄 onboarding-flow.spec.js
│   └── 📄 webhook-flow.spec.js
├── 📁 performance/
│   ├── 📄 load-test.js
│   ├── 📄 stress-test.js
│   └── 📄 spike-test.js
├── 📁 utils/
│   ├── 📄 api-client.js
│   ├── 📄 test-data.js
│   ├── 📄 helpers.js
│   └── 📄 config.js
├── 📁 fixtures/
│   ├── 📄 users.json
│   ├── 📄 companies.json
│   └── 📄 transactions.json
└── 📁 reports/
    ├── 📁 html/
    ├── 📁 json/
    └── 📁 screenshots/
```

---

## 🔧 **Configuração das Ferramentas**

### **🚀 Playwright Setup**

#### **Instalação**
```bash
# Criar projeto de testes
mkdir fintechpsp-tests
cd fintechpsp-tests

# Inicializar projeto Node.js
npm init -y

# Instalar Playwright
npm install -D @playwright/test
npx playwright install

# Instalar dependências adicionais
npm install -D dotenv axios
```

#### **Configuração playwright.config.js**
```javascript
// playwright.config.js
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['json', { outputFile: 'reports/results.json' }],
    ['junit', { outputFile: 'reports/results.xml' }]
  ],
  use: {
    baseURL: 'http://localhost:5000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure'
  },
  projects: [
    {
      name: 'API Tests',
      testMatch: 'tests/api/**/*.spec.js',
      use: { ...devices['Desktop Chrome'] }
    },
    {
      name: 'UI Tests - Chrome',
      testMatch: 'tests/ui/**/*.spec.js',
      use: { ...devices['Desktop Chrome'] }
    },
    {
      name: 'UI Tests - Firefox',
      testMatch: 'tests/ui/**/*.spec.js',
      use: { ...devices['Desktop Firefox'] }
    },
    {
      name: 'E2E Tests',
      testMatch: 'tests/e2e/**/*.spec.js',
      use: { ...devices['Desktop Chrome'] }
    }
  ],
  webServer: {
    command: 'npm run start:test',
    port: 5000,
    reuseExistingServer: !process.env.CI
  }
});
```

### **📝 Exemplo de Teste API**

#### **tests/api/auth/login.spec.js**
```javascript
import { test, expect } from '@playwright/test';
import { ApiClient } from '../../utils/api-client.js';

test.describe('AuthService - Login', () => {
  let apiClient;

  test.beforeEach(async () => {
    apiClient = new ApiClient();
  });

  test('TC-AUTH-001: Login com credenciais válidas', async () => {
    // Arrange
    const loginData = {
      email: 'admin@fintechpsp.com',
      password: 'admin123'
    };

    // Act
    const response = await apiClient.post('/auth/login', loginData);

    // Assert
    expect(response.status()).toBe(200);
    
    const responseBody = await response.json();
    expect(responseBody).toHaveProperty('accessToken');
    expect(responseBody).toHaveProperty('tokenType', 'Bearer');
    expect(responseBody).toHaveProperty('expiresIn', 3600);
    expect(responseBody.user).toHaveProperty('email', loginData.email);
    expect(responseBody.user).toHaveProperty('role', 'Admin');

    // Validar JWT token
    expect(responseBody.accessToken).toMatch(/^eyJ[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$/);
  });

  test('TC-AUTH-002: Login com credenciais inválidas', async () => {
    // Arrange
    const loginData = {
      email: 'admin@fintechpsp.com',
      password: 'senhaerrada'
    };

    // Act
    const response = await apiClient.post('/auth/login', loginData);

    // Assert
    expect(response.status()).toBe(401);
    
    const responseBody = await response.json();
    expect(responseBody).toHaveProperty('message');
    expect(responseBody.message).toContain('Credenciais inválidas');
    expect(responseBody).not.toHaveProperty('accessToken');
  });

  test('TC-AUTH-003: Login com dados inválidos', async () => {
    // Arrange
    const invalidData = [
      { email: '', password: 'admin123' },
      { email: 'admin@fintechpsp.com', password: '' },
      { email: 'email-invalido', password: 'admin123' },
      {}
    ];

    // Act & Assert
    for (const data of invalidData) {
      const response = await apiClient.post('/auth/login', data);
      expect(response.status()).toBe(400);
    }
  });
});
```

### **🖥️ Exemplo de Teste UI**

#### **tests/ui/backoffice/login.spec.js**
```javascript
import { test, expect } from '@playwright/test';

test.describe('BackofficeWeb - Login', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('http://localhost:3000');
  });

  test('TC-UI-001: Login admin bem-sucedido', async ({ page }) => {
    // Arrange
    await expect(page).toHaveTitle(/Fintech PSP/);
    
    // Act
    await page.fill('[data-testid="email-input"]', 'admin@fintechpsp.com');
    await page.fill('[data-testid="password-input"]', 'admin123');
    await page.click('[data-testid="login-button"]');

    // Assert
    await expect(page).toHaveURL(/.*dashboard/);
    await expect(page.locator('[data-testid="user-menu"]')).toBeVisible();
    await expect(page.locator('[data-testid="sidebar"]')).toBeVisible();
    
    // Verificar dados do usuário
    await page.click('[data-testid="user-menu"]');
    await expect(page.locator('text=admin@fintechpsp.com')).toBeVisible();
  });

  test('TC-UI-002: Login com credenciais inválidas', async ({ page }) => {
    // Act
    await page.fill('[data-testid="email-input"]', 'admin@fintechpsp.com');
    await page.fill('[data-testid="password-input"]', 'senhaerrada');
    await page.click('[data-testid="login-button"]');

    // Assert
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText('Credenciais inválidas');
    await expect(page).toHaveURL(/.*login/);
  });

  test('TC-UI-003: Validação de campos obrigatórios', async ({ page }) => {
    // Act - tentar login sem preencher campos
    await page.click('[data-testid="login-button"]');

    // Assert
    await expect(page.locator('[data-testid="email-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-error"]')).toBeVisible();
  });
});
```

### **🔄 Exemplo de Teste E2E**

#### **tests/e2e/complete-pix-flow.spec.js**
```javascript
import { test, expect } from '@playwright/test';
import { ApiClient } from '../utils/api-client.js';

test.describe('E2E - Fluxo Completo PIX', () => {
  let apiClient;
  let adminToken;
  let clientToken;

  test.beforeAll(async () => {
    apiClient = new ApiClient();
    
    // Login admin
    const adminLogin = await apiClient.post('/auth/login', {
      email: 'admin@fintechpsp.com',
      password: 'admin123'
    });
    adminToken = (await adminLogin.json()).accessToken;
  });

  test('TC-E2E-001: Fluxo completo de criação e uso de QR Code PIX', async ({ page }) => {
    // 1. Admin cria empresa
    const companyData = {
      razaoSocial: 'Empresa E2E Teste Ltda',
      cnpj: '12.345.678/0001-99',
      email: 'e2e@teste.com'
    };
    
    const companyResponse = await apiClient.post('/companies', companyData, {
      headers: { Authorization: `Bearer ${adminToken}` }
    });
    expect(companyResponse.status()).toBe(201);
    const company = await companyResponse.json();

    // 2. Admin aprova empresa
    const approveResponse = await apiClient.put(`/companies/${company.id}/approve`, {}, {
      headers: { Authorization: `Bearer ${adminToken}` }
    });
    expect(approveResponse.status()).toBe(200);

    // 3. Admin cria usuário cliente
    const userData = {
      name: 'Cliente E2E',
      email: 'cliente.e2e@teste.com',
      password: '123456',
      role: 'cliente',
      isActive: true
    };
    
    const userResponse = await apiClient.post('/admin/users', userData, {
      headers: { Authorization: `Bearer ${adminToken}` }
    });
    expect(userResponse.status()).toBe(201);

    // 4. Cliente faz login
    const clientLogin = await apiClient.post('/auth/login', {
      email: 'cliente.e2e@teste.com',
      password: '123456'
    });
    expect(clientLogin.status()).toBe(200);
    clientToken = (await clientLogin.json()).accessToken;

    // 5. Cliente acessa InternetBankingWeb
    await page.goto('http://localhost:3001');
    await page.fill('[data-testid="email-input"]', 'cliente.e2e@teste.com');
    await page.fill('[data-testid="password-input"]', '123456');
    await page.click('[data-testid="login-button"]');
    
    await expect(page).toHaveURL(/.*dashboard/);

    // 6. Cliente navega para PIX
    await page.click('[data-testid="pix-menu"]');
    await expect(page).toHaveURL(/.*pix/);

    // 7. Cliente gera QR Code dinâmico
    await page.click('[data-testid="generate-qr-button"]');
    await page.fill('[data-testid="amount-input"]', '100.50');
    await page.fill('[data-testid="description-input"]', 'Teste E2E QR Code');
    await page.click('[data-testid="create-qr-button"]');

    // 8. Verificar QR Code gerado
    await expect(page.locator('[data-testid="qr-code-image"]')).toBeVisible();
    await expect(page.locator('[data-testid="pix-copy-paste"]')).toBeVisible();
    
    const pixCopyPaste = await page.locator('[data-testid="pix-copy-paste"]').textContent();
    expect(pixCopyPaste).toHaveLength(248); // EMV code padrão

    // 9. Verificar transação criada via API
    const transactionsResponse = await apiClient.get('/transacoes/pix', {
      headers: { Authorization: `Bearer ${clientToken}` }
    });
    expect(transactionsResponse.status()).toBe(200);
    
    const transactions = await transactionsResponse.json();
    expect(transactions.length).toBeGreaterThan(0);
    
    const lastTransaction = transactions[0];
    expect(lastTransaction.amount).toBe(100.50);
    expect(lastTransaction.description).toBe('Teste E2E QR Code');
    expect(lastTransaction.status).toBe('PENDING');

    // 10. Verificar integração com Sicoob
    const integrationResponse = await apiClient.get('/integrations/health');
    expect(integrationResponse.status()).toBe(200);
    
    const healthData = await integrationResponse.json();
    expect(healthData.sicoob.isHealthy).toBe(true);
  });
});
```

---

## 🔄 **Integração CI/CD**

### **🚀 GitHub Actions**

#### **.github/workflows/tests.yml**
```yaml
name: Automated Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  unit-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Run Unit Tests
        run: |
          dotnet test --configuration Release --logger trx --collect:"XPlat Code Coverage"
      
      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        with:
          name: unit-test-results
          path: TestResults/

  integration-tests:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
      
      redis:
        image: redis:7
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install Playwright
        run: |
          npm ci
          npx playwright install --with-deps
      
      - name: Start Services
        run: |
          docker-compose -f docker-compose.test.yml up -d
          ./wait-for-services.sh
      
      - name: Run API Tests
        run: npx playwright test tests/api/
      
      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: api-test-results
          path: |
            test-results/
            playwright-report/

  e2e-tests:
    runs-on: ubuntu-latest
    needs: [unit-tests, integration-tests]
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '18'
      
      - name: Install Playwright
        run: |
          npm ci
          npx playwright install --with-deps
      
      - name: Start Full Environment
        run: |
          ./setup-test-environment.sh
      
      - name: Run E2E Tests
        run: npx playwright test tests/e2e/
      
      - name: Upload Test Results
        uses: actions/upload-artifact@v3
        if: always()
        with:
          name: e2e-test-results
          path: |
            test-results/
            playwright-report/
            screenshots/
            videos/

  performance-tests:
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup K6
        run: |
          sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
          echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
          sudo apt-get update
          sudo apt-get install k6
      
      - name: Start Environment
        run: ./setup-test-environment.sh
      
      - name: Run Performance Tests
        run: k6 run tests/performance/load-test.js
      
      - name: Upload Performance Results
        uses: actions/upload-artifact@v3
        with:
          name: performance-results
          path: performance-results/
```

---

## 📊 **Relatórios e Métricas**

### **📈 Dashboard de Qualidade**

#### **Métricas Coletadas**
```yaml
Cobertura de Testes:
  - Unit Tests: 80%+
  - Integration Tests: 70%+
  - E2E Tests: 50%+
  - Overall Coverage: 75%+

Performance:
  - API Response Time: < 2s
  - UI Load Time: < 3s
  - E2E Test Duration: < 30min
  - Test Suite Duration: < 45min

Qualidade:
  - Test Pass Rate: 98%+
  - Flaky Test Rate: < 2%
  - Bug Detection Rate: 95%+
  - Regression Detection: 100%
```

#### **Relatórios Automáticos**
```javascript
// generate-report.js
const fs = require('fs');
const path = require('path');

class TestReportGenerator {
  generateHtmlReport(results) {
    const template = `
    <!DOCTYPE html>
    <html>
    <head>
      <title>FintechPSP - Test Report</title>
      <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .summary { background: #f5f5f5; padding: 20px; border-radius: 5px; }
        .passed { color: green; }
        .failed { color: red; }
        .skipped { color: orange; }
        table { width: 100%; border-collapse: collapse; margin: 20px 0; }
        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
        th { background-color: #f2f2f2; }
      </style>
    </head>
    <body>
      <h1>FintechPSP - Test Execution Report</h1>
      
      <div class="summary">
        <h2>Summary</h2>
        <p><strong>Total Tests:</strong> ${results.total}</p>
        <p><strong class="passed">Passed:</strong> ${results.passed}</p>
        <p><strong class="failed">Failed:</strong> ${results.failed}</p>
        <p><strong class="skipped">Skipped:</strong> ${results.skipped}</p>
        <p><strong>Success Rate:</strong> ${results.successRate}%</p>
        <p><strong>Duration:</strong> ${results.duration}</p>
      </div>
      
      <h2>Test Results by Module</h2>
      <table>
        <thead>
          <tr>
            <th>Module</th>
            <th>Total</th>
            <th>Passed</th>
            <th>Failed</th>
            <th>Success Rate</th>
          </tr>
        </thead>
        <tbody>
          ${results.modules.map(module => `
            <tr>
              <td>${module.name}</td>
              <td>${module.total}</td>
              <td class="passed">${module.passed}</td>
              <td class="failed">${module.failed}</td>
              <td>${module.successRate}%</td>
            </tr>
          `).join('')}
        </tbody>
      </table>
      
      <h2>Failed Tests</h2>
      ${results.failedTests.length > 0 ? `
        <table>
          <thead>
            <tr>
              <th>Test Case</th>
              <th>Module</th>
              <th>Error Message</th>
            </tr>
          </thead>
          <tbody>
            ${results.failedTests.map(test => `
              <tr>
                <td>${test.name}</td>
                <td>${test.module}</td>
                <td>${test.error}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>
      ` : '<p>No failed tests! 🎉</p>'}
      
      <footer>
        <p>Generated on: ${new Date().toISOString()}</p>
        <p>Environment: ${process.env.NODE_ENV || 'test'}</p>
      </footer>
    </body>
    </html>
    `;
    
    return template;
  }
}
```

---

## 🚀 **Scripts de Execução**

### **📜 package.json**
```json
{
  "name": "fintechpsp-tests",
  "version": "1.0.0",
  "scripts": {
    "test": "playwright test",
    "test:api": "playwright test tests/api/",
    "test:ui": "playwright test tests/ui/",
    "test:e2e": "playwright test tests/e2e/",
    "test:smoke": "playwright test --grep @smoke",
    "test:regression": "playwright test --grep @regression",
    "test:parallel": "playwright test --workers=4",
    "test:headed": "playwright test --headed",
    "test:debug": "playwright test --debug",
    "report": "playwright show-report",
    "setup": "./setup-test-environment.sh",
    "cleanup": "./cleanup-test-environment.sh"
  }
}
```

### **🔧 Makefile**
```makefile
# Makefile para automação de testes

.PHONY: setup test clean report

setup:
	@echo "🚀 Configurando ambiente de testes..."
	./setup-test-environment.sh

test-unit:
	@echo "🧪 Executando testes unitários..."
	dotnet test --configuration Release

test-api:
	@echo "🔌 Executando testes de API..."
	npm run test:api

test-ui:
	@echo "🖥️ Executando testes de UI..."
	npm run test:ui

test-e2e:
	@echo "🔄 Executando testes E2E..."
	npm run test:e2e

test-all: test-unit test-api test-ui test-e2e
	@echo "✅ Todos os testes executados!"

test-smoke:
	@echo "💨 Executando smoke tests..."
	npm run test:smoke

test-regression:
	@echo "🔄 Executando testes de regressão..."
	npm run test:regression

report:
	@echo "📊 Gerando relatórios..."
	npm run report
	node generate-report.js

clean:
	@echo "🧹 Limpando ambiente..."
	./cleanup-test-environment.sh

ci: setup test-all report clean
	@echo "🎉 Pipeline CI executado com sucesso!"
```

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**🤖 Automação**: Completa
