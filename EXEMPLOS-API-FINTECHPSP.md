# 🔌 **FintechPSP - Exemplos de Uso da API**

## 📋 **Visão Geral**

Este documento contém exemplos práticos de como usar as APIs do FintechPSP para integração com sistemas externos ou desenvolvimento de aplicações cliente.

---

## 🔐 **Autenticação**

### **1. Login de Usuário**

```bash
# cURL
curl -X POST "http://localhost:5001/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@fintechpsp.com",
    "password": "admin123"
  }'
```

```javascript
// JavaScript/Node.js
const response = await fetch('http://localhost:5001/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'admin@fintechpsp.com',
    password: 'admin123'
  })
});

const data = await response.json();
const token = data.accessToken;
```

```python
# Python
import requests

response = requests.post('http://localhost:5001/auth/login', json={
    'email': 'admin@fintechpsp.com',
    'password': 'admin123'
})

data = response.json()
token = data['accessToken']
```

### **2. Token OAuth 2.0 (Client Credentials)**

```bash
# cURL
curl -X POST "http://localhost:5001/auth/token" \
  -H "Content-Type: application/json" \
  -d '{
    "grant_type": "client_credentials",
    "client_id": "integration_test",
    "client_secret": "test_secret_000",
    "scope": "pix banking admin"
  }'
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "token_type": "Bearer",
  "expires_in": 3600,
  "scope": "pix banking admin"
}
```

---

## 🏦 **Gestão de Contas Bancárias**

### **3. Listar Contas do Cliente**

```bash
# cURL
curl -X GET "http://localhost:5000/banking/contas" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

```javascript
// JavaScript
const response = await fetch('http://localhost:5000/banking/contas', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const contas = await response.json();
```

**Response:**
```json
{
  "data": [
    {
      "contaId": "123e4567-e89b-12d3-a456-426614174000",
      "clienteId": "12345678-1234-1234-1234-123456789012",
      "bankCode": "341",
      "accountNumber": "12345-6",
      "description": "Conta Principal Itaú",
      "isActive": true,
      "createdAt": "2024-01-15T10:30:00Z"
    }
  ],
  "total": 1,
  "page": 1,
  "limit": 10
}
```

### **4. Cadastrar Nova Conta Bancária**

```bash
# cURL
curl -X POST "http://localhost:5000/banking/contas" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "bankCode": "341",
    "accountNumber": "12345-6",
    "description": "Conta Itaú Principal",
    "credentials": {
      "username": "usuario_banco",
      "password": "senha_banco",
      "certificate": "base64_certificate_data"
    }
  }'
```

---

## 💰 **Transações PIX**

### **5. Criar Cobrança PIX**

```bash
# cURL
curl -X POST "http://localhost:5000/pix/cobranca" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "valor": 100.50,
    "descricao": "Pagamento de serviços",
    "externalId": "PEDIDO_001",
    "expiracaoMinutos": 60,
    "webhookUrl": "https://meusite.com/webhook/pix"
  }'
```

**Response:**
```json
{
  "cobrancaId": "E12345678202410101030123456789012",
  "qrCode": "00020126580014br.gov.bcb.pix...",
  "qrCodeImage": "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAA...",
  "pixCopiaECola": "00020126580014br.gov.bcb.pix0136123e4567-e89b-12d3-a456-426614174000520400005303986540510.505802BR5913EMPRESA TESTE6009SAO PAULO62070503***6304ABCD",
  "status": "ATIVA",
  "valor": 100.50,
  "expiresAt": "2024-10-10T11:30:00Z",
  "createdAt": "2024-10-10T10:30:00Z"
}
```

### **6. Consultar Status da Cobrança PIX**

```bash
# cURL
curl -X GET "http://localhost:5000/pix/cobranca/E12345678202410101030123456789012" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Response:**
```json
{
  "cobrancaId": "E12345678202410101030123456789012",
  "status": "PAGA",
  "valor": 100.50,
  "valorPago": 100.50,
  "pagador": {
    "nome": "João Silva",
    "documento": "12345678901",
    "banco": "341"
  },
  "pagamentoAt": "2024-10-10T10:45:00Z",
  "endToEndId": "E34112345202410101045123456789012"
}
```

---

## 💸 **Transferências TED**

### **7. Realizar TED**

```bash
# cURL
curl -X POST "http://localhost:5000/ted/transferencia" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "valor": 1500.00,
    "favorecido": {
      "nome": "Maria Santos",
      "documento": "98765432100",
      "banco": "033",
      "agencia": "1234",
      "conta": "567890",
      "tipoConta": "CORRENTE"
    },
    "descricao": "Pagamento fornecedor",
    "externalId": "TED_001",
    "agendamento": "2024-10-11T09:00:00Z"
  }'
```

