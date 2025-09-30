using System;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Repositories;
using FintechPSP.Shared.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.TransactionService.Controllers;

/// <summary>
/// Controller para gerenciamento de transações - Administração
/// </summary>
[ApiController]
[Route("admin/transacoes")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class AdminTransactionController : ControllerBase
{
    private readonly ILogger<AdminTransactionController> _logger;
    private readonly ITransactionRepository _transactionRepository;

    public AdminTransactionController(
        ILogger<AdminTransactionController> logger,
        ITransactionRepository transactionRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
    }

    /// <summary>
    /// Lista transações (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            _logger.LogInformation("Admin listando transações - página {Page}", page);

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            // Buscar transações dos últimos 30 dias
            var allTransactions = await _transactionRepository.GetByDateRangeAsync(
                DateTime.UtcNow.AddDays(-30),
                DateTime.UtcNow);

            var transactionsList = allTransactions.ToList();
            var skip = (page - 1) * limit;
            var pagedTransactions = transactionsList.Skip(skip).Take(limit);

            var response = new
            {
                transactions = pagedTransactions.Select(t => new
                {
                    id = t.TransactionId.ToString(),
                    externalId = t.ExternalId,
                    type = t.Type.ToString().ToLower(),
                    amount = t.Amount.Amount,
                    description = t.Description ?? "",
                    status = t.Status.ToString(),
                    createdAt = t.CreatedAt,
                    updatedAt = t.UpdatedAt,
                    bankCode = t.BankCode,
                    pixKey = t.PixKey,
                    endToEndId = t.EndToEndId
                }),
                total = transactionsList.Count,
                page,
                limit,
                totalPages = (int)Math.Ceiling((double)transactionsList.Count / limit)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transações para admin");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém histórico de transações (admin)
    /// </summary>
    [HttpGet("historico")]
    public async Task<IActionResult> GetTransactionHistory(
        [FromQuery] string? type = null,
        [FromQuery] string? clienteId = null,
        [FromQuery] string? contaId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            _logger.LogInformation("Admin obtendo histórico de transações - página {Page}", page);

            if (page < 1) page = 1;
            if (limit < 1 || limit > 100) limit = 20;

            // Definir período de busca
            var startDateTime = DateTime.UtcNow.AddDays(-90); // Últimos 90 dias por padrão
            var endDateTime = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var parsedStartDate))
                startDateTime = parsedStartDate;

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var parsedEndDate))
                endDateTime = parsedEndDate;

            // Buscar transações no período
            var allTransactions = await _transactionRepository.GetByDateRangeAsync(startDateTime, endDateTime);
            var transactionsList = allTransactions.ToList();

            // Aplicar filtros se fornecidos
            if (!string.IsNullOrEmpty(type) && Enum.TryParse<TransactionType>(type, true, out var transactionType))
            {
                transactionsList = transactionsList.Where(t => t.Type == transactionType).ToList();
            }

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<TransactionStatus>(status, true, out var transactionStatus))
            {
                transactionsList = transactionsList.Where(t => t.Status == transactionStatus).ToList();
            }

            // Aplicar paginação
            var total = transactionsList.Count;
            var pagedTransactions = transactionsList
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(t => new
                {
                    id = t.TransactionId.ToString(),
                    externalId = t.ExternalId,
                    type = t.Type.ToString().ToLower(),
                    amount = t.Amount.Amount,
                    description = t.Description ?? "",
                    status = t.Status.ToString().ToLower(),
                    bankCode = t.BankCode,
                    createdAt = t.CreatedAt,
                    updatedAt = t.UpdatedAt,
                    // Campos específicos por tipo
                    pixKey = t.PixKey,
                    endToEndId = t.EndToEndId,
                    accountBranch = t.AccountBranch,
                    accountNumber = t.AccountNumber,
                    taxId = t.TaxId,
                    name = t.Name,
                    dueDate = t.DueDate,
                    payerTaxId = t.PayerTaxId,
                    payerName = t.PayerName,
                    boletoBarcode = t.BoletoBarcode,
                    boletoUrl = t.BoletoUrl,
                    cryptoType = t.CryptoType,
                    walletAddress = t.WalletAddress,
                    cryptoTxHash = t.CryptoTxHash
                });

            return Ok(new
            {
                transactions = pagedTransactions,
                total,
                page,
                limit,
                totalPages = (int)Math.Ceiling((double)total / limit),
                filters = new
                {
                    type,
                    status,
                    startDate = startDateTime.ToString("yyyy-MM-dd"),
                    endDate = endDateTime.ToString("yyyy-MM-dd")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter histórico de transações para admin");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém relatório de transações para dashboard (admin)
    /// </summary>
    [HttpGet("report")]
    public async Task<IActionResult> GetTransactionReport()
    {
        try
        {
            _logger.LogInformation("Admin obtendo relatório de transações");

            // Buscar transações dos últimos 30 dias para o relatório
            var startDate = DateTime.UtcNow.AddDays(-30);
            var endDate = DateTime.UtcNow;
            var allTransactions = await _transactionRepository.GetByDateRangeAsync(startDate, endDate);
            var transactionsList = allTransactions.ToList();

            // Calcular estatísticas
            var totalTransactions = transactionsList.Count;
            var totalVolume = transactionsList.Sum(t => t.Amount.Amount);
            var successfulTransactions = transactionsList.Count(t => t.Status == TransactionStatus.CONFIRMED);
            var failedTransactions = transactionsList.Count(t => t.Status == TransactionStatus.FAILED || t.Status == TransactionStatus.REJECTED);
            var pendingTransactions = transactionsList.Count(t => t.Status == TransactionStatus.PENDING || t.Status == TransactionStatus.PROCESSING || t.Status == TransactionStatus.INITIATED);
            var averageAmount = totalTransactions > 0 ? totalVolume / totalTransactions : 0;

            // Agrupar por tipo
            var transactionsByType = transactionsList
                .GroupBy(t => t.Type)
                .Select(g => new
                {
                    type = g.Key.ToString().ToLower(),
                    count = g.Count(),
                    volume = g.Sum(t => t.Amount.Amount)
                })
                .ToArray();

            // Agrupar por status
            var transactionsByStatus = transactionsList
                .GroupBy(t => t.Status)
                .Select(g => new
                {
                    status = g.Key.ToString().ToLower(),
                    count = g.Count(),
                    percentage = totalTransactions > 0 ? Math.Round((double)g.Count() / totalTransactions * 100, 1) : 0
                })
                .ToArray();

            var report = new
            {
                totalTransactions,
                totalVolume,
                successfulTransactions,
                failedTransactions,
                pendingTransactions,
                averageAmount = Math.Round(averageAmount, 2),
                transactionsByType,
                transactionsByStatus,
                reportPeriod = new
                {
                    startDate = startDate.ToString("yyyy-MM-dd"),
                    endDate = endDate.ToString("yyyy-MM-dd")
                }
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de transações para admin");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma transação específica (admin)
    /// </summary>
    [HttpGet("{transactionId}")]
    public async Task<IActionResult> GetTransactionDetails([FromRoute] string transactionId)
    {
        try
        {
            _logger.LogInformation("Admin obtendo detalhes da transação {TransactionId}", transactionId);

            if (!Guid.TryParse(transactionId, out var transactionGuid))
            {
                return BadRequest(new { message = "ID da transação inválido" });
            }

            var transaction = await _transactionRepository.GetByIdAsync(transactionGuid);

            if (transaction == null)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            var response = new
            {
                id = transaction.TransactionId.ToString(),
                externalId = transaction.ExternalId,
                type = transaction.Type.ToString().ToLower(),
                amount = transaction.Amount.Amount,
                currency = transaction.Amount.Currency,
                description = transaction.Description,
                status = transaction.Status.ToString().ToLower(),
                bankCode = transaction.BankCode,
                createdAt = transaction.CreatedAt,
                updatedAt = transaction.UpdatedAt,
                webhookUrl = transaction.WebhookUrl,
                // Campos específicos por tipo
                pixKey = transaction.PixKey,
                endToEndId = transaction.EndToEndId,
                accountBranch = transaction.AccountBranch,
                accountNumber = transaction.AccountNumber,
                taxId = transaction.TaxId,
                name = transaction.Name,
                dueDate = transaction.DueDate,
                payerTaxId = transaction.PayerTaxId,
                payerName = transaction.PayerName,
                instructions = transaction.Instructions,
                boletoBarcode = transaction.BoletoBarcode,
                boletoUrl = transaction.BoletoUrl,
                cryptoType = transaction.CryptoType,
                walletAddress = transaction.WalletAddress,
                cryptoTxHash = transaction.CryptoTxHash
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter detalhes da transação {TransactionId}", transactionId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Reprocessa uma transação falhada (admin)
    /// </summary>
    [HttpPost("{transactionId}/reprocess")]
    public async Task<IActionResult> ReprocessTransaction([FromRoute] string transactionId)
    {
        try
        {
            _logger.LogInformation("Admin reprocessando transação {TransactionId}", transactionId);

            if (!Guid.TryParse(transactionId, out var transactionGuid))
            {
                return BadRequest(new { message = "ID da transação inválido" });
            }

            var transaction = await _transactionRepository.GetByIdAsync(transactionGuid);

            if (transaction == null)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            // Verificar se a transação pode ser reprocessada
            if (transaction.Status != TransactionStatus.FAILED && transaction.Status != TransactionStatus.REJECTED)
            {
                return BadRequest(new { message = "Apenas transações falhadas ou rejeitadas podem ser reprocessadas" });
            }

            // Atualizar status para reprocessamento
            transaction.UpdateStatus(TransactionStatus.PROCESSING, "Reprocessamento iniciado pelo admin");
            await _transactionRepository.UpdateAsync(transaction);

            return Ok(new
            {
                transactionId,
                status = "processing",
                message = "Transação enviada para reprocessamento",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reprocessar transação {TransactionId}", transactionId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cancela uma transação pendente (admin)
    /// </summary>
    [HttpPost("{transactionId}/cancel")]
    public async Task<IActionResult> CancelTransaction([FromRoute] string transactionId, [FromBody] CancelTransactionRequest request)
    {
        try
        {
            _logger.LogInformation("Admin cancelando transação {TransactionId}", transactionId);

            if (!Guid.TryParse(transactionId, out var transactionGuid))
            {
                return BadRequest(new { message = "ID da transação inválido" });
            }

            var transaction = await _transactionRepository.GetByIdAsync(transactionGuid);

            if (transaction == null)
            {
                return NotFound(new { message = "Transação não encontrada" });
            }

            // Verificar se a transação pode ser cancelada
            if (transaction.Status == TransactionStatus.CONFIRMED ||
                transaction.Status == TransactionStatus.CANCELLED ||
                transaction.Status == TransactionStatus.FAILED)
            {
                return BadRequest(new { message = "Transação não pode ser cancelada no status atual" });
            }

            // Cancelar a transação
            var cancelReason = !string.IsNullOrEmpty(request.Reason) ? request.Reason : "Cancelado pelo admin";
            transaction.UpdateStatus(TransactionStatus.CANCELLED, cancelReason);
            await _transactionRepository.UpdateAsync(transaction);

            return Ok(new
            {
                transactionId,
                status = "cancelled",
                reason = cancelReason,
                message = "Transação cancelada com sucesso",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao cancelar transação {TransactionId}", transactionId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}

public class CancelTransactionRequest
{
    public string Reason { get; set; } = string.Empty;
}
