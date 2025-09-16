using FintechPSP.WebhookService.DTOs;
using MediatR;

namespace FintechPSP.WebhookService.Queries;

/// <summary>
/// Query para obter webhooks de um cliente
/// </summary>
public class ObterWebhooksQuery : IRequest<WebhookListResponse>
{
    public Guid ClientId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public bool? Active { get; set; }

    public ObterWebhooksQuery(Guid clientId, int page = 1, int pageSize = 50, bool? active = null)
    {
        ClientId = clientId;
        Page = page;
        PageSize = pageSize;
        Active = active;
    }
}
