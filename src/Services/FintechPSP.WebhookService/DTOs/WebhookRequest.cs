using System.Text.Json.Serialization;

namespace FintechPSP.WebhookService.DTOs;

/// <summary>
/// Request para cadastrar webhook
/// </summary>
public class WebhookRequest
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("events")]
    public List<string> Events { get; set; } = new();

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; } = true;

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Request para atualizar webhook
/// </summary>
public class UpdateWebhookRequest
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("events")]
    public List<string>? Events { get; set; }

    [JsonPropertyName("secret")]
    public string? Secret { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
