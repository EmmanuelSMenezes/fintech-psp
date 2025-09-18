using System;
using System.Linq;
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
/// Controller para gerenciamento de contas bancárias - Administração
/// </summary>
[ApiController]
[Route("admin/contas")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class ContaAdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IContaBancariaRepository _contaRepository;
    private readonly ILogger<ContaAdminController> _logger;

    public ContaAdminController(
        IMediator mediator,
        IContaBancariaRepository contaRepository,
        ILogger<ContaAdminController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Cria uma nova conta bancária para um cliente específico (admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CriarConta([FromBody] CriarContaRequest request)
    {
        try
        {
            if (!request.ClienteId.HasValue)
            {
                return BadRequest(new { error = "invalid_request", message = "ClienteId é obrigatório para operações admin" });
            }

            var command = new CriarContaCommand(
                request.ClienteId.Value,
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
            _logger.LogError(ex, "Erro ao criar conta bancária (admin)");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma conta bancária específica (admin)
    /// </summary>
    [HttpPut("{contaId}")]
    public async Task<IActionResult> AtualizarConta([FromRoute] Guid contaId, [FromBody] AtualizarContaRequest request)
    {
        try
        {
            // Buscar a conta para obter o clienteId
            var conta = await _contaRepository.GetByIdAsync(contaId);
            if (conta == null)
            {
                return NotFound(new { error = "not_found", message = "Conta não encontrada" });
            }

            var command = new AtualizarContaCommand(
                contaId,
                conta.ClienteId,
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
            _logger.LogError(ex, "Erro ao atualizar conta bancária {ContaId} (admin)", contaId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove uma conta bancária específica (admin)
    /// </summary>
    [HttpDelete("{contaId}")]
    public async Task<IActionResult> RemoverConta([FromRoute] Guid contaId)
    {
        try
        {
            // Buscar a conta para obter o clienteId
            var conta = await _contaRepository.GetByIdAsync(contaId);
            if (conta == null)
            {
                return NotFound(new { error = "not_found", message = "Conta não encontrada" });
            }

            var command = new RemoverContaCommand(contaId, conta.ClienteId);
            var response = await _mediator.Send(command);
            
            if (response.Status == "DELETED")
            {
                return Ok(response);
            }
            
            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover conta bancária {ContaId} (admin)", contaId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as contas do sistema (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarTodasContas([FromQuery] string? clienteId = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(clienteId) && Guid.TryParse(clienteId, out var clienteGuid))
            {
                // Se clienteId foi fornecido, listar contas do cliente específico
                var contasCliente = await _contaRepository.GetByClienteIdAsync(clienteGuid);

                var responseCliente = new
                {
                    clienteId = clienteGuid,
                    contas = contasCliente.Select(c => new ContaDetalhesResponse
                    {
                        ContaId = c.Id,
                        ClienteId = c.ClienteId,
                        BankCode = c.BankCode,
                        AccountNumber = c.AccountNumber,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        CreatedAt = c.CreatedAt,
                        LastUpdated = c.UpdatedAt,
                        CredentialsTokenId = c.CredentialsTokenId
                    }).ToList()
                };

                return Ok(responseCliente);
            }
            else
            {
                // Listar todas as contas do sistema
                await Task.Delay(50); // Simular consulta DB

                var todasContas = new[]
                {
                    new ContaDetalhesResponse
                    {
                        ContaId = Guid.NewGuid(),
                        ClienteId = Guid.NewGuid(),
                        BankCode = "001",
                        AccountNumber = "12345-6",
                        Description = "Conta Principal - Banco do Brasil",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        LastUpdated = DateTime.UtcNow.AddDays(-5),
                        CredentialsTokenId = "token_001"
                    },
                    new ContaDetalhesResponse
                    {
                        ContaId = Guid.NewGuid(),
                        ClienteId = Guid.NewGuid(),
                        BankCode = "237",
                        AccountNumber = "98765-4",
                        Description = "Conta Secundária - Bradesco",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        LastUpdated = DateTime.UtcNow.AddDays(-2),
                        CredentialsTokenId = "token_002"
                    },
                    new ContaDetalhesResponse
                    {
                        ContaId = Guid.NewGuid(),
                        ClienteId = Guid.NewGuid(),
                        BankCode = "341",
                        AccountNumber = "55555-5",
                        Description = "Conta Itaú - Inativa",
                        IsActive = false,
                        CreatedAt = DateTime.UtcNow.AddDays(-60),
                        LastUpdated = DateTime.UtcNow.AddDays(-10),
                        CredentialsTokenId = "token_003"
                    }
                };

                return Ok(new { contas = todasContas, total = todasContas.Length });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contas");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista todas as contas de um cliente específico (admin)
    /// </summary>
    [HttpGet("{clienteId}")]
    public async Task<IActionResult> ListarContasCliente([FromRoute] Guid clienteId)
    {
        try
        {
            var contas = await _contaRepository.GetByClienteIdAsync(clienteId);
            
            var response = new
            {
                clienteId = clienteId,
                contas = contas.Select(c => new ContaDetalhesResponse
                {
                    ContaId = c.Id,
                    ClienteId = c.ClienteId,
                    BankCode = c.BankCode,
                    AccountNumber = c.AccountNumber,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    LastUpdated = c.UpdatedAt,
                    CredentialsTokenId = c.CredentialsTokenId // Apenas para admin
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contas do cliente {ClienteId} (admin)", clienteId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma conta específica (admin)
    /// </summary>
    [HttpGet("detalhes/{contaId}")]
    public async Task<IActionResult> ObterDetalhesConta([FromRoute] Guid contaId)
    {
        try
        {
            var conta = await _contaRepository.GetByIdAsync(contaId);
            if (conta == null)
            {
                return NotFound(new { error = "not_found", message = "Conta não encontrada" });
            }

            var response = new ContaDetalhesResponse
            {
                ContaId = conta.Id,
                ClienteId = conta.ClienteId,
                BankCode = conta.BankCode,
                AccountNumber = conta.AccountNumber,
                Description = conta.Description,
                IsActive = conta.IsActive,
                CreatedAt = conta.CreatedAt,
                LastUpdated = conta.UpdatedAt,
                CredentialsTokenId = conta.CredentialsTokenId // Apenas para admin
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter detalhes da conta {ContaId} (admin)", contaId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }
}
