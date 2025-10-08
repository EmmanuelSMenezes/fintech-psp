# 📋 **CASOS DE TESTE DETALHADOS - FINTECHPSP**

## 🎯 **Matriz de Casos de Teste**

### **📊 Resumo por Módulo**

| Módulo | Casos Funcionais | Casos Negativos | Casos E2E | Total |
|--------|------------------|-----------------|-----------|-------|
| AuthService | 5 | 3 | 1 | 9 |
| UserService | 6 | 4 | 1 | 11 |
| TransactionService | 8 | 5 | 2 | 15 |
| BalanceService | 4 | 3 | 1 | 8 |
| CompanyService | 5 | 3 | 1 | 9 |
| ConfigService | 4 | 2 | 1 | 7 |
| IntegrationService | 6 | 4 | 2 | 12 |
| WebhookService | 4 | 3 | 1 | 8 |
| BackofficeWeb | 8 | 4 | 2 | 14 |
| InternetBankingWeb | 6 | 3 | 2 | 11 |
| **TOTAL** | **56** | **34** | **14** | **104** |

---

## 🔐 **AUTHSERVICE - CASOS DETALHADOS**

### **TC-AUTH-001: Login Usuário Válido**
```yaml
ID: TC-AUTH-001
Módulo: AuthService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Sistema funcionando
  - Usuário cadastrado: admin@fintechpsp.com / admin123

Passos:
  1. Enviar POST /auth/login
     Body: {"email":"admin@fintechpsp.com","password":"admin123"}
  2. Verificar status code 200
  3. Verificar estrutura da resposta
  4. Validar token JWT no campo accessToken
  5. Verificar dados do usuário retornados

Dados de Teste:
  email: admin@fintechpsp.com
  password: admin123

Resultado Esperado:
  - Status: 200 OK
  - Response contém: accessToken, tokenType, expiresIn, user
  - Token JWT válido
  - user.email = admin@fintechpsp.com
  - user.role = Admin

Critérios de Aceitação:
  - Login realizado em < 2 segundos
  - Token expira em 1 hora
  - Log de auditoria criado
```

### **TC-AUTH-002: Login Credenciais Inválidas**
```yaml
ID: TC-AUTH-002
Módulo: AuthService
Prioridade: Alta
Tipo: Negativo

Pré-condições:
  - Sistema funcionando

Passos:
  1. Enviar POST /auth/login
     Body: {"email":"admin@fintechpsp.com","password":"senhaerrada"}
  2. Verificar status code 401
  3. Verificar mensagem de erro
  4. Verificar que não retorna token

Dados de Teste:
  email: admin@fintechpsp.com
  password: senhaerrada

Resultado Esperado:
  - Status: 401 Unauthorized
  - Mensagem: "Credenciais inválidas"
  - Sem token retornado
  - Log de tentativa falhosa criado
```

### **TC-AUTH-003: OAuth Client Credentials**
```yaml
ID: TC-AUTH-003
Módulo: AuthService
Prioridade: Média
Tipo: Funcional

Pré-condições:
  - Cliente OAuth configurado no banco

Passos:
  1. Enviar POST /auth/token
     Body: {
       "grant_type": "client_credentials",
       "client_id": "fintech_admin",
       "client_secret": "admin_secret_789"
     }
  2. Verificar status code 200
  3. Validar access_token retornado
  4. Verificar expiração

Resultado Esperado:
  - Status: 200 OK
  - access_token válido
  - token_type: "Bearer"
  - expires_in: 3600
```

---

## 👥 **USERSERVICE - CASOS DETALHADOS**

### **TC-USER-001: Criar Usuário Cliente**
```yaml
ID: TC-USER-001
Módulo: UserService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Admin autenticado
  - Token JWT válido

Passos:
  1. Enviar POST /admin/users
     Headers: Authorization: Bearer {token}
     Body: {
       "name": "Cliente Teste",
       "email": "cliente.teste@email.com",
       "password": "123456",
       "role": "cliente",
       "isActive": true
     }
  2. Verificar status code 201
  3. Verificar usuário criado
  4. Validar senha criptografada no banco

Resultado Esperado:
  - Status: 201 Created
  - Usuário criado com ID único
  - Senha armazenada com BCrypt
  - Email único no sistema
```

