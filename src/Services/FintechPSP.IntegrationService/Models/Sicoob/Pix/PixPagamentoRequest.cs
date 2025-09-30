using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Request para realizar pagamento PIX
/// </summary>
public class PixPagamentoRequest
{
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("pagador")]
    public PagadorPix Pagador { get; set; } = new();

    [JsonPropertyName("favorecido")]
    public FavorecidoPix Favorecido { get; set; } = new();

    [JsonPropertyName("infoPagador")]
    public string? InfoPagador { get; set; }
}

/// <summary>
/// Dados do pagador PIX
/// </summary>
public class PagadorPix
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("contaCorrente")]
    public string ContaCorrente { get; set; } = string.Empty;
}

/// <summary>
/// Dados do favorecido PIX
/// </summary>
public class FavorecidoPix
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;
}
