using FintechPSP.WebhookService.Models;

namespace FintechPSP.WebhookService.Repositories;

/// <summary>
/// Repository para webhooks
/// </summary>
public interface IWebhookRepository
{
    /// <summary>
    /// Busca webhook por ID
    /// </summary>
    Task<Webhook?> GetByIdAsync(Guid id, Guid clientId);

    /// <summary>
    /// Lista webhooks de um cliente
    /// </summary>
    Task<IEnumerable<Webhook>> GetByClientIdAsync(Guid clientId, int page = 1, int pageSize = 50, bool? active = null);

    /// <summary>
    /// Conta total de webhooks de um cliente
    /// </summary>
    Task<int> CountByClientIdAsync(Guid clientId, bool? active = null);

    /// <summary>
    /// Cria um novo webhook
    /// </summary>
    Task<Webhook> CreateAsync(Webhook webhook);

    /// <summary>
    /// Atualiza um webhook
    /// </summary>
    Task<Webhook> UpdateAsync(Webhook webhook);

    /// <summary>
    /// Deleta um webhook
    /// </summary>
    Task<bool> DeleteAsync(Guid id, Guid clientId);

    /// <summary>
    /// Busca webhooks ativos por evento
    /// </summary>
    Task<IEnumerable<Webhook>> GetActiveWebhooksByEventAsync(string eventType);
}
