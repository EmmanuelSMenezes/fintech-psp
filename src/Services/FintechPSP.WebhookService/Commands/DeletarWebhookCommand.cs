using MediatR;

namespace FintechPSP.WebhookService.Commands;

/// <summary>
/// Command para deletar webhook
/// </summary>
public class DeletarWebhookCommand : IRequest<bool>
{
    public Guid WebhookId { get; set; }
    public Guid ClientId { get; set; }

    public DeletarWebhookCommand(Guid webhookId, Guid clientId)
    {
        WebhookId = webhookId;
        ClientId = clientId;
    }
}
