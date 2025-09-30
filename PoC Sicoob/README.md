# 🏦 Sicoob Integration API - C#

API completa de integração com todas as APIs do Sicoob usando **OAuth 2.0** e **mTLS** (certificado digital ICP Brasil).

---

## 🎯 APIs Integradas

### 1. **Cobrança Bancária** (`/api/CobrancaBancaria`)
- ✅ Consultar boletos
- ✅ Incluir boletos
- ✅ Alterar boletos
- ✅ Listar boletos

### 2. **Pagamentos** (`/api/Pagamentos`)
- ✅ Incluir pagamento de boleto
- ✅ Consultar pagamento
- ✅ Alterar pagamento

### 3. **Conta Corrente** (`/api/ContaCorrente`)
- ✅ Consultar saldo
- ✅ Consultar extrato
- ✅ Realizar transferências

### 4. **PIX Recebimentos** (`/api/Pix/recebimentos`)
- ✅ Criar cobrança imediata
- ✅ Criar cobrança com vencimento
- ✅ Consultar cobrança
- ✅ Listar cobranças
- ✅ Consultar PIX recebidos

### 5. **PIX Pagamentos** (`/api/Pix/pagamentos`)
- ✅ Realizar pagamento PIX
- ✅ Consultar pagamento
- ✅ Listar pagamentos

### 6. **SPB Transferências** (`/api/SPB`)
- ✅ Realizar TED
- ✅ Consultar TED
- ✅ Listar TEDs

### 7. **Open Finance** (em desenvolvimento)
- Iniciação de Pagamento

---

## 🔐 Segurança Implementada

✅ **OAuth 2.0 Client Credentials** - Autenticação segura
✅ **mTLS** - Certificado digital ICP Brasil
✅ **HTTPS/TLS 1.2+** - Comunicação criptografada
✅ **Token Caching** - Gerenciamento automático de tokens
✅ **Renovação Automática** - Tokens renovados antes de expirar
✅ **Headers Automáticos** - `Authorization` e `client_id` em todas as requisições

---

## 📋 Pré-requisitos

- .NET 8.0 ou superior
- Certificado digital ICP Brasil (e-CNPJ ou e-CPF tipo A1)
- Client ID fornecido pelo Sicoob
- Visual Studio 2022 ou VS Code

---

## 🚀 Instalação e Configuração

### 1. Clone ou navegue até a pasta Fintech

```bash
cd Fintech
```

### 2. Configure o appsettings.json

Edite o arquivo `SicoobIntegration.API/appsettings.json`:

```json
{
  "SicoobSettings": {
    "ClientId": "SEU_CLIENT_ID_AQUI",
    "CertificatePath": "../Certificates/dd533251-7a11-4939-8713-016763653f3c.pfx",
    "CertificatePassword": "Vi294141"
  }
}
```

### 3. Restaure as dependências

```bash
dotnet restore
```

### 4. Execute o projeto

```bash
cd SicoobIntegration.API
dotnet run
```

---

## 📖 Uso da API

### Swagger UI

Acesse a documentação interativa:
```
https://localhost:7000/swagger
```

### Exemplo: Consultar Saldo

```bash
GET https://localhost:7000/api/ContaCorrente/12345/saldo
```

### Exemplo: Criar Cobrança PIX

```bash
POST https://localhost:7000/api/Pix/recebimentos/cobranca-imediata
Content-Type: application/json

{
  "calendario": {
    "expiracao": 3600
  },
  "devedor": {
    "cpf": "12345678909",
    "nome": "João da Silva"
  },
  "valor": {
    "original": "100.00"
  },
  "chave": "sua-chave-pix",
  "solicitacaoPagador": "Pagamento de serviços"
}
```

---

## 🏗️ Estrutura do Projeto

```
Fintech/
├── Certificates/
│   └── dd533251-7a11-4939-8713-016763653f3c.pfx
│
├── SicoobIntegration.API/
│   ├── Controllers/
│   │   ├── CobrancaBancariaController.cs
│   │   ├── ContaCorrenteController.cs
│   │   ├── PixController.cs
│   │   └── SPBController.cs
│   │
│   ├── Models/
│   │   ├── SicoobSettings.cs
│   │   └── OAuth/
│   │       └── TokenResponse.cs
│   │
│   ├── Services/
│   │   ├── Base/
│   │   │   └── SicoobServiceBase.cs
│   │   ├── ISicoobAuthService.cs
│   │   ├── SicoobAuthService.cs
│   │   ├── CobrancaBancaria/
│   │   ├── ContaCorrente/
│   │   ├── Pagamentos/
│   │   ├── Pix/
│   │   └── SPB/
│   │
│   ├── Program.cs
│   └── appsettings.json
│
└── SicoobIntegration.sln
```

---

