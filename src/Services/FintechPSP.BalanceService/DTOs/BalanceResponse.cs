using System.Text.Json.Serialization;

namespace FintechPSP.BalanceService.DTOs;

/// <summary>
/// Response com informações de saldo
/// </summary>
public class BalanceResponse
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("availableBalance")]
    public decimal AvailableBalance { get; set; }

    [JsonPropertyName("blockedBalance")]
    public decimal BlockedBalance { get; set; }

    [JsonPropertyName("totalBalance")]
    public decimal TotalBalance { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "BRL";

    [JsonPropertyName("lastUpdated")]
    public DateTime LastUpdated { get; set; }
}
