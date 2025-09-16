using System.Text.Json.Serialization;

namespace FintechPSP.AuthService.DTOs;

/// <summary>
/// Response do token OAuth 2.0
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; } = 3600;

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}
