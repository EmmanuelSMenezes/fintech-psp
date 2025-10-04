using FintechPSP.TransactionService.Models;

namespace FintechPSP.TransactionService.Repositories;

/// <summary>
/// Repository para transações
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Busca transação por ID
    /// </summary>
    Task<Transaction?> GetByIdAsync(Guid id);

    /// <summary>
    /// Busca transação por external ID
    /// </summary>
    Task<Transaction?> GetByExternalIdAsync(string externalId);

    /// <summary>
    /// Busca transação por EndToEndId (PIX)
    /// </summary>
    Task<Transaction?> GetByEndToEndIdAsync(string endToEndId);

    /// <summary>
    /// Cria uma nova transação
    /// </summary>
    Task<Transaction> CreateAsync(Transaction transaction);

    /// <summary>
    /// Atualiza uma transação
    /// </summary>
    Task<Transaction> UpdateAsync(Transaction transaction);

    /// <summary>
    /// Lista transações por status
    /// </summary>
    Task<IEnumerable<Transaction>> GetByStatusAsync(Shared.Domain.Enums.TransactionStatus status);

    /// <summary>
    /// Lista transações por período
    /// </summary>
    Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Lista transações com paginação
    /// </summary>
    Task<(IEnumerable<Transaction> transactions, int totalCount)> GetPagedAsync(int page, int pageSize);
}
