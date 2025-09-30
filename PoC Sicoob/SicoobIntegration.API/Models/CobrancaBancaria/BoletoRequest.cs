using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.CobrancaBancaria;

/// <summary>
/// Request para incluir boleto
/// </summary>
public class BoletoRequest
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
    public string? NossoNumero { get; set; }

    [JsonPropertyName("seuNumero")]
    public string? SeuNumero { get; set; }

    [JsonPropertyName("identificacaoBoletoEmpresa")]
    public string? IdentificacaoBoletoEmpresa { get; set; }

    [JsonPropertyName("identificacaoEmissaoBoleto")]
    public int IdentificacaoEmissaoBoleto { get; set; }

    [JsonPropertyName("identificacaoDistribuicaoBoleto")]
    public int IdentificacaoDistribuicaoBoleto { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string DataVencimento { get; set; } = string.Empty;

    [JsonPropertyName("dataLimitePagamento")]
    public string? DataLimitePagamento { get; set; }

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

    [JsonPropertyName("numeroDocumento")]
    public string? NumeroDocumento { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorBoleto Pagador { get; set; } = new();

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }
}

/// <summary>
/// Dados do pagador do boleto
/// </summary>
public class PagadorBoleto
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("bairro")]
    public string Bairro { get; set; } = string.Empty;

    [JsonPropertyName("cidade")]
    public string Cidade { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;
}

/// <summary>
/// Request para alterar boleto
/// </summary>
public class BoletoAlteracaoRequest
{
    [JsonPropertyName("numeroContrato")]
    public string? NumeroContrato { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string? DataVencimento { get; set; }

    [JsonPropertyName("valor")]
    public decimal? Valor { get; set; }

    [JsonPropertyName("valorAbatimento")]
    public decimal? ValorAbatimento { get; set; }

    [JsonPropertyName("tipoDesconto")]
    public int? TipoDesconto { get; set; }

    [JsonPropertyName("valorPrimeiroDesconto")]
    public decimal? ValorPrimeiroDesconto { get; set; }

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }
}

