using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Request para transação de criptomoeda
/// </summary>
public class CryptoTransactionRequest
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

    [JsonPropertyName("cryptoType")]
    [Required]
    public string CryptoType { get; set; } = string.Empty; // USDT, BTC, ETH, etc.

    [JsonPropertyName("walletAddress")]
    [Required]
    public string WalletAddress { get; set; } = string.Empty;

    [JsonPropertyName("fiatCurrency")]
    public string FiatCurrency { get; set; } = "BRL";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }
}
