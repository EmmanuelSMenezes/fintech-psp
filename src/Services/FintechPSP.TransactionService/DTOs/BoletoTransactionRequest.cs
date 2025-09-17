using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Request para emissão de boleto
/// </summary>
public class BoletoTransactionRequest
{
    [JsonPropertyName("contaId")]
    public Guid? ContaId { get; set; }

    [JsonPropertyName("externalId")]
    [Required]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [JsonPropertyName("dueDate")]
    [Required]
    public DateTime DueDate { get; set; }

    [JsonPropertyName("payerTaxId")]
    [Required]
    public string PayerTaxId { get; set; } = string.Empty;

    [JsonPropertyName("payerName")]
    [Required]
    public string PayerName { get; set; } = string.Empty;

    [JsonPropertyName("instructions")]
    public string? Instructions { get; set; }

    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }

    [JsonPropertyName("bankCode")]
    public string? BankCode { get; set; }
}
