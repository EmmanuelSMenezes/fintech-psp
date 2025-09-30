using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.OAuth;

/// <summary>
/// Resposta do token OAuth 2.0 do Sicoob
/// </summary>
public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Data de expiração calculada
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Verifica se o token está expirado
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Verifica se o token está próximo do vencimento (5 minutos)
    /// </summary>
    public bool IsExpiringSoon => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5);
}
