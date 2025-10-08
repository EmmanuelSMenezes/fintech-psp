using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.CobrancaBancaria;

/// <summary>
/// Resposta da API de boleto do Sicoob
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

    [JsonPropertyName("tipoDesconto")]
    public int? TipoDesconto { get; set; }

    [JsonPropertyName("dataPrimeiroDesconto")]
    public string? DataPrimeiroDesconto { get; set; }

    [JsonPropertyName("valorPrimeiroDesconto")]
    public decimal? ValorPrimeiroDesconto { get; set; }

    [JsonPropertyName("tipoJurosMora")]
    public int? TipoJurosMora { get; set; }

    [JsonPropertyName("valorJurosMora")]
    public decimal? ValorJurosMora { get; set; }

    [JsonPropertyName("tipoMulta")]
    public int? TipoMulta { get; set; }

    [JsonPropertyName("valorMulta")]
    public decimal? ValorMulta { get; set; }

    [JsonPropertyName("numeroDocumento")]
    public string? NumeroDocumento { get; set; }

    [JsonPropertyName("identificacaoEmissaoBoleto")]
    public int IdentificacaoEmissaoBoleto { get; set; }

    [JsonPropertyName("identificacaoDistribuicaoBoleto")]
    public int IdentificacaoDistribuicaoBoleto { get; set; }

    [JsonPropertyName("urlBoleto")]
    public string? UrlBoleto { get; set; }

    [JsonPropertyName("dataPagamento")]
    public string? DataPagamento { get; set; }

    [JsonPropertyName("valorPago")]
    public decimal? ValorPago { get; set; }
}

/// <summary>
/// Request para alteração de boleto
/// </summary>
public class BoletoAlteracaoRequest
{
    [JsonPropertyName("dataVencimento")]
    public string? DataVencimento { get; set; }

    [JsonPropertyName("valorAbatimento")]
    public decimal? ValorAbatimento { get; set; }

    [JsonPropertyName("tipoDesconto")]
    public int? TipoDesconto { get; set; }

    [JsonPropertyName("dataPrimeiroDesconto")]
    public string? DataPrimeiroDesconto { get; set; }

    [JsonPropertyName("valorPrimeiroDesconto")]
    public decimal? ValorPrimeiroDesconto { get; set; }

    [JsonPropertyName("tipoJurosMora")]
    public int? TipoJurosMora { get; set; }

    [JsonPropertyName("valorJurosMora")]
    public decimal? ValorJurosMora { get; set; }

    [JsonPropertyName("tipoMulta")]
    public int? TipoMulta { get; set; }

    [JsonPropertyName("valorMulta")]
    public decimal? ValorMulta { get; set; }

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }
}
