namespace FintechPSP.WebhookService.Models;

/// <summary>
/// Modelo para entrega de webhook
/// </summary>
public class WebhookDelivery
{
    public Guid Id { get; set; }
    public Guid WebhookId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Status { get; set; } = "PENDING"; // PENDING, SUCCESS, FAILED, RETRYING
    public int? HttpStatusCode { get; set; }
    public string? ResponseBody { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTime? NextRetryAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }

    public WebhookDelivery() { }

    public WebhookDelivery(Guid webhookId, string eventType, string payload)
    {
        Id = Guid.NewGuid();
        WebhookId = webhookId;
        EventType = eventType;
        Payload = payload;
        Status = "PENDING";
        AttemptCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSuccess(int httpStatusCode, string? responseBody = null)
    {
        Status = "SUCCESS";
        HttpStatusCode = httpStatusCode;
        ResponseBody = responseBody;
        DeliveredAt = DateTime.UtcNow;
        ErrorMessage = null;
        NextRetryAt = null;
    }

    public void MarkAsFailed(string errorMessage, int? httpStatusCode = null, string? responseBody = null)
    {
        Status = "FAILED";
        ErrorMessage = errorMessage;
        HttpStatusCode = httpStatusCode;
        ResponseBody = responseBody;
        AttemptCount++;

        // Calcular próxima tentativa (exponential backoff)
        var delayMinutes = Math.Pow(2, AttemptCount - 1); // 1, 2, 4, 8, 16 minutos
        NextRetryAt = DateTime.UtcNow.AddMinutes(delayMinutes);

        // Máximo de 5 tentativas
        if (AttemptCount >= 5)
        {
            NextRetryAt = null;
        }
    }

    public void MarkAsRetrying()
    {
        Status = "RETRYING";
        AttemptCount++;
    }

    public bool ShouldRetry()
    {
        return Status == "FAILED" && 
               AttemptCount < 5 && 
               NextRetryAt.HasValue && 
               NextRetryAt.Value <= DateTime.UtcNow;
    }
}
