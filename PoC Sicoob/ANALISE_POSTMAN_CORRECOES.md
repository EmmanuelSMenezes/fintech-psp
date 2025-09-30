# 📋 Análise Postman - Correções Necessárias

## 🔍 Resumo da Análise

Após análise detalhada das collections Postman fornecidas, foram identificadas **discrepâncias críticas** entre a implementação atual e as especificações reais da API Sicoob.

---

## ❌ Problemas Identificados

### 1. **Conta Corrente - Rotas Incorretas**

#### ❌ Implementação Atual:
```
GET /contas/{numeroConta}/saldo
GET /contas/{numeroConta}/extrato?dataInicio={yyyy-MM-dd}&dataFim={yyyy-MM-dd}
POST /contas/{numeroConta}/transferencias
```

#### ✅ Rotas Corretas (Postman):
```
GET /conta-corrente/v4/saldo?numeroContaCorrente={string}
GET /conta-corrente/v4/extrato/{mes}/{ano}?diaInicial={dd}&diaFinal={dd}&agruparCNAB={bool}&numeroContaCorrente={string}
POST /conta-corrente/v4/transferencias
```

**Problemas:**
- ❌ Versão da API: `/v2` → deve ser `/v4`
- ❌ Estrutura do endpoint de saldo: path param → query param
- ❌ Estrutura do endpoint de extrato: completamente diferente
- ❌ Parâmetros de data: formato ISO → mês/ano + dia inicial/final

---

### 2. **Conta Corrente - Models Incorretas**

#### ❌ SaldoResponse Atual:
```csharp
public class SaldoResultado
{
    public string Saldo { get; set; }
    public string SaldoLimite { get; set; }
    public string SaldoBloqueado { get; set; }
}
```

#### ✅ SaldoResponse Correto (Postman):
```json
{
  "resultado": {
    "saldo": 0,
    "saldoLimite": 0
  }
}
```

**Problemas:**
- ❌ Tipo: `string` → deve ser `decimal` ou `double`
- ❌ Falta: não tem `saldoBloqueado` na resposta real

---

#### ❌ ExtratoResponse Atual:
```csharp
public class ExtratoResultado
{
    public List<Lancamento> Lancamentos { get; set; }
    public string SaldoInicial { get; set; }
    public string SaldoFinal { get; set; }
}
```

#### ✅ ExtratoResponse Correto (Postman):
```json
{
  "saldoAtual": "string",
  "saldoBloqueado": "string",
  "saldoLimite": "string",
  "saldoAnterior": "string",
  "saldoBloqueioJudicial": "string",
  "saldoBloqueioJudicialAnterior": "string",
  "transacoes": [
    {
      "tipo": "string",
      "valor": "string",
      "data": "string",
      "dataLote": "string",
      "descricao": "string",
      "numeroDocumento": "string",
      "cpfCnpj": "string",
      "descInfComplementar": "string"
    }
  ]
}
```

**Problemas:**
- ❌ Estrutura completamente diferente
- ❌ Não tem wrapper `resultado`
- ❌ Campos diferentes: `lancamentos` → `transacoes`
- ❌ Faltam campos: `saldoBloqueioJudicial`, `saldoBloqueioJudicialAnterior`
- ❌ Campos de transação diferentes

---

#### ❌ TransferenciaRequest Atual:
```csharp
public class TransferenciaRequest
{
    public string ContaOrigem { get; set; }
    public string ContaDestino { get; set; }
    public decimal Valor { get; set; }
    public string Descricao { get; set; }
}
```

#### ✅ TransferenciaRequest Correto (Postman):
```json
{
  "amount": 0,
  "date": "2024-04-01T18:03:33.060Z",
  "debtorAccount": {
    "issuer": "string",
    "number": "string",
    "accountType": 0,
    "personType": 0
  },
  "creditorAccount": {
    "issuer": "string",
    "number": "string",
    "accountType": 0,
    "personType": 0
  }
}
```

**Problemas:**
- ❌ Estrutura completamente diferente
- ❌ Nomes em português → devem ser em inglês
- ❌ Faltam objetos complexos: `debtorAccount`, `creditorAccount`
- ❌ Falta campo `date`
- ❌ Faltam campos `issuer`, `accountType`, `personType`

---

### 3. **PIX - Rotas Corretas ✅**

As rotas de PIX estão **corretas**:
```
PUT /cob/:txid
GET /cob/:txid
PATCH /cob/:txid
GET /cob?inicio={ISO8601}&fim={ISO8601}&paginacao.paginaAtual={int}&paginacao.itensPorPagina={int}
```

**Models PIX estão corretas** ✅

---

### 4. **Cobrança Bancária - Rotas Corretas ✅**

```
POST /cobranca-bancaria/v3/boletos
GET /cobranca-bancaria/v3/boletos/{linhaDigitavel}
PATCH /cobranca-bancaria/v3/boletos/{linhaDigitavel}
```

#### ❌ BoletoRequest - Incompleto

