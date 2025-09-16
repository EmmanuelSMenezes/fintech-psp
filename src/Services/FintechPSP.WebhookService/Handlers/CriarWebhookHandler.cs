using System.Threading;
using System.Threading.Tasks;
using FintechPSP.WebhookService.Commands;
using FintechPSP.WebhookService.DTOs;
using FintechPSP.WebhookService.Models;
using FintechPSP.WebhookService.Repositories;
using FintechPSP.Shared.Infrastructure.EventStore;
using MediatR;

namespace FintechPSP.WebhookService.Handlers;

/// <summary>
/// Handler para criar webhook
/// </summary>
public class CriarWebhookHandler : IRequestHandler<CriarWebhookCommand, WebhookResponse>
{
    private readonly IWebhookRepository _webhookRepository;
    private readonly IEventStore _eventStore;
    private readonly ILogger<CriarWebhookHandler> _logger;

    public CriarWebhookHandler(
        IWebhookRepository webhookRepository,
        IEventStore eventStore,
        ILogger<CriarWebhookHandler> logger)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WebhookResponse> Handle(CriarWebhookCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando webhook para cliente {ClientId}", request.ClientId);

        // Criar webhook
        var webhook = new Webhook(
            request.ClientId,
            request.Url,
            request.Events,
            request.Secret,
            request.Description);

        // Salvar no repositório
        await _webhookRepository.CreateAsync(webhook);

        // Salvar eventos no event store
        await _eventStore.SaveEventsAsync<Webhook>(webhook.Id, webhook.UncommittedEvents, webhook.Version);

        _logger.LogInformation("Webhook {WebhookId} criado com sucesso para cliente {ClientId}", 
            webhook.Id, request.ClientId);

        return new WebhookResponse
        {
            Id = webhook.Id,
            ClientId = webhook.ClientId,
            Url = webhook.Url,
            Events = webhook.Events,
            Active = webhook.Active,
            Description = webhook.Description,
            CreatedAt = webhook.CreatedAt,
            LastTriggered = webhook.LastTriggered,
            SuccessCount = webhook.SuccessCount,
            FailureCount = webhook.FailureCount
        };
    }
}
