using SicoobIntegration.API.Models.OAuth;

namespace SicoobIntegration.API.Services;

/// <summary>
/// Interface para serviço de autenticação OAuth 2.0 do Sicoob
/// </summary>
public interface ISicoobAuthService
{
    /// <summary>
    /// Obtém um token de acesso válido (usa cache se disponível)
    /// </summary>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Força a obtenção de um novo token
    /// </summary>
    Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o token atual está válido
    /// </summary>
    bool IsTokenValid();
}

