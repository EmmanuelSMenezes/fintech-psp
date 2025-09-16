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
                request.EndToEndId);

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
