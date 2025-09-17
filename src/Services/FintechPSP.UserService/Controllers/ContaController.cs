using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de contas bancárias - Internet Banking
/// </summary>
[ApiController]
[Route("banking/contas")]
[Authorize(Policy = "BankingScope")]
[Produces("application/json")]
public class ContaBankingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IContaBancariaRepository _contaRepository;
    private readonly ILogger<ContaBankingController> _logger;

    public ContaBankingController(
        IMediator mediator,
        IContaBancariaRepository contaRepository,
        ILogger<ContaBankingController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria uma nova conta bancária para o cliente logado
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var command = new CriarContaCommand(
                clienteId,
                request.BankCode,
                request.AccountNumber,
                request.Credentials,
                request.Description);

            var response = await _mediator.Send(command);
            
            if (response.Status == "CREATED")
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta bancária");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma conta bancária do cliente logado
    /// </summary>
    [HttpPut("{contaId}")]
    public async Task<IActionResult> AtualizarConta([FromRoute] Guid contaId, [FromBody] AtualizarContaRequest request)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            // Verificar se a conta pertence ao cliente
            var isOwner = await _contaRepository.IsContaOwnedByClienteAsync(contaId, clienteId);
            if (!isOwner)
            {
                return Forbid("Conta não pertence ao cliente");
            }

            var command = new AtualizarContaCommand(
                contaId,
                clienteId,
                request.Credentials,
                request.Description);

            var response = await _mediator.Send(command);
            
            if (response.Status == "UPDATED")
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta bancária {ContaId}", contaId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove uma conta bancária do cliente logado
    /// </summary>
    [HttpDelete("{contaId}")]
    public async Task<IActionResult> RemoverConta([FromRoute] Guid contaId)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            // Verificar se a conta pertence ao cliente
            var isOwner = await _contaRepository.IsContaOwnedByClienteAsync(contaId, clienteId);
            if (!isOwner)
            {
                return Forbid("Conta não pertence ao cliente");
            }

            var command = new RemoverContaCommand(contaId, clienteId);
            var response = await _mediator.Send(command);
            
            if (response.Status == "DELETED")
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover conta bancária {ContaId}", contaId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as contas do cliente logado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarContas()
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var contas = await _contaRepository.GetByClienteIdAsync(clienteId);
            
            var response = new ListarContasResponse
            {
                ClienteId = clienteId,
                Contas = contas.Select(c => new ContaResumo
                {
                    ContaId = c.Id,
                    BankCode = c.BankCode,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    LastUpdated = c.UpdatedAt
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contas do cliente");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    private Guid GetCurrentClientId()
    {
        var clientIdClaim = User.FindFirst("clienteId")?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(clientIdClaim, out var clienteId) ? clienteId : Guid.Empty;
    }
}
