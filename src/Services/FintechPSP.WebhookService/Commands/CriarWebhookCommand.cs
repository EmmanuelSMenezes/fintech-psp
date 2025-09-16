using FintechPSP.WebhookService.DTOs;
using MediatR;

namespace FintechPSP.WebhookService.Commands;

/// <summary>
/// Command para criar webhook
/// </summary>
public class CriarWebhookCommand : IRequest<WebhookResponse>
{
    public Guid ClientId { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string> Events { get; set; } = new();
    public string? Secret { get; set; }
    public string? Description { get; set; }

    public CriarWebhookCommand(Guid clientId, string url, List<string> events, 
        string? secret = null, string? description = null)
    {
        ClientId = clientId;
        Url = url;
        Events = events;
        Secret = secret;
        Description = description;
    }
}
