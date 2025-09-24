using System;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Infrastructure.Messaging;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Controllers;

[ApiController]
[Route("admin/contas")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly ILogger<AccountsController> _logger;
    private readonly IAccountRepository _repo;
    private readonly IEventPublisher _publisher;

    public AccountsController(ILogger<AccountsController> logger, IAccountRepository repo, IEventPublisher publisher)
    {
        _logger = logger;
        _repo = repo;
        _publisher = publisher;
    }

    // GET /admin/contas?clienteId=&page=&limit=
    [HttpGet]
    public async Task<IActionResult> GetAccounts([FromQuery] Guid? clienteId, [FromQuery] int page = 1, [FromQuery] int limit = 50)
    {
        var (contas, total) = await _repo.GetPagedAsync(page, limit, clienteId);
        var mapped = contas.Select(MapToResponse).ToList();
        return Ok(new { contas = mapped, total, page, limit });
    }

    // GET /admin/contas/{clienteId}
    [HttpGet("{clienteId}")]
    public async Task<IActionResult> GetAccountsByClient([FromRoute] Guid clienteId)
    {
        var contas = await _repo.GetByClientAsync(clienteId);
        var mapped = contas.Select(MapToResponse).ToList();
        return Ok(new { clienteId, contas = mapped });
    }

    // POST /admin/contas
    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateBankAccountRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var created = await _repo.CreateAsync(request);

        // Publish event
        await _publisher.PublishAsync(new ContaBancariaCriada(
            created.ContaId,
            created.ClienteId,
            created.BankCode,
            created.AccountNumber,
            created.Description ?? string.Empty,
            created.CredentialsTokenId
        ));

        return CreatedAtAction(nameof(GetAccountsByClient), new { clienteId = created.ClienteId }, MapToResponse(created));
    }

    // PUT /admin/contas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAccount([FromRoute] Guid id, [FromBody] UpdateBankAccountRequest request)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return NotFound(new { error = "not_found", message = "Conta não encontrada" });

        var updated = await _repo.UpdateAsync(id, request);

        await _publisher.PublishAsync(new ContaBancariaAtualizada(
            updated.ContaId,
            updated.ClienteId,
            updated.Description,
            updated.CredentialsTokenId
        ));

        return Ok(MapToResponse(updated));
    }

    // DELETE /admin/contas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing is null) return NotFound();

        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound();

        await _publisher.PublishAsync(new ContaBancariaRemovida(existing.ContaId, existing.ClienteId, existing.BankCode));
        return NoContent();
    }

    private static object MapToResponse(FintechPSP.UserService.Models.BankAccount a)
    {
        // Compatibilidade com o front: campos banco/agencia/conta/tipoConta
        return new
        {
            id = a.ContaId, // usar mesmo valor
            contaId = a.ContaId,
            clienteId = a.ClienteId,
            banco = a.BankCode, // nome pode ser resolvido no front a partir do code
            agencia = string.Empty,
            conta = a.AccountNumber,
            tipoConta = "corrente",
            bankCode = a.BankCode,
            accountNumber = a.AccountNumber,
            description = a.Description,
            isActive = a.IsActive,
            createdAt = a.CreatedAt,
            updatedAt = a.UpdatedAt,
            lastUpdated = a.UpdatedAt,
            credentialsTokenId = a.CredentialsTokenId
        };
    }
}

