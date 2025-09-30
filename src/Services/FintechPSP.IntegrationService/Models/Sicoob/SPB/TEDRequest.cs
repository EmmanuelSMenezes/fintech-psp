using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.SPB;

/// <summary>
/// Request para TED
/// </summary>
public class TEDRequest
{
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    [JsonPropertyName("contaOrigem")]
    public ContaTED ContaOrigem { get; set; } = new();

    [JsonPropertyName("contaDestino")]
    public ContaTED ContaDestino { get; set; } = new();

    [JsonPropertyName("finalidade")]
    public string? Finalidade { get; set; }

    [JsonPropertyName("codigoFinalidade")]
    public string? CodigoFinalidade { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("dataAgendamento")]
    public string? DataAgendamento { get; set; }
}

/// <summary>
/// Dados da conta para TED
/// </summary>
public class ContaTED
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
    public string? TipoConta { get; set; } // "CC" para conta corrente, "CP" para poupança

    [JsonPropertyName("titular")]
    public TitularConta? Titular { get; set; }
}

/// <summary>
/// Dados do titular da conta
/// </summary>
public class TitularConta
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }
}
