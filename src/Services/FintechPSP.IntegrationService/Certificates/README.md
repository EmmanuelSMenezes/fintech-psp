# Certificados Sicoob

Esta pasta deve conter os certificados necessários para a integração com o Sicoob.

## Configuração

1. **Obtenha o certificado ICP-Brasil** (e-CNPJ ou e-CPF tipo A1) junto ao Sicoob
2. **Coloque o arquivo PFX** nesta pasta
3. **Configure o appsettings.json** com o caminho e senha do certificado:

```json
{
  "SicoobSettings": {
    "CertificatePath": "Certificates/seu-certificado.pfx",
    "CertificatePassword": "sua-senha-aqui"
  }
}
```

## Segurança

⚠️ **IMPORTANTE:**
- **Nunca commite** certificados no repositório
- **Use variáveis de ambiente** em produção
- **Proteja a senha** do certificado
- **Monitore a validade** do certificado

## Exemplo de Configuração em Produção

```bash
export SicoobSettings__CertificatePath="/app/certificates/sicoob.pfx"
export SicoobSettings__CertificatePassword="senha-segura"
```

## Estrutura Esperada

```
Certificates/
├── README.md (este arquivo)
├── sicoob-certificate.pfx (seu certificado)
└── .gitignore (para ignorar certificados)
```