## 🔧 Configuração Avançada

### Escopos Personalizados

Para usar apenas escopos específicos, edite o `appsettings.json`:

```json
{
  "SicoobSettings": {
    "Scopes": {
      "PixRecebimentos": [
        "pix.read",
        "cob.read",
        "cob.write"
      ]
    }
  }
}
```

### Ambientes (Sandbox vs Produção)

Para usar o ambiente de sandbox, altere as URLs:

```json
{
  "SicoobSettings": {
    "BaseUrl": "https://sandbox.sicoob.com.br",
    "AuthUrl": "https://auth-sandbox.sicoob.com.br/..."
  }
}
```

---

## 🧪 Testando a API

### 1. Teste de Autenticação

Ao iniciar a aplicação, o sistema automaticamente testa a autenticação:

```
🔐 Testando autenticação OAuth 2.0...
✅ Token obtido com sucesso!
   Token: eyJhbGciOiJSUzI1NiI...
```

### 2. Teste via Swagger

1. Acesse `https://localhost:7000/swagger`
2. Expanda qualquer endpoint
3. Clique em "Try it out"
4. Preencha os parâmetros
5. Clique em "Execute"

### 3. Teste via cURL

```bash
curl -X GET "https://localhost:7000/api/ContaCorrente/12345/saldo" \
     -H "accept: application/json"
```

---

## 📊 Logs e Monitoramento

A aplicação registra logs detalhados:

```
✅ Certificado carregado com sucesso!
   Subject: CN=OWAYPAY SOLUCOES DE PAGAMENTOS LTDA...
   Válido até: 29/08/2026

🔐 Testando autenticação OAuth 2.0...
✅ Token obtido com sucesso!

🚀 API iniciada com sucesso!
```

---

## ⚠️ Troubleshooting

### ❌ Erro: "The SSL connection could not be established"

**Solução:**
- ✅ **RESOLVIDO!** O projeto agora inclui `CertificateHelper`
- O certificado é carregado corretamente com mTLS
- Veja logs detalhados ao iniciar a aplicação
- **Consulte:** [TROUBLESHOOTING_SSL.md](./TROUBLESHOOTING_SSL.md)

### Erro: "Certificado não encontrado"

**Solução:**
- Verifique se o arquivo PFX está em `Fintech/Certificates/`
- Confirme o caminho no `appsettings.json`

### Erro: "Invalid client credentials"

**Solução:**
- Verifique se o `ClientId` está correto
- Confirme que o certificado está válido
- Verifique se a chave pública foi enviada ao Sicoob

### Erro: "Token expired"

**Solução:**
- O sistema renova automaticamente
- Se persistir, reinicie a aplicação

### 📚 Guia Completo de Troubleshooting

Para problemas de SSL/Certificado, veja: **[TROUBLESHOOTING_SSL.md](./TROUBLESHOOTING_SSL.md)**

---

## 🔒 Segurança

### ⚠️ IMPORTANTE:

- ❌ **Nunca commite** o arquivo `appsettings.json` com credenciais reais
- ❌ **Nunca compartilhe** o certificado PFX
- ❌ **Nunca exponha** a senha do certificado
- ✅ **Use** variáveis de ambiente em produção
- ✅ **Rotacione** credenciais periodicamente
- ✅ **Monitore** logs de acesso

### Produção

Para produção, use variáveis de ambiente:

```bash
export SicoobSettings__ClientId="seu_client_id"
export SicoobSettings__CertificatePassword="sua_senha"
```

---

## 📚 Documentação Adicional

- [Documentação Oficial Sicoob](https://developers.sicoob.com.br)
- [OAuth 2.0 RFC 6749](https://tools.ietf.org/html/rfc6749)
- [mTLS RFC 8705](https://tools.ietf.org/html/rfc8705)

---

## 🤝 Suporte

Para dúvidas ou problemas:

1. Verifique os logs da aplicação
2. Consulte a documentação do Sicoob
3. Verifique se o certificado está válido

---

## 📝 Informações do Certificado

- **Empresa:** OWAYPAY SOLUCOES DE PAGAMENTOS LTDA
- **CNPJ:** 62470268000144
- **Tipo:** e-CNPJ A1 (ICP-Brasil)
- **Válido até:** 29/08/2026
- **Senha:** Vi294141

---

## ✅ Checklist de Configuração

- [ ] .NET 8.0 instalado
- [ ] Certificado PFX na pasta Certificates
- [ ] Client ID configurado no appsettings.json
- [ ] Dependências restauradas (`dotnet restore`)
- [ ] Aplicação executando sem erros
- [ ] Token obtido com sucesso
- [ ] Swagger acessível
- [ ] Endpoints testados

---

**Versão:** 1.0.0  
**Data:** 2025-09-29  
**Status:** ✅ Pronto para uso

