# 🧪 **PLANO DE TESTES QA FUNCIONAL - FINTECHPSP**

## 📋 **Visão Geral**

Este documento define o plano completo de testes funcionais para o sistema FintechPSP, cobrindo todos os microserviços, integrações e interfaces de usuário.

---

## 🎯 **Objetivos dos Testes**

### **Garantir Qualidade**
- ✅ Funcionalidades atendem aos requisitos
- ✅ Integrações funcionam corretamente
- ✅ Performance dentro dos SLAs
- ✅ Segurança e conformidade

### **Reduzir Riscos**
- ❌ Bugs em produção
- ❌ Falhas de integração
- ❌ Problemas de performance
- ❌ Vulnerabilidades de segurança

---

## 🏗️ **Escopo dos Testes**

### **✅ INCLUÍDO**
- Testes funcionais de todos os microserviços
- Testes de integração entre serviços
- Testes de API (REST endpoints)
- Testes de interface (BackofficeWeb e InternetBankingWeb)
- Testes de fluxos end-to-end
- Testes de segurança básicos
- Testes de performance básicos

### **❌ EXCLUÍDO**
- Testes de carga (load testing)
- Testes de stress
- Testes de penetração avançados
- Testes de compatibilidade de browser
- Testes de acessibilidade

---

## 🛠️ **Setup do Ambiente de Testes**

### **📋 Pré-requisitos**

#### **Software Necessário**
```bash
# Ferramentas obrigatórias
- Docker Desktop 4.0+
- Node.js 18+
- .NET 8 SDK
- PostgreSQL Client
- Postman ou Insomnia
- Git

# Ferramentas opcionais
- Visual Studio Code
- DBeaver (para banco)
- Redis CLI
```

#### **Hardware Mínimo**
```
- CPU: 4 cores
- RAM: 8GB
- Disk: 20GB livres
- Network: Banda larga estável
```

### **🚀 Setup Inicial**

#### **1. Clone do Repositório**
```bash
git clone <repository-url>
cd fintech
```

#### **2. Configuração de Ambiente**
```bash
# Copiar arquivo de configuração
cp .env.example .env

# Editar variáveis de ambiente
# DATABASE_URL=postgresql://postgres:postgres@localhost:5432/fintechpsp
# REDIS_URL=redis://localhost:6379
# RABBITMQ_URL=amqp://guest:guest@localhost:5672
```

#### **3. Inicialização dos Serviços**
```bash
# Subir infraestrutura
docker-compose up -d postgres redis rabbitmq

# Aguardar serviços ficarem prontos (30-60 segundos)
docker-compose ps

# Subir microserviços
docker-compose up -d

# Verificar status
docker-compose ps
```

#### **4. Verificação do Setup**
```bash
# Health checks
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health

# Frontends
curl http://localhost:3000
curl http://localhost:3001
```

### **🗄️ Preparação dos Dados de Teste**

#### **Usuários de Teste**
```sql
-- Admin Master (já existe)
Email: admin@fintechpsp.com
Senha: admin123
Role: Admin

-- Cliente de Teste
Email: cliente@empresateste.com
Senha: 123456
Role: cliente
```

#### **Empresa de Teste**
```json
{
  "razaoSocial": "EmpresaTeste Ltda",
  "cnpj": "12.345.678/0001-99",
  "email": "contato@empresateste.com",
  "status": "Approved"
}
```

#### **Dados Bancários de Teste**
```json
{
  "bankCode": "SICOOB",
  "pixKey": "12345678901",
  "accountNumber": "123456-7",
  "branch": "0001"
}
```

---

## 📊 **Estratégia de Testes**

### **🔄 Tipos de Teste**

#### **1. Testes Unitários** (Desenvolvedor)
- Métodos individuais
- Lógica de negócio
- Validações
- Coverage: 80%+

#### **2. Testes de Integração** (QA)
- Comunicação entre serviços
- APIs REST
- Banco de dados
- Message queues

#### **3. Testes Funcionais** (QA)
- Casos de uso completos
- Fluxos de negócio
- Validações end-to-end
- Interface de usuário

#### **4. Testes de Regressão** (QA)
- Funcionalidades existentes
- Após mudanças
- Automação prioritária
- Smoke tests

### **📋 Níveis de Teste**

#### **Nível 1: Componente**
- Microserviço individual
- APIs isoladas
- Mocks para dependências

#### **Nível 2: Integração**
- Múltiplos microserviços
- Banco de dados real
- Message broker real