**Response:**
```json
{
  "transacaoId": "ted_123e4567-e89b-12d3-a456-426614174000",
  "status": "AGENDADA",
  "valor": 1500.00,
  "taxa": 15.00,
  "valorTotal": 1515.00,
  "agendadaPara": "2024-10-11T09:00:00Z",
  "protocolo": "TED20241010001234"
}
```

---

## 📄 **Boletos Bancários**

### **8. Gerar Boleto**

```bash
# cURL
curl -X POST "http://localhost:5000/boleto/gerar" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "valor": 250.00,
    "vencimento": "2024-10-20",
    "pagador": {
      "nome": "Carlos Oliveira",
      "documento": "12345678901",
      "endereco": {
        "logradouro": "Rua das Flores, 123",
        "cidade": "São Paulo",
        "uf": "SP",
        "cep": "01234567"
      }
    },
    "descricao": "Mensalidade outubro/2024",
    "externalId": "BOLETO_001",
    "multa": 2.0,
    "juros": 0.033
  }'
```

**Response:**
```json
{
  "boletoId": "341123456789012345678901234567890",
  "linhaDigitavel": "34191.23456 78901.234567 89012.345678 9 12340000025000",
  "codigoBarras": "34199123400000250001234567890123456789012345678",
  "nossoNumero": "12345678901",
  "vencimento": "2024-10-20",
  "valor": 250.00,
  "status": "EMITIDO",
  "urlBoleto": "https://api.fintechpsp.com/boleto/341123456789012345678901234567890/pdf"
}
```

---

## 💰 **Gestão de Saldos**

### **9. Consultar Saldo**

```bash
# cURL
curl -X GET "http://localhost:5000/banking/balance/12345678-1234-1234-1234-123456789012" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Response:**
```json
{
  "clienteId": "12345678-1234-1234-1234-123456789012",
  "saldoDisponivel": 5420.75,
  "saldoBloqueado": 150.00,
  "saldoTotal": 5570.75,
  "moeda": "BRL",
  "ultimaAtualizacao": "2024-10-10T10:30:00Z"
}
```

### **10. Realizar Cash-Out**

```bash
# cURL
curl -X POST "http://localhost:5000/saldo/cash-out" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "clienteId": "12345678-1234-1234-1234-123456789012",
    "valor": 500.00,
    "tipo": "PIX",
    "descricao": "Saque via PIX",
    "chavePix": "usuario@email.com"
  }'
```

**Response:**
```json
{
  "transacaoId": "cashout_123e4567-e89b-12d3-a456-426614174000",
  "status": "PROCESSANDO",
  "valor": 500.00,
  "taxa": 2.50,
  "valorLiquido": 497.50,
  "novoSaldo": 4920.75,
  "previsaoCredito": "2024-10-10T10:35:00Z"
}
```

---

## 📊 **Relatórios e Extratos**

### **11. Extrato de Transações**

```bash
# cURL
curl -X GET "http://localhost:5000/banking/transacoes/historico?dataInicio=2024-10-01&dataFim=2024-10-10&tipo=PIX&status=CONFIRMED&page=1&limit=50" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Response:**
```json
{
  "data": [
    {
      "transacaoId": "pix_123e4567-e89b-12d3-a456-426614174000",
      "externalId": "PEDIDO_001",
      "tipo": "PIX",
      "status": "CONFIRMED",
      "valor": 100.50,
      "descricao": "Pagamento de serviços",
      "dataHora": "2024-10-10T10:45:00Z",
      "endToEndId": "E34112345202410101045123456789012"
    }
  ],
  "total": 1,
  "page": 1,
  "limit": 50,
  "totalPages": 1
}
```

### **12. Relatório Financeiro**

```bash
# cURL
curl -X GET "http://localhost:5000/relatorios/financeiro?periodo=2024-10&formato=json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Response:**
```json
{
  "periodo": "2024-10",
  "resumo": {
    "totalTransacoes": 1250,
    "volumeTotal": 125000.00,
    "receitaTaxas": 2500.00,
    "ticketMedio": 100.00
  },
  "porTipo": {
    "PIX": {
      "quantidade": 800,
      "volume": 80000.00,
      "receita": 1600.00
    },
    "TED": {
      "quantidade": 300,
      "volume": 35000.00,
      "receita": 750.00
    },
    "BOLETO": {
      "quantidade": 150,
      "volume": 10000.00,
      "receita": 150.00
    }
  }
}
```

---

## 🔔 **Webhooks**

### **13. Configurar Webhook**

```bash
# cURL
curl -X POST "http://localhost:5000/webhooks" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://meusite.com/webhook/fintechpsp",
    "eventos": ["PIX_CONFIRMADO", "TED_PROCESSADA", "BOLETO_PAGO"],
    "ativo": true,
    "secretKey": "minha_chave_secreta_webhook"
  }'
