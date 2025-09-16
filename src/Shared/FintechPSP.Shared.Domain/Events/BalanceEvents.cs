using System;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando o saldo de um cliente é atualizado
/// </summary>
public class SaldoAtualizado : DomainEvent
{
    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }
    
    [JsonPropertyName("previousBalance")]
    public decimal PreviousBalance { get; set; }
    
    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "BRL";
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [JsonPropertyName("transactionId")]
    public Guid? TransactionId { get; set; }

    public SaldoAtualizado() { }

    public SaldoAtualizado(Guid clienteId, decimal previousBalance, decimal newBalance, 
        string reason, string currency = "BRL", Guid? transactionId = null)
    {
        ClienteId = clienteId;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Currency = currency;
        Reason = reason;
        TransactionId = transactionId;
    }
}
