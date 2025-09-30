using System.Security.Claims;
using FintechPSP.WebhookService.Commands;
using FintechPSP.WebhookService.DTOs;
using FintechPSP.WebhookService.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.WebhookService.Controllers;

/// <summary>
/// Controller para gerenciamento de webhooks - Administração
/// </summary>
[ApiController]
[Route("admin/webhooks")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class AdminWebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminWebhookController> _logger;

    public AdminWebhookController(IMediator mediator, ILogger<AdminWebhookController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista todos os webhooks (admin)
    /// </summary>
    /// <param name="clientId">ID do cliente (opcional)</param>
    /// <param name="page">Página (padrão: 1)</param>
    /// <param name="pageSize">Tamanho da página (padrão: 50)</param>
    /// <returns>Lista paginada de webhooks</returns>
    [HttpGet]
    public async Task<ActionResult<object>> GetAllWebhooks(
        [FromQuery] Guid? clientId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Admin listando webhooks - Cliente: {ClientId}, Página: {Page}", clientId, page);

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 50;

            // Simular consulta de webhooks
            await Task.Delay(100);

            var webhooks = new[]
            {
                new
                {
                    id = Guid.NewGuid(),
                    clientId = clientId ?? Guid.NewGuid(),
                    url = "https://webhook.site/unique-id-1",
                    events = new[] { "transaction.completed", "transaction.failed" },
                    isActive = true,
                    createdAt = DateTime.UtcNow.AddDays(-10),
                    lastDelivery = DateTime.UtcNow.AddHours(-2),
                    successCount = 45,
                    failureCount = 2
                },
                new
                {
                    id = Guid.NewGuid(),
                    clientId = clientId ?? Guid.NewGuid(),
                    url = "https://api.example.com/webhooks/fintech",
                    events = new[] { "balance.updated" },
                    isActive = true,
                    createdAt = DateTime.UtcNow.AddDays(-5),
                    lastDelivery = DateTime.UtcNow.AddMinutes(-30),
                    successCount = 23,
                    failureCount = 0
                }
            };

            var total = webhooks.Length;
            var pagedWebhooks = webhooks.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                webhooks = pagedWebhooks,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)total / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar webhooks (admin)");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém webhook específico (admin)
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <returns>Informações do webhook</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetWebhook([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Admin obtendo webhook {WebhookId}", id);

            await Task.Delay(50);

            var webhook = new
            {
                id,
                clientId = Guid.NewGuid(),
                url = "https://webhook.site/unique-id",
                events = new[] { "transaction.completed", "transaction.failed", "balance.updated" },
                secret = "wh_secret_***",
                description = "Webhook para notificações de transações",
                isActive = true,
                createdAt = DateTime.UtcNow.AddDays(-7),
                updatedAt = DateTime.UtcNow.AddDays(-1),
                lastDelivery = DateTime.UtcNow.AddHours(-1),
                successCount = 156,
                failureCount = 3,
                configuration = new
                {
                    timeout = 30,
                    retryAttempts = 3,
                    retryDelay = 60
                }
            };

            return Ok(webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter webhook {WebhookId} (admin)", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria webhook para um cliente (admin)
    /// </summary>
    /// <param name="request">Dados do webhook</param>
    /// <returns>Webhook criado</returns>
    [HttpPost]
    public async Task<ActionResult<object>> CreateWebhook([FromBody] AdminWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Admin criando webhook para cliente {ClientId}", request.ClientId);

            // Validações
            if (request.ClientId == Guid.Empty)
            {
                return BadRequest(new { message = "ClientId é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.Url))
            {
                return BadRequest(new { message = "URL é obrigatória" });
            }

            if (request.Events == null || !request.Events.Any())
            {
                return BadRequest(new { message = "Pelo menos um evento deve ser especificado" });
            }

            await Task.Delay(100);

            var webhook = new
            {
                id = Guid.NewGuid(),
                clientId = request.ClientId,
                url = request.Url,
                events = request.Events,
                secret = $"wh_secret_{Guid.NewGuid():N}",
                description = request.Description,
                isActive = true,
                createdAt = DateTime.UtcNow,
                updatedAt = DateTime.UtcNow,
                successCount = 0,
                failureCount = 0
            };

            return CreatedAtAction(nameof(GetWebhook), new { id = webhook.id }, webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar webhook (admin)");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza webhook (admin)
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <param name="request">Dados para atualização</param>
    /// <returns>Webhook atualizado</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<object>> UpdateWebhook([FromRoute] Guid id, [FromBody] AdminWebhookRequest request)
    {
        try
        {
            _logger.LogInformation("Admin atualizando webhook {WebhookId}", id);

            await Task.Delay(50);

            var webhook = new
            {
                id,
                clientId = request.ClientId,
                url = request.Url,
                events = request.Events,
                description = request.Description,
                isActive = true,
                updatedAt = DateTime.UtcNow,
                message = "Webhook atualizado com sucesso"
            };

            return Ok(webhook);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar webhook {WebhookId} (admin)", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui webhook (admin)
    /// </summary>
    /// <param name="id">ID do webhook</param>
    /// <returns>Confirmação de exclusão</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteWebhook([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Admin excluindo webhook {WebhookId}", id);

            await Task.Delay(50);

            return Ok(new { message = "Webhook excluído com sucesso", webhookId = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir webhook {WebhookId} (admin)", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém estatísticas de webhooks (admin)
    /// </summary>
    /// <returns>Estatísticas gerais</returns>
    [HttpGet("stats")]
    public async Task<ActionResult<object>> GetWebhookStats()
    {
        try
        {
            _logger.LogInformation("Admin obtendo estatísticas de webhooks");

            await Task.Delay(100);

            var stats = new
            {
                totalWebhooks = 47,
                activeWebhooks = 42,
                inactiveWebhooks = 5,
                totalDeliveries = 15420,
                successfulDeliveries = 14891,
                failedDeliveries = 529,
                successRate = 96.57,
                averageResponseTime = 245,
                lastHourDeliveries = 23,
                topEvents = new[]
                {
                    new { eventType = "transaction.completed", count = 8934 },
                    new { eventType = "balance.updated", count = 4521 },
                    new { eventType = "transaction.failed", count = 1965 }
                }
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de webhooks (admin)");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}

public class AdminWebhookRequest
{
    public Guid ClientId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string[] Events { get; set; } = Array.Empty<string>();
    public string? Secret { get; set; }
    public string? Description { get; set; }
}
