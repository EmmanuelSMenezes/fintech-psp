using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.AuthService.DTOs;

/// <summary>
/// Request para obter token OAuth 2.0
/// </summary>
public class TokenRequest
{
    [JsonPropertyName("grant_type")]
    [Required]
    public string GrantType { get; set; } = "client_credentials";

    [JsonPropertyName("client_id")]
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
