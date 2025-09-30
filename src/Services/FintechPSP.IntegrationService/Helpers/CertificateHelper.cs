using System.Security.Cryptography.X509Certificates;

namespace FintechPSP.IntegrationService.Helpers;

/// <summary>
/// Helper para carregar e validar certificados digitais
/// </summary>
public static class CertificateHelper
{
    /// <summary>
    /// Carrega um certificado PFX do arquivo
    /// </summary>
    public static X509Certificate2 LoadCertificate(string path, string password)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Certificado não encontrado: {path}");
        }

        try
        {
            // Lê o arquivo PFX
            var certBytes = File.ReadAllBytes(path);

            // Carrega o certificado com as flags corretas para Windows
            // UserKeySet é mais confiável que MachineKeySet para mTLS
            var certificate = new X509Certificate2(
                certBytes,
                password,
                X509KeyStorageFlags.Exportable |
                X509KeyStorageFlags.PersistKeySet |
                X509KeyStorageFlags.UserKeySet);  // Mudado de MachineKeySet para UserKeySet

            // Valida o certificado
            ValidateCertificate(certificate);

            Console.WriteLine($"   🔑 Chave privada carregada: {certificate.HasPrivateKey}");
            Console.WriteLine($"   📋 Tipo de chave: {certificate.PrivateKey?.GetType().Name ?? "N/A"}");

            return certificate;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Erro ao carregar certificado: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Valida se o certificado está correto e válido
    /// </summary>
    public static void ValidateCertificate(X509Certificate2 certificate)
    {
        // Verifica se tem chave privada
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException(
                "O certificado não possui chave privada. " +
                "Certifique-se de usar um arquivo PFX/P12 completo.");
        }

        // Verifica validade
        var now = DateTime.Now;
        if (now < certificate.NotBefore)
        {
            throw new InvalidOperationException(
                $"O certificado ainda não é válido. Válido a partir de: {certificate.NotBefore:dd/MM/yyyy}");
        }

        if (now > certificate.NotAfter)
        {
            throw new InvalidOperationException(
                $"O certificado está expirado. Válido até: {certificate.NotAfter:dd/MM/yyyy}");
        }

        // Avisa se está próximo de expirar (30 dias)
        var daysUntilExpiry = (certificate.NotAfter - now).Days;
        if (daysUntilExpiry <= 30)
        {
            Console.WriteLine($"⚠️  AVISO: Certificado expira em {daysUntilExpiry} dias!");
        }
    }

    /// <summary>
    /// Exibe informações do certificado
    /// </summary>
    public static void PrintCertificateInfo(X509Certificate2 certificate)
    {
        Console.WriteLine($"📜 Informações do Certificado:");
        Console.WriteLine($"   Subject: {certificate.Subject}");
        Console.WriteLine($"   Issuer: {certificate.Issuer}");
        Console.WriteLine($"   Serial Number: {certificate.SerialNumber}");
        Console.WriteLine($"   Válido de: {certificate.NotBefore:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"   Válido até: {certificate.NotAfter:dd/MM/yyyy HH:mm:ss}");
        Console.WriteLine($"   Tem chave privada: {certificate.HasPrivateKey}");
        Console.WriteLine($"   Algoritmo: {certificate.SignatureAlgorithm.FriendlyName}");
        
        var daysUntilExpiry = (certificate.NotAfter - DateTime.Now).Days;
        Console.WriteLine($"   Dias até expirar: {daysUntilExpiry}");
    }

    /// <summary>
    /// Cria um HttpClientHandler configurado com o certificado
    /// </summary>
    public static HttpClientHandler CreateHttpClientHandler(X509Certificate2 certificate, bool validateServerCertificate = false)
    {
        // Verifica se o certificado tem chave privada
        if (!certificate.HasPrivateKey)
        {
            throw new InvalidOperationException("O certificado não possui chave privada!");
        }

        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12 |
                          System.Security.Authentication.SslProtocols.Tls13,
            CheckCertificateRevocationList = false, // Desabilita verificação de revogação
            UseDefaultCredentials = false,
            PreAuthenticate = false
        };

        // Limpa certificados existentes
        handler.ClientCertificates.Clear();

        // Adiciona o certificado
        handler.ClientCertificates.Add(certificate);

        Console.WriteLine($"   📋 Certificado adicionado ao handler");
        Console.WriteLine($"   🔑 Subject: {certificate.Subject.Substring(0, Math.Min(50, certificate.Subject.Length))}...");
        Console.WriteLine($"   🔑 Thumbprint: {certificate.Thumbprint}");

        // Configuração de validação do certificado do servidor
        if (!validateServerCertificate)
        {
            // Modo desenvolvimento: aceita qualquer certificado
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (errors != System.Net.Security.SslPolicyErrors.None)
                {
                    Console.WriteLine($"⚠️  Aviso SSL: {errors}");
                    if (cert != null)
                    {
                        Console.WriteLine($"   Servidor: {cert.Subject}");
                    }
                }
                return true;
            };
        }
        else
        {
            // Modo produção: valida certificado corretamente
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (errors == System.Net.Security.SslPolicyErrors.None)
                {
                    return true;
                }

                Console.WriteLine($"❌ Erro de validação SSL: {errors}");
                if (cert != null)
                {
                    Console.WriteLine($"   Certificado do servidor: {cert.Subject}");
                }
                return false;
            };
        }

        return handler;
    }
}
