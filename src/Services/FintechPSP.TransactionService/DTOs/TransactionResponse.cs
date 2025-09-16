using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Enums;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Response padrão para transações
/// </summary>
public class TransactionResponse
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public TransactionStatus Status { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("type")]
    public TransactionType Type { get; set; }

    [JsonPropertyName("bankCode")]
    public string? BankCode { get; set; }

    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    [JsonPropertyName("boletoBarcode")]
    public string? BoletoBarcode { get; set; }

    [JsonPropertyName("boletoUrl")]
    public string? BoletoUrl { get; set; }

    [JsonPropertyName("cryptoTxHash")]
    public string? CryptoTxHash { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
