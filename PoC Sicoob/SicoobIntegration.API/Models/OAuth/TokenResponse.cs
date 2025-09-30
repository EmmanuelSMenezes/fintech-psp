using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.OAuth;

/// <summary>
/// Resposta do endpoint de autenticação OAuth 2.0
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Data/hora de expiração calculada
    /// </summary>
    [JsonIgnore]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Verifica se o token está expirado
    /// </summary>
    [JsonIgnore]
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Verifica se o token está próximo de expirar (menos de 5 minutos)
    /// </summary>
    [JsonIgnore]
    public bool IsExpiringSoon => DateTime.UtcNow.AddMinutes(5) >= ExpiresAt;
}

