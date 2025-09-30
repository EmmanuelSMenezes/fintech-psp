using FintechPSP.ConfigService.Models;

namespace FintechPSP.ConfigService.Repositories;

/// <summary>
/// Repository para configurações bancárias
/// </summary>
public interface IBankingConfigRepository
{
    /// <summary>
    /// Busca configuração por ID
    /// </summary>
    Task<BankingConfig?> GetByIdAsync(Guid id);

    /// <summary>
    /// Busca configuração por nome
    /// </summary>
    Task<BankingConfig?> GetByNameAsync(string name);

    /// <summary>
    /// Lista todas as configurações
    /// </summary>
    Task<IEnumerable<BankingConfig>> GetAllAsync();

    /// <summary>
    /// Lista configurações paginadas
    /// </summary>
    Task<IEnumerable<BankingConfig>> GetPagedAsync(int page = 1, int pageSize = 50, bool? enabled = null);

    /// <summary>
    /// Conta total de configurações
    /// </summary>
    Task<int> CountAsync(bool? enabled = null);

    /// <summary>
    /// Lista configurações por tipo
    /// </summary>
    Task<IEnumerable<BankingConfig>> GetByTypeAsync(string type);

    /// <summary>
    /// Cria uma nova configuração
    /// </summary>
    Task<BankingConfig> CreateAsync(BankingConfig config);

    /// <summary>
    /// Atualiza uma configuração
    /// </summary>
    Task<BankingConfig> UpdateAsync(BankingConfig config);

    /// <summary>
    /// Deleta uma configuração
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se existe configuração com o nome
    /// </summary>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);

    /// <summary>
    /// Ativa/desativa uma configuração
    /// </summary>
    Task<bool> SetEnabledAsync(Guid id, bool enabled, string? updatedBy = null);
}
