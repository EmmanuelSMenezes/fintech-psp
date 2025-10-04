using FintechPSP.IntegrationService.Models.Sicoob.Pix;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Interface para serviço de geração de QR Code PIX
/// </summary>
public interface IPixQrCodeService
{
    /// <summary>
    /// Gera QR Code PIX baseado nos dados da cobrança
    /// </summary>
    /// <param name="cobranca">Dados da cobrança PIX</param>
    /// <returns>Dados do QR Code gerado</returns>
    Task<PixQrCodeResult> GerarQrCodeAsync(CobrancaResponse cobranca);

    /// <summary>
    /// Gera PIX Copia e Cola baseado nos dados da cobrança
    /// </summary>
    /// <param name="cobranca">Dados da cobrança PIX</param>
    /// <returns>String PIX Copia e Cola</returns>
    Task<string> GerarPixCopiaEColaAsync(CobrancaResponse cobranca);
}

/// <summary>
/// Resultado da geração do QR Code PIX
/// </summary>
public class PixQrCodeResult
{
    /// <summary>
    /// String PIX Copia e Cola (EMV)
    /// </summary>
    public string PixCopiaECola { get; set; } = string.Empty;

    /// <summary>
    /// QR Code em formato Base64 (imagem PNG)
    /// </summary>
    public string QrCodeBase64 { get; set; } = string.Empty;

    /// <summary>
    /// URL do QR Code (se disponível)
    /// </summary>
    public string? QrCodeUrl { get; set; }

    /// <summary>
    /// Dados adicionais do QR Code
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
