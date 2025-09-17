using System;
using System.Threading.Tasks;
using FintechPSP.UserService.DTOs;

namespace FintechPSP.UserService.Services;

/// <summary>
/// Interface para serviço de tokenização de credenciais
/// </summary>
public interface ICredentialsTokenService
{
    /// <summary>
    /// Tokeniza as credenciais de uma conta
    /// </summary>
    /// <param name="contaId">ID da conta</param>
    /// <param name="credentials">Credenciais a serem tokenizadas</param>
    /// <returns>ID do token gerado</returns>
    Task<string> TokenizeCredentialsAsync(Guid contaId, ContaCredentials credentials);

    /// <summary>
    /// Recupera as credenciais a partir do token
    /// </summary>
    /// <param name="tokenId">ID do token</param>
    /// <returns>Credenciais descriptografadas</returns>
    Task<ContaCredentials?> GetCredentialsAsync(string tokenId);

    /// <summary>
    /// Atualiza as credenciais de um token existente
    /// </summary>
    /// <param name="tokenId">ID do token</param>
    /// <param name="credentials">Novas credenciais</param>
    /// <returns>True se atualizado com sucesso</returns>
    Task<bool> UpdateCredentialsAsync(string tokenId, ContaCredentials credentials);

    /// <summary>
    /// Remove um token de credenciais
    /// </summary>
    /// <param name="tokenId">ID do token</param>
    /// <returns>True se removido com sucesso</returns>
    Task<bool> RemoveCredentialsAsync(string tokenId);
}
