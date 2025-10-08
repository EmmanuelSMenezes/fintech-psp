using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FintechPSP.TransactionService.Services;

namespace FintechPSP.TransactionService.Controllers;

/// <summary>
/// Controller para conciliação de transações
/// </summary>
[ApiController]
[Route("reconciliation")]
[Authorize]
public class ReconciliationController : ControllerBase
{
    private readonly IReconciliationService _reconciliationService;
    private readonly ILogger<ReconciliationController> _logger;

    public ReconciliationController(
        IReconciliationService reconciliationService,
        ILogger<ReconciliationController> logger)
    {
        _reconciliationService = reconciliationService ?? throw new ArgumentNullException(nameof(reconciliationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Executar conciliação com Sicoob
    /// </summary>
    /// <param name="request">Parâmetros da conciliação</param>
    /// <returns>Resultado da conciliação</returns>
    [HttpPost("sicoob")]
    public async Task<ActionResult<ReconciliationResult>> ReconcileSicoob([FromBody] ReconciliationRequest request)
    {
        try
        {
            _logger.LogInformation("Iniciando conciliação Sicoob - Período: {StartDate} a {EndDate}", 
                request.StartDate, request.EndDate);

            var result = await _reconciliationService.ReconcileSicoobTransactionsAsync(
                request.StartDate, request.EndDate);

            _logger.LogInformation("Conciliação concluída - Taxa: {Rate}%, Total: {Total}", 
                result.ReconciliationRate, result.TotalTransactions);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na conciliação Sicoob");
            return StatusCode(500, new { error = "Erro interno na conciliação" });
        }
    }

    /// <summary>
    /// Consultar histórico de conciliação
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Histórico de conciliação</returns>
    [HttpGet("sicoob/history")]
    public async Task<ActionResult<ReconciliationResult>> GetReconciliationHistory(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Consultando histórico de conciliação - Período: {StartDate} a {EndDate}", 
                startDate, endDate);

            var result = await _reconciliationService.GetReconciliationHistoryAsync(startDate, endDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar histórico de conciliação");
            return StatusCode(500, new { error = "Erro interno na consulta" });
        }
    }

    /// <summary>
    /// Executar conciliação automática (últimos 30 dias)
    /// </summary>
    /// <returns>Resultado da conciliação</returns>
    [HttpPost("sicoob/auto")]
    public async Task<ActionResult<ReconciliationResult>> AutoReconcileSicoob()
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            _logger.LogInformation("Iniciando conciliação automática Sicoob - Últimos 30 dias");

            var result = await _reconciliationService.ReconcileSicoobTransactionsAsync(startDate, endDate);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na conciliação automática Sicoob");
            return StatusCode(500, new { error = "Erro interno na conciliação automática" });
        }
    }

    /// <summary>
    /// Obter estatísticas de conciliação
    /// </summary>
    /// <param name="days">Número de dias para análise (padrão: 30)</param>
    /// <returns>Estatísticas de conciliação</returns>
    [HttpGet("sicoob/stats")]
    public async Task<ActionResult<ReconciliationStats>> GetReconciliationStats([FromQuery] int days = 30)
    {
        try
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days);

            _logger.LogInformation("Consultando estatísticas de conciliação - Últimos {Days} dias", days);

            var result = await _reconciliationService.GetReconciliationHistoryAsync(startDate, endDate);

