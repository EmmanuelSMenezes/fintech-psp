using System.Security.Claims;
using FintechPSP.BalanceService.DTOs;
using FintechPSP.BalanceService.Queries;
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

    public BalanceController(IMediator mediator, ILogger<BalanceController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
