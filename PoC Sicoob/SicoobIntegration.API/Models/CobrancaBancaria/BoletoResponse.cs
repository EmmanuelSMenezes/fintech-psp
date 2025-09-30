using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.CobrancaBancaria;

/// <summary>
/// Resposta da API de boleto
/// </summary>
public class BoletoResponse
{
    [JsonPropertyName("numeroContrato")]
    public string NumeroContrato { get; set; } = string.Empty;

    [JsonPropertyName("modalidade")]
    public int Modalidade { get; set; }

    [JsonPropertyName("numeroContaCorrente")]
    public string NumeroContaCorrente { get; set; } = string.Empty;

    [JsonPropertyName("especieDocumento")]
    public string EspecieDocumento { get; set; } = string.Empty;

    [JsonPropertyName("dataEmissao")]
    public string DataEmissao { get; set; } = string.Empty;

    [JsonPropertyName("nossoNumero")]
    public string NossoNumero { get; set; } = string.Empty;

    [JsonPropertyName("seuNumero")]
    public string? SeuNumero { get; set; }

    [JsonPropertyName("identificacaoBoletoEmpresa")]
    public string? IdentificacaoBoletoEmpresa { get; set; }

    [JsonPropertyName("codigoBarras")]
    public string? CodigoBarras { get; set; }

    [JsonPropertyName("linhaDigitavel")]
    public string? LinhaDigitavel { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string DataVencimento { get; set; } = string.Empty;

    [JsonPropertyName("dataLimitePagamento")]
    public string? DataLimitePagamento { get; set; }

    [JsonPropertyName("valorAbatimento")]
    public decimal? ValorAbatimento { get; set; }

    [JsonPropertyName("situacao")]
    public string? Situacao { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorBoleto Pagador { get; set; } = new();

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }
}

/// <summary>
/// Lista de boletos
/// </summary>
public class ListaBoletosResponse
{
    [JsonPropertyName("boletos")]
    public List<BoletoResponse> Boletos { get; set; } = new();

    [JsonPropertyName("totalRegistros")]
    public int TotalRegistros { get; set; }

    [JsonPropertyName("paginaAtual")]
    public int PaginaAtual { get; set; }

    [JsonPropertyName("totalPaginas")]
    public int TotalPaginas { get; set; }
}

