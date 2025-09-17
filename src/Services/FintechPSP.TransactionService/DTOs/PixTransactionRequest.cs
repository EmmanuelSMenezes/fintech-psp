using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Request para transação PIX
/// </summary>
public class PixTransactionRequest
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

    [JsonPropertyName("pixKey")]
    [Required]
    public string PixKey { get; set; } = string.Empty;

    [JsonPropertyName("bankCode")]
    [Required]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }

    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }
}
