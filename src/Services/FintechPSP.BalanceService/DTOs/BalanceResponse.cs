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

/// <summary>
/// Response com informações de saldo incluindo contas bancárias
/// </summary>
public class BalanceWithAccountsResponse
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

    [JsonPropertyName("bankAccounts")]
    public List<BankAccountInfo> BankAccounts { get; set; } = new();
}

/// <summary>
/// Informações de conta bancária para o saldo
/// </summary>
public class BankAccountInfo
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("priority")]
    public decimal? Priority { get; set; } // Percentual de priorização, se configurado
}
