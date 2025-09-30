using System.Security.Claims;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FintechPSP.TransactionService.Controllers;

/// <summary>
/// Controller para operações de QR Code PIX
/// </summary>
[ApiController]
[Authorize]
public class QrCodeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<QrCodeController> _logger;

    public QrCodeController(IMediator mediator, ILogger<QrCodeController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gera QR Code PIX estático (reutilizável, sem valor fixo)
    /// </summary>
    /// <param name="request">Dados para geração do QR Code estático</param>
    /// <returns>QR Code gerado com payload EMV e imagem base64</returns>
    [HttpPost("transacoes/pix/qrcode/estatico")]
    public async Task<ActionResult<QrCodeResponse>> GerarQrEstatico([FromBody] QrCodeEstaticoRequest request)
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Gerando QR Code estático para cliente {ClientId}, externalId {ExternalId}", 
                clientId, request.ExternalId);

            // Validações básicas
            if (string.IsNullOrWhiteSpace(request.ExternalId))
            {
                return BadRequest(new { message = "ExternalId é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.PixKey))
            {
                return BadRequest(new { message = "PixKey é obrigatória" });
            }

            if (string.IsNullOrWhiteSpace(request.BankCode))
            {
                return BadRequest(new { message = "BankCode é obrigatório" });
            }

            var command = new GerarQrEstaticoCommand(
                request.ExternalId,
                request.PixKey,
                request.BankCode,
                request.Description,
                clientId);

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para geração de QR Code estático");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida para QR Code estático");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar QR Code estático");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Gera QR Code PIX dinâmico (com valor e expiração)
    /// </summary>
    /// <param name="request">Dados para geração do QR Code dinâmico</param>
    /// <returns>QR Code gerado com payload EMV e imagem base64</returns>
    [HttpPost("transacoes/pix/qrcode/dinamico")]
    public async Task<ActionResult<QrCodeResponse>> GerarQrDinamico([FromBody] QrCodeDinamicoRequest request)
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Gerando QR Code dinâmico para cliente {ClientId}, externalId {ExternalId}, valor {Amount}", 
                clientId, request.ExternalId, request.Amount);

            // Validações básicas
            if (string.IsNullOrWhiteSpace(request.ExternalId))
            {
                return BadRequest(new { message = "ExternalId é obrigatório" });
            }

            if (request.Amount <= 0)
            {
                return BadRequest(new { message = "Amount deve ser maior que zero" });
            }

            if (string.IsNullOrWhiteSpace(request.PixKey))
            {
                return BadRequest(new { message = "PixKey é obrigatória" });
            }

            if (string.IsNullOrWhiteSpace(request.BankCode))
            {
                return BadRequest(new { message = "BankCode é obrigatório" });
            }

            if (request.ExpiresIn <= 0)
            {
                return BadRequest(new { message = "ExpiresIn deve ser maior que zero" });
            }

            var command = new GerarQrDinamicoCommand(
                request.ExternalId,
                request.Amount,
                request.PixKey,
                request.BankCode,
                request.Description,
                request.ExpiresIn,
                clientId);

            var result = await _mediator.Send(command);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Dados inválidos para geração de QR Code dinâmico");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operação inválida para QR Code dinâmico");
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar QR Code dinâmico");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Gera QR Code PIX estático para internet banking (scope 'banking')
    /// </summary>
    /// <param name="request">Dados para geração do QR Code estático</param>
    /// <returns>QR Code gerado com payload EMV e imagem base64</returns>
    [HttpPost("banking/transacoes/pix/qrcode/estatico")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult<QrCodeResponse>> GerarQrEstaticoBanking([FromBody] QrCodeEstaticoRequest request)
    {
        // Reutiliza a mesma lógica do endpoint principal
        return await GerarQrEstatico(request);
    }

    /// <summary>
    /// Gera QR Code PIX dinâmico para internet banking (scope 'banking')
    /// </summary>
    /// <param name="request">Dados para geração do QR Code dinâmico</param>
    /// <returns>QR Code gerado com payload EMV e imagem base64</returns>
    [HttpPost("banking/transacoes/pix/qrcode/dinamico")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult<QrCodeResponse>> GerarQrDinamicoBanking([FromBody] QrCodeDinamicoRequest request)
    {
        // Reutiliza a mesma lógica do endpoint principal
        return await GerarQrDinamico(request);
    }

    /// <summary>
    /// Lista QR Codes PIX do cliente
    /// </summary>
    [HttpGet("pix/qr-code")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult> GetQrCodes()
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Listando QR Codes para cliente {ClientId}", clientId);

            // TODO: Implementar repositório para QR Codes quando necessário
            // Por enquanto, usando dados mock sem Task.Delay

            var qrCodes = new object[]
            {
                new {
                    id = Guid.NewGuid(),
                    externalId = "QR-001",
                    type = "static",
                    pixKey = "user@example.com",
                    qrcodePayload = "00020126580014BR.GOV.BCB.PIX0136123e4567-e89b-12d3-a456-4266141740005204000053039865802BR5913FULANO DE TAL6008BRASILIA62070503***63041D3D",
                    createdAt = DateTime.UtcNow.AddDays(-1),
                    isActive = true
                },
                new {
                    id = Guid.NewGuid(),
                    externalId = "QR-002",
                    type = "dynamic",
                    pixKey = "user@example.com",
                    amount = 50.00m,
                    qrcodePayload = "00020126580014BR.GOV.BCB.PIX0136123e4567-e89b-12d3-a456-4266141740005204000053039865802BR5913FULANO DE TAL6008BRASILIA62070503***63041D3D",
                    createdAt = DateTime.UtcNow.AddHours(-2),
                    expiresAt = DateTime.UtcNow.AddHours(1),
                    isActive = true
                }
            };

            return Ok(qrCodes);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista chaves PIX do cliente
    /// </summary>
    [HttpGet("pix/chaves")]
    [Authorize(Policy = "BankingScope")]
    public async Task<ActionResult> GetPixKeys()
    {
        try
        {
            var clientId = GetCurrentClientId();
            if (clientId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            _logger.LogInformation("Listando chaves PIX para cliente {ClientId}", clientId);

            // TODO: Implementar repositório para chaves PIX quando necessário
            // Por enquanto, usando dados mock sem Task.Delay

            var pixKeys = new object[]
            {
                new {
                    id = Guid.NewGuid(),
                    key = "user@example.com",
                    type = "email",
                    status = "active",
                    createdAt = DateTime.UtcNow.AddDays(-30),
                    bankCode = "001",
                    accountNumber = "12345-6"
                },
                new {
                    id = Guid.NewGuid(),
                    key = "+5511999999999",
                    type = "phone",
                    status = "active",
                    createdAt = DateTime.UtcNow.AddDays(-15),
                    bankCode = "001",
                    accountNumber = "12345-6"
                }
            };

            return Ok(pixKeys);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Health check do serviço de QR Code
    /// </summary>
    [HttpGet("qrcode/health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new {
            status = "healthy",
            service = "QrCodeService",
            timestamp = DateTime.UtcNow,
            features = new[] { "static_qr", "dynamic_qr", "emv_payload", "base64_image" }
        });
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
