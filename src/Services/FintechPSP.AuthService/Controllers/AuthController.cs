using System;
using System.Threading.Tasks;
using FintechPSP.AuthService.Commands;
using FintechPSP.AuthService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.AuthService.Controllers;

/// <summary>
/// Controller para autenticação OAuth 2.0
/// </summary>
[ApiController]
[Route("auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Obtém token de acesso OAuth 2.0
    /// </summary>
    /// <param name="request">Dados para obtenção do token</param>
    /// <returns>Token de acesso</returns>
    /// <response code="200">Token gerado com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Credenciais inválidas</response>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TokenResponse>> GetToken([FromBody] TokenRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new ObterTokenCommand(
                request.GrantType,
                request.ClientId,
                request.ClientSecret,
                request.Scope);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = "invalid_client", error_description = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "invalid_request", error_description = ex.Message });
        }
    }

    /// <summary>
    /// Health check do serviço
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(200)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "AuthService", timestamp = DateTime.UtcNow });
    }
}
