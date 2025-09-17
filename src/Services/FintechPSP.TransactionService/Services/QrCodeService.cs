using System.Globalization;
using System.Text;
using QRCoder;

namespace FintechPSP.TransactionService.Services;

/// <summary>
/// Serviço para geração de QR Code PIX com payload EMV
/// </summary>
public class QrCodeService : IQrCodeService
{
    private readonly ILogger<QrCodeService> _logger;

    public QrCodeService(ILogger<QrCodeService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string GenerateStaticEmvPayload(string pixKey, string bankCode, string? description = null)
    {
        _logger.LogDebug("Gerando payload EMV estático para PIX Key {PixKey}", pixKey);

        var payload = new StringBuilder();

        // Payload Format Indicator (ID 00) - Sempre "01"
        payload.Append("000201");

        // Point of Initiation Method (ID 01) - "12" para QR reutilizável
        payload.Append("010212");

        // Merchant Account Information (ID 26) - PIX
        var pixData = BuildPixData(pixKey, description);
        payload.Append($"26{pixData.Length:D2}{pixData}");

        // Merchant Category Code (ID 52) - "0000" para transferência
        payload.Append("52040000");

        // Transaction Currency (ID 53) - "986" para BRL
        payload.Append("5303986");

        // Country Code (ID 58) - "BR"
        payload.Append("5802BR");

        // Merchant Name (ID 59) - Nome do recebedor (limitado a 25 chars)
        var merchantName = GetMerchantName(pixKey);
        payload.Append($"59{merchantName.Length:D2}{merchantName}");

        // Merchant City (ID 60) - Cidade (padrão)
        var city = "SAO PAULO";
        payload.Append($"60{city.Length:D2}{city}");

        // Additional Data Field Template (ID 62) - Informações adicionais
        if (!string.IsNullOrEmpty(description))
        {
            var additionalData = $"05{description.Length:D2}{description}";
            payload.Append($"62{additionalData.Length:D2}{additionalData}");
        }

        // CRC16 (ID 63) - Calculado sobre todo o payload
        var payloadWithoutCrc = payload.ToString() + "6304";
        var crc = CalculateCrc16(payloadWithoutCrc);
        payload.Append($"63{crc:X4}");

        var finalPayload = payload.ToString();
        _logger.LogDebug("Payload EMV estático gerado: {Payload}", finalPayload);

        return finalPayload;
    }

    public string GenerateDynamicEmvPayload(string pixKey, decimal amount, string bankCode, 
        string? description = null, DateTime? expiresAt = null)
    {
        _logger.LogDebug("Gerando payload EMV dinâmico para PIX Key {PixKey}, valor {Amount}", pixKey, amount);

        var payload = new StringBuilder();

        // Payload Format Indicator (ID 00) - Sempre "01"
        payload.Append("000201");

        // Point of Initiation Method (ID 01) - "11" para QR de uso único
        payload.Append("010211");

        // Merchant Account Information (ID 26) - PIX
        var pixData = BuildPixData(pixKey, description);
        payload.Append($"26{pixData.Length:D2}{pixData}");

        // Merchant Category Code (ID 52) - "0000" para transferência
        payload.Append("52040000");

        // Transaction Currency (ID 53) - "986" para BRL
        payload.Append("5303986");

        // Transaction Amount (ID 54) - Valor formatado
        var amountStr = amount.ToString("F2", CultureInfo.InvariantCulture);
        payload.Append($"54{amountStr.Length:D2}{amountStr}");

        // Country Code (ID 58) - "BR"
        payload.Append("5802BR");

        // Merchant Name (ID 59) - Nome do recebedor
        var merchantName = GetMerchantName(pixKey);
        payload.Append($"59{merchantName.Length:D2}{merchantName}");

        // Merchant City (ID 60) - Cidade
        var city = "SAO PAULO";
        payload.Append($"60{city.Length:D2}{city}");

        // Additional Data Field Template (ID 62) - Informações adicionais
        var additionalDataBuilder = new StringBuilder();
        
        if (!string.IsNullOrEmpty(description))
        {
            additionalDataBuilder.Append($"05{description.Length:D2}{description}");
        }

        // Transaction ID (ID 07) - Identificador único
        var txId = Guid.NewGuid().ToString("N")[..25]; // Máximo 25 caracteres
        additionalDataBuilder.Append($"07{txId.Length:D2}{txId}");

        if (additionalDataBuilder.Length > 0)
        {
            var additionalData = additionalDataBuilder.ToString();
            payload.Append($"62{additionalData.Length:D2}{additionalData}");
        }

        // CRC16 (ID 63) - Calculado sobre todo o payload
        var payloadWithoutCrc = payload.ToString() + "6304";
        var crc = CalculateCrc16(payloadWithoutCrc);
        payload.Append($"63{crc:X4}");

        var finalPayload = payload.ToString();
        _logger.LogDebug("Payload EMV dinâmico gerado: {Payload}", finalPayload);

        return finalPayload;
    }

    public string GenerateQrCodeImage(string emvPayload)
    {
        try
        {
            _logger.LogDebug("Gerando imagem QR Code para payload de {Length} caracteres", emvPayload.Length);

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(emvPayload, QRCodeGenerator.ECCLevel.M);
            using var qrCode = new PngByteQRCode(qrCodeData);
            
            var qrCodeBytes = qrCode.GetGraphic(10); // 10 pixels por módulo
            var base64Image = Convert.ToBase64String(qrCodeBytes);

            _logger.LogDebug("Imagem QR Code gerada com {Size} bytes", qrCodeBytes.Length);

            return base64Image;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar imagem QR Code");
            throw new InvalidOperationException("Erro ao gerar imagem QR Code", ex);
        }
    }

    public bool ValidateEmvPayload(string emvPayload)
    {
        if (string.IsNullOrWhiteSpace(emvPayload))
            return false;

        try
        {
            // Verificar se começa com "000201" (Payload Format Indicator)
            if (!emvPayload.StartsWith("000201"))
                return false;

            // Verificar se termina com CRC válido
            if (emvPayload.Length < 8)
                return false;

            var payloadWithoutCrc = emvPayload[..^4] + "6304";
            var expectedCrc = CalculateCrc16(payloadWithoutCrc);
            var actualCrc = emvPayload[^4..];

            return actualCrc.Equals($"{expectedCrc:X4}", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private string BuildPixData(string pixKey, string? description)
    {
        var pixData = new StringBuilder();

        // GUI (ID 00) - Identificador do PIX
        var gui = "br.gov.bcb.pix";
        pixData.Append($"00{gui.Length:D2}{gui}");

        // PIX Key (ID 01)
        pixData.Append($"01{pixKey.Length:D2}{pixKey}");

        // Description (ID 02) - Opcional
        if (!string.IsNullOrEmpty(description))
        {
            var desc = description.Length > 72 ? description[..72] : description; // Máximo 72 caracteres
            pixData.Append($"02{desc.Length:D2}{desc}");
        }

        return pixData.ToString();
    }

    private string GetMerchantName(string pixKey)
    {
        // Simplificação: usar parte da chave PIX como nome do merchant
        // Em produção, isso deveria vir de um cadastro de usuários
        if (pixKey.Contains("@"))
        {
            var name = pixKey.Split('@')[0].ToUpper();
            return name.Length > 25 ? name[..25] : name;
        }

        if (pixKey.All(char.IsDigit))
        {
            return "USUARIO PIX";
        }

        return "FINTECH PSP";
    }

    private ushort CalculateCrc16(string data)
    {
        const ushort polynomial = 0x1021;
        var bytes = Encoding.UTF8.GetBytes(data);
        ushort crc = 0xFFFF;

        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                {
                    crc = (ushort)((crc << 1) ^ polynomial);
                }
                else
                {
                    crc <<= 1;
                }
            }
        }

        return crc;
    }
}
