using System;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Domain.ValueObjects;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando uma transação PIX é iniciada
/// </summary>
public class PixIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("webhookUrl")]
    public string? WebhookUrl { get; set; }
    
    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    public PixIniciado() { }

    public PixIniciado(Guid transactionId, string externalId, decimal amount, string pixKey, 
        string bankCode, string description, string? webhookUrl = null, string? endToEndId = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        PixKey = pixKey;
        BankCode = bankCode;
        Description = description;
        WebhookUrl = webhookUrl;
        EndToEndId = endToEndId;
    }
}

/// <summary>
/// Evento disparado quando uma transação PIX é confirmada
/// </summary>
public class PixConfirmado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("endToEndId")]
    public string EndToEndId { get; set; } = string.Empty;
    
    [JsonPropertyName("txId")]
    public string? TxId { get; set; }
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    public PixConfirmado() { }

    public PixConfirmado(Guid transactionId, string endToEndId, decimal amount, string? txId = null)
    {
        TransactionId = transactionId;
        EndToEndId = endToEndId;
        Amount = amount;
        TxId = txId;
    }
}

/// <summary>
/// Evento disparado quando uma transação TED é iniciada
/// </summary>
public class TedIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("accountBranch")]
    public string AccountBranch { get; set; } = string.Empty;
    
    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("taxId")]
    public string TaxId { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    public TedIniciado() { }

    public TedIniciado(Guid transactionId, string externalId, decimal amount, string bankCode,
        string accountBranch, string accountNumber, string taxId, string? name = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        BankCode = bankCode;
        AccountBranch = accountBranch;
        AccountNumber = accountNumber;
        TaxId = taxId;
        Name = name;
    }
}

/// <summary>
/// Evento disparado quando um boleto é emitido
/// </summary>
public class BoletoEmitido : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; set; }
    
    [JsonPropertyName("payerTaxId")]
    public string PayerTaxId { get; set; } = string.Empty;
    
    [JsonPropertyName("payerName")]
    public string PayerName { get; set; } = string.Empty;
    
    [JsonPropertyName("instructions")]
    public string Instructions { get; set; } = string.Empty;
    
    [JsonPropertyName("boletoId")]
    public string? BoletoId { get; set; }

    public BoletoEmitido() { }

    public BoletoEmitido(Guid transactionId, string externalId, decimal amount, DateTime dueDate,
        string payerTaxId, string payerName, string instructions, string? boletoId = null)
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        DueDate = dueDate;
        PayerTaxId = payerTaxId;
        PayerName = payerName;
        Instructions = instructions;
        BoletoId = boletoId;
    }
}

/// <summary>
/// Evento disparado quando uma transação cripto é iniciada
/// </summary>
public class CriptoIniciado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("cryptoType")]
    public string CryptoType { get; set; } = string.Empty;
    
    [JsonPropertyName("walletAddress")]
    public string WalletAddress { get; set; } = string.Empty;
    
    [JsonPropertyName("fiatCurrency")]
    public string FiatCurrency { get; set; } = "BRL";

    public CriptoIniciado() { }

    public CriptoIniciado(Guid transactionId, string externalId, decimal amount, 
        string cryptoType, string walletAddress, string fiatCurrency = "BRL")
    {
        TransactionId = transactionId;
        ExternalId = externalId;
        Amount = amount;
        CryptoType = cryptoType;
        WalletAddress = walletAddress;
        FiatCurrency = fiatCurrency;
    }
}

/// <summary>
/// Evento disparado quando o status de uma transação é alterado
/// </summary>
public class StatusTransacaoAlterado : DomainEvent
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("previousStatus")]
    public TransactionStatus PreviousStatus { get; set; }
    
    [JsonPropertyName("newStatus")]
    public TransactionStatus NewStatus { get; set; }
    
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    public StatusTransacaoAlterado() { }

    public StatusTransacaoAlterado(Guid transactionId, TransactionStatus previousStatus, 
        TransactionStatus newStatus, string? reason = null)
    {
        TransactionId = transactionId;
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        Reason = reason;
    }
}

/// <summary>
/// Evento disparado quando uma conta é criada
/// </summary>
public class ContaCriada : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("initialBalance")]
    public decimal InitialBalance { get; set; }

    public ContaCriada() { }

    public ContaCriada(Guid clientId, string accountId, decimal initialBalance)
    {
        ClientId = clientId;
        AccountId = accountId;
        InitialBalance = initialBalance;
    }
}

/// <summary>
/// Evento disparado quando saldo é creditado
/// </summary>
public class SaldoCreditado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("oldBalance")]
    public decimal OldBalance { get; set; }

    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoCreditado() { }

    public SaldoCreditado(Guid clientId, string accountId, decimal amount,
        decimal oldBalance, decimal newBalance, string description, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Description = description;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é debitado
/// </summary>
public class SaldoDebitado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("oldBalance")]
    public decimal OldBalance { get; set; }

    [JsonPropertyName("newBalance")]
    public decimal NewBalance { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoDebitado() { }

    public SaldoDebitado(Guid clientId, string accountId, decimal amount,
        decimal oldBalance, decimal newBalance, string description, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        OldBalance = oldBalance;
        NewBalance = newBalance;
        Description = description;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é bloqueado
/// </summary>
public class SaldoBloqueado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoBloqueado() { }

    public SaldoBloqueado(Guid clientId, string accountId, decimal amount, string reason, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        Reason = reason;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando saldo é desbloqueado
/// </summary>
public class SaldoDesbloqueado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("accountId")]
    public string AccountId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public string TransactionId { get; set; } = string.Empty;

    public SaldoDesbloqueado() { }

    public SaldoDesbloqueado(Guid clientId, string accountId, decimal amount, string reason, string transactionId)
    {
        ClientId = clientId;
        AccountId = accountId;
        Amount = amount;
        Reason = reason;
        TransactionId = transactionId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é criado
/// </summary>
public class WebhookCriado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    public WebhookCriado() { }

    public WebhookCriado(Guid webhookId, Guid clientId, string url, List<string> events, bool active, string? description = null)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Url = url;
        Events = events;
        Active = active;
        Description = description;
    }
}

/// <summary>
/// Evento disparado quando um webhook é atualizado
/// </summary>
public class WebhookAtualizado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; } = string.Empty;

    [JsonPropertyName("oldValue")]
    public string OldValue { get; set; } = string.Empty;

    [JsonPropertyName("newValue")]
    public string NewValue { get; set; } = string.Empty;

    public WebhookAtualizado() { }

    public WebhookAtualizado(Guid webhookId, Guid clientId, string field, string oldValue, string newValue)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Field = field;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// Evento disparado quando um webhook é ativado
/// </summary>
public class WebhookAtivado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public WebhookAtivado() { }

    public WebhookAtivado(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é desativado
/// </summary>
public class WebhookDesativado : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public WebhookDesativado() { }

    public WebhookDesativado(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}

/// <summary>
/// Evento disparado quando um webhook é entregue
/// </summary>
public class WebhookEntregue : DomainEvent
{
    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    public WebhookEntregue() { }

    public WebhookEntregue(Guid webhookId, Guid clientId, bool success, string? errorMessage = null)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Success = success;
        ErrorMessage = errorMessage;
    }
}
