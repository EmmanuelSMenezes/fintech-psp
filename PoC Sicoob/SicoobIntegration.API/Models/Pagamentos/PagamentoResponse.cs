using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.Pagamentos;

/// <summary>
/// Resposta da API de pagamento
/// </summary>
public class PagamentoResponse
{
    [JsonPropertyName("idPagamento")]
    public string IdPagamento { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrente")]
    public string NumeroContaCorrente { get; set; } = string.Empty;

    [JsonPropertyName("codigoBarras")]
    public string CodigoBarras { get; set; } = string.Empty;

    [JsonPropertyName("dataPagamento")]
    public string DataPagamento { get; set; } = string.Empty;

    [JsonPropertyName("valorPagamento")]
    public decimal ValorPagamento { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("dataInclusao")]
    public string? DataInclusao { get; set; }

    [JsonPropertyName("dataProcessamento")]
    public string? DataProcessamento { get; set; }
}

