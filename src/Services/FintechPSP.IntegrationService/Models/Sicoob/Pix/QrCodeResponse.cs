using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Resposta do endpoint GET /pix/api/v2/loc/{id}/qrcode do Sicoob
/// </summary>
public class QrCodeResponse
{
    /// <summary>
    /// String PIX Copia e Cola (EMV)
    /// </summary>
    [JsonPropertyName("qrcode")]
    public string? QrCode { get; set; }

    /// <summary>
    /// Imagem do QR Code em Base64
    /// </summary>
    [JsonPropertyName("imagemQrcode")]
    public string? ImagemQrCode { get; set; }
}
