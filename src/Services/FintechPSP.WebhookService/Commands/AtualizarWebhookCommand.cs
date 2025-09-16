using FintechPSP.WebhookService.DTOs;
using MediatR;

namespace FintechPSP.WebhookService.Commands;

/// <summary>
/// Command para atualizar webhook
/// </summary>
public class AtualizarWebhookCommand : IRequest<WebhookResponse>
{
    public Guid WebhookId { get; set; }
    public Guid ClientId { get; set; }
    public string? Url { get; set; }
    public List<string>? Events { get; set; }
    public string? Secret { get; set; }
    public bool? Active { get; set; }
    public string? Description { get; set; }

    public AtualizarWebhookCommand(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}
