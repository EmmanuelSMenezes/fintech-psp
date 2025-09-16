using FintechPSP.WebhookService.Models;

namespace FintechPSP.WebhookService.Repositories;

/// <summary>
/// Repository para entregas de webhook
/// </summary>
public interface IWebhookDeliveryRepository
{
    /// <summary>
    /// Busca entrega por ID
    /// </summary>
    Task<WebhookDelivery?> GetByIdAsync(Guid id);

    /// <summary>
    /// Lista entregas de um webhook
    /// </summary>
    Task<IEnumerable<WebhookDelivery>> GetByWebhookIdAsync(Guid webhookId, int page = 1, int pageSize = 50, string? status = null);

    /// <summary>
    /// Conta total de entregas de um webhook
    /// </summary>
    Task<int> CountByWebhookIdAsync(Guid webhookId, string? status = null);

    /// <summary>
    /// Cria uma nova entrega
    /// </summary>
    Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery);

    /// <summary>
    /// Atualiza uma entrega
    /// </summary>
    Task<WebhookDelivery> UpdateAsync(WebhookDelivery delivery);

    /// <summary>
    /// Busca entregas pendentes de retry
    /// </summary>
    Task<IEnumerable<WebhookDelivery>> GetPendingRetriesAsync();
}