**Faltam muitos campos obrigatórios:**
- `numeroCliente`, `codigoModalidade`, `numeroContaCorrente`
- `codigoEspecieDocumento`, `dataEmissao`, `nossoNumero`
- `seuNumero`, `identificacaoBoletoEmpresa`
- `identificacaoEmissaoBoleto`, `identificacaoDistribuicaoBoleto`
- `dataLimitePagamento`, `valorAbatimento`
- `tipoDesconto`, `dataPrimeiroDesconto`, `valorPrimeiroDesconto`
- `dataSegundoDesconto`, `valorSegundoDesconto`
- `dataTerceiroDesconto`, `valorTerceiroDesconto`
- `tipoMulta`, `dataMulta`, `valorMulta`
- `tipoJurosMora`, `dataJurosMora`, `valorJurosMora`
- `numeroParcela`, `aceite`
- `codigoNegativacao`, `numeroDiasNegativacao`
- `codigoProtesto`, `numeroDiasProtesto`
- `beneficiarioFinal`, `mensagensInstrucao`
- `gerarPdf`, `rateioCreditos`
- `codigoCadastrarPIX`, `numeroContratoCobranca`

---

### 5. **SPB (TED) - Rotas Corretas ✅**

```
POST /spb/v2/transferencias
GET /spb/v2/transferencias?dataInicio={yyyy-MM-dd}&dataFim={yyyy-MM-dd}
```

#### ❌ TEDRequest - Incompleto

**Faltam campos:**
- `creditor` (objeto com `personType`, `cpfCnpj`, `name`)
- `finalidade`
- `numeroPa`
- `historico`

**Estrutura de conta está diferente:**
- Atual: `FavorecidoTED` com `Banco`, `Agencia`, `Conta`, `TipoConta`
- Correto: `debtorAccount` e `creditorAccount` com `ispb`, `issuer`, `number`, `accountType`, `personType`

---

### 6. **Pagamentos - Precisa Verificar**

A collection "API-Cobranca-Bancaria-Pagamentos-3" tem apenas "EndPoints" sem detalhes.

Precisa de mais informações sobre:
- Rotas exatas
- Payloads de request/response
- Estrutura completa

---

## 📊 Resumo de Correções Necessárias

| Serviço | Rotas | Models Request | Models Response | Prioridade |
|---------|-------|----------------|-----------------|------------|
| **Conta Corrente** | ❌ Incorretas | ❌ Incorretas | ❌ Incorretas | 🔴 **CRÍTICA** |
| **PIX Recebimentos** | ✅ Corretas | ✅ Corretas | ✅ Corretas | ✅ OK |
| **Cobrança Bancária** | ✅ Corretas | ⚠️ Incompletas | ⚠️ Incompletas | 🟡 **ALTA** |
| **SPB (TED)** | ✅ Corretas | ❌ Incorretas | ⚠️ Incompletas | 🟡 **ALTA** |
| **Pagamentos** | ❓ Verificar | ❓ Verificar | ❓ Verificar | 🟡 **MÉDIA** |
| **PIX Pagamentos** | ❓ Verificar | ❓ Verificar | ❓ Verificar | 🟡 **MÉDIA** |

---

## 🎯 Próximos Passos

### 1. **Conta Corrente (CRÍTICO)** 🔴
- [ ] Corrigir rotas de `/v2` para `/v4`
- [ ] Refazer `SaldoResponse` com tipos corretos
- [ ] Refazer `ExtratoResponse` com estrutura correta
- [ ] Refazer `TransferenciaRequest` com estrutura em inglês
- [ ] Atualizar `ContaCorrenteService` com novos endpoints
- [ ] Atualizar `ContaCorrenteController` com novos parâmetros

### 2. **Cobrança Bancária (ALTA)** 🟡
- [ ] Completar `BoletoRequest` com todos os campos obrigatórios
- [ ] Adicionar models para `Pagador`, `BeneficiarioFinal`, `RateioCredito`
- [ ] Atualizar `BoletoResponse` com campos completos

### 3. **SPB/TED (ALTA)** 🟡
- [ ] Refazer `TEDRequest` com estrutura correta
- [ ] Adicionar model `Creditor`
- [ ] Atualizar estrutura de contas (`debtorAccount`, `creditorAccount`)
- [ ] Adicionar campos `finalidade`, `numeroPa`, `historico`

### 4. **Pagamentos (MÉDIA)** 🟡
- [ ] Analisar collection detalhadamente
- [ ] Extrair rotas e payloads
- [ ] Criar/atualizar models

### 5. **PIX Pagamentos (MÉDIA)** 🟡
- [ ] Analisar collection detalhadamente
- [ ] Verificar se models estão corretas
- [ ] Atualizar se necessário

---

## ⚠️ Observações Importantes

1. **Nomenclatura**: Sicoob usa **inglês** nos payloads (amount, date, debtorAccount, etc.)
2. **Tipos de dados**: Valores monetários podem ser `decimal` ou `string` dependendo do endpoint
3. **Versionamento**: Cada API tem sua versão (`/v2`, `/v3`, `/v4`)
4. **Headers obrigatórios**: `Authorization: Bearer {token}` + `client_id: {uuid}`
5. **Estrutura de resposta**: Nem todas as respostas têm wrapper `resultado`

---

**Data da Análise**: 2025-09-29  
**Status**: 🔴 **CORREÇÕES CRÍTICAS NECESSÁRIAS**

