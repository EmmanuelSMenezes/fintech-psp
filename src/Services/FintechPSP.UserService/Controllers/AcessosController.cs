using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.DTOs;
using System.Security.Claims;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de acessos RBAC
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AcessosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AcessosController> _logger;

    public AcessosController(IMediator mediator, ILogger<AcessosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Criar acesso para sub-usuário (scope banking)
    /// </summary>
    [HttpPost("banking")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult<AccessResponse>> CreateSubUserAccess([FromBody] CreateSubUserAccessRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var command = new CreateSubUserAccessCommand
            {
                ParentUserId = currentUserId,
                SubUserEmail = request.SubUserEmail,
                Role = request.Role,
                Permissions = request.Permissions,
                CreatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Sub-usuário criado com sucesso: {Email} por {UserId}", 
                request.SubUserEmail, currentUserId);

            return CreatedAtAction(nameof(GetAccessById), new { id = result.AcessoId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro ao criar sub-usuário: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos para criar sub-usuário: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao criar sub-usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter acessos do usuário atual (scope banking)
    /// </summary>
    [HttpGet("banking")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult<List<AccessResponse>>> GetMySubUsers()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var query = new GetSubUsersQuery { ParentUserId = currentUserId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar sub-usuários");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualizar acesso de sub-usuário (scope banking)
    /// </summary>
    [HttpPut("banking/{id}")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult<AccessResponse>> UpdateSubUserAccess(Guid id, [FromBody] UpdateAccessRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Verificar se o acesso pertence ao usuário atual
            var existingAccess = await _mediator.Send(new GetAccessByIdQuery { AcessoId = id });
            if (existingAccess == null)
            {
                return NotFound(new { message = "Acesso não encontrado" });
            }

            if (existingAccess.ParentUserId != currentUserId)
            {
                return Forbid("Você só pode atualizar seus próprios sub-usuários");
            }

            var command = new UpdateAccessCommand
            {
                AcessoId = id,
                Role = request.Role,
                Permissions = request.Permissions,
                UpdatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Sub-usuário atualizado com sucesso: {AcessoId} por {UserId}", id, currentUserId);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos para atualizar sub-usuário: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao atualizar sub-usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remover acesso de sub-usuário (scope banking)
    /// </summary>
    [HttpDelete("banking/{id}")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult> RemoveSubUserAccess(Guid id, [FromQuery] string motivo = "Removido pelo usuário")
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Verificar se o acesso pertence ao usuário atual
            var existingAccess = await _mediator.Send(new GetAccessByIdQuery { AcessoId = id });
            if (existingAccess == null)
            {
                return NotFound(new { message = "Acesso não encontrado" });
            }

            if (existingAccess.ParentUserId != currentUserId)
            {
                return Forbid("Você só pode remover seus próprios sub-usuários");
            }

            var command = new RemoveAccessCommand
            {
                AcessoId = id,
                Motivo = motivo,
                RemovedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            
            if (result)
            {
                _logger.LogInformation("Sub-usuário removido com sucesso: {AcessoId} por {UserId}", id, currentUserId);
                return NoContent();
            }

            return NotFound(new { message = "Acesso não encontrado" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao remover sub-usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Criar acesso para usuário (scope admin)
    /// </summary>
    [HttpPost("admin")]
    [Authorize(Policy = "AdminScope")]
    public async Task<ActionResult<AccessResponse>> CreateUserAccess([FromBody] CreateUserAccessRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            var command = new CreateUserAccessCommand
            {
                UserId = request.UserId,
                Role = request.Role,
                Permissions = request.Permissions,
                CreatedBy = currentUserId
            };

            var result = await _mediator.Send(command);
            
            _logger.LogInformation("Acesso de usuário criado com sucesso: {UserId} por {AdminId}", 
                request.UserId, currentUserId);

            return CreatedAtAction(nameof(GetAccessById), new { id = result.AcessoId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro ao criar acesso de usuário: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Dados inválidos para criar acesso de usuário: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro interno ao criar acesso de usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter acessos de um usuário específico (scope admin)
    /// </summary>
    [HttpGet("admin/{userId}")]
    [Authorize(Policy = "AdminScope")]
    public async Task<ActionResult<List<AccessResponse>>> GetUserAccesses(Guid userId)
    {
        try
        {
            var query = new GetUserAccessesQuery { UserId = userId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar acessos do usuário: {UserId}", userId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Listar todos os acessos com filtros (scope admin)
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Policy = "AdminScope")]
    public async Task<ActionResult<AccessListResponse>> GetAllAccesses([FromQuery] GetAccessesRequest request)
    {
        try
        {
            var query = new GetAccessesQuery
            {
                UserId = request.UserId,
                Role = request.Role,
                IsActive = request.IsActive,
                Page = request.Page,
                Limit = request.Limit
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar acessos");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter acesso por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AccessResponse>> GetAccessById(Guid id)
    {
        try
        {
            var query = new GetAccessByIdQuery { AcessoId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Acesso não encontrado" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar acesso: {AcessoId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Validar permissão de usuário
    /// </summary>
    [HttpPost("validate-permission")]
    public async Task<ActionResult<ValidatePermissionResponse>> ValidatePermission([FromBody] ValidatePermissionRequest request)
    {
        try
        {
            var query = new ValidatePermissionQuery
            {
                UserId = request.UserId,
                Permission = request.Permission
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar permissão");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obter permissões disponíveis por role
    /// </summary>
    [HttpGet("permissions")]
    public async Task<ActionResult<PermissionsResponse>> GetAvailablePermissions()
    {
        try
        {
            var query = new GetAvailablePermissionsQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar permissões disponíveis");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("UserId não encontrado no token");
        }
        return userId;
    }
}
