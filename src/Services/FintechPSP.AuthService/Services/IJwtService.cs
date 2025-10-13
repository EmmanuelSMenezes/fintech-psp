using System.Security.Claims;

namespace FintechPSP.AuthService.Services;

/// <summary>
/// Interface para serviço de geração de tokens JWT
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Gera token JWT com claims customizados
    /// </summary>
    /// <param name="claims">Claims para incluir no token</param>
    /// <returns>Token JWT</returns>
    string GenerateToken(Dictionary<string, object> claims);

    /// <summary>
    /// Gera token JWT para usuário do sistema
    /// </summary>
    /// <param name="user">Usuário do sistema</param>
    /// <returns>Token JWT</returns>
    string GenerateToken(Models.SystemUser user);
}
