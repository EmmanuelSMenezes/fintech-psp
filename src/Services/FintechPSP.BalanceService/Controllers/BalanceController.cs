using System.Security.Claims;
using FintechPSP.BalanceService.DTOs;
using FintechPSP.BalanceService.Queries;
using FintechPSP.BalanceService.Repositories;
using FintechPSP.BalanceService.Models;
using FintechPSP.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.BalanceService.Controllers;

/// <summary>
/// Controller para operações de saldo e extrato
/// </summary>
[ApiController]
[Route("saldo")]
[Authorize]
public class BalanceController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<BalanceController> _logger;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;

    public BalanceController(
        IMediator mediator,
        ILogger<BalanceController> logger,
        IAccountRepository accountRepository,
        ITransactionHistoryRepository transactionHistoryRepository)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _transactionHistoryRepository = transactionHistoryRepository ?? throw new ArgumentNullException(nameof(transactionHistoryRepository));
    }

    /// <summary>
    /// Consulta saldo de uma conta
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <param name="accountId">ID da conta (opcional)</param>
    /// <returns>Informações de saldo</returns>
    [HttpGet("{clientId}")]
    public async Task<ActionResult<BalanceResponse>> GetBalance(
        [FromRoute] Guid clientId,
        [FromQuery] string? accountId = null)
    {
        try
        {
            _logger.LogInformation("Consultando saldo para cliente {ClientId}, conta {AccountId}", 
                clientId, accountId ?? "principal");

            // Validar se o cliente tem permissão para consultar este saldo
            var currentClientId = GetCurrentClientId();
            if (currentClientId != clientId && !IsAdmin())
            {
                return Forbid("Acesso negado para consultar saldo de outro cliente");
            }

            var query = new ObterSaldoQuery(clientId, accountId);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Saldo consultado com sucesso para cliente {ClientId}", clientId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conta não encontrada para cliente {ClientId}", clientId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar saldo para cliente {ClientId}", clientId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Consulta extrato de uma conta
    /// </summary>
    /// <param name="clientId">ID do cliente</param>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="accountId">ID da conta (opcional)</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <returns>Extrato de movimentações</returns>
    [HttpGet("{clientId}/extrato")]
    public async Task<ActionResult<StatementResponse>> GetStatement(
        [FromRoute] Guid clientId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? accountId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Consultando extrato para cliente {ClientId} de {StartDate} até {EndDate}", 
                clientId, startDate, endDate);

            // Validar se o cliente tem permissão para consultar este extrato
            var currentClientId = GetCurrentClientId();
            if (currentClientId != clientId && !IsAdmin())
            {
                return Forbid("Acesso negado para consultar extrato de outro cliente");
            }

            // Validar parâmetros
            if (startDate > endDate)
            {
                return BadRequest(new { message = "Data inicial deve ser menor que data final" });
            }

            if ((endDate - startDate).TotalDays > 90)
            {
                return BadRequest(new { message = "Período máximo de consulta é 90 dias" });
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = new ObterExtratoQuery(clientId, startDate, endDate, accountId, page, pageSize);
            var result = await _mediator.Send(query);

            _logger.LogInformation("Extrato consultado com sucesso para cliente {ClientId}", clientId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conta não encontrada para cliente {ClientId}", clientId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar extrato para cliente {ClientId}", clientId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Relatório de saldos (admin)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminScope")]
    public async Task<IActionResult> GetBalanceReport()
    {
        try
        {
            _logger.LogInformation("Admin consultando relatório de saldos");

            // Simular dados de relatório
            var report = new
            {
                totalAccounts = 5,
                totalBalance = 15000.00m,
                activeAccounts = 4,
                inactiveAccounts = 1,
                averageBalance = 3000.00m,
                lastUpdated = DateTime.UtcNow,
                balancesByStatus = new[]
                {
                    new { status = "ACTIVE", count = 4, totalBalance = 14500.00m },
                    new { status = "INACTIVE", count = 1, totalBalance = 500.00m }
                }
            };

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar relatório de saldos");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Health check do serviço
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "BalanceService", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Teste POST simples - NOVO ENDPOINT PARA DEBUG
    /// </summary>
    [HttpPost("health")]
    [AllowAnonymous]
    public IActionResult HealthPost()
    {
        return Ok(new { status = "POST funcionando!", service = "BalanceService", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Endpoint universal que aceita qualquer método HTTP
    /// </summary>
    [Route("universal")]
    [AllowAnonymous]
    public IActionResult Universal()
    {
        var method = Request.Method;
        return Ok(new {
            message = $"Método {method} funcionando!",
            service = "BalanceService",
            timestamp = DateTime.UtcNow,
            headers = Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
        });
    }

    /// <summary>
    /// Endpoint POST super simples - SEM NENHUM ATRIBUTO ESPECIAL
    /// </summary>
    public IActionResult SimplePost()
    {
        return Ok(new { message = "POST SIMPLES FUNCIONANDO!", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Teste simples de POST sem parâmetros
    /// </summary>
    [HttpPost("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new { message = "POST funcionando", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Teste simples de POST com parâmetros
    /// </summary>
    [HttpPost("test-data")]
    [AllowAnonymous]
    public IActionResult TestData([FromBody] dynamic data)
    {
        return Ok(new { message = "POST com dados funcionando", data = data, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Endpoint para receber notificações de PIX confirmado
    /// </summary>
    [HttpPost("pix-confirmado")]
    [AllowAnonymous]
    public async Task<IActionResult> PixConfirmado([FromBody] PixConfirmadoNotification notification)
    {
        try
        {
            // Extrair dados do objeto
            string txId = notification?.TxId ?? "UNKNOWN";
            decimal amount = notification?.Amount ?? 0;
            string payerName = notification?.PayerName ?? "N/A";
            string payerDocument = notification?.PayerDocument ?? "";
            DateTime confirmedAt = notification?.ConfirmedAt ?? DateTime.UtcNow;

            _logger.LogInformation("💰 Recebendo notificação PIX confirmado via HTTP - TxId: {TxId}, Valor: R$ {Amount}",
                txId, amount);

            // 1. Buscar a transação original pelo TxId
            var transactionHistory = await _transactionHistoryRepository.GetByExternalIdAsync(txId);

            // Se não encontrar, criar nova entrada para PIX recebido
            if (transactionHistory == null)
            {
                _logger.LogInformation("🔍 TxId {TxId} não encontrado, criando nova entrada para PIX recebido...", txId);

                // Usar ClientId padrão para teste
                var defaultClientId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");

                // TEMPORÁRIO: HARDCODAR AccountId para testar
                _logger.LogInformation("🔍 TEMPORÁRIO: Usando AccountId hardcoded para teste");
                string hardcodedAccountId = "CONTA_PRINCIPAL";
                _logger.LogInformation("✅ Usando AccountId hardcoded: {AccountId}", hardcodedAccountId);

                // Criar entrada de histórico para o PIX recebido
                var newTransactionId = Guid.NewGuid();
                transactionHistory = new TransactionHistory
                {
                    TransactionId = newTransactionId,
                    ClientId = defaultClientId,
                    AccountId = hardcodedAccountId, // ✅ TEMPORÁRIO: Usando AccountId hardcoded
                    ExternalId = txId,
                    Type = "PIX_RECEIVED",
                    Amount = amount,
                    Description = $"PIX recebido de {payerName}",
                    Status = "PENDING",
                    Operation = "CREDIT",
                    CreatedAt = confirmedAt
                };

                await _transactionHistoryRepository.AddTransactionAsync(transactionHistory);
                _logger.LogInformation("📝 Nova entrada de histórico criada para PIX recebido - TxId: {TxId}", txId);
            }

            // 2. Buscar a conta do cliente
            var account = await _accountRepository.GetByClientIdAsync(transactionHistory.ClientId);
            if (account == null)
            {
                _logger.LogError("❌ Conta não encontrada para cliente: {ClientId}", transactionHistory.ClientId);
                return BadRequest(new { error = "Conta não encontrada" });
            }

            // 3. Creditar o valor na conta
            var oldBalance = account.AvailableBalance.Amount;
            account.Credit(amount, $"PIX recebido - TxId: {txId}", transactionHistory.TransactionId.ToString());

            // 4. Salvar a conta atualizada
            await _accountRepository.UpdateAsync(account);

            // 5. Atualizar o histórico da transação
            await _transactionHistoryRepository.UpdateTransactionStatusAsync(
                transactionHistory.TransactionId,
                "CONFIRMED",
                $"PIX confirmado - Pagador: {payerName}");

            _logger.LogInformation("✅ PIX processado com sucesso via HTTP - Cliente: {ClientId}, Valor: R$ {Amount}, Saldo anterior: R$ {OldBalance}, Novo saldo: R$ {NewBalance}",
                account.ClientId, amount, oldBalance, account.AvailableBalance.Amount);

            return Ok(new {
                success = true,
                message = "PIX processado com sucesso",
                txId = txId,
                amount = amount,
                newBalance = account.AvailableBalance.Amount
            });
        }
        catch (Exception ex)
        {
            string txId = "UNKNOWN";
            try { txId = notification?.TxId ?? "UNKNOWN"; } catch { }

            _logger.LogError(ex, "💥 Erro ao processar PIX confirmado via HTTP - TxId: {TxId}", txId);
            return StatusCode(500, new { error = "Erro interno do servidor" });
        }
    }

    private Guid GetCurrentClientId()
    {
        // Usar o mesmo padrão dos outros controllers - ClaimTypes.NameIdentifier é o 'sub' do JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Guid.Empty;
        }
        return userId;
    }

    private bool IsAdmin()
    {
        return User.HasClaim("scope", "admin");
    }
}
