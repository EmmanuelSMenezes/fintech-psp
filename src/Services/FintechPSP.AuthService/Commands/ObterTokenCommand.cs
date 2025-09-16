using FintechPSP.AuthService.DTOs;
using MediatR;

namespace FintechPSP.AuthService.Commands;

/// <summary>
/// Comando para obter token OAuth 2.0
/// </summary>
public class ObterTokenCommand : IRequest<TokenResponse>
{
    public string GrantType { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? Scope { get; set; }

    public ObterTokenCommand(string grantType, string clientId, string clientSecret, string? scope = null)
    {
        GrantType = grantType;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scope = scope;
    }
}
