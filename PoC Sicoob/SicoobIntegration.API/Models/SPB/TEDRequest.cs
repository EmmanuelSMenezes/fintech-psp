using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.SPB;

/// <summary>
/// Request para realizar TED
/// </summary>
public class TEDRequest
{
    [JsonPropertyName("numeroContaCorrenteDebito")]
    public string NumeroContaCorrenteDebito { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("finalidade")]
    public int Finalidade { get; set; }

    [JsonPropertyName("favorecido")]
    public FavorecidoTED Favorecido { get; set; } = new();

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }
}

/// <summary>
/// Dados do favorecido da TED
/// </summary>
public class FavorecidoTED
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("codigoBanco")]
    public string CodigoBanco { get; set; } = string.Empty;

    [JsonPropertyName("numeroAgencia")]
    public string NumeroAgencia { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrente")]
    public string NumeroContaCorrente { get; set; } = string.Empty;

    [JsonPropertyName("tipoConta")]
    public string TipoConta { get; set; } = string.Empty;
}

