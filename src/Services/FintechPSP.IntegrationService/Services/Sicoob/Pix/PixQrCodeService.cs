using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using System.Text;
using System.Globalization;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Serviço para geração de QR Code PIX baseado no padrão EMV
/// </summary>
public class PixQrCodeService : IPixQrCodeService
{
    private readonly ILogger<PixQrCodeService> _logger;

    public PixQrCodeService(ILogger<PixQrCodeService> logger)
    {
        _logger = logger;
    }

    public async Task<PixQrCodeResult> GerarQrCodeAsync(CobrancaResponse cobranca)
    {
        try
        {
            _logger.LogInformation("Gerando QR Code PIX para TxId: {TxId}", cobranca.TxId);

            var pixCopiaECola = await GerarPixCopiaEColaAsync(cobranca);
            
            var result = new PixQrCodeResult
            {
                PixCopiaECola = pixCopiaECola,
                QrCodeBase64 = GerarQrCodeBase64(pixCopiaECola),
                QrCodeUrl = cobranca.Location ?? cobranca.Loc?.Location,
                AdditionalData = new Dictionary<string, object>
                {
                    ["txId"] = cobranca.TxId ?? "",
                    ["valor"] = cobranca.Valor?.Original ?? "",
                    ["chave"] = cobranca.Chave ?? "",
                    ["status"] = cobranca.Status ?? "",
                    ["expiracao"] = cobranca.Calendario?.Expiracao ?? 0
                }
            };

            _logger.LogInformation("QR Code PIX gerado com sucesso. TxId: {TxId}, Tamanho: {Tamanho} caracteres", 
                cobranca.TxId, pixCopiaECola.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar QR Code PIX para TxId: {TxId}", cobranca.TxId);
            throw;
        }
    }

    public async Task<string> GerarPixCopiaEColaAsync(CobrancaResponse cobranca)
    {
        try
        {
            _logger.LogInformation("Gerando PIX Copia e Cola para TxId: {TxId}", cobranca.TxId);

            // Construir string PIX Copia e Cola baseado no padrão EMV
            var pixString = ConstruirPixEmv(cobranca);

            _logger.LogInformation("PIX Copia e Cola gerado com sucesso. TxId: {TxId}", cobranca.TxId);

            return await Task.FromResult(pixString);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar PIX Copia e Cola para TxId: {TxId}", cobranca.TxId);
            throw;
        }
    }

    private string ConstruirPixEmv(CobrancaResponse cobranca)
    {
        var emv = new StringBuilder();

        // Payload Format Indicator (ID 00) - Sempre "01"
        emv.Append(FormatarCampoEmv("00", "01"));

        // Point of Initiation Method (ID 01) - "12" para QR Code dinâmico
        emv.Append(FormatarCampoEmv("01", "12"));

        // Merchant Account Information (ID 26) - Dados do PIX
        var merchantInfo = new StringBuilder();
        merchantInfo.Append(FormatarCampoEmv("00", "br.gov.bcb.pix")); // GUI
        merchantInfo.Append(FormatarCampoEmv("01", cobranca.Chave ?? "")); // Chave PIX
        
        if (!string.IsNullOrEmpty(cobranca.TxId))
        {
            merchantInfo.Append(FormatarCampoEmv("02", cobranca.TxId)); // TxId
        }

        emv.Append(FormatarCampoEmv("26", merchantInfo.ToString()));

        // Merchant Category Code (ID 52) - "0000" para pessoa física
        emv.Append(FormatarCampoEmv("52", "0000"));

        // Transaction Currency (ID 53) - "986" para Real brasileiro
        emv.Append(FormatarCampoEmv("53", "986"));

        // Transaction Amount (ID 54) - Valor da transação
        if (cobranca.Valor != null && !string.IsNullOrEmpty(cobranca.Valor.Original))
        {
            var valor = decimal.Parse(cobranca.Valor.Original, CultureInfo.InvariantCulture);
            emv.Append(FormatarCampoEmv("54", valor.ToString("F2", CultureInfo.InvariantCulture)));
        }

        // Country Code (ID 58) - "BR" para Brasil
        emv.Append(FormatarCampoEmv("58", "BR"));

        // Merchant Name (ID 59) - Nome do recebedor
        var merchantName = cobranca.Recebedor?.Nome ?? "OWAYPAY SOLUCOES DE PAGAMENTOS LTDA";
        emv.Append(FormatarCampoEmv("59", merchantName));

        // Merchant City (ID 60) - Cidade do recebedor
        var merchantCity = cobranca.Recebedor?.Cidade ?? "SAO PAULO";
        emv.Append(FormatarCampoEmv("60", merchantCity));

        // Additional Data Field Template (ID 62) - Informações adicionais
        var additionalData = new StringBuilder();
        if (!string.IsNullOrEmpty(cobranca.TxId))
        {
            additionalData.Append(FormatarCampoEmv("05", cobranca.TxId)); // Reference Label
        }
        
        if (additionalData.Length > 0)
        {
            emv.Append(FormatarCampoEmv("62", additionalData.ToString()));
        }

        // CRC16 (ID 63) - Será calculado e adicionado
        var payload = emv.ToString() + "6304";
        var crc = CalcularCrc16(payload);
        emv.Append(FormatarCampoEmv("63", crc.ToString("X4")));

        return emv.ToString();
    }

    private string FormatarCampoEmv(string id, string valor)
    {
        var tamanho = valor.Length.ToString("D2");
        return $"{id}{tamanho}{valor}";
    }

    private ushort CalcularCrc16(string data)
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

    private string GerarQrCodeBase64(string pixCopiaECola)
    {
        try
        {
            // Por enquanto, retornar uma string indicativa
            // Em uma implementação completa, usaria uma biblioteca como QRCoder
            // para gerar a imagem do QR Code em Base64
            
            _logger.LogInformation("Gerando QR Code Base64 para PIX Copia e Cola de {Tamanho} caracteres", 
                pixCopiaECola.Length);

            // Placeholder - em produção, implementar geração real do QR Code
            return $"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8/5+hHgAHggJ/PchI7wAAAABJRU5ErkJggg==";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar QR Code Base64");
            return string.Empty;
        }
    }
}
