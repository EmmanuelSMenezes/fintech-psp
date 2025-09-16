using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Domain.ValueObjects;

namespace FintechPSP.BalanceService.Models;

/// <summary>
/// Aggregate root para contas
/// </summary>
public class Account : AggregateRoot
{
    public Guid ClientId { get; private set; }
    public string AccountId { get; private set; } = string.Empty;
    public Money AvailableBalance { get; private set; } = new(0, "BRL");
    public Money BlockedBalance { get; private set; } = new(0, "BRL");
    public DateTime CreatedAt { get; private set; }
    public DateTime LastUpdated { get; private set; }

    // Construtor privado para EF Core
    private Account() { }

    public Account(Guid clientId, string accountId)
    {
        ClientId = clientId;
        AccountId = accountId;
        AvailableBalance = new Money(0, "BRL");
        BlockedBalance = new Money(0, "BRL");
        CreatedAt = DateTime.UtcNow;
        LastUpdated = DateTime.UtcNow;

        ApplyEvent(new ContaCriada(ClientId, AccountId, AvailableBalance.Amount));
    }

    public void Credit(decimal amount, string description, string transactionId)
    {
        var oldBalance = AvailableBalance.Amount;
        AvailableBalance = new Money(AvailableBalance.Amount + amount, AvailableBalance.Currency);
        LastUpdated = DateTime.UtcNow;

        ApplyEvent(new SaldoCreditado(
            ClientId,
            AccountId,
            amount,
            oldBalance,
            AvailableBalance.Amount,
            description,
            transactionId));
    }

    public void Debit(decimal amount, string description, string transactionId)
    {
        if (AvailableBalance.Amount < amount)
        {
            throw new InvalidOperationException("Saldo insuficiente");
        }

        var oldBalance = AvailableBalance.Amount;
        AvailableBalance = new Money(AvailableBalance.Amount - amount, AvailableBalance.Currency);
        LastUpdated = DateTime.UtcNow;

        ApplyEvent(new SaldoDebitado(
            ClientId,
            AccountId,
            amount,
            oldBalance,
            AvailableBalance.Amount,
            description,
            transactionId));
    }

    public void BlockAmount(decimal amount, string reason, string transactionId)
    {
        if (AvailableBalance.Amount < amount)
        {
            throw new InvalidOperationException("Saldo insuficiente para bloqueio");
        }

        AvailableBalance = new Money(AvailableBalance.Amount - amount, AvailableBalance.Currency);
        BlockedBalance = new Money(BlockedBalance.Amount + amount, BlockedBalance.Currency);
        LastUpdated = DateTime.UtcNow;

        ApplyEvent(new SaldoBloqueado(
            ClientId,
            AccountId,
            amount,
            reason,
            transactionId));
    }

    public void UnblockAmount(decimal amount, string reason, string transactionId)
    {
        if (BlockedBalance.Amount < amount)
        {
            throw new InvalidOperationException("Valor bloqueado insuficiente");
        }

        BlockedBalance = new Money(BlockedBalance.Amount - amount, BlockedBalance.Currency);
        AvailableBalance = new Money(AvailableBalance.Amount + amount, AvailableBalance.Currency);
        LastUpdated = DateTime.UtcNow;

        ApplyEvent(new SaldoDesbloqueado(
            ClientId,
            AccountId,
            amount,
            reason,
            transactionId));
    }

    public decimal GetTotalBalance()
    {
        return AvailableBalance.Amount + BlockedBalance.Amount;
    }

    // Métodos Apply para Event Sourcing
    public void Apply(ContaCriada @event)
    {
        ClientId = @event.ClientId;
        AccountId = @event.AccountId;
        AvailableBalance = new Money(@event.InitialBalance, "BRL");
        BlockedBalance = new Money(0, "BRL");
        CreatedAt = @event.OccurredAt;
        LastUpdated = @event.OccurredAt;
    }

    public void Apply(SaldoCreditado @event)
    {
        AvailableBalance = new Money(@event.NewBalance, AvailableBalance.Currency);
        LastUpdated = @event.OccurredAt;
    }

    public void Apply(SaldoDebitado @event)
    {
        AvailableBalance = new Money(@event.NewBalance, AvailableBalance.Currency);
        LastUpdated = @event.OccurredAt;
    }

    public void Apply(SaldoBloqueado @event)
    {
        // Lógica já aplicada no método BlockAmount
        LastUpdated = @event.OccurredAt;
    }

    public void Apply(SaldoDesbloqueado @event)
    {
        // Lógica já aplicada no método UnblockAmount
        LastUpdated = @event.OccurredAt;
    }
}
