using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Resposta da cobrança PIX
/// </summary>
public class CobrancaResponse
{
    [JsonPropertyName("txid")]
    public string? TxId { get; set; }

    [JsonPropertyName("revisao")]
    public int Revisao { get; set; }

    [JsonPropertyName("loc")]
    public LocationInfo? Loc { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("devedor")]
    public DevedorCobranca? Devedor { get; set; }

    [JsonPropertyName("recebedor")]
    public RecebedorCobranca? Recebedor { get; set; }

    [JsonPropertyName("valor")]
    public ValorCobranca? Valor { get; set; }

    [JsonPropertyName("chave")]
    public string? Chave { get; set; }

    [JsonPropertyName("solicitacaoPagador")]
    public string? SolicitacaoPagador { get; set; }

    [JsonPropertyName("infoAdicionais")]
    public List<InfoAdicional>? InfoAdicionais { get; set; }

    [JsonPropertyName("calendario")]
    public CalendarioCobrancaResponse? Calendario { get; set; }

    [JsonPropertyName("pixCopiaECola")]
    public string? PixCopiaECola { get; set; }

    [JsonPropertyName("qrcode")]
    public string? QrCode { get; set; }
}

/// <summary>
/// Informações de localização
/// </summary>
public class LocationInfo
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("tipoCob")]
    public string? TipoCob { get; set; }

    [JsonPropertyName("criacao")]
    public DateTime? Criacao { get; set; }
}

/// <summary>
/// Dados do recebedor
/// </summary>
public class RecebedorCobranca
{
    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("cidade")]
    public string? Cidade { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

/// <summary>
/// Calendário na resposta
/// </summary>
public class CalendarioCobrancaResponse
{
    [JsonPropertyName("criacao")]
    public DateTime Criacao { get; set; }

    [JsonPropertyName("expiracao")]
    public int Expiracao { get; set; }

    [JsonPropertyName("dataDeVencimento")]
    public string? DataDeVencimento { get; set; }

    [JsonPropertyName("validadeAposVencimento")]
    public int? ValidadeAposVencimento { get; set; }
}