            var stats = new ReconciliationStats
            {
                Period = $"Últimos {days} dias",
                StartDate = startDate,
                EndDate = endDate,
                TotalTransactions = result.TotalTransactions,
                ReconciledCount = result.ReconciledTransactions.Count,
                DivergentCount = result.DivergentTransactions.Count,
                MissingInBankCount = result.MissingInSicoob.Count,
                MissingInInternalCount = result.MissingInInternal.Count,
                ReconciliationRate = result.ReconciliationRate,
                TotalAmount = result.ReconciledTransactions.Sum(t => t.InternalTransaction?.Amount ?? 0),
                DivergentAmount = result.DivergentTransactions.Sum(t => t.InternalTransaction?.Amount ?? 0)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar estatísticas de conciliação");
            return StatusCode(500, new { error = "Erro interno na consulta de estatísticas" });
        }
    }

    /// <summary>
    /// Exportar resultado de conciliação para CSV
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <returns>Arquivo CSV</returns>
    [HttpGet("sicoob/export")]
    public async Task<IActionResult> ExportReconciliation(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Exportando conciliação - Período: {StartDate} a {EndDate}", 
                startDate, endDate);

            var result = await _reconciliationService.GetReconciliationHistoryAsync(startDate, endDate);

            var csv = GenerateCsv(result);
            var fileName = $"conciliacao_sicoob_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao exportar conciliação");
            return StatusCode(500, new { error = "Erro interno na exportação" });
        }
    }

    /// <summary>
    /// Health check do serviço de conciliação
    /// </summary>
    /// <returns>Status do serviço</returns>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "ReconciliationService"
        });
    }

    #region Private Methods

    private string GenerateCsv(ReconciliationResult result)
    {
        var csv = new System.Text.StringBuilder();
        
        // Header
        csv.AppendLine("Status,TransactionId,ExternalId,Amount,Type,TxId,EndToEndId,NossoNumero,CreatedAt,DivergenceReason");

        // Reconciled transactions
        foreach (var tx in result.ReconciledTransactions)
        {
            csv.AppendLine($"Conciliada,{tx.InternalTransaction?.TransactionId},{tx.InternalTransaction?.ExternalId},{tx.InternalTransaction?.Amount:F2},{tx.InternalTransaction?.Type},{tx.InternalTransaction?.TxId},{tx.InternalTransaction?.EndToEndId},{tx.InternalTransaction?.NossoNumero},{tx.InternalTransaction?.CreatedAt:yyyy-MM-dd HH:mm:ss},");
        }

        // Divergent transactions
        foreach (var tx in result.DivergentTransactions)
        {
            csv.AppendLine($"Divergente,{tx.InternalTransaction?.TransactionId},{tx.InternalTransaction?.ExternalId},{tx.InternalTransaction?.Amount:F2},{tx.InternalTransaction?.Type},{tx.InternalTransaction?.TxId},{tx.InternalTransaction?.EndToEndId},{tx.InternalTransaction?.NossoNumero},{tx.InternalTransaction?.CreatedAt:yyyy-MM-dd HH:mm:ss},\"{tx.DivergenceReason}\"");
        }

        // Missing in Sicoob
        foreach (var tx in result.MissingInSicoob)
        {
            csv.AppendLine($"Faltante no Sicoob,{tx.InternalTransaction?.TransactionId},{tx.InternalTransaction?.ExternalId},{tx.InternalTransaction?.Amount:F2},{tx.InternalTransaction?.Type},{tx.InternalTransaction?.TxId},{tx.InternalTransaction?.EndToEndId},{tx.InternalTransaction?.NossoNumero},{tx.InternalTransaction?.CreatedAt:yyyy-MM-dd HH:mm:ss},\"{tx.DivergenceReason}\"");
        }

        // Missing in Internal
        foreach (var tx in result.MissingInInternal)
        {
            csv.AppendLine($"Faltante Interno,,{tx.SicoobTransaction?.ExternalId},{tx.SicoobTransaction?.Amount:F2},{tx.SicoobTransaction?.Type},{tx.SicoobTransaction?.TxId},{tx.SicoobTransaction?.EndToEndId},{tx.SicoobTransaction?.NossoNumero},{tx.SicoobTransaction?.ProcessedAt:yyyy-MM-dd HH:mm:ss},\"{tx.DivergenceReason}\"");
        }

        return csv.ToString();
    }

    #endregion
}

/// <summary>
/// Request para conciliação
/// </summary>
public class ReconciliationRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// Estatísticas de conciliação
/// </summary>
public class ReconciliationStats
{
    public string Period { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTransactions { get; set; }
    public int ReconciledCount { get; set; }
    public int DivergentCount { get; set; }
    public int MissingInBankCount { get; set; }
    public int MissingInInternalCount { get; set; }
    public decimal ReconciliationRate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DivergentAmount { get; set; }
}
