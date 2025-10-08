using System.Text.Json;
using System.Text.Json.Serialization;
using FintechPSP.TransactionService.Models;
using FintechPSP.TransactionService.Repositories;

namespace FintechPSP.TransactionService.Services;

/// <summary>
/// Serviço para conciliação de transações com bancos
/// </summary>
public class ReconciliationService : IReconciliationService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(
        ITransactionRepository transactionRepository,
        HttpClient httpClient,
        ILogger<ReconciliationService> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReconciliationResult> ReconcileSicoobTransactionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Iniciando conciliação Sicoob - Período: {StartDate} a {EndDate}", startDate, endDate);

            // 1. Buscar transações internas
            var internalTransactions = await GetInternalTransactions(startDate, endDate);
            _logger.LogInformation("Encontradas {Count} transações internas", internalTransactions.Count);

            // 2. Buscar extrato Sicoob
            var sicoobTransactions = await GetSicoobTransactions(startDate, endDate);
            _logger.LogInformation("Encontradas {Count} transações no Sicoob", sicoobTransactions.Count);

            // 3. Realizar conciliação
            var reconciliationResult = await PerformReconciliation(internalTransactions, sicoobTransactions);

            // 4. Salvar resultado da conciliação
            await SaveReconciliationResult(reconciliationResult);

            _logger.LogInformation("Conciliação concluída - Conciliadas: {Reconciled}, Divergentes: {Divergent}", 
                reconciliationResult.ReconciledTransactions.Count, 
                reconciliationResult.DivergentTransactions.Count);

            return reconciliationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na conciliação Sicoob");
            throw;
        }
    }

    public async Task<ReconciliationResult> GetReconciliationHistoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Consultando histórico de conciliação - Período: {StartDate} a {EndDate}", startDate, endDate);

            // Buscar resultados de conciliação salvos
            var reconciliationHistory = await _transactionRepository.GetReconciliationHistoryAsync(startDate, endDate);

            return new ReconciliationResult
            {
                StartDate = startDate,
                EndDate = endDate,
                ProcessedAt = DateTime.UtcNow,
                ReconciledTransactions = reconciliationHistory.Where(r => r.Status == ReconciliationStatus.Reconciled).ToList(),
                DivergentTransactions = reconciliationHistory.Where(r => r.Status == ReconciliationStatus.Divergent).ToList(),
                MissingInSicoob = reconciliationHistory.Where(r => r.Status == ReconciliationStatus.MissingInBank).ToList(),
                MissingInInternal = reconciliationHistory.Where(r => r.Status == ReconciliationStatus.MissingInInternal).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar histórico de conciliação");
            throw;
        }
    }

    private async Task<List<InternalTransaction>> GetInternalTransactions(DateTime startDate, DateTime endDate)
    {
        var transactions = await _transactionRepository.GetTransactionsByDateRangeAsync(startDate, endDate);
        
        return transactions
            .Where(t => t.BankCode == "756") // Sicoob
            .Select(t => new InternalTransaction
            {
                TransactionId = t.TransactionId,
                ExternalId = t.ExternalId,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Status = t.Status.ToString(),
                CreatedAt = t.CreatedAt,
                TxId = t.TxId,
                EndToEndId = t.EndToEndId,
                NossoNumero = t.NossoNumero
            })
            .ToList();
    }

    private async Task<List<SicoobTransaction>> GetSicoobTransactions(DateTime startDate, DateTime endDate)
    {
        try
        {
            // Chamar API do IntegrationService para buscar extrato Sicoob
            var url = $"http://localhost:5005/integrations/sicoob/conta/extrato?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            
            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erro ao buscar extrato Sicoob - Status: {StatusCode}", response.StatusCode);
                return new List<SicoobTransaction>();
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var sicoobResponse = JsonSerializer.Deserialize<SicoobStatementResponse>(jsonContent);

            return sicoobResponse?.Transactions ?? new List<SicoobTransaction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar transações do Sicoob");
            return new List<SicoobTransaction>();
        }
    }

    private async Task<ReconciliationResult> PerformReconciliation(
        List<InternalTransaction> internalTransactions, 
        List<SicoobTransaction> sicoobTransactions)
    {
        var result = new ReconciliationResult
        {
            StartDate = DateTime.Today.AddDays(-30),
            EndDate = DateTime.Today,
            ProcessedAt = DateTime.UtcNow
        };

        // Conciliar por TxId (PIX) e NossoNumero (Boleto)
        foreach (var internalTx in internalTransactions)
        {
            var matchingSicoobTx = FindMatchingTransaction(internalTx, sicoobTransactions);

            if (matchingSicoobTx != null)
            {
                // Verificar se os valores batem
                if (Math.Abs(internalTx.Amount - matchingSicoobTx.Amount) < 0.01m)
                {
                    result.ReconciledTransactions.Add(new ReconciledTransaction
                    {
                        InternalTransaction = internalTx,
                        SicoobTransaction = matchingSicoobTx,
                        Status = ReconciliationStatus.Reconciled,
                        ReconciledAt = DateTime.UtcNow
                    });
                }
                else
                {
                    result.DivergentTransactions.Add(new ReconciledTransaction
                    {
                        InternalTransaction = internalTx,
                        SicoobTransaction = matchingSicoobTx,
                        Status = ReconciliationStatus.Divergent,
                        DivergenceReason = $"Valor divergente - Interno: {internalTx.Amount:C}, Sicoob: {matchingSicoobTx.Amount:C}",
                        ReconciledAt = DateTime.UtcNow
                    });
                }
            }
            else
            {
                result.MissingInSicoob.Add(new ReconciledTransaction
                {
                    InternalTransaction = internalTx,
                    Status = ReconciliationStatus.MissingInBank,
                    DivergenceReason = "Transação não encontrada no Sicoob",
                    ReconciledAt = DateTime.UtcNow
                });
            }
        }

        // Verificar transações que existem no Sicoob mas não internamente
        foreach (var sicoobTx in sicoobTransactions)
        {
            var matchingInternalTx = internalTransactions.FirstOrDefault(i => 
                (!string.IsNullOrEmpty(i.TxId) && i.TxId == sicoobTx.TxId) ||
                (!string.IsNullOrEmpty(i.NossoNumero) && i.NossoNumero == sicoobTx.NossoNumero));

            if (matchingInternalTx == null)
            {
                result.MissingInInternal.Add(new ReconciledTransaction
                {
                    SicoobTransaction = sicoobTx,
                    Status = ReconciliationStatus.MissingInInternal,
                    DivergenceReason = "Transação não encontrada internamente",
                    ReconciledAt = DateTime.UtcNow
                });
            }
        }

        return result;
    }

    private SicoobTransaction? FindMatchingTransaction(InternalTransaction internalTx, List<SicoobTransaction> sicoobTransactions)
    {
        // Tentar match por TxId (PIX)
        if (!string.IsNullOrEmpty(internalTx.TxId))
        {
            var match = sicoobTransactions.FirstOrDefault(s => s.TxId == internalTx.TxId);
            if (match != null) return match;
        }

        // Tentar match por EndToEndId (PIX)
        if (!string.IsNullOrEmpty(internalTx.EndToEndId))
        {
            var match = sicoobTransactions.FirstOrDefault(s => s.EndToEndId == internalTx.EndToEndId);
            if (match != null) return match;
        }

        // Tentar match por NossoNumero (Boleto)
        if (!string.IsNullOrEmpty(internalTx.NossoNumero))
        {
            var match = sicoobTransactions.FirstOrDefault(s => s.NossoNumero == internalTx.NossoNumero);
            if (match != null) return match;
        }

        // Tentar match por ExternalId e valor (fallback)
        return sicoobTransactions.FirstOrDefault(s => 
            s.ExternalId == internalTx.ExternalId && 
            Math.Abs(s.Amount - internalTx.Amount) < 0.01m);
    }

    private async Task SaveReconciliationResult(ReconciliationResult result)
    {
        try
        {
            // Salvar todas as transações conciliadas
            var allReconciliations = new List<ReconciledTransaction>();
            allReconciliations.AddRange(result.ReconciledTransactions);
            allReconciliations.AddRange(result.DivergentTransactions);
            allReconciliations.AddRange(result.MissingInSicoob);
            allReconciliations.AddRange(result.MissingInInternal);

            await _transactionRepository.SaveReconciliationResultAsync(allReconciliations);
            
            _logger.LogInformation("Resultado da conciliação salvo - {Count} registros", allReconciliations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar resultado da conciliação");
            throw;
        }
    }
}