### **TC-USER-002: Listar Usuários com Paginação**
```yaml
ID: TC-USER-002
Módulo: UserService
Prioridade: Média
Tipo: Funcional

Pré-condições:
  - Admin autenticado
  - Múltiplos usuários cadastrados

Passos:
  1. Enviar GET /admin/users?page=1&pageSize=10
  2. Verificar status code 200
  3. Validar estrutura de paginação
  4. Verificar dados dos usuários

Resultado Esperado:
  - Status: 200 OK
  - Lista de usuários
  - Metadados de paginação
  - Senhas não expostas
```

### **TC-USER-003: Endpoint /client-users/me**
```yaml
ID: TC-USER-003
Módulo: UserService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Cliente autenticado
  - Token JWT válido

Passos:
  1. Enviar GET /client-users/me
     Headers: Authorization: Bearer {token}
  2. Verificar status code 200
  3. Validar dados do usuário logado
  4. Verificar que senha não é retornada

Resultado Esperado:
  - Status: 200 OK
  - Dados do usuário atual
  - Sem informações sensíveis
  - Compatível com InternetBankingWeb
```

---

## 💰 **TRANSACTIONSERVICE - CASOS DETALHADOS**

### **TC-TRANS-001: QR Code PIX Dinâmico**
```yaml
ID: TC-TRANS-001
Módulo: TransactionService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Cliente autenticado
  - Conta bancária configurada

Passos:
  1. Enviar POST /transacoes/pix/qrcode/dinamico
     Body: {
       "externalId": "QR001",
       "amount": 100.50,
       "pixKey": "12345678901",
       "bankCode": "SICOOB",
       "description": "Pagamento teste"
     }
  2. Verificar status code 201
  3. Validar QR Code gerado
  4. Verificar EMV code válido

Resultado Esperado:
  - Status: 201 Created
  - QR Code com valor fixo
  - EMV code de 248+ caracteres
  - Expiração configurada
```

### **TC-TRANS-002: QR Code PIX Estático**
```yaml
ID: TC-TRANS-002
Módulo: TransactionService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Cliente autenticado

Passos:
  1. Enviar POST /transacoes/pix/qrcode/estatico
     Body: {
       "externalId": "QR002",
       "pixKey": "12345678901",
       "bankCode": "SICOOB",
       "description": "QR Code reutilizável"
     }
  2. Verificar status code 201
  3. Validar QR Code sem valor
  4. Verificar reutilização

Resultado Esperado:
  - Status: 201 Created
  - QR Code sem valor fixo
  - Pode ser usado múltiplas vezes
  - Não expira
```

### **TC-TRANS-003: Consultar Transações PIX**
```yaml
ID: TC-TRANS-003
Módulo: TransactionService
Prioridade: Média
Tipo: Funcional

Pré-condições:
  - Cliente autenticado
  - Transações PIX existentes

Passos:
  1. Enviar GET /transacoes/pix
  2. Verificar status code 200
  3. Validar lista de transações
  4. Testar filtros por data

Resultado Esperado:
  - Status: 200 OK
  - Lista de transações PIX
  - Filtros funcionando
  - Paginação implementada
```

---

## 🔌 **INTEGRATIONSERVICE - CASOS DETALHADOS**

### **TC-INT-001: Cobrança PIX Sicoob**
```yaml
ID: TC-INT-001
Módulo: IntegrationService
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - Credenciais Sicoob configuradas
  - Certificado mTLS válido

Passos:
  1. Enviar POST /integrations/sicoob/pix/cobranca
     Body: {
       "txId": "TXN001",
       "valor": 150.00,
       "chavePix": "12345678901",
       "solicitacaoPagador": "Teste cobrança"
     }
  2. Verificar status code 201
  3. Validar resposta do Sicoob
  4. Verificar PIX Copia e Cola

Resultado Esperado:
  - Status: 201 Created
  - Cobrança criada no Sicoob
  - PIX Copia e Cola retornado
  - Status "ATIVA"
```

### **TC-INT-002: Health Check Integrações**
```yaml
ID: TC-INT-002
Módulo: IntegrationService
Prioridade: Média
Tipo: Funcional

Passos:
  1. Enviar GET /integrations/health
  2. Verificar status code 200
  3. Validar status de cada integração
  4. Verificar tempos de resposta

Resultado Esperado:
  - Status: 200 OK
  - Status de todas as integrações
  - Tempos de resposta < 5s
  - Indicadores de saúde claros
```

---

## 🌐 **TESTES DE INTERFACE**

