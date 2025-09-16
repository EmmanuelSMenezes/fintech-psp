using System;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando um token OAuth é renovado
/// </summary>
public class TokenRenovado : DomainEvent
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();

    public TokenRenovado() { }

    public TokenRenovado(string clientId, string accessToken, DateTime expiresAt, string[] scopes)
    {
        ClientId = clientId;
        AccessToken = accessToken;
        ExpiresAt = expiresAt;
        Scopes = scopes;
    }
}

/// <summary>
/// Evento disparado quando uma autenticação OAuth é realizada
/// </summary>
public class AutenticacaoOAuthRealizada : DomainEvent
{
    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = string.Empty;
    
    [JsonPropertyName("scopes")]
    public string[] Scopes { get; set; } = Array.Empty<string>();
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonPropertyName("ipAddress")]
    public string? IpAddress { get; set; }

    public AutenticacaoOAuthRealizada() { }

    public AutenticacaoOAuthRealizada(string clientId, string[] scopes, DateTime expiresAt, string? ipAddress = null)
    {
        ClientId = clientId;
        Scopes = scopes;
        ExpiresAt = expiresAt;
        IpAddress = ipAddress;
    }
}
