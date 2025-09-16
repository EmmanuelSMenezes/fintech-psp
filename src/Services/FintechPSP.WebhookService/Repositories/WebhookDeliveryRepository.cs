using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.WebhookService.Models;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.WebhookService.Repositories;

/// <summary>
/// Implementação do repository de entregas de webhook usando Dapper
/// </summary>
public class WebhookDeliveryRepository : IWebhookDeliveryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WebhookDeliveryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<WebhookDelivery?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, webhook_id as WebhookId, event_type as EventType, payload,
                   status, http_status_code as HttpStatusCode, response_body as ResponseBody,
                   error_message as ErrorMessage, attempt_count as AttemptCount,
                   next_retry_at as NextRetryAt, created_at as CreatedAt, delivered_at as DeliveredAt
            FROM webhook_deliveries 
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<WebhookDelivery>(sql, new { Id = id });
    }

    public async Task<IEnumerable<WebhookDelivery>> GetByWebhookIdAsync(Guid webhookId, int page = 1, int pageSize = 50, string? status = null)
    {
        var sql = @"
            SELECT id, webhook_id as WebhookId, event_type as EventType, payload,
                   status, http_status_code as HttpStatusCode, response_body as ResponseBody,
                   error_message as ErrorMessage, attempt_count as AttemptCount,
                   next_retry_at as NextRetryAt, created_at as CreatedAt, delivered_at as DeliveredAt
            FROM webhook_deliveries 
            WHERE webhook_id = @WebhookId";

        object parameters = new { WebhookId = webhookId, Offset = (page - 1) * pageSize, PageSize = pageSize };

        if (!string.IsNullOrEmpty(status))
        {
            sql += " AND status = @Status";
            parameters = new { WebhookId = webhookId, Status = status, Offset = (page - 1) * pageSize, PageSize = pageSize };
        }

        sql += " ORDER BY created_at DESC OFFSET @Offset LIMIT @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<WebhookDelivery>(sql, parameters);
    }

    public async Task<int> CountByWebhookIdAsync(Guid webhookId, string? status = null)
    {
        var sql = "SELECT COUNT(*) FROM webhook_deliveries WHERE webhook_id = @WebhookId";
        object parameters = new { WebhookId = webhookId };

        if (!string.IsNullOrEmpty(status))
        {
            sql += " AND status = @Status";
            parameters = new { WebhookId = webhookId, Status = status };
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task<WebhookDelivery> CreateAsync(WebhookDelivery delivery)
    {
        const string sql = @"
            INSERT INTO webhook_deliveries 
            (id, webhook_id, event_type, payload, status, attempt_count, created_at)
            VALUES 
            (@Id, @WebhookId, @EventType, @Payload, @Status, @AttemptCount, @CreatedAt)";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, delivery);
        
        return delivery;
    }

    public async Task<WebhookDelivery> UpdateAsync(WebhookDelivery delivery)
    {
        const string sql = @"
            UPDATE webhook_deliveries 
            SET status = @Status, http_status_code = @HttpStatusCode, response_body = @ResponseBody,
                error_message = @ErrorMessage, attempt_count = @AttemptCount,
                next_retry_at = @NextRetryAt, delivered_at = @DeliveredAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, delivery);
        
        return delivery;
    }

    public async Task<IEnumerable<WebhookDelivery>> GetPendingRetriesAsync()
    {
        const string sql = @"
            SELECT id, webhook_id as WebhookId, event_type as EventType, payload,
                   status, http_status_code as HttpStatusCode, response_body as ResponseBody,
                   error_message as ErrorMessage, attempt_count as AttemptCount,
                   next_retry_at as NextRetryAt, created_at as CreatedAt, delivered_at as DeliveredAt
            FROM webhook_deliveries 
            WHERE status = 'FAILED' 
              AND attempt_count < 5 
              AND next_retry_at IS NOT NULL 
              AND next_retry_at <= NOW()
            ORDER BY next_retry_at";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<WebhookDelivery>(sql);
    }
}
