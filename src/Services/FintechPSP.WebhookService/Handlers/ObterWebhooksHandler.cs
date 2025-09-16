using System.Threading;
using System.Threading.Tasks;
using FintechPSP.WebhookService.DTOs;
using FintechPSP.WebhookService.Queries;
using FintechPSP.WebhookService.Repositories;
using MediatR;

namespace FintechPSP.WebhookService.Handlers;

/// <summary>
/// Handler para obter webhooks
/// </summary>
public class ObterWebhooksHandler : IRequestHandler<ObterWebhooksQuery, WebhookListResponse>
{
    private readonly IWebhookRepository _webhookRepository;

    public ObterWebhooksHandler(IWebhookRepository webhookRepository)
    {
        _webhookRepository = webhookRepository ?? throw new ArgumentNullException(nameof(webhookRepository));
    }

    public async Task<WebhookListResponse> Handle(ObterWebhooksQuery request, CancellationToken cancellationToken)
    {
        var webhooks = await _webhookRepository.GetByClientIdAsync(
            request.ClientId, 
            request.Page, 
            request.PageSize, 
            request.Active);

        var totalCount = await _webhookRepository.CountByClientIdAsync(request.ClientId, request.Active);

        return new WebhookListResponse
        {
            Webhooks = webhooks.Select(w => new WebhookResponse
            {
                Id = w.Id,
                ClientId = w.ClientId,
                Url = w.Url,
                Events = w.Events,
                Active = w.Active,
                Description = w.Description,
                CreatedAt = w.CreatedAt,
                LastTriggered = w.LastTriggered,
                SuccessCount = w.SuccessCount,
                FailureCount = w.FailureCount
            }).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