#### **Nível 3: Sistema**
- Sistema completo
- Todos os componentes
- Dados reais de teste

#### **Nível 4: Aceitação**
- Cenários de usuário
- Fluxos completos
- Critérios de negócio

---

## 🧪 **Casos de Teste por Módulo**

### **🔐 AuthService**

#### **TC001: Login de Usuário**
```
Pré-condição: Usuário cadastrado no sistema
Passos:
1. POST /auth/login com email/senha válidos
2. Verificar resposta 200 OK
3. Verificar token JWT retornado
4. Verificar dados do usuário

Resultado Esperado: Login realizado com sucesso
```

#### **TC002: Login com Credenciais Inválidas**
```
Pré-condição: Sistema funcionando
Passos:
1. POST /auth/login com email/senha inválidos
2. Verificar resposta 401 Unauthorized
3. Verificar mensagem de erro

Resultado Esperado: Login rejeitado
```

#### **TC003: OAuth 2.0 Client Credentials**
```
Pré-condição: Cliente OAuth configurado
Passos:
1. POST /auth/token com client_id/client_secret
2. Verificar resposta 200 OK
3. Verificar access_token retornado
4. Verificar expiração do token

Resultado Esperado: Token OAuth obtido
```

### **👥 UserService**

#### **TC004: Criar Usuário Cliente**
```
Pré-condição: Admin logado
Passos:
1. POST /admin/users com dados válidos
2. Verificar resposta 201 Created
3. Verificar usuário criado no banco
4. Verificar senha criptografada

Resultado Esperado: Usuário criado com sucesso
```

#### **TC005: Listar Usuários**
```
Pré-condição: Usuários cadastrados
Passos:
1. GET /admin/users
2. Verificar resposta 200 OK
3. Verificar lista de usuários
4. Verificar paginação

Resultado Esperado: Lista retornada corretamente
```

### **💰 TransactionService**

#### **TC006: Criar QR Code PIX Dinâmico**
```
Pré-condição: Cliente autenticado
Passos:
1. POST /transacoes/pix/qrcode/dinamico
2. Verificar resposta 201 Created
3. Verificar QR Code gerado
4. Verificar EMV code válido

Resultado Esperado: QR Code criado
```

#### **TC007: Criar QR Code PIX Estático**
```
Pré-condição: Cliente autenticado
Passos:
1. POST /transacoes/pix/qrcode/estatico
2. Verificar resposta 201 Created
3. Verificar QR Code gerado
4. Verificar ausência de valor

Resultado Esperado: QR Code estático criado
```

#### **TC008: Consultar Transações**
```
Pré-condição: Transações existentes
Passos:
1. GET /transacoes/pix
2. Verificar resposta 200 OK
3. Verificar lista de transações
4. Verificar filtros funcionando

Resultado Esperado: Transações listadas
```

### **🔌 IntegrationService**

#### **TC009: Integração Sicoob PIX**
```
Pré-condição: Credenciais Sicoob configuradas
Passos:
1. POST /integrations/sicoob/pix/cobranca
2. Verificar resposta 201 Created
3. Verificar cobrança criada no Sicoob
4. Verificar PIX Copia e Cola retornado

Resultado Esperado: Cobrança PIX criada
```

#### **TC010: Health Check Integrações**
```
Pré-condição: Sistema funcionando
Passos:
1. GET /integrations/health
2. Verificar resposta 200 OK
3. Verificar status de cada integração
4. Verificar tempos de resposta

Resultado Esperado: Status das integrações
```

### **🏢 CompanyService**

#### **TC011: Cadastrar Empresa**
```
Pré-condição: Admin logado
Passos:
1. POST /companies com dados válidos
2. Verificar resposta 201 Created
3. Verificar empresa criada
4. Verificar status "PendingDocuments"

Resultado Esperado: Empresa cadastrada
```

#### **TC012: Aprovar Empresa**
```
Pré-condição: Empresa em análise
Passos:
1. PUT /companies/{id}/approve
2. Verificar resposta 200 OK
3. Verificar status "Approved"
4. Verificar data de aprovação

Resultado Esperado: Empresa aprovada
```

---

## 🌐 **Testes de Interface**

### **🖥️ BackofficeWeb (Admin)**

#### **TC013: Login Admin**
```
URL: http://localhost:3000
Passos:
1. Acessar página de login
2. Inserir admin@fintechpsp.com / admin123
3. Clicar em "Entrar"
4. Verificar redirecionamento para dashboard

Resultado Esperado: Login realizado
```

