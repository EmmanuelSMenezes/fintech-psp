using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.SPB;

/// <summary>
/// Resposta da TED
/// </summary>
public class TEDResponse
{
    [JsonPropertyName("numeroDocumento")]
    public string? NumeroDocumento { get; set; }

    [JsonPropertyName("codigoTransacao")]
    public string? CodigoTransacao { get; set; }

    [JsonPropertyName("valor")]
    public string? Valor { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("dataHoraSolicitacao")]
    public DateTime? DataHoraSolicitacao { get; set; }

    [JsonPropertyName("dataHoraEfetivacao")]
    public DateTime? DataHoraEfetivacao { get; set; }

    [JsonPropertyName("contaOrigem")]
    public ContaTEDResponse? ContaOrigem { get; set; }

    [JsonPropertyName("contaDestino")]
    public ContaTEDResponse? ContaDestino { get; set; }

    [JsonPropertyName("finalidade")]
    public string? Finalidade { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("tarifa")]
    public decimal? Tarifa { get; set; }
}

/// <summary>
/// Dados da conta na resposta da TED
/// </summary>
public class ContaTEDResponse
{
    [JsonPropertyName("banco")]
    public string? Banco { get; set; }

    [JsonPropertyName("agencia")]
    public string? Agencia { get; set; }

    [JsonPropertyName("conta")]
    public string? Conta { get; set; }

    [JsonPropertyName("digito")]
    public string? Digito { get; set; }

    [JsonPropertyName("tipoConta")]
    public string? TipoConta { get; set; }

    [JsonPropertyName("titular")]
    public TitularContaResponse? Titular { get; set; }
}

/// <summary>
/// Dados do titular na resposta
/// </summary>
public class TitularContaResponse
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("documento")]
    public string? Documento { get; set; }

    [JsonPropertyName("tipoDocumento")]
    public string? TipoDocumento { get; set; } // "CPF" ou "CNPJ"
}
