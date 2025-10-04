using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Domain.ValueObjects;

namespace FintechPSP.TransactionService.Models;

/// <summary>
/// Aggregate root para transações
/// </summary>
public class Transaction : AggregateRoot
{
    public Guid TransactionId { get; private set; }
    public string ExternalId { get; private set; } = string.Empty;
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public Money Amount { get; private set; } = new(0, "BRL");
    public string? BankCode { get; private set; }
    public string? PixKey { get; private set; }
    public string? EndToEndId { get; private set; }
    public string? AccountBranch { get; private set; }
    public string? AccountNumber { get; private set; }
    public string? TaxId { get; private set; }
    public string? Name { get; private set; }
    public string? Description { get; private set; }
    public string? WebhookUrl { get; private set; }
    public DateTime? DueDate { get; private set; }
    public string? PayerTaxId { get; private set; }
    public string? PayerName { get; private set; }
    public string? Instructions { get; private set; }
    public string? BoletoBarcode { get; private set; }
    public string? BoletoUrl { get; private set; }
    public string? CryptoType { get; private set; }
    public string? WalletAddress { get; private set; }
    public string? CryptoTxHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Construtor privado para EF Core
    private Transaction() { }

    // Factory methods
    public static Transaction CreatePixTransaction(
        string externalId,
        decimal amount,
        string pixKey,
        string bankCode,
        string? description = null,
        string? webhookUrl = null,
        string? endToEndId = null)
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            ExternalId = externalId,
            Type = TransactionType.PIX,
            Status = TransactionStatus.PENDING,
            Amount = new Money(amount, "BRL"),
            PixKey = pixKey,
            BankCode = bankCode,
            Description = description,
            WebhookUrl = webhookUrl,
            EndToEndId = endToEndId ?? GenerateEndToEndId(),
            CreatedAt = DateTime.UtcNow
        };

        transaction.ApplyEvent(new PixIniciado(
            transaction.TransactionId,
            transaction.ExternalId,
            transaction.Amount.Amount,
            transaction.PixKey,
            transaction.BankCode,
            transaction.Description ?? "",
            transaction.WebhookUrl,
            transaction.EndToEndId));

        return transaction;
    }

    public static Transaction CreateTedTransaction(
        string externalId,
        decimal amount,
        string bankCode,
        string accountBranch,
        string accountNumber,
        string taxId,
        string name,
        string? description = null,
        string? webhookUrl = null)
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            ExternalId = externalId,
            Type = TransactionType.TED,
            Status = TransactionStatus.PENDING,
            Amount = new Money(amount, "BRL"),
            BankCode = bankCode,
            AccountBranch = accountBranch,
            AccountNumber = accountNumber,
            TaxId = taxId,
            Name = name,
            Description = description,
            WebhookUrl = webhookUrl,
            CreatedAt = DateTime.UtcNow
        };

        transaction.ApplyEvent(new TedIniciado(
            transaction.TransactionId,
            transaction.ExternalId,
            transaction.Amount.Amount,
            transaction.BankCode,
            transaction.AccountBranch,
            transaction.AccountNumber,
            transaction.TaxId,
            transaction.Name));

        return transaction;
    }

    public static Transaction CreateBoletoTransaction(
        string externalId,
        decimal amount,
        DateTime dueDate,
        string payerTaxId,
        string payerName,
        string? instructions = null,
        string? webhookUrl = null,
        string? bankCode = null)
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            ExternalId = externalId,
            Type = TransactionType.BOLETO,
            Status = TransactionStatus.PENDING,
            Amount = new Money(amount, "BRL"),
            DueDate = dueDate,
            PayerTaxId = payerTaxId,
            PayerName = payerName,
            Instructions = instructions,
            WebhookUrl = webhookUrl,
            BankCode = bankCode,
            CreatedAt = DateTime.UtcNow
        };

        transaction.ApplyEvent(new BoletoEmitido(
            transaction.TransactionId,
            transaction.ExternalId,
            transaction.Amount.Amount,
            transaction.DueDate.Value,
            transaction.PayerTaxId,
            transaction.PayerName,
            transaction.Instructions ?? ""));

        return transaction;
    }

    public static Transaction CreateCryptoTransaction(
        string externalId,
        decimal amount,
        string cryptoType,
        string walletAddress,
        string fiatCurrency = "BRL",
        string? description = null,
        string? webhookUrl = null)
    {
        var transaction = new Transaction
        {
            TransactionId = Guid.NewGuid(),
            ExternalId = externalId,
            Type = TransactionType.CRYPTO,
            Status = TransactionStatus.PENDING,
            Amount = new Money(amount, fiatCurrency),
            CryptoType = cryptoType,
            WalletAddress = walletAddress,
            Description = description,
            WebhookUrl = webhookUrl,
            CreatedAt = DateTime.UtcNow
        };

        transaction.ApplyEvent(new CriptoIniciado(
            transaction.TransactionId,
            transaction.ExternalId,
            transaction.Amount.Amount,
            transaction.CryptoType,
            transaction.WalletAddress,
            transaction.Amount.Currency));

        return transaction;
    }

    public void UpdateStatus(TransactionStatus newStatus, string? message = null)
    {
        if (Status == newStatus) return;

        var oldStatus = Status;
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;

        ApplyEvent(new StatusTransacaoAlterado(
            TransactionId,
            oldStatus,
            newStatus,
            message));
    }

    public void SetBoletoDetails(string barcode, string url)
    {
        BoletoBarcode = barcode;
        BoletoUrl = url;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetCryptoTxHash(string txHash)
    {
        CryptoTxHash = txHash;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string GenerateEndToEndId()
    {
        // Formato: E + ISPB (8 dígitos) + YYYYMMDD + sequencial (10 dígitos)
        var ispb = "12345678"; // ISPB da fintech
        var date = DateTime.Now.ToString("yyyyMMdd");
        var sequential = Random.Shared.Next(1000000000, int.MaxValue).ToString();
        return $"E{ispb}{date}{sequential}";
    }

    // Métodos Apply para Event Sourcing
    public void Apply(PixIniciado @event)
    {
        TransactionId = @event.TransactionId;
        ExternalId = @event.ExternalId;
        Type = TransactionType.PIX;
        Status = TransactionStatus.PENDING;
        Amount = new Money(@event.Amount, "BRL");
        PixKey = @event.PixKey;
        BankCode = @event.BankCode;
        Description = @event.Description;
        WebhookUrl = @event.WebhookUrl;
        EndToEndId = @event.EndToEndId;
        CreatedAt = @event.OccurredAt;
    }

    public void Apply(TedIniciado @event)
    {
        TransactionId = @event.TransactionId;
        ExternalId = @event.ExternalId;
        Type = TransactionType.TED;
        Status = TransactionStatus.PENDING;
        Amount = new Money(@event.Amount, "BRL");
        BankCode = @event.BankCode;
        AccountBranch = @event.AccountBranch;
        AccountNumber = @event.AccountNumber;
        TaxId = @event.TaxId;
        Name = @event.Name;
        CreatedAt = @event.OccurredAt;
    }

    public void Apply(BoletoEmitido @event)
    {
        TransactionId = @event.TransactionId;
        ExternalId = @event.ExternalId;
        Type = TransactionType.BOLETO;
        Status = TransactionStatus.ISSUED;
        Amount = new Money(@event.Amount, "BRL");
        DueDate = @event.DueDate;
        PayerTaxId = @event.PayerTaxId;
        PayerName = @event.PayerName;
        Instructions = @event.Instructions;
        CreatedAt = @event.OccurredAt;
    }

    public void Apply(CriptoIniciado @event)
    {
        TransactionId = @event.TransactionId;
        ExternalId = @event.ExternalId;
        Type = TransactionType.CRYPTO;
        Status = TransactionStatus.PENDING;
        Amount = new Money(@event.Amount, @event.FiatCurrency);
        CryptoType = @event.CryptoType;
        WalletAddress = @event.WalletAddress;
        CreatedAt = @event.OccurredAt;
    }

    public void Apply(StatusTransacaoAlterado @event)
    {
        Status = @event.NewStatus;
        UpdatedAt = @event.OccurredAt;
    }
}
