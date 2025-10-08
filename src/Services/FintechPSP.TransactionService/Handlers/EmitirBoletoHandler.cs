using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Models;
using FintechPSP.TransactionService.Repositories;
using FintechPSP.Shared.Infrastructure.EventStore;
using FintechPSP.Shared.Infrastructure.Messaging;
using FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration;
using static FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration.ITransactionIntegrationService;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FintechPSP.TransactionService.Handlers;

/// <summary>
/// Handler para emitir boleto
/// </summary>
public class EmitirBoletoHandler : IRequestHandler<EmitirBoletoCommand, TransactionResponse>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEventStore _eventStore;
    private readonly IEventPublisher _eventPublisher;
    private readonly ITransactionIntegrationService? _transactionIntegrationService;
    private readonly ILogger<EmitirBoletoHandler> _logger;

    public EmitirBoletoHandler(
        ITransactionRepository transactionRepository,
        IEventStore eventStore,
        IEventPublisher eventPublisher,
        ITransactionIntegrationService? transactionIntegrationService,
        ILogger<EmitirBoletoHandler> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _transactionIntegrationService = transactionIntegrationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionResponse> Handle(EmitirBoletoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando emissão de boleto: {ExternalId}", request.ExternalId);

            // Verificar se já existe transação com o mesmo externalId (idempotência)
            var existingTransaction = await _transactionRepository.GetByExternalIdAsync(request.ExternalId);
            if (existingTransaction != null)
            {
                _logger.LogInformation("Transação já existe: {ExternalId}", request.ExternalId);
                return MapToResponse(existingTransaction);
            }

            // Criar nova transação de boleto
            var transaction = Transaction.CreateBoletoTransaction(
                request.ExternalId,
                request.Amount,
                request.DueDate,
                request.PayerTaxId,
                request.PayerName,
                request.Instructions,
                request.WebhookUrl,
                request.BankCode ?? "756"); // Default Sicoob

            // Salvar no repositório
            await _transactionRepository.CreateAsync(transaction);
            _logger.LogInformation("Transação de boleto salva no repositório: {TransactionId}", transaction.TransactionId);

            // Salvar eventos no event store
            await _eventStore.SaveEventsAsync<Transaction>(transaction.Id, transaction.UncommittedEvents, transaction.Version);
            _logger.LogInformation("Eventos salvos no event store para transação: {TransactionId}", transaction.TransactionId);

            // Publicar eventos
            foreach (var domainEvent in transaction.UncommittedEvents)
            {
                await _eventPublisher.PublishAsync(domainEvent);
            }
            _logger.LogInformation("Eventos publicados para transação: {TransactionId}", transaction.TransactionId);

            transaction.MarkEventsAsCommitted();

            // Integração com Sicoob (se disponível)
            if (_transactionIntegrationService != null)
            {
                try
                {
                    _logger.LogInformation("Iniciando integração com Sicoob para boleto: {ExternalId}", request.ExternalId);
                    
                    var transactionDto = MapToTransactionDto(transaction);
                    var integrationResult = await _transactionIntegrationService.ProcessBoletoTransactionAsync(transactionDto, cancellationToken);

                    if (integrationResult.Success)
                    {
                        _logger.LogInformation("Integração com Sicoob bem-sucedida. SicoobTransactionId: {SicoobTransactionId}", 
                            integrationResult.SicoobTransactionId);
                        
                        // Atualizar resposta com dados do Sicoob
                        var response = MapToResponse(transaction);
                        response.Message = $"Boleto criado com sucesso no Sicoob. NossoNumero: {integrationResult.SicoobTransactionId}";
                        return response;
                    }
                    else
                    {
                        _logger.LogWarning("Falha na integração com Sicoob: {ErrorMessage}", integrationResult.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro durante integração com Sicoob para boleto: {ExternalId}", request.ExternalId);
                }
            }
            else
            {
                _logger.LogInformation("Serviço de integração Sicoob não disponível");
            }

            return MapToResponse(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao emitir boleto: {ExternalId}", request.ExternalId);
            throw;
        }
    }

    private static TransactionResponse MapToResponse(Transaction transaction)
    {
        return new TransactionResponse
        {
            TransactionId = transaction.TransactionId,
            ExternalId = transaction.ExternalId,
            Amount = transaction.Amount.Amount,
            Status = transaction.Status,
            Type = transaction.Type,
            CreatedAt = transaction.CreatedAt,
            BankCode = transaction.BankCode
        };
    }

    private static TransactionDto MapToTransactionDto(Transaction transaction)
    {
        return new TransactionDto
        {
            TransactionId = transaction.TransactionId,
            ExternalId = transaction.ExternalId,
            Amount = transaction.Amount.Amount,
            Type = transaction.Type,
            Status = transaction.Status,
            DueDate = transaction.DueDate,
            PayerTaxId = transaction.PayerTaxId,
            PayerName = transaction.PayerName,
            Instructions = transaction.Instructions,
            BankCode = transaction.BankCode,
            CreatedAt = transaction.CreatedAt
        };
    }
}
