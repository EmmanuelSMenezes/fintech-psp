using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.ContaCorrente;

/// <summary>
/// Request para realizar transferência entre contas (v4)
/// Baseado na documentação Postman oficial
/// </summary>
public class TransferenciaRequest
{
    /// <summary>
    /// Valor da transferência
    /// </summary>
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Data da transferência (formato ISO 8601)
    /// </summary>
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Conta do devedor (origem)
    /// </summary>
    [JsonPropertyName("debtorAccount")]
    public ContaTransferencia DebtorAccount { get; set; } = new();

    /// <summary>
    /// Conta do credor (destino)
    /// </summary>
    [JsonPropertyName("creditorAccount")]
    public ContaTransferencia CreditorAccount { get; set; } = new();
}

/// <summary>
/// Dados da conta para transferência
/// </summary>
public class ContaTransferencia
{
    /// <summary>
    /// Emissor da conta (agência)
    /// </summary>
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Número da conta
    /// </summary>
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de conta (0 = Conta Corrente, 1 = Poupança)
    /// </summary>
    [JsonPropertyName("accountType")]
    public int AccountType { get; set; }

    /// <summary>
    /// Tipo de pessoa (0 = Pessoa Física, 1 = Pessoa Jurídica)
    /// </summary>
    [JsonPropertyName("personType")]
    public int PersonType { get; set; }
}

/// <summary>
/// Resposta da API de transferência (v4)
/// </summary>
public class TransferenciaResponse
{
    [JsonPropertyName("mensagens")]
    public List<string> Mensagens { get; set; } = new();

    [JsonPropertyName("resultado")]
    public TransferenciaResultado? Resultado { get; set; }
}

/// <summary>
/// Resultado da transferência
/// </summary>
public class TransferenciaResultado
{
    [JsonPropertyName("idTransferencia")]
    public string IdTransferencia { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("dataHora")]
    public string DataHora { get; set; } = string.Empty;
}

