using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.Pagamentos;

/// <summary>
/// Request para incluir pagamento de boleto
/// </summary>
public class PagamentoBoletoRequest
{
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
}

/// <summary>
/// Request para alterar pagamento
/// </summary>
public class PagamentoAlteracaoRequest
{
    [JsonPropertyName("dataPagamento")]
    public string? DataPagamento { get; set; }

    [JsonPropertyName("valorPagamento")]
    public decimal? ValorPagamento { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }
}