### **TC-UI-001: Login BackofficeWeb**
```yaml
ID: TC-UI-001
Módulo: BackofficeWeb
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - BackofficeWeb rodando em localhost:3000
  - Usuário admin existe

Passos:
  1. Acessar http://localhost:3000
  2. Inserir email: admin@fintechpsp.com
  3. Inserir senha: admin123
  4. Clicar em "Entrar"
  5. Verificar redirecionamento

Resultado Esperado:
  - Login realizado com sucesso
  - Redirecionamento para dashboard
  - Menu lateral visível
  - Dados do usuário exibidos
```

### **TC-UI-002: Gerar QR Code InternetBankingWeb**
```yaml
ID: TC-UI-002
Módulo: InternetBankingWeb
Prioridade: Alta
Tipo: Funcional

Pré-condições:
  - InternetBankingWeb rodando em localhost:3001
  - Cliente logado

Passos:
  1. Navegar para seção PIX
  2. Clicar em "Gerar QR Code"
  3. Preencher valor: R$ 50,00
  4. Preencher descrição: "Teste QA"
  5. Clicar em "Gerar"
  6. Verificar QR Code exibido

Resultado Esperado:
  - QR Code gerado e exibido
  - Valor correto mostrado
  - Opção de download disponível
  - PIX Copia e Cola visível
```

---

## 🔄 **TESTES END-TO-END**

### **TC-E2E-001: Fluxo Completo PIX**
```yaml
ID: TC-E2E-001
Tipo: End-to-End
Prioridade: Crítica
Duração: ~15 minutos

Cenário: Cliente gera QR Code PIX e sistema processa corretamente

Passos:
  1. Admin faz login no BackofficeWeb
  2. Admin cria nova empresa "Teste E2E Ltda"
  3. Admin aprova a empresa
  4. Admin cria usuário cliente para a empresa
  5. Cliente faz login no InternetBankingWeb
  6. Cliente navega para seção PIX
  7. Cliente gera QR Code dinâmico de R$ 100,00
  8. Sistema integra com Sicoob
  9. QR Code é exibido com sucesso
  10. Verificar transação criada no banco
  11. Verificar logs de integração

Resultado Esperado:
  - Fluxo completo sem erros
  - QR Code funcional gerado
  - Integração Sicoob bem-sucedida
  - Dados persistidos corretamente
  - Logs de auditoria criados
```

### **TC-E2E-002: Fluxo de Webhook**
```yaml
ID: TC-E2E-002
Tipo: End-to-End
Prioridade: Alta
Duração: ~10 minutos

Cenário: Sistema recebe webhook de pagamento PIX

Passos:
  1. Configurar webhook de teste
  2. Gerar QR Code PIX
  3. Simular pagamento via webhook
  4. Verificar processamento do webhook
  5. Verificar atualização de saldo
  6. Verificar notificação ao cliente

Resultado Esperado:
  - Webhook processado corretamente
  - Saldo atualizado
  - Cliente notificado
  - Logs de auditoria criados
```

---

## 📊 **MÉTRICAS DE QUALIDADE**

### **🎯 Metas de Cobertura**
- **Casos de Teste**: 104 casos
- **Cobertura Funcional**: 95%+
- **Cobertura de APIs**: 100%
- **Cobertura de UI**: 80%+

### **⚡ Metas de Performance**
- **APIs**: < 2 segundos
- **UI**: < 3 segundos
- **Integrações**: < 5 segundos
- **E2E**: < 30 segundos

### **🔍 Critérios de Aprovação**
- **Taxa de Sucesso**: 98%+
- **Bugs Críticos**: 0
- **Bugs Altos**: < 3
- **Performance**: Dentro das metas

---

## 🚀 **AUTOMAÇÃO**

### **📋 Casos Priorizados para Automação**
1. **Login/Logout** - TC-AUTH-001, TC-UI-001
2. **CRUD Básico** - TC-USER-001, TC-TRANS-001
3. **Integrações** - TC-INT-001, TC-INT-002
4. **Smoke Tests** - Health checks
5. **Regressão** - Casos críticos

### **🛠️ Ferramentas Sugeridas**
- **API**: Postman/Newman, RestAssured
- **UI**: Selenium, Cypress, Playwright
- **Performance**: JMeter, K6
- **CI/CD**: GitHub Actions, Jenkins

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**📊 Total de Casos**: 104 casos de teste