#### **TC014: Dashboard Admin**
```
Pré-condição: Admin logado
Passos:
1. Verificar métricas exibidas
2. Verificar gráficos carregando
3. Verificar menu lateral
4. Verificar links funcionando

Resultado Esperado: Dashboard funcional
```

#### **TC015: Gestão de Empresas**
```
Pré-condição: Admin logado
Passos:
1. Navegar para "Empresas"
2. Verificar lista de empresas
3. Clicar em "Nova Empresa"
4. Preencher formulário
5. Salvar empresa

Resultado Esperado: Empresa criada via interface
```

### **💻 InternetBankingWeb (Cliente)**

#### **TC016: Login Cliente**
```
URL: http://localhost:3001
Passos:
1. Acessar página de login
2. Inserir cliente@empresateste.com / 123456
3. Clicar em "Entrar"
4. Verificar redirecionamento para dashboard

Resultado Esperado: Login realizado
```

#### **TC017: Gerar QR Code PIX**
```
Pré-condição: Cliente logado
Passos:
1. Navegar para "PIX"
2. Clicar em "Gerar QR Code"
3. Preencher valor e descrição
4. Clicar em "Gerar"
5. Verificar QR Code exibido

Resultado Esperado: QR Code gerado na interface
```

---

## 🔄 **Testes End-to-End**

### **E2E001: Fluxo Completo PIX**
```
Cenário: Cliente gera QR Code e recebe pagamento
Passos:
1. Admin cria empresa no BackofficeWeb
2. Admin aprova empresa
3. Admin cria usuário cliente
4. Cliente faz login no InternetBankingWeb
5. Cliente gera QR Code PIX dinâmico
6. Sistema integra com Sicoob
7. QR Code é exibido para cliente
8. Webhook de pagamento é recebido
9. Saldo é atualizado
10. Cliente visualiza transação

Resultado Esperado: Fluxo completo funcional
```

### **E2E002: Fluxo de Onboarding**
```
Cenário: Nova empresa se cadastra no sistema
Passos:
1. Empresa solicita cadastro
2. Admin analisa documentos
3. Admin aprova empresa
4. Sistema cria conta bancária
5. Sistema envia credenciais
6. Empresa faz primeiro login
7. Empresa configura PIX
8. Empresa realiza primeira transação

Resultado Esperado: Onboarding completo
```

---

## 📊 **Critérios de Aceitação**

### **✅ Funcionalidade**
- Todos os casos de teste passam
- Fluxos end-to-end funcionam
- Integrações estáveis
- Interfaces responsivas

### **⚡ Performance**
- APIs respondem em < 2s
- Páginas carregam em < 3s
- Transações processam em < 5s
- Sistema suporta 100 usuários simultâneos

### **🔐 Segurança**
- Autenticação funciona
- Autorização respeitada
- Dados sensíveis protegidos
- Logs de auditoria gerados

### **🛡️ Confiabilidade**
- Sistema disponível 99.9%
- Recuperação automática de falhas
- Backup e restore funcionam
- Monitoramento ativo

---

## 🚀 **Execução dos Testes**

### **📅 Cronograma**

#### **Fase 1: Setup (1 dia)**
- Configuração do ambiente
- Preparação dos dados
- Validação do setup

#### **Fase 2: Testes Unitários (2 dias)**
- Execução automática
- Análise de coverage
- Correção de falhas

#### **Fase 3: Testes de Integração (3 dias)**
- APIs individuais
- Integrações entre serviços
- Banco de dados

#### **Fase 4: Testes Funcionais (5 dias)**
- Casos de uso completos
- Interfaces de usuário
- Fluxos end-to-end

#### **Fase 5: Testes de Regressão (2 dias)**
- Re-execução de casos críticos
- Validação de correções
- Smoke tests

### **👥 Responsabilidades**

#### **QA Lead**
- Planejamento dos testes
- Revisão dos casos de teste
- Coordenação da equipe
- Relatórios de qualidade

#### **QA Analyst**
- Execução dos casos de teste
- Documentação de bugs
- Validação de correções
- Automação de testes

#### **Developer**
- Correção de bugs
- Testes unitários
- Code review
- Suporte ao QA

---

**📝 Última atualização**: 2025-10-08  
**🔄 Versão**: 1.0.0  
**👥 Equipe QA**: FintechPSP
