using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.SPB;

/// <summary>
/// Resposta da API de TED
/// </summary>
public class TEDResponse
{
    [JsonPropertyName("idTransferencia")]
    public string IdTransferencia { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrenteDebito")]
    public string NumeroContaCorrenteDebito { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("finalidade")]
    public int Finalidade { get; set; }

    [JsonPropertyName("dataTransferencia")]
    public string DataTransferencia { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("favorecido")]
    public FavorecidoTED Favorecido { get; set; } = new();

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("numeroDocumento")]
    public string? NumeroDocumento { get; set; }
}

/// <summary>
/// Lista de TEDs
/// </summary>
public class ListaTEDsResponse
{
    [JsonPropertyName("transferencias")]
    public List<TEDResponse> Transferencias { get; set; } = new();

    [JsonPropertyName("totalRegistros")]
    public int TotalRegistros { get; set; }

    [JsonPropertyName("paginaAtual")]
    public int PaginaAtual { get; set; }

    [JsonPropertyName("totalPaginas")]
    public int TotalPaginas { get; set; }
}

