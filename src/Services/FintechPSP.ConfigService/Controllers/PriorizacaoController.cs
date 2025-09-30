using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FintechPSP.ConfigService.Commands;
using FintechPSP.ConfigService.DTOs;
using FintechPSP.ConfigService.Models;
using FintechPSP.ConfigService.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.ConfigService.Controllers;

/// <summary>
/// Controller para configuração de roteamento - Internet Banking
/// </summary>
[ApiController]
[Route("banking/configs/roteamento")]
[Authorize(Policy = "BankingScope")]
[Produces("application/json")]
public class RoteamentoBankingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPriorizacaoRepository _priorizacaoRepository;
    private readonly ILogger<RoteamentoBankingController> _logger;

    public RoteamentoBankingController(
        IMediator mediator,
        IPriorizacaoRepository priorizacaoRepository,
        ILogger<RoteamentoBankingController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _priorizacaoRepository = priorizacaoRepository ?? throw new ArgumentNullException(nameof(priorizacaoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Configura a priorização de contas para o cliente logado
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ConfigurarPriorizacao([FromBody] ConfigurarPriorizacaoRequest request)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var command = new ConfigurarPriorizacaoCommand(clienteId, request.Prioridades);
            var response = await _mediator.Send(command);

            if (response.Status == "CONFIGURED" || response.Status == "CONFIGURED_WITH_WARNING")
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar priorização");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém a configuração de priorização atual do cliente logado
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ObterPriorizacao()
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var configuracao = await _priorizacaoRepository.GetByClienteIdAsync(clienteId);
            if (configuracao == null)
            {
                return NotFound(new { error = "not_found", message = "Configuração de priorização não encontrada" });
            }

            var response = new PriorizacaoDetalhesResponse
            {
                ConfigId = configuracao.Id,
                ClienteId = configuracao.ClienteId,
                Prioridades = configuracao.Prioridades.Select(p => new PrioridadeContaResponse
                {
                    ContaId = p.ContaId,
                    BankCode = p.BankCode,
                    Percentual = p.Percentual
                }).ToList(),
                TotalPercentual = configuracao.TotalPercentual,
                IsValid = configuracao.IsValid,
                CreatedAt = configuracao.CreatedAt,
                LastUpdated = configuracao.UpdatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração de priorização");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove a configuração de priorização do cliente logado
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> RemoverPriorizacao()
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var removida = await _priorizacaoRepository.DeleteByClienteIdAsync(clienteId);
            if (!removida)
            {
                return NotFound(new { error = "not_found", message = "Configuração de priorização não encontrada" });
            }

            return Ok(new { status = "DELETED", message = "Configuração de priorização removida com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover configuração de priorização");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
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
}

/// <summary>
/// Controller para gerenciamento de bancos - Internet Banking
/// </summary>
[ApiController]
[Route("banking/bancos")]
[Authorize(Policy = "BankingScope")]
[Produces("application/json")]
public class BancosBankingController : ControllerBase
{
    private readonly IBancoPersonalizadoRepository _bancoRepository;
    private readonly ILogger<BancosBankingController> _logger;

    public BancosBankingController(
        IBancoPersonalizadoRepository bancoRepository,
        ILogger<BancosBankingController> logger)
    {
        _bancoRepository = bancoRepository ?? throw new ArgumentNullException(nameof(bancoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista todos os bancos disponíveis (padrão + personalizados do cliente)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ListarBancos()
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var bancosDefault = BancoDefault.GetBancosDefault()
                .Select(b => new BancoDefaultResponse
                {
                    BankCode = b.BankCode,
                    Name = b.Name,
                    IsSupported = b.IsSupported
                }).ToList();

            var bancosPersonalizados = await _bancoRepository.GetByClienteIdAsync(clienteId);
            var bancosPersonalizadosResponse = bancosPersonalizados.Select(b => new BancoDetalhesResponse
            {
                BankId = b.Id,
                ClienteId = b.ClienteId,
                BankCode = b.BankCode,
                Endpoint = b.Endpoint,
                CredentialsTemplate = b.CredentialsTemplate,
                CreatedAt = b.CreatedAt,
                LastUpdated = b.UpdatedAt
            }).ToList();

            var response = new ListarBancosResponse
            {
                BancosDefault = bancosDefault,
                BancosPersonalizados = bancosPersonalizadosResponse
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar bancos");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
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
}
