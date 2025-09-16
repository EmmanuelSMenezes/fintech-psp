namespace FintechPSP.BalanceService.Repositories;

/// <summary>
/// Modelo para histórico de transações
/// </summary>
public class TransactionHistory
{
    public Guid TransactionId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Operation { get; set; } = string.Empty; // DEBIT, CREDIT
}

/// <summary>
/// Repository para histórico de transações
/// </summary>
public interface ITransactionHistoryRepository
{
    /// <summary>
    /// Busca histórico de transações
    /// </summary>
    Task<IEnumerable<TransactionHistory>> GetTransactionHistoryAsync(
        Guid clientId,
        string? accountId,
        DateTime startDate,
        DateTime endDate,
        int page = 1,
        int pageSize = 50);

    /// <summary>
    /// Conta total de transações no período
    /// </summary>
    Task<int> GetTransactionCountAsync(
        Guid clientId,
        string? accountId,
        DateTime startDate,
        DateTime endDate);

    /// <summary>
    /// Adiciona transação ao histórico
    /// </summary>
    Task AddTransactionAsync(TransactionHistory transaction);
}
