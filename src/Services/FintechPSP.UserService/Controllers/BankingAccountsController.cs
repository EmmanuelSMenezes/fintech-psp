using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Infrastructure.Messaging;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de contas bancárias - Internet Banking
/// </summary>
[ApiController]
[Route("banking/contas")]
[Authorize(Policy = "BankingScope")]
[Produces("application/json")]
public class BankingAccountsController : ControllerBase
{
    private readonly IAccountRepository _repo;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<BankingAccountsController> _logger;

    public BankingAccountsController(
        IAccountRepository repo,
        IEventPublisher publisher,
        ILogger<BankingAccountsController> logger)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista as contas bancárias do usuário atual
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMyAccounts()
    {
        try
        {
            // Log de debug para autorização
            var claims = User.Claims.Select(c => $"{c.Type}={c.Value}").ToList();
            _logger.LogInformation("Claims do usuário no BankingAccountsController: {Claims}", string.Join(", ", claims));
            _logger.LogInformation("User.Identity.IsAuthenticated: {IsAuthenticated}", User.Identity?.IsAuthenticated);

            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                _logger.LogWarning("Cliente não identificado - GetCurrentClientId() retornou Guid.Empty");
                return Unauthorized("Cliente não identificado");
            }

            var contas = await _repo.GetByClientAsync(clienteId);
            var mapped = contas.Select(MapToResponse).ToList();

            _logger.LogInformation("Listando {Count} contas para cliente {ClienteId}", mapped.Count, clienteId);
            return Ok(mapped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar contas do cliente");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria uma nova conta bancária para o usuário atual
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateMyAccount([FromBody] CreateMyBankAccountRequest request)
    {
        try
        {
            if (!ModelState.IsValid) 
                return ValidationProblem(ModelState);

            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            // Criar request completo com clienteId
            var fullRequest = new CreateBankAccountRequest
            {
                ClienteId = clienteId.ToString(),
                BankCode = request.BankCode,
                AccountNumber = request.AccountNumber,
                Description = request.Description,
                Credentials = request.Credentials
            };

            var created = await _repo.CreateAsync(fullRequest);

            // Publish event
            await _publisher.PublishAsync(new ContaBancariaCriada(
                created.ContaId,
                created.ClienteId,
                created.BankCode,
                created.AccountNumber,
                created.Description ?? string.Empty,
                created.CredentialsTokenId
            ));

            _logger.LogInformation("Conta bancária criada: {ContaId} para cliente {ClienteId}", 
                created.ContaId, clienteId);

            return CreatedAtAction(nameof(GetMyAccounts), MapToResponse(created));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta bancária");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza uma conta bancária do usuário atual
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMyAccount([FromRoute] Guid id, [FromBody] UpdateMyBankAccountRequest request)
    {
        try
        {
            if (!ModelState.IsValid) 
                return ValidationProblem(ModelState);

            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var existing = await _repo.GetByIdAsync(id);
            if (existing is null) 
                return NotFound(new { error = "not_found", message = "Conta não encontrada" });

            // Verificar se a conta pertence ao usuário atual
            if (existing.ClienteId != clienteId)
            {
                return Forbid("Você só pode atualizar suas próprias contas");
            }

            // Criar request de atualização
            var updateRequest = new UpdateBankAccountRequest
            {
                Description = request.Description,
                Credentials = request.Credentials
            };

            var updated = await _repo.UpdateAsync(id, updateRequest);

            await _publisher.PublishAsync(new ContaBancariaAtualizada(
                updated.ContaId,
                updated.ClienteId,
                updated.Description,
                updated.CredentialsTokenId
            ));

            _logger.LogInformation("Conta bancária atualizada: {ContaId} para cliente {ClienteId}", 
                id, clienteId);

            return Ok(MapToResponse(updated));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta bancária");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove uma conta bancária do usuário atual
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMyAccount([FromRoute] Guid id)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var existing = await _repo.GetByIdAsync(id);
            if (existing is null) 
                return NotFound(new { error = "not_found", message = "Conta não encontrada" });

            // Verificar se a conta pertence ao usuário atual
            if (existing.ClienteId != clienteId)
            {
                return Forbid("Você só pode remover suas próprias contas");
            }

            var ok = await _repo.DeleteAsync(id);
            if (!ok) 
                return NotFound();

            await _publisher.PublishAsync(new ContaBancariaRemovida(
                existing.ContaId, 
                existing.ClienteId, 
                existing.BankCode));

            _logger.LogInformation("Conta bancária removida: {ContaId} para cliente {ClienteId}", 
                id, clienteId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover conta bancária");
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

    private static BankAccountResponse MapToResponse(BankAccount account)
    {
        return new BankAccountResponse
        {
            ContaId = account.ContaId,
            ClienteId = account.ClienteId,
            BankCode = account.BankCode,
            AccountNumber = account.AccountNumber,
            Description = account.Description,
            IsActive = account.IsActive,
            CreatedAt = account.CreatedAt,
            UpdatedAt = account.UpdatedAt,
            CredentialsTokenId = account.CredentialsTokenId
        };
    }
}
