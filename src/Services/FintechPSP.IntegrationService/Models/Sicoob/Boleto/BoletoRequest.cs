using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Boleto;

/// <summary>
/// Request para emissão de boleto no Sicoob
/// </summary>
public class BoletoRequest
{
    [JsonPropertyName("numeroContrato")]
    public string NumeroContrato { get; set; } = string.Empty;

    [JsonPropertyName("modalidade")]
    public int Modalidade { get; set; } = 1; // 1 = Simples

    [JsonPropertyName("numeroContaCorrente")]
    public long NumeroContaCorrente { get; set; }

    [JsonPropertyName("especieDocumento")]
    public string EspecieDocumento { get; set; } = "DM"; // Duplicata Mercantil

    [JsonPropertyName("codigoBarras")]
    public string? CodigoBarras { get; set; }

    [JsonPropertyName("linhaDigitavel")]
    public string? LinhaDigitavel { get; set; }

    [JsonPropertyName("nossoNumero")]
    public string? NossoNumero { get; set; }

    [JsonPropertyName("seuNumero")]
    public string SeuNumero { get; set; } = string.Empty;

    [JsonPropertyName("identificacao")]
    public IdentificacaoBoleto Identificacao { get; set; } = new();

    [JsonPropertyName("pagador")]
    public PagadorBoleto Pagador { get; set; } = new();

    [JsonPropertyName("beneficiarioFinal")]
    public BeneficiarioFinalBoleto? BeneficiarioFinal { get; set; }

    [JsonPropertyName("rateioCredito")]
    public List<RateioCreditoBoleto>? RateioCredito { get; set; }

    [JsonPropertyName("multa")]
    public MultaBoleto? Multa { get; set; }

    [JsonPropertyName("juros")]
    public JurosBoleto? Juros { get; set; }

    [JsonPropertyName("desconto")]
    public List<DescontoBoleto>? Desconto { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("dataVencimento")]
    public string DataVencimento { get; set; } = string.Empty; // YYYY-MM-DD

    [JsonPropertyName("dataEmissao")]
    public string DataEmissao { get; set; } = string.Empty; // YYYY-MM-DD

    [JsonPropertyName("mensagem")]
    public MensagemBoleto? Mensagem { get; set; }

    [JsonPropertyName("gerarPdf")]
    public bool GerarPdf { get; set; } = true;
}

public class IdentificacaoBoleto
{
    [JsonPropertyName("numero")]
    public string Numero { get; set; } = string.Empty;

    [JsonPropertyName("dataVencimento")]
    public string DataVencimento { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }
}

public class PagadorBoleto
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("cidade")]
    public string Cidade { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("uf")]
    public string Uf { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("ddd")]
    public string? Ddd { get; set; }

    [JsonPropertyName("telefone")]
    public string? Telefone { get; set; }
}

public class BeneficiarioFinalBoleto
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

public class RateioCreditoBoleto
{
    [JsonPropertyName("codigoInstituicao")]
    public string CodigoInstituicao { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrente")]
    public long NumeroContaCorrente { get; set; }

    [JsonPropertyName("contaDv")]
    public string ContaDv { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("percentual")]
    public decimal? Percentual { get; set; }

    [JsonPropertyName("codigoFinalidade")]
    public string CodigoFinalidade { get; set; } = string.Empty;
}

public class MultaBoleto
{
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("taxa")]
    public decimal Taxa { get; set; }

    [JsonPropertyName("valor")]
    public decimal? Valor { get; set; }
}

public class JurosBoleto
{
    [JsonPropertyName("tipo")]
    public int Tipo { get; set; } = 1; // 1 = Valor por dia, 2 = Taxa mensal

    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("taxa")]
    public decimal Taxa { get; set; }

    [JsonPropertyName("valor")]
    public decimal? Valor { get; set; }
}

public class DescontoBoleto
{
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }
}

public class MensagemBoleto
{
    [JsonPropertyName("linha1")]
    public string? Linha1 { get; set; }

    [JsonPropertyName("linha2")]
    public string? Linha2 { get; set; }

    [JsonPropertyName("linha3")]
    public string? Linha3 { get; set; }

    [JsonPropertyName("linha4")]
    public string? Linha4 { get; set; }

    [JsonPropertyName("linha5")]
    public string? Linha5 { get; set; }
}
