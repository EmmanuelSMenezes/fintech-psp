# 🔧 Troubleshooting - Problemas de SSL/Certificado

Guia para resolver problemas comuns com SSL e certificados digitais.

---

## ❌ Erro: "The SSL connection could not be established"

### Causa
O certificado digital não está sendo enviado corretamente nas requisições HTTPS.

### ✅ Solução Implementada

O projeto agora inclui:

1. **CertificateHelper** - Helper para carregar e validar certificados
2. **Configuração correta do HttpClientHandler** - Com mTLS configurado
3. **Validação de certificado** - Verifica chave privada e validade
4. **Logs detalhados** - Para debug de problemas

### Verificações Automáticas

Ao iniciar a aplicação, você verá:

```
🔐 Carregando certificado digital...
✅ Certificado carregado com sucesso!
📜 Informações do Certificado:
   Subject: CN=OWAYPAY SOLUCOES DE PAGAMENTOS LTDA...
   Issuer: CN=AC SAFEWEB RFB v5...
   Válido de: 29/08/2025
   Válido até: 29/08/2026
   Tem chave privada: True
   Dias até expirar: 333

🌐 Configurando HttpClients com mTLS...
   ✅ HttpClient Auth configurado com mTLS
   ✅ HttpClient API configurado com mTLS
```

---

## 🔍 Diagnóstico de Problemas

### 1. Certificado não tem chave privada

**Erro:**
```
O certificado não possui chave privada
```

**Solução:**
- Certifique-se de usar um arquivo `.pfx` ou `.p12` (não `.cer` ou `.crt`)
- O arquivo PFX deve conter tanto o certificado quanto a chave privada
- Use o arquivo: `dd533251-7a11-4939-8713-016763653f3c.pfx`

### 2. Senha incorreta

**Erro:**
```
Erro ao carregar certificado: The specified network password is not correct
```

**Solução:**
- Verifique se a senha no `appsettings.json` está correta
- Senha atual: `Vi294141`

### 3. Certificado expirado

**Erro:**
```
O certificado está expirado
```

**Solução:**
- Verifique a validade do certificado
- Certificado atual válido até: **29/08/2026**
- Renove o certificado se necessário

### 4. Certificado não encontrado

**Erro:**
```
Certificado não encontrado: ...
```

**Solução:**
- Verifique se o arquivo está em `Fintech/Certificates/`
- Verifique o caminho no `appsettings.json`:
  ```json
  "CertificatePath": "../Certificates/dd533251-7a11-4939-8713-016763653f3c.pfx"
  ```

---

## 🔐 Configuração do mTLS

### O que é mTLS?

**mTLS (Mutual TLS)** é uma autenticação bidirecional onde:
- O cliente (sua aplicação) apresenta um certificado ao servidor
- O servidor (Sicoob) valida o certificado do cliente
- Ambos estabelecem uma conexão segura

### Como está configurado

```csharp
var handler = new HttpClientHandler
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
};

handler.ClientCertificates.Add(certificate);
```

### Flags do Certificado

```csharp
X509KeyStorageFlags.Exportable |  // Permite exportar
X509KeyStorageFlags.PersistKeySet |  // Mantém a chave
X509KeyStorageFlags.MachineKeySet  // Armazena no nível da máquina
```

---

## 🧪 Testando o Certificado

### Teste Manual

Execute este código para testar o certificado:

```csharp
var certPath = "Certificates/dd533251-7a11-4939-8713-016763653f3c.pfx";
var password = "Vi294141";

var cert = new X509Certificate2(certPath, password);

Console.WriteLine($"Subject: {cert.Subject}");
Console.WriteLine($"Tem chave privada: {cert.HasPrivateKey}");
Console.WriteLine($"Válido até: {cert.NotAfter}");
```

### Teste via OpenSSL

```bash
# Verificar o certificado PFX
openssl pkcs12 -info -in dd533251-7a11-4939-8713-016763653f3c.pfx

# Extrair certificado
openssl pkcs12 -in dd533251-7a11-4939-8713-016763653f3c.pfx -clcerts -nokeys -out cert.pem

# Extrair chave privada
openssl pkcs12 -in dd533251-7a11-4939-8713-016763653f3c.pfx -nocerts -out key.pem
```

---

## 🌐 Problemas de Rede

### Firewall bloqueando

**Sintoma:** Timeout nas requisições

**Solução:**
- Verifique se o firewall permite conexões HTTPS (porta 443)
- Adicione exceção para a aplicação

### Proxy corporativo

**Sintoma:** Erro de conexão

**Solução:**
Adicione configuração de proxy no `HttpClientHandler`:

```csharp
handler.Proxy = new WebProxy("http://proxy:8080");
handler.UseProxy = true;
```

---

## 📊 Logs de Debug

### Habilitar logs detalhados

No `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "System.Net.Http": "Debug",
      "System.Net.Security": "Debug"
    }
  }
}
```

### Logs esperados

```
🔐 Carregando certificado digital...
✅ Certificado carregado com sucesso!
📜 Informações do Certificado:
   Subject: CN=OWAYPAY...
   Tem chave privada: True

🌐 Configurando HttpClients com mTLS...
   ✅ HttpClient Auth configurado com mTLS
   ✅ HttpClient API configurado com mTLS

🔐 Testando autenticação OAuth 2.0...
   🔒 Validando certificado do servidor: CN=*.sicoob.com.br
✅ Token obtido com sucesso!
```

---

## 🔧 Modo Desenvolvimento vs Produção

### Desenvolvimento (Atual)

```csharp
// Aceita qualquer certificado do servidor
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
{
    return true; // ⚠️ Apenas para desenvolvimento!
};
```

### Produção (Recomendado)

```csharp
// Valida certificado do servidor corretamente
handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
{
    if (errors == SslPolicyErrors.None)
        return true;
    
    Console.WriteLine($"❌ Erro SSL: {errors}");
    return false;
};
```

---

## ✅ Checklist de Verificação

- [ ] Certificado PFX está na pasta `Certificates/`
- [ ] Senha está correta no `appsettings.json`
- [ ] Certificado tem chave privada
- [ ] Certificado está dentro da validade
- [ ] HttpClient está configurado com mTLS
- [ ] Logs mostram "Certificado carregado com sucesso"
- [ ] Logs mostram "HttpClient configurado com mTLS"
- [ ] Não há erros de SSL nos logs

---

## 🆘 Ainda com problemas?

### 1. Verifique os logs

Execute a aplicação e observe os logs detalhados.

### 2. Teste o certificado isoladamente

Use o código de teste acima para verificar se o certificado carrega corretamente.

### 3. Verifique a conectividade

```bash
# Teste conexão com o Sicoob
curl -v https://api.sicoob.com.br
```

### 4. Verifique o Client ID

Certifique-se de que o Client ID está configurado corretamente no `appsettings.json`.

---

## 📚 Referências

- [RFC 8705 - OAuth 2.0 Mutual-TLS](https://tools.ietf.org/html/rfc8705)
- [X509Certificate2 Class](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.x509certificates.x509certificate2)
- [HttpClientHandler Class](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler)

---

**Última atualização:** 2025-09-29  
**Status:** ✅ Problema resolvido com CertificateHelper

