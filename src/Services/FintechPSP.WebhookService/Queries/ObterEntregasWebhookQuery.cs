using FintechPSP.WebhookService.DTOs;
using MediatR;

namespace FintechPSP.WebhookService.Queries;

/// <summary>
/// Query para obter entregas de um webhook
/// </summary>
public class ObterEntregasWebhookQuery : IRequest<WebhookDeliveryListResponse>
{
    public Guid WebhookId { get; set; }
    public Guid ClientId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string? Status { get; set; }

    public ObterEntregasWebhookQuery(Guid webhookId, Guid clientId, int page = 1, 
        int pageSize = 50, string? status = null)
    {
        WebhookId = webhookId;
        ClientId = clientId;
        Page = page;
        PageSize = pageSize;
        Status = status;
    }
}
