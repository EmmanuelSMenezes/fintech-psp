using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.Pix;

/// <summary>
/// Resposta da API de cobrança PIX
/// </summary>
public class CobrancaResponse
{
    [JsonPropertyName("calendario")]
    public Calendario Calendario { get; set; } = new();

    [JsonPropertyName("txid")]
    public string Txid { get; set; } = string.Empty;

    [JsonPropertyName("revisao")]
    public int Revisao { get; set; }

    [JsonPropertyName("loc")]
    public Loc? Loc { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("devedor")]
    public Devedor? Devedor { get; set; }

    [JsonPropertyName("valor")]
    public Valor Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    [JsonPropertyName("solicitacaoPagador")]
    public string? SolicitacaoPagador { get; set; }

    [JsonPropertyName("infoAdicionais")]
    public List<InfoAdicional>? InfoAdicionais { get; set; }

    [JsonPropertyName("pixCopiaECola")]
    public string? PixCopiaECola { get; set; }
}

/// <summary>
/// Localização da cobrança
/// </summary>
public class Loc
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("tipoCob")]
    public string TipoCob { get; set; } = string.Empty;

    [JsonPropertyName("criacao")]
    public string? Criacao { get; set; }
}

/// <summary>
/// Lista de cobranças
/// </summary>
public class ListaCobrancasResponse
{
    [JsonPropertyName("parametros")]
    public Parametros Parametros { get; set; } = new();

    [JsonPropertyName("cobs")]
    public List<CobrancaResponse> Cobs { get; set; } = new();
}

/// <summary>
/// Parâmetros da consulta
/// </summary>
public class Parametros
{
    [JsonPropertyName("inicio")]
    public string Inicio { get; set; } = string.Empty;

    [JsonPropertyName("fim")]
    public string Fim { get; set; } = string.Empty;

    [JsonPropertyName("paginacao")]
    public Paginacao Paginacao { get; set; } = new();
}

/// <summary>
/// Informações de paginação
/// </summary>
public class Paginacao
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

