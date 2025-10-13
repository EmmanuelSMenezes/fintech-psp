using FintechPSP.AuthService.Models;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Interface para repositório de API Keys
/// </summary>
public interface IApiKeyRepository
{
    /// <summary>
    /// Cria uma nova API Key
    /// </summary>
    Task<ApiKey> CreateAsync(ApiKey apiKey);

    /// <summary>
    /// Obtém API Key por chave pública
    /// </summary>
    Task<ApiKey?> GetByPublicKeyAsync(string publicKey);

    /// <summary>
    /// Obtém API Key por ID
    /// </summary>
    Task<ApiKey?> GetByIdAsync(Guid id);

    /// <summary>
    /// Lista API Keys de uma empresa
    /// </summary>
    Task<(List<ApiKey> apiKeys, int total)> GetByCompanyAsync(Guid companyId, int page = 1, int limit = 10);

    /// <summary>
    /// Atualiza última utilização da API Key
    /// </summary>
    Task UpdateLastUsedAsync(Guid id, DateTime lastUsed);

    /// <summary>
    /// Ativa/desativa API Key
    /// </summary>
    Task UpdateStatusAsync(Guid id, bool isActive);

    /// <summary>
    /// Deleta API Key
    /// </summary>
    Task DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se chave pública já existe
    /// </summary>
    Task<bool> PublicKeyExistsAsync(string publicKey);
}
