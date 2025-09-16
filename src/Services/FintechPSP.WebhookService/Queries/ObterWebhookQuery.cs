using FintechPSP.WebhookService.DTOs;
using MediatR;

namespace FintechPSP.WebhookService.Queries;

/// <summary>
/// Query para obter um webhook específico
/// </summary>
public class ObterWebhookQuery : IRequest<WebhookResponse?>
{
    public Guid WebhookId { get; set; }
    public Guid ClientId { get; set; }

    public ObterWebhookQuery(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}
