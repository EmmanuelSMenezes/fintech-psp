using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Controllers;

/// <summary>
/// Modelo para webhook PIX do Sicoob
/// </summary>
public class SicoobPixWebhook
{
    [JsonPropertyName("txId")]
    public string TxId { get; set; } = string.Empty;

    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("valor")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("pagador")]
    public SicoobWebhookPayer? Payer { get; set; }

    [JsonPropertyName("recebedor")]
    public SicoobWebhookReceiver? Receiver { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("motivoRejeicao")]
    public string? RejectionReason { get; set; }

    [JsonPropertyName("descricao")]
    public string? Description { get; set; }

    // Propriedades derivadas para facilitar o uso
    public string? PayerDocument => Payer?.Document;
    public string? PayerName => Payer?.Name;
    public string? ReceiverDocument => Receiver?.Document;
    public string? ReceiverName => Receiver?.Name;
}

/// <summary>
/// Modelo para webhook Boleto do Sicoob
/// </summary>
public class SicoobBoletoWebhook
{
    [JsonPropertyName("nossoNumero")]
    public string NossoNumero { get; set; } = string.Empty;

    [JsonPropertyName("seuNumero")]
    public string? SeuNumero { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("valor")]
    public decimal? Amount { get; set; }

    [JsonPropertyName("valorPago")]
    public decimal? PaidAmount { get; set; }

    [JsonPropertyName("dataVencimento")]
    public DateTime? DueDate { get; set; }

    [JsonPropertyName("dataPagamento")]
    public DateTime? PaymentDate { get; set; }

    [JsonPropertyName("pagador")]
    public SicoobWebhookPayer? Payer { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime? Timestamp { get; set; }

    [JsonPropertyName("motivoCancelamento")]
    public string? CancellationReason { get; set; }

    [JsonPropertyName("descricao")]
    public string? Description { get; set; }

    // Propriedades derivadas
    public string? PayerDocument => Payer?.Document;
    public string? PayerName => Payer?.Name;
}

/// <summary>
/// Dados do pagador no webhook
/// </summary>
public class SicoobWebhookPayer
{
    [JsonPropertyName("nome")]
    public string? Name { get; set; }

    [JsonPropertyName("documento")]
    public string? Document { get; set; }

    [JsonPropertyName("tipoPessoa")]
    public string? PersonType { get; set; }

    [JsonPropertyName("banco")]
    public string? Bank { get; set; }

    [JsonPropertyName("agencia")]
    public string? Agency { get; set; }

    [JsonPropertyName("conta")]
    public string? Account { get; set; }
}

/// <summary>
/// Dados do recebedor no webhook
/// </summary>
public class SicoobWebhookReceiver
{
    [JsonPropertyName("nome")]
    public string? Name { get; set; }

    [JsonPropertyName("documento")]
    public string? Document { get; set; }

    [JsonPropertyName("tipoPessoa")]
    public string? PersonType { get; set; }

    [JsonPropertyName("chave")]
    public string? PixKey { get; set; }

    [JsonPropertyName("banco")]
    public string? Bank { get; set; }

    [JsonPropertyName("agencia")]
    public string? Agency { get; set; }

    [JsonPropertyName("conta")]
    public string? Account { get; set; }
}
