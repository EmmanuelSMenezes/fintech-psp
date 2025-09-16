using MediatR;

namespace FintechPSP.WebhookService.Commands;

/// <summary>
/// Command para enviar webhook
/// </summary>
public class EnviarWebhookCommand : IRequest<bool>
{
    public Guid WebhookId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;

    public EnviarWebhookCommand(Guid webhookId, string eventType, string payload)
    {
        WebhookId = webhookId;
        EventType = eventType;
        Payload = payload;
    }
}
