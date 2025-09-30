using System;
using System.Threading.Tasks;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.TransactionService.Controllers;

/// <summary>
/// Controller para transações
/// </summary>
[ApiController]
[Route("transacoes")]
[Authorize]
[Produces("application/json")]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        IMediator mediator,
        ITransactionRepository transactionRepository,
        ILogger<TransactionController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista transações do cliente
    /// </summary>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    /// <param name="type">Tipo de transação (pix, ted, boleto)</param>
    /// <param name="status">Status da transação</param>
    /// <returns>Lista de transações</returns>
    [HttpGet]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        try
        {
            _logger.LogInformation("Listando transações - Página: {Page}, Limite: {Limit}, Tipo: {Type}, Status: {Status}",
                page, limit, type, status);

            // Obter ID do usuário do token JWT
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("ID do usuário não encontrado no token");
                return Unauthorized("Usuário não identificado");
            }

            var result = await _transactionRepository.GetPagedAsync(userId, page, limit, type, status);

            _logger.LogInformation("Encontradas {TotalCount} transações para usuário {UserId}",
                result.TotalCount, userId);

            return Ok(new {
                transactions = result.Data,
                total = result.TotalCount,
                page = result.CurrentPage,
                limit = result.PageSize,
                totalPages = result.TotalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transações");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação PIX
    /// </summary>
    /// <param name="request">Dados da transação PIX</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação PIX iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("pix")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoPix([FromBody] PixTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoPixCommand(
                request.ExternalId,
                request.Amount,
                request.PixKey,
                request.BankCode,
                request.Description,
                request.WebhookUrl,
                request.EndToEndId,
                request.ContaId);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação TED
    /// </summary>
    /// <param name="request">Dados da transação TED</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação TED iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("ted")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoTed([FromBody] TedTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoTedCommand(
                request.ExternalId,
                request.Amount,
                request.BankCode,
                request.AccountBranch,
                request.AccountNumber,
                request.TaxId,
                request.Name,
                request.Description,
                request.WebhookUrl);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Emite um boleto
    /// </summary>
    /// <param name="request">Dados do boleto</param>
    /// <returns>Dados do boleto criado</returns>
    /// <response code="200">Boleto emitido com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("boleto")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> EmitirBoleto([FromBody] BoletoTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new EmitirBoletoCommand(
                request.ExternalId,
                request.Amount,
                request.DueDate,
                request.PayerTaxId,
                request.PayerName,
                request.Instructions,
                request.WebhookUrl,
                request.BankCode);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação de criptomoeda
    /// </summary>
    /// <param name="request">Dados da transação cripto</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação cripto iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("cripto")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoCripto([FromBody] CryptoTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoCriptoCommand(
                request.ExternalId,
                request.Amount,
                request.CryptoType,
                request.WalletAddress,
                request.FiatCurrency,
                request.Description,
                request.WebhookUrl);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Consulta status de uma transação
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <returns>Status da transação</returns>
    /// <response code="200">Status da transação obtido com sucesso</response>
    /// <response code="404">Transação não encontrada</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> GetTransactionStatus([FromRoute] string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { error = "invalid_request", message = "ID da transação é obrigatório" });
            }

            _logger.LogInformation("Consultando status da transação {TransactionId}", id);

            // Tentar converter o ID para Guid
            if (!Guid.TryParse(id, out var transactionId))
            {
                _logger.LogWarning("ID da transação inválido: {TransactionId}", id);
                return BadRequest(new { error = "invalid_id", message = "ID da transação deve ser um GUID válido" });
            }

            var transaction = await _transactionRepository.GetByIdAsync(transactionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transação {TransactionId} não encontrada", transactionId);
                return NotFound(new { error = "not_found", message = "Transação não encontrada" });
            }

            var response = new
            {
                transactionId = transaction.Id,
                externalId = transaction.ExternalId,
                status = transaction.Status.ToString().ToLower(),
                statusDescription = transaction.Status switch
                {
                    Models.TransactionStatus.Completed => "Transação concluída com sucesso",
                    Models.TransactionStatus.Pending => "Transação pendente de processamento",
                    Models.TransactionStatus.Processing => "Transação em processamento",
                    Models.TransactionStatus.Failed => "Transação falhou",
                    Models.TransactionStatus.Cancelled => "Transação cancelada",
                    _ => "Status desconhecido"
                },
                amount = transaction.Amount,
                type = transaction.Type.ToString().ToLower(),
                description = transaction.Description,
                createdAt = transaction.CreatedAt,
                updatedAt = transaction.UpdatedAt,
                timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da transação {TransactionId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Health check do serviço
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "TransactionService", timestamp = DateTime.UtcNow });
    }
}
