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

    /// <summary>
    /// Debita valor de uma conta
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <param name="amount">Valor a ser debitado</param>
    /// <param name="description">Descrição da operação</param>
    /// <returns>True se o débito foi realizado com sucesso</returns>
    Task<bool> DebitAsync(Guid clientId, decimal amount, string description);

    /// <summary>
    /// Credita valor em uma conta
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <param name="amount">Valor a ser creditado</param>
    /// <param name="description">Descrição da operação</param>
    /// <returns>True se o crédito foi realizado com sucesso</returns>
    Task<bool> CreditAsync(Guid clientId, decimal amount, string description);
}
