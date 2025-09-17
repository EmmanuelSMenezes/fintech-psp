namespace FintechPSP.TransactionService.Services;

/// <summary>
/// Interface para serviço de geração de QR Code PIX
/// </summary>
public interface IQrCodeService
{
    /// <summary>
    /// Gera payload EMV para QR Code PIX estático
    /// </summary>
    /// <param name="pixKey">Chave PIX</param>
    /// <param name="bankCode">Código do banco</param>
    /// <param name="description">Descrição opcional</param>
    /// <returns>Payload EMV formatado</returns>
    string GenerateStaticEmvPayload(string pixKey, string bankCode, string? description = null);

    /// <summary>
    /// Gera payload EMV para QR Code PIX dinâmico
    /// </summary>
    /// <param name="pixKey">Chave PIX</param>
    /// <param name="amount">Valor da transação</param>
    /// <param name="bankCode">Código do banco</param>
    /// <param name="description">Descrição opcional</param>
    /// <param name="expiresAt">Data de expiração</param>
    /// <returns>Payload EMV formatado</returns>
    string GenerateDynamicEmvPayload(string pixKey, decimal amount, string bankCode, 
        string? description = null, DateTime? expiresAt = null);

    /// <summary>
    /// Gera imagem base64 do QR Code a partir do payload EMV
    /// </summary>
    /// <param name="emvPayload">Payload EMV</param>
    /// <returns>Imagem PNG em base64</returns>
    string GenerateQrCodeImage(string emvPayload);

    /// <summary>
    /// Valida se um payload EMV é válido
    /// </summary>
    /// <param name="emvPayload">Payload EMV</param>
    /// <returns>True se válido</returns>
    bool ValidateEmvPayload(string emvPayload);
}
