using FintechPSP.IntegrationService.Models.Sicoob.OAuth;

namespace FintechPSP.IntegrationService.Services.Sicoob;

/// <summary>
/// Interface para serviço de autenticação OAuth 2.0 do Sicoob
/// </summary>
public interface ISicoobAuthService
{
    /// <summary>
    /// Obtém token de acesso válido (usa cache se disponível)
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Token de acesso</returns>
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Força a renovação do token
    /// </summary>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Nova resposta de token</returns>
    Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se o token atual é válido
    /// </summary>
    /// <returns>True se o token é válido</returns>
    bool IsTokenValid();
}
