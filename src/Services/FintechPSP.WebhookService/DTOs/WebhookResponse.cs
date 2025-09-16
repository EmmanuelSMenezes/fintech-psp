using System.Text.Json.Serialization;

namespace FintechPSP.WebhookService.DTOs;

/// <summary>
/// Response com informações do webhook
/// </summary>
public class WebhookResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastTriggered")]
    public DateTime? LastTriggered { get; set; }

    [JsonPropertyName("successCount")]
    public int SuccessCount { get; set; }

    [JsonPropertyName("failureCount")]
    public int FailureCount { get; set; }
}

/// <summary>
/// Response com lista de webhooks
/// </summary>
public class WebhookListResponse
{
    [JsonPropertyName("webhooks")]
    public List<WebhookResponse> Webhooks { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}
