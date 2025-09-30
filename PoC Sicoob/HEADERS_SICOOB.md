# 📋 Headers Obrigatórios - APIs Sicoob

Documentação sobre os headers obrigatórios para todas as requisições às APIs do Sicoob.

---

## 🎯 Headers Obrigatórios

Todas as requisições às APIs do Sicoob devem incluir:

### 1. **Authorization** (Bearer Token)
```
Authorization: Bearer eyJhbGciOiJSUzI1NiIs...
```

### 2. **client_id** ✅ OBRIGATÓRIO
```
client_id: seu-client-id-aqui
```

---

## 🔧 Implementação

### Configuração Automática

O projeto já está configurado para adicionar automaticamente ambos os headers em **todas as requisições**.

<augment_code_snippet path="Fintech/SicoobIntegration.API/Services/Base/SicoobServiceBase.cs" mode="EXCERPT">
````csharp
protected async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(
    HttpMethod method,
    string url,
    CancellationToken cancellationToken = default)
{
    var token = await AuthService.GetAccessTokenAsync(cancellationToken);
    var request = new HttpRequestMessage(method, url);
    
    // ✅ Adiciona o token Bearer
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    
    // ✅ Adiciona o client_id no header (obrigatório para Sicoob)
    request.Headers.Add("client_id", Settings.ClientId);
    
    return request;
}
````
</augment_code_snippet>

---

## 📊 Fluxo de Requisição

```
1. Aplicação chama método do serviço
   ↓
2. CreateAuthenticatedRequestAsync() é chamado
   ↓
3. Obtém token OAuth 2.0 (se necessário)
   ↓
4. Cria HttpRequestMessage
   ↓
5. Adiciona header "Authorization: Bearer {token}"  ✅
   ↓
6. Adiciona header "client_id: {client_id}"  ✅
   ↓
7. Envia requisição ao Sicoob
   ↓
8. Sicoob valida ambos os headers
   ↓
9. Retorna resposta
```

---

## 🧪 Exemplo de Requisição

### GET - Consultar Saldo

```http
GET /conta-corrente/v4/contas/12345/saldo HTTP/1.1
Host: api.sicoob.com.br
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
client_id: dd533251-7a11-4939-8713-016763653f3c
Accept: application/json
```

### POST - Criar Cobrança PIX

```http
POST /pix/api/v2/cob HTTP/1.1
Host: api.sicoob.com.br
Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...
client_id: dd533251-7a11-4939-8713-016763653f3c
Content-Type: application/json
Accept: application/json

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
  "chave": "sua-chave-pix"
}
```

---

## 🔍 Logs de Debug

Quando você executar a aplicação com log level `Debug`, verá:

```
Headers adicionados - Authorization: Bearer eyJhbGciOiJSUzI1NiIs..., client_id: dd533251-7a11-4939-8713-016763653f3c
```

### Habilitar Logs Debug

No `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SicoobIntegration.API.Services": "Debug"
    }
  }
}
```

---

## ✅ Verificação

### Checklist de Headers

Todas as requisições incluem:

- [x] **Authorization**: Bearer token obtido via OAuth 2.0
- [x] **client_id**: UUID do cliente configurado
- [x] **Content-Type**: application/json (para POST/PUT/PATCH)
- [x] **Accept**: application/json

---

## 🚨 Erros Comuns

### ❌ Erro: "client_id header is required"

**Causa:** Header `client_id` não foi enviado

**Solução:** ✅ Já corrigido! O projeto adiciona automaticamente.

### ❌ Erro: "Invalid client_id"

**Causa:** `client_id` incorreto no `appsettings.json`

**Solução:** Verifique o `client_id` no arquivo de configuração:

```json
{
  "SicoobSettings": {
    "ClientId": "seu-client-id-correto-aqui"
  }
}
```

### ❌ Erro: "Unauthorized"

**Causa:** Token expirado ou inválido

**Solução:** O sistema renova automaticamente. Se persistir, reinicie a aplicação.

---

## 📚 Onde os Headers são Usados

### Todos os Serviços

Todos os serviços herdam de `SicoobServiceBase` e usam automaticamente os headers:

1. **CobrancaBancariaService** ✅
2. **PagamentosService** ✅
3. **ContaCorrenteService** ✅
4. **PixRecebimentosService** ✅
5. **PixPagamentosService** ✅
6. **SPBService** ✅

### Todos os Métodos HTTP

Os headers são adicionados em todos os métodos:

- ✅ `GetAsync<T>()` - GET
- ✅ `PostAsync<TRequest, TResponse>()` - POST
- ✅ `PutAsync<TRequest, TResponse>()` - PUT
- ✅ `PatchAsync<TRequest, TResponse>()` - PATCH
- ✅ `DeleteAsync()` - DELETE

---

## 🔐 Segurança

### Token Bearer

- ✅ Obtido via OAuth 2.0 com mTLS
- ✅ Renovado automaticamente antes de expirar
- ✅ Armazenado em cache (memória)
- ✅ Nunca exposto em logs (apenas primeiros 20 caracteres)

### Client ID

- ✅ Configurado no `appsettings.json`
- ✅ Validado pelo Sicoob em cada requisição
- ✅ Associado ao certificado digital

---

## 🧪 Testando

### Via Swagger

1. Acesse: `http://localhost:5148/swagger`
2. Expanda qualquer endpoint
3. Clique em "Try it out"
4. Execute

Os headers serão adicionados automaticamente! ✅

### Via cURL

```bash
# O client_id é adicionado automaticamente pela API
curl -X GET "http://localhost:5148/api/ContaCorrente/12345/saldo"
```

### Via Postman

Importe a coleção `Sicoob_API_Collection.postman_collection.json` (já configurada).

---

## 📖 Documentação Sicoob

Segundo a documentação oficial do Sicoob:

> "Todas as requisições às APIs devem incluir o header `client_id` 
> além do token de autenticação Bearer."

**Referência:** [Documentação Sicoob - Autenticação](https://developers.sicoob.com.br)

---

## ✅ Resumo

| Header | Valor | Obrigatório | Adicionado Automaticamente |
|--------|-------|-------------|----------------------------|
| **Authorization** | Bearer {token} | ✅ Sim | ✅ Sim |
| **client_id** | {uuid} | ✅ Sim | ✅ Sim |
| **Content-Type** | application/json | POST/PUT/PATCH | ✅ Sim |
| **Accept** | application/json | Recomendado | ✅ Sim |

---

## 🎯 Próximos Passos

1. ✅ Headers configurados automaticamente
2. ⬜ Configure o `client_id` no `appsettings.json`
3. ⬜ Execute a aplicação
4. ⬜ Teste os endpoints
5. ⬜ Verifique os logs para confirmar headers

---

**Status:** ✅ **HEADERS CONFIGURADOS AUTOMATICAMENTE!**  
**Data:** 2025-09-29  
**Versão:** 1.3.0

Todas as requisições agora incluem automaticamente:
- ✅ Authorization: Bearer token
- ✅ client_id: UUID do cliente

**Nada mais precisa ser feito manualmente! 🎉**

