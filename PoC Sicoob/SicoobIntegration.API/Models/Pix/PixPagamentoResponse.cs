using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.Pix;

/// <summary>
/// Resposta da API de pagamento PIX
/// </summary>
public class PixPagamentoResponse
{
    [JsonPropertyName("endToEndId")]
    public string EndToEndId { get; set; } = string.Empty;

    [JsonPropertyName("txid")]
    public string? Txid { get; set; }

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("horario")]
    public string Horario { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

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
    [JsonPropertyName("parametros")]
    public ParametrosPix Parametros { get; set; } = new();

    [JsonPropertyName("pix")]
    public List<PixPagamentoResponse> Pix { get; set; } = new();
}

/// <summary>
/// Parâmetros da consulta de pagamentos PIX
/// </summary>
public class ParametrosPix
{
    [JsonPropertyName("inicio")]
    public string Inicio { get; set; } = string.Empty;

    [JsonPropertyName("fim")]
    public string Fim { get; set; } = string.Empty;

    [JsonPropertyName("paginacao")]
    public PaginacaoPix Paginacao { get; set; } = new();
}

/// <summary>
/// Informações de paginação PIX
/// </summary>
public class PaginacaoPix
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

