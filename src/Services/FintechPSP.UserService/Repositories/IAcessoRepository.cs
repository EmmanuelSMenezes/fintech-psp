using FintechPSP.UserService.Models;

namespace FintechPSP.UserService.Repositories;

/// <summary>
/// Interface para repositório de acessos
/// </summary>
public interface IAcessoRepository
{
    /// <summary>
    /// Criar um novo acesso
    /// </summary>
    Task<Acesso> CreateAsync(Acesso acesso);

    /// <summary>
    /// Obter acesso por ID
    /// </summary>
    Task<Acesso?> GetByIdAsync(Guid acessoId);

    /// <summary>
    /// Obter acesso por UserId
    /// </summary>
    Task<Acesso?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Obter acesso por email
    /// </summary>
    Task<Acesso?> GetByEmailAsync(string email);

    /// <summary>
    /// Obter todos os acessos de um usuário pai (sub-usuários)
    /// </summary>
    Task<List<Acesso>> GetByParentUserIdAsync(Guid parentUserId);

    /// <summary>
    /// Obter acessos com filtros
    /// </summary>
    Task<(List<Acesso> acessos, int total)> GetWithFiltersAsync(
        Guid? userId = null,
        Guid? parentUserId = null,
        string? role = null,
        bool? isActive = null,
        int page = 1,
        int limit = 50);

    /// <summary>
    /// Obter acessos por role
    /// </summary>
    Task<List<Acesso>> GetByRoleAsync(string role);

    /// <summary>
    /// Atualizar um acesso
    /// </summary>
    Task<Acesso> UpdateAsync(Acesso acesso);

    /// <summary>
    /// Remover um acesso
    /// </summary>
    Task<bool> DeleteAsync(Guid acessoId);

    /// <summary>
    /// Verificar se um usuário tem uma permissão específica
    /// </summary>
    Task<bool> HasPermissionAsync(Guid userId, string permission);

    /// <summary>
    /// Obter todas as permissões de um usuário
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(Guid userId);

    /// <summary>
    /// Verificar se um email já possui acesso
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Contar total de acessos por role
    /// </summary>
    Task<Dictionary<string, int>> GetAccessCountByRoleAsync();

    /// <summary>
    /// Obter acessos ativos
    /// </summary>
    Task<List<Acesso>> GetActiveAccessesAsync();

    /// <summary>
    /// Obter acessos inativos
    /// </summary>
    Task<List<Acesso>> GetInactiveAccessesAsync();

    /// <summary>
    /// Obter acessos criados em um período
    /// </summary>
    Task<List<Acesso>> GetAccessesCreatedBetweenAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Verificar se um usuário é sub-usuário de outro
    /// </summary>
    Task<bool> IsSubUserOfAsync(Guid subUserId, Guid parentUserId);

    /// <summary>
    /// Obter hierarquia de usuários (usuário pai e todos os sub-usuários)
    /// </summary>
    Task<List<Acesso>> GetUserHierarchyAsync(Guid userId);
}
