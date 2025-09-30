using System;
using System.Threading.Tasks;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public TransactionController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
            await Task.Delay(50); // Simular consulta DB

            var transactions = new object[]
            {
                new {
                    id = Guid.NewGuid(),
                    externalId = "TXN-001",
                    type = "pix",
                    amount = 100.50m,
                    status = "completed",
                    description = "Pagamento PIX",
                    createdAt = DateTime.UtcNow.AddHours(-1),
                    pixKey = "user@example.com"
                },
                new {
                    id = Guid.NewGuid(),
                    externalId = "TXN-002",
                    type = "ted",
                    amount = 250.00m,
                    status = "processing",
                    description = "Transferência TED",
                    createdAt = DateTime.UtcNow.AddHours(-2),
                    bankCode = "001"
                }
            };

            return Ok(new { transactions, total = transactions.Length, page, limit });
        }
        catch (Exception ex)
        {
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

            // Simular consulta de status
            await Task.Delay(50);

            // Mock de status baseado no ID
            var status = (id.GetHashCode() % 4) switch
            {
                0 => "completed",
                1 => "pending",
                2 => "processing",
                _ => "failed"
            };

            var response = new
            {
                transactionId = id,
                status = status,
                statusDescription = status switch
                {
                    "completed" => "Transação concluída com sucesso",
                    "pending" => "Transação pendente de processamento",
                    "processing" => "Transação em processamento",
                    "failed" => "Transação falhou",
                    _ => "Status desconhecido"
                },
                timestamp = DateTime.UtcNow,
                details = new
                {
                    lastUpdate = DateTime.UtcNow.AddMinutes(-5),
                    attempts = 1,
                    nextRetry = status == "failed" ? DateTime.UtcNow.AddMinutes(10) : (DateTime?)null
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
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
