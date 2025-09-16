using FintechPSP.BalanceService.Models;

namespace FintechPSP.BalanceService.Repositories;

/// <summary>
/// Repository para contas
/// </summary>
public interface IAccountRepository
{
    /// <summary>
    /// Busca conta por cliente ID
    /// </summary>
    Task<Account?> GetByClientIdAsync(Guid clientId, string? accountId = null);

    /// <summary>
    /// Cria uma nova conta
    /// </summary>
    Task<Account> CreateAsync(Account account);

    /// <summary>
    /// Atualiza uma conta
    /// </summary>
    Task<Account> UpdateAsync(Account account);

    /// <summary>
    /// Lista todas as contas de um cliente
    /// </summary>
    Task<IEnumerable<Account>> GetAccountsByClientIdAsync(Guid clientId);
}
