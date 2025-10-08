using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.CobrancaBancaria;

/// <summary>
/// Request para criação de boleto bancário no Sicoob
/// </summary>
public class BoletoRequest
{
    [JsonPropertyName("numeroCliente")]
    public int NumeroCliente { get; set; }

    [JsonPropertyName("codigoModalidade")]
    public int CodigoModalidade { get; set; }

    [JsonPropertyName("numeroContaCorrente")]
    public int NumeroContaCorrente { get; set; }

    [JsonPropertyName("codigoEspecieDocumento")]
    public string CodigoEspecieDocumento { get; set; } = "DM"; // Duplicata Mercantil

    [JsonPropertyName("dataEmissao")]
    public string DataEmissao { get; set; } = string.Empty;

    [JsonPropertyName("nossoNumero")]
    public string? NossoNumero { get; set; }

    [JsonPropertyName("seuNumero")]
    public string? SeuNumero { get; set; }

    [JsonPropertyName("identificacaoBoletoEmpresa")]
    public string? IdentificacaoBoletoEmpresa { get; set; }

    [JsonPropertyName("identificacaoEmissaoBoleto")]
    public int IdentificacaoEmissaoBoleto { get; set; } = 2; // Banco emite

    [JsonPropertyName("identificacaoDistribuicaoBoleto")]
    public int IdentificacaoDistribuicaoBoleto { get; set; } = 2; // Cliente via internet

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

    // [JsonPropertyName("numeroDocumento")]
    // public string? NumeroDocumento { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorBoleto Pagador { get; set; } = new();

    [JsonPropertyName("mensagensInstrucao")]
    public List<string>? MensagensInstrucao { get; set; }

    // [JsonPropertyName("numeroContratoCobranca")]
    // public int? NumeroContratoCobranca { get; set; }

    [JsonPropertyName("gerarPdf")]
    public bool GerarPdf { get; set; } = false;

    [JsonPropertyName("aceite")]
    public bool? Aceite { get; set; }

    [JsonPropertyName("numeroParcela")]
    public int? NumeroParcela { get; set; }
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
    public string? Bairro { get; set; }

    [JsonPropertyName("cidade")]
    public string Cidade { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    // [JsonPropertyName("telefone")]
    // public string? Telefone { get; set; }
}
