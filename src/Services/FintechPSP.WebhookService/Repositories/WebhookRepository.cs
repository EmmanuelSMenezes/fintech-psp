using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.WebhookService.Models;
using FintechPSP.Shared.Infrastructure.Database;
using System.Text.Json;

namespace FintechPSP.WebhookService.Repositories;

/// <summary>
/// Implementação do repository de webhooks usando Dapper
/// </summary>
public class WebhookRepository : IWebhookRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WebhookRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Webhook?> GetByIdAsync(Guid id, Guid clientId)
    {
        const string sql = @"
            SELECT id, client_id as ClientId, url, events, secret, active, description,
                   created_at as CreatedAt, last_triggered as LastTriggered,
                   success_count as SuccessCount, failure_count as FailureCount
            FROM webhooks 
            WHERE id = @Id AND client_id = @ClientId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id, ClientId = clientId });
        
        return result != null ? MapToWebhook(result) : null;
    }

    public async Task<IEnumerable<Webhook>> GetByClientIdAsync(Guid clientId, int page = 1, int pageSize = 50, bool? active = null)
    {
        var sql = @"
            SELECT id, client_id as ClientId, url, events, secret, active, description,
                   created_at as CreatedAt, last_triggered as LastTriggered,
                   success_count as SuccessCount, failure_count as FailureCount
            FROM webhooks 
            WHERE client_id = @ClientId";

        object parameters = new { ClientId = clientId, Offset = (page - 1) * pageSize, PageSize = pageSize };

        if (active.HasValue)
        {
            sql += " AND active = @Active";
            parameters = new { ClientId = clientId, Active = active.Value, Offset = (page - 1) * pageSize, PageSize = pageSize };
        }

        sql += " ORDER BY created_at DESC OFFSET @Offset LIMIT @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, parameters);
        
        return results.Select(MapToWebhook);
    }

    public async Task<int> CountByClientIdAsync(Guid clientId, bool? active = null)
    {
        var sql = "SELECT COUNT(*) FROM webhooks WHERE client_id = @ClientId";
        object parameters = new { ClientId = clientId };

        if (active.HasValue)
        {
            sql += " AND active = @Active";
            parameters = new { ClientId = clientId, Active = active.Value };
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task<Webhook> CreateAsync(Webhook webhook)
    {
        const string sql = @"
            INSERT INTO webhooks (id, client_id, url, events, secret, active, description, created_at, success_count, failure_count)
            VALUES (@Id, @ClientId, @Url, @Events::jsonb, @Secret, @Active, @Description, @CreatedAt, @SuccessCount, @FailureCount)";

        var parameters = new
        {
            webhook.Id,
            webhook.ClientId,
            webhook.Url,
            Events = JsonSerializer.Serialize(webhook.Events),
            webhook.Secret,
            webhook.Active,
            webhook.Description,
            webhook.CreatedAt,
            webhook.SuccessCount,
            webhook.FailureCount
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return webhook;
    }

    public async Task<Webhook> UpdateAsync(Webhook webhook)
    {
        const string sql = @"
            UPDATE webhooks
            SET url = @Url, events = @Events::jsonb, secret = @Secret, active = @Active,
                description = @Description, last_triggered = @LastTriggered,
                success_count = @SuccessCount, failure_count = @FailureCount
            WHERE id = @Id AND client_id = @ClientId";

        var parameters = new
        {
            webhook.Url,
            Events = JsonSerializer.Serialize(webhook.Events),
            webhook.Secret,
            webhook.Active,
            webhook.Description,
            webhook.LastTriggered,
            webhook.SuccessCount,
            webhook.FailureCount,
            webhook.Id,
            webhook.ClientId
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return webhook;
    }

    public async Task<bool> DeleteAsync(Guid id, Guid clientId)
    {
        const string sql = "DELETE FROM webhooks WHERE id = @Id AND client_id = @ClientId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, ClientId = clientId });
        
        return rowsAffected > 0;
    }

    public async Task<IEnumerable<Webhook>> GetActiveWebhooksByEventAsync(string eventType)
    {
        const string sql = @"
            SELECT id, client_id as ClientId, url, events, secret, active, description,
                   created_at as CreatedAt, last_triggered as LastTriggered,
                   success_count as SuccessCount, failure_count as FailureCount
            FROM webhooks 
            WHERE active = true AND events::text LIKE @EventType";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, new { EventType = $"%{eventType}%" });
        
        return results.Select(MapToWebhook);
    }

    private static Webhook MapToWebhook(dynamic result)
    {
        // Usar reflection para criar instância privada
        var webhook = (Webhook)Activator.CreateInstance(typeof(Webhook), true)!;
        
        // Mapear propriedades usando reflection
        var type = typeof(Webhook);
        
        type.GetProperty("Id")?.SetValue(webhook, (Guid)result.id);
        type.GetProperty("ClientId")?.SetValue(webhook, (Guid)result.ClientId);
        type.GetProperty("Url")?.SetValue(webhook, (string)result.url);
        
        var eventsJson = (string)result.events;
        var events = JsonSerializer.Deserialize<List<string>>(eventsJson) ?? new List<string>();
        type.GetProperty("Events")?.SetValue(webhook, events);
        
        type.GetProperty("Secret")?.SetValue(webhook, result.secret as string);
        type.GetProperty("Active")?.SetValue(webhook, (bool)result.active);
        type.GetProperty("Description")?.SetValue(webhook, result.description as string);
        type.GetProperty("CreatedAt")?.SetValue(webhook, (DateTime)result.CreatedAt);
        type.GetProperty("LastTriggered")?.SetValue(webhook, result.LastTriggered as DateTime?);
        type.GetProperty("SuccessCount")?.SetValue(webhook, (int)result.SuccessCount);
        type.GetProperty("FailureCount")?.SetValue(webhook, (int)result.FailureCount);
        
        return webhook;
    }
}
