using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Request para transação TED
/// </summary>
public class TedTransactionRequest
{
    [JsonPropertyName("externalId")]
    [Required]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [JsonPropertyName("bankCode")]
    [Required]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("accountBranch")]
    [Required]
    public string AccountBranch { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("taxId")]
    [Required]
    public string TaxId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }
}
