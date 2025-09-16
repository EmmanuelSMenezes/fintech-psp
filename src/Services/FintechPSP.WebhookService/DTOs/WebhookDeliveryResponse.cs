using System.Text.Json.Serialization;

namespace FintechPSP.WebhookService.DTOs;

/// <summary>
/// Response com informações de entrega do webhook
/// </summary>
public class WebhookDeliveryResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("webhookId")]
    public Guid WebhookId { get; set; }

    [JsonPropertyName("eventType")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty; // PENDING, SUCCESS, FAILED, RETRYING

    [JsonPropertyName("httpStatusCode")]
    public int? HttpStatusCode { get; set; }

    [JsonPropertyName("responseBody")]
    public string? ResponseBody { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("attemptCount")]
    public int AttemptCount { get; set; }

    [JsonPropertyName("nextRetryAt")]
    public DateTime? NextRetryAt { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }
}

/// <summary>
/// Response com lista de entregas
/// </summary>
public class WebhookDeliveryListResponse
{
    [JsonPropertyName("deliveries")]
    public List<WebhookDeliveryResponse> Deliveries { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}
