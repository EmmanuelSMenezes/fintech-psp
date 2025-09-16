using System.Threading;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Infrastructure.EventStore;
using FintechPSP.Shared.Infrastructure.Messaging;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Models;
using FintechPSP.TransactionService.Repositories;
using MediatR;

namespace FintechPSP.TransactionService.Handlers;

/// <summary>
/// Handler para iniciar transação PIX
/// </summary>
public class IniciarTransacaoPixHandler : IRequestHandler<IniciarTransacaoPixCommand, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEventStore _eventStore;
    private readonly IEventPublisher _eventPublisher;

    public IniciarTransacaoPixHandler(
        ITransactionRepository transactionRepository,
        IEventStore eventStore,
        IEventPublisher eventPublisher)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
    }

    public async Task<TransactionResponse> Handle(IniciarTransacaoPixCommand request, CancellationToken cancellationToken)
    {
        // Verificar se já existe transação com o mesmo externalId (idempotência)
        var existingTransaction = await _transactionRepository.GetByExternalIdAsync(request.ExternalId);
        if (existingTransaction != null)
        {
            return MapToResponse(existingTransaction);
        }

        // Criar nova transação PIX
        var transaction = Transaction.CreatePixTransaction(
            request.ExternalId,
            request.Amount,
            request.PixKey,
            request.BankCode,
            request.Description,
            request.WebhookUrl,
            request.EndToEndId);

        // Salvar no repositório
        await _transactionRepository.CreateAsync(transaction);

        // Salvar eventos no event store
        await _eventStore.SaveEventsAsync<Transaction>(transaction.Id, transaction.UncommittedEvents, transaction.Version);

        // Publicar eventos
        foreach (var domainEvent in transaction.UncommittedEvents)
        {
            await _eventPublisher.PublishAsync(domainEvent);
        }

        transaction.MarkEventsAsCommitted();

        return MapToResponse(transaction);
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            TransactionId = transaction.TransactionId,
            ExternalId = transaction.ExternalId,
            Status = transaction.Status,
            Amount = transaction.Amount.Amount,
            Type = transaction.Type,
            BankCode = transaction.BankCode,
            EndToEndId = transaction.EndToEndId,
            CreatedAt = transaction.CreatedAt,
            UpdatedAt = transaction.UpdatedAt,
            Message = GetStatusMessage(transaction.Status)
        };
    }

    private static string GetStatusMessage(TransactionStatus status)
    {
        return status switch
        {
            TransactionStatus.PENDING => "Transação PIX iniciada com sucesso",
            TransactionStatus.PROCESSING => "Transação PIX em processamento",
            TransactionStatus.CONFIRMED => "Transação PIX confirmada",
            TransactionStatus.FAILED => "Transação PIX falhou",
            TransactionStatus.CANCELLED => "Transação PIX cancelada",
            _ => "Status desconhecido"
        };
    }
}
