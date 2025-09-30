using System.Security.Claims;
using FintechPSP.WebhookService.Commands;
using FintechPSP.WebhookService.DTOs;
using FintechPSP.WebhookService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.WebhookService.Controllers;

/// <summary>
/// Controller para operações de webhook
/// </summary>
[ApiController]
[Route("webhooks")]
[Authorize]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(IMediator mediator, ILogger<WebhookController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista webhooks do cliente
    /// </summary>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <param name="active">Filtrar por status ativo</param>
    /// <returns>Lista de webhooks</returns>
    [HttpGet]
    public async Task<ActionResult<WebhookListResponse>> GetWebhooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] bool? active = null)
    {
        try
        {
            var clientId = GetCurrentClientId();
            var isAdmin = IsAdmin();

            if (clientId == Guid.Empty && !isAdmin)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Listando webhooks para cliente {ClientId} (Admin: {IsAdmin})", clientId, isAdmin);

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = new ObterWebhooksQuery(clientId, page, pageSize, active);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar webhooks");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém webhook específico
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <returns>Informações do webhook</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<WebhookResponse>> GetWebhook([FromRoute] Guid id)
    {
        try
        {
            var clientId = GetCurrentClientId();
            var isAdmin = IsAdmin();

            if (clientId == Guid.Empty && !isAdmin)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Obtendo webhook {WebhookId} para cliente {ClientId} (Admin: {IsAdmin})", id, clientId, isAdmin);

            var query = new ObterWebhookQuery(id, clientId);
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new { message = "Webhook não encontrado" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter webhook {WebhookId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria novo webhook
    /// </summary>
    /// <param name="request">Dados do webhook</param>
    /// <returns>Webhook criado</returns>
    [HttpPost]
    public async Task<ActionResult<WebhookResponse>> CreateWebhook([FromBody] WebhookRequest request)
    {
        try
        {
            var clientId = GetCurrentClientId();
            var isAdmin = IsAdmin();

            if (clientId == Guid.Empty && !isAdmin)
            {
                return Unauthorized("Cliente não identificado");
            }

            // Se for admin e não tiver clientId, usar um clientId padrão ou do primeiro cliente
            if (isAdmin && clientId == Guid.Empty)
            {
                // Para admin, usar o sub (ID do usuário admin) como clientId temporário
                var subClaim = User.FindFirst("sub")?.Value;
                if (Guid.TryParse(subClaim, out var adminId))
                {
                    clientId = adminId;
                }
            }

            _logger.LogInformation("Criando webhook para cliente {ClientId} (Admin: {IsAdmin})", clientId, isAdmin);

            // Validar request
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { message = "URL é obrigatória" });
            }

            if (request.Events == null || !request.Events.Any())
            {
                return BadRequest(new { message = "Pelo menos um evento deve ser especificado" });
            }

            var command = new CriarWebhookCommand(
                clientId,
                request.Url,
                request.Events,
                request.Secret,
                request.Description);

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetWebhook), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para criação de webhook");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar webhook");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza webhook
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <param name="request">Dados para atualização</param>
    /// <returns>Webhook atualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<WebhookResponse>> UpdateWebhook(
        [FromRoute] Guid id,
        [FromBody] UpdateWebhookRequest request)
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Atualizando webhook {WebhookId} para cliente {ClientId}", id, clientId);

            var command = new AtualizarWebhookCommand(id, clientId)
            {
                Url = request.Url,
                Events = request.Events,
                Secret = request.Secret,
                Active = request.Active,
                Description = request.Description
            };

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Webhook {WebhookId} não encontrado", id);
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para atualização de webhook");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar webhook {WebhookId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Deleta webhook
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <returns>Resultado da operação</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWebhook([FromRoute] Guid id)
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Deletando webhook {WebhookId} para cliente {ClientId}", id, clientId);

            var command = new DeletarWebhookCommand(id, clientId);
            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = "Webhook não encontrado" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar webhook {WebhookId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém entregas de um webhook
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <param name="status">Filtrar por status</param>
    /// <returns>Lista de entregas</returns>
    [HttpGet("{id}/deliveries")]
    public async Task<ActionResult<WebhookDeliveryListResponse>> GetWebhookDeliveries(
        [FromRoute] Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? status = null)
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Obtendo entregas do webhook {WebhookId} para cliente {ClientId}", id, clientId);

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            var query = new ObterEntregasWebhookQuery(id, clientId, page, pageSize, status);
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter entregas do webhook {WebhookId}", id);
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
        return Ok(new { status = "healthy", service = "WebhookService", timestamp = DateTime.UtcNow });
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