/// <summary>
/// Interface para o serviço de conciliação
/// </summary>
public interface IReconciliationService
{
    Task<ReconciliationResult> ReconcileSicoobTransactionsAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<ReconciliationResult> GetReconciliationHistoryAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado da conciliação
/// </summary>
public class ReconciliationResult
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<ReconciledTransaction> ReconciledTransactions { get; set; } = new();
    public List<ReconciledTransaction> DivergentTransactions { get; set; } = new();
    public List<ReconciledTransaction> MissingInSicoob { get; set; } = new();
    public List<ReconciledTransaction> MissingInInternal { get; set; } = new();

    public int TotalTransactions => ReconciledTransactions.Count + DivergentTransactions.Count + MissingInSicoob.Count + MissingInInternal.Count;
    public decimal ReconciliationRate => TotalTransactions > 0 ? (decimal)ReconciledTransactions.Count / TotalTransactions * 100 : 0;
}

/// <summary>
/// Transação conciliada
/// </summary>
public class ReconciledTransaction
{
    public InternalTransaction? InternalTransaction { get; set; }
    public SicoobTransaction? SicoobTransaction { get; set; }
    public ReconciliationStatus Status { get; set; }
    public string? DivergenceReason { get; set; }
    public DateTime ReconciledAt { get; set; }
}

/// <summary>
/// Status da conciliação
/// </summary>
public enum ReconciliationStatus
{
    Reconciled,
    Divergent,
    MissingInBank,
    MissingInInternal
}

/// <summary>
/// Transação interna
/// </summary>
public class InternalTransaction
{
    public Guid TransactionId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? TxId { get; set; }
    public string? EndToEndId { get; set; }
    public string? NossoNumero { get; set; }
}

/// <summary>
/// Transação do Sicoob
/// </summary>
public class SicoobTransaction
{
    [JsonPropertyName("txId")]
    public string? TxId { get; set; }

    [JsonPropertyName("endToEndId")]
    public string? EndToEndId { get; set; }

    [JsonPropertyName("nossoNumero")]
    public string? NossoNumero { get; set; }

    [JsonPropertyName("externalId")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("processedAt")]
    public DateTime ProcessedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Resposta do extrato Sicoob
/// </summary>
public class SicoobStatementResponse
{
    [JsonPropertyName("transactions")]
    public List<SicoobTransaction> Transactions { get; set; } = new();

    [JsonPropertyName("startDate")]
    public DateTime StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime EndDate { get; set; }

    [JsonPropertyName("totalTransactions")]
    public int TotalTransactions { get; set; }
}
