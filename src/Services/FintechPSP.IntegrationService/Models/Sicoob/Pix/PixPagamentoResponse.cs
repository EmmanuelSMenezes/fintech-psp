using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Resposta do pagamento PIX
/// </summary>
public class PixPagamentoResponse
{
    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    [JsonPropertyName("txid")]
    public string? Txid { get; set; }

    [JsonPropertyName("valor")]
    public string? Valor { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("horario")]
    public string? Horario { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorPix? Pagador { get; set; }

    [JsonPropertyName("favorecido")]
    public FavorecidoPix? Favorecido { get; set; }

    [JsonPropertyName("infoPagador")]
    public string? InfoPagador { get; set; }
}



/// <summary>
/// Lista de pagamentos PIX
/// </summary>
public class ListaPixPagamentosResponse
{
    [JsonPropertyName("pagamentos")]
    public List<PixPagamentoResponse> Pagamentos { get; set; } = new();

    [JsonPropertyName("parametros")]
    public ParametrosPaginacao? Parametros { get; set; }
}

/// <summary>
/// Parâmetros de paginação
/// </summary>
public class ParametrosPaginacao
{
    [JsonPropertyName("inicio")]
    public DateTime Inicio { get; set; }

    [JsonPropertyName("fim")]
    public DateTime Fim { get; set; }

    [JsonPropertyName("paginacao")]
    public PaginacaoInfo? Paginacao { get; set; }
}

/// <summary>
/// Informações de paginação
/// </summary>
public class PaginacaoInfo
{
    [JsonPropertyName("paginaAtual")]
    public int PaginaAtual { get; set; }

    [JsonPropertyName("itensPorPagina")]
    public int ItensPorPagina { get; set; }

    [JsonPropertyName("quantidadeDePaginas")]
    public int QuantidadeDePaginas { get; set; }

    [JsonPropertyName("quantidadeTotalDeItens")]
    public int QuantidadeTotalDeItens { get; set; }
}