```

### **14. Exemplo de Webhook Recebido**

```json
{
  "evento": "PIX_CONFIRMADO",
  "timestamp": "2024-10-10T10:45:00Z",
  "dados": {
    "transacaoId": "pix_123e4567-e89b-12d3-a456-426614174000",
    "externalId": "PEDIDO_001",
    "valor": 100.50,
    "pagador": {
      "nome": "João Silva",
      "documento": "12345678901"
    },
    "endToEndId": "E34112345202410101045123456789012"
  },
  "assinatura": "sha256=a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3"
}
```

---

## 🔧 **Utilitários**

### **15. Health Check**

```bash
# Verificar status de todos os serviços
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5003/test/health
curl http://localhost:5004/health
curl http://localhost:5005/health
curl http://localhost:5006/health
curl http://localhost:5007/health
curl http://localhost:5008/health
curl http://localhost:5010/health
```

### **16. Validação de CPF/CNPJ**

```bash
# cURL
curl -X GET "http://localhost:5000/utils/validar-documento/12345678901" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

**Response:**
```json
{
  "documento": "12345678901",
  "tipo": "CPF",
  "valido": true,
  "formatado": "123.456.789-01"
}
```

---

## 📚 **SDKs e Bibliotecas**

### **JavaScript/TypeScript**

```javascript
// Exemplo de SDK personalizado
class FintechPSPClient {
  constructor(baseUrl, token) {
    this.baseUrl = baseUrl;
    this.token = token;
  }

  async criarCobrancaPix(dados) {
    const response = await fetch(`${this.baseUrl}/pix/cobranca`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${this.token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(dados)
    });
    return response.json();
  }

  async consultarSaldo(clienteId) {
    const response = await fetch(`${this.baseUrl}/banking/balance/${clienteId}`, {
      headers: {
        'Authorization': `Bearer ${this.token}`
      }
    });
    return response.json();
  }
}

// Uso
const client = new FintechPSPClient('http://localhost:5000', 'seu_token_aqui');
const cobranca = await client.criarCobrancaPix({
  valor: 100.50,
  descricao: 'Pagamento teste'
});
```

### **Python**

```python
import requests
from typing import Dict, Any

class FintechPSPClient:
    def __init__(self, base_url: str, token: str):
        self.base_url = base_url
        self.headers = {
            'Authorization': f'Bearer {token}',
            'Content-Type': 'application/json'
        }

    def criar_cobranca_pix(self, dados: Dict[str, Any]) -> Dict[str, Any]:
        response = requests.post(
            f'{self.base_url}/pix/cobranca',
            json=dados,
            headers=self.headers
        )
        response.raise_for_status()
        return response.json()

    def consultar_saldo(self, cliente_id: str) -> Dict[str, Any]:
        response = requests.get(
            f'{self.base_url}/banking/balance/{cliente_id}',
            headers=self.headers
        )
        response.raise_for_status()
        return response.json()

# Uso
client = FintechPSPClient('http://localhost:5000', 'seu_token_aqui')
cobranca = client.criar_cobranca_pix({
    'valor': 100.50,
    'descricao': 'Pagamento teste'
})
```

---

## 🚨 **Tratamento de Erros**

### **Códigos de Status HTTP**

| Código | Significado | Ação Recomendada |
|--------|-------------|-------------------|
| 200 | Sucesso | Processar resposta |
| 201 | Criado | Recurso criado com sucesso |
| 400 | Bad Request | Verificar dados enviados |
| 401 | Unauthorized | Renovar token de acesso |
| 403 | Forbidden | Verificar permissões |
| 404 | Not Found | Verificar URL/ID do recurso |
| 429 | Rate Limit | Implementar retry com backoff |
| 500 | Server Error | Tentar novamente mais tarde |

### **Exemplo de Resposta de Erro**

```json
{
  "error": "invalid_request",
  "message": "O valor da transação deve ser maior que zero",
  "details": {
    "field": "valor",
    "code": "VALOR_INVALIDO"
  },
  "timestamp": "2024-10-10T10:30:00Z",
  "requestId": "req_123e4567-e89b-12d3-a456-426614174000"
}
```

---

**🏆 FintechPSP APIs - Integração completa para Payment Service Provider!** 🚀
