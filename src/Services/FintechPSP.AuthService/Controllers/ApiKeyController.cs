using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FintechPSP.AuthService.Models;
using FintechPSP.AuthService.Services;
using System.Security.Claims;

namespace FintechPSP.AuthService.Controllers;

/// <summary>
/// Controller para gerenciamento de API Keys
/// </summary>
[ApiController]
[Route("api-keys")]
[Produces("application/json")]
public class ApiKeyController : ControllerBase
{
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ApiKeyController> _logger;

    public ApiKeyController(IApiKeyService apiKeyService, ILogger<ApiKeyController> logger)
    {
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica usando API Key e retorna JWT token
    /// </summary>
    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] ApiKeyAuthRequest request)
    {
        try
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _apiKeyService.AuthenticateAsync(request, clientIp);

            if (result == null)
            {
                return Unauthorized(new { error = "invalid_api_key", message = "API Key inválida ou expirada" });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na autenticação via API Key");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria uma nova API Key para uma empresa
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateApiKey([FromBody] CreateApiKeyRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var createdBy))
            {
                return BadRequest(new { error = "invalid_user", message = "Usuário inválido" });
            }

            var result = await _apiKeyService.CreateApiKeyAsync(request, createdBy);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar API Key para empresa {CompanyId}", request.CompanyId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista API Keys de uma empresa
    /// </summary>
    [HttpGet("company/{companyId}")]
    [Authorize]
    public async Task<IActionResult> GetCompanyApiKeys(
        [FromRoute] Guid companyId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10)
    {
        try
        {
            var (apiKeys, total) = await _apiKeyService.GetCompanyApiKeysAsync(companyId, page, limit);

            // Remover dados sensíveis da resposta
            var response = apiKeys.Select(ak => new
            {
                ak.Id,
                ak.PublicKey,
                ak.Name,
                ak.Scopes,
                ak.IsActive,
                ak.CreatedAt,
                ak.ExpiresAt,
                ak.LastUsedAt,
                ak.RateLimitPerMinute
            }).ToList();

            return Ok(new
            {
                apiKeys = response,
                total,
                page,
                limit,
                totalPages = (int)Math.Ceiling((double)total / limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar API Keys da empresa {CompanyId}", companyId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém detalhes de uma API Key
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetApiKey([FromRoute] Guid id)
    {
        try
        {
            var apiKey = await _apiKeyService.GetApiKeyAsync(id);

            if (apiKey == null)
            {
                return NotFound(new { error = "not_found", message = "API Key não encontrada" });
            }

            // Remover dados sensíveis
            var response = new
            {
                apiKey.Id,
                apiKey.PublicKey,
                apiKey.Name,
                apiKey.Scopes,
                apiKey.IsActive,
                apiKey.CreatedAt,
                apiKey.ExpiresAt,
                apiKey.LastUsedAt,
                apiKey.RateLimitPerMinute,
                apiKey.CompanyId
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter API Key {Id}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Ativa/desativa uma API Key
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateApiKeyStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateApiKeyStatusRequest request)
    {
        try
        {
            var success = await _apiKeyService.UpdateApiKeyStatusAsync(id, request.IsActive);

            if (!success)
            {
                return NotFound(new { error = "not_found", message = "API Key não encontrada" });
            }

            return Ok(new { message = "Status atualizado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status da API Key {Id}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Deleta uma API Key
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteApiKey([FromRoute] Guid id)
    {
        try
        {
            var success = await _apiKeyService.DeleteApiKeyAsync(id);

            if (!success)
            {
                return NotFound(new { error = "not_found", message = "API Key não encontrada" });
            }

            return Ok(new { message = "API Key deletada com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar API Key {Id}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Valida se uma API Key tem permissão para um escopo
    /// </summary>
    [HttpPost("validate-scope")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidateScope([FromBody] ValidateScopeRequest request)
    {
        try
        {
            var hasPermission = await _apiKeyService.ValidateApiKeyScopeAsync(request.PublicKey, request.Scope);

            return Ok(new { hasPermission });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar escopo da API Key");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }
}

/// <summary>
/// Request para atualizar status da API Key
/// </summary>
public class UpdateApiKeyStatusRequest
{
    public bool IsActive { get; set; }
}

/// <summary>
/// Request para validar escopo
/// </summary>
public class ValidateScopeRequest
{
    public string PublicKey { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}
