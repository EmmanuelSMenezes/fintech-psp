using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FintechPSP.IntegrationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.IntegrationService.Controllers;

/// <summary>
/// Controller para integrações com bancos
/// </summary>
[ApiController]
[Route("integrations")]
[Authorize]
public class IntegrationController : ControllerBase
{
    private readonly ILogger<IntegrationController> _logger;
    private readonly IRoutingService _routingService;

    public IntegrationController(
        ILogger<IntegrationController> logger,
        IRoutingService routingService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
    }

    /// <summary>
    /// Seleciona a melhor conta para uma transação usando roteamento ponderado
    /// </summary>
    [HttpPost("routing/select-account")]
    public async Task<IActionResult> SelectAccountForTransaction([FromBody] SelectAccountRequest request)
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var selectedAccount = await _routingService.SelectAccountForTransactionAsync(
                clienteId,
                request.BankCode,
                request.Amount);

            if (selectedAccount == null)
            {
                return NotFound(new { error = "no_account_available", message = "Nenhuma conta disponível para a transação" });
            }

            return Ok(new
            {
                selectedAccount = new
                {
                    contaId = selectedAccount.ContaId,
                    bankCode = selectedAccount.BankCode,
                    accountNumber = selectedAccount.AccountNumber,
                    description = selectedAccount.Description,
                    priority = selectedAccount.Priority,
                    selectionReason = selectedAccount.SelectionReason
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao selecionar conta para transação");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Lista contas com suas prioridades configuradas
    /// </summary>
    [HttpGet("routing/accounts-priority")]
    public async Task<IActionResult> GetAccountsWithPriority()
    {
        try
        {
            var clienteId = GetCurrentClientId();
            if (clienteId == Guid.Empty)
            {
                return Unauthorized("Cliente não identificado");
            }

            var accounts = await _routingService.GetAccountsWithPriorityAsync(clienteId);

            return Ok(new
            {
                clienteId = clienteId,
                accounts = accounts.Select(a => new
                {
                    contaId = a.ContaId,
                    bankCode = a.BankCode,
                    accountNumber = a.AccountNumber,
                    description = a.Description,
                    isActive = a.IsActive,
                    priority = a.Priority,
                    hasPriorityConfig = a.HasPriorityConfig
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contas com prioridade");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Integração Stark Bank - Transferências
    /// </summary>
    [HttpPost("stark-bank/transfers")]
    public async Task<IActionResult> StarkBankTransfer([FromBody] object request)
    {
        _logger.LogInformation("Processando transferência Stark Bank");
        
        // Mock da integração Stark Bank
        await Task.Delay(100); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            bankResponse = "Transfer processed successfully",
            provider = "stark-bank"
        });
    }

    /// <summary>
    /// Integração Sicoob - PSP
    /// </summary>
    [HttpPost("sicoob/psp")]
    public async Task<IActionResult> SicoobPsp([FromBody] object request)
    {
        _logger.LogInformation("Processando PSP Sicoob");
        
        // Mock da integração Sicoob
        await Task.Delay(150); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            bankResponse = "PSP transaction completed",
            provider = "sicoob"
        });
    }

    /// <summary>
    /// Integração Banco Genial - PIX Open Finance
    /// </summary>
    [HttpPost("banco-genial/pix")]
    public async Task<IActionResult> BancoGenialPix([FromBody] object request)
    {
        _logger.LogInformation("Processando PIX Banco Genial");
        
        // Mock da integração Banco Genial
        await Task.Delay(120); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            endToEndId = $"E{DateTime.Now:yyyyMMdd}{Guid.NewGuid().ToString("N")[..10]}",
            bankResponse = "PIX transaction initiated",
            provider = "banco-genial"
        });
    }

    /// <summary>
    /// Integração Efí - PIX e Boletos
    /// </summary>
    [HttpPost("efi/pix")]
    public async Task<IActionResult> EfiPix([FromBody] object request)
    {
        _logger.LogInformation("Processando PIX Efí");
        
        // Mock da integração Efí
        await Task.Delay(110); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            txId = Guid.NewGuid().ToString("N")[..32],
            bankResponse = "PIX sent successfully",
            provider = "efi"
        });
    }

    /// <summary>
    /// Integração Efí - Boletos
    /// </summary>
    [HttpPost("efi/boletos")]
    public async Task<IActionResult> EfiBoleto([FromBody] object request)
    {
        _logger.LogInformation("Processando boleto Efí");
        
        // Mock da integração Efí
        await Task.Delay(200); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            boletoId = Guid.NewGuid(),
            barCode = "34191234567890123456789012345678901234",
            bankResponse = "Boleto created successfully",
            provider = "efi"
        });
    }

    /// <summary>
    /// Integração Celcoin - PIX
    /// </summary>
    [HttpPost("celcoin/pix")]
    public async Task<IActionResult> CelcoinPix([FromBody] object request)
    {
        _logger.LogInformation("Processando PIX Celcoin");
        
        // Mock da integração Celcoin
        await Task.Delay(130); // Simular chamada API
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            galaxId = $"GAL{DateTime.Now:yyyyMMddHHmmss}",
            bankResponse = "PIX payment processed",
            provider = "celcoin"
        });
    }

    /// <summary>
    /// Mock para integrações de criptomoedas
    /// </summary>
    [HttpPost("crypto/{currency}")]
    public async Task<IActionResult> CryptoTransaction([FromRoute] string currency, [FromBody] object request)
    {
        _logger.LogInformation("Processando transação cripto {Currency}", currency);
        
        // Mock da integração crypto
        await Task.Delay(300); // Simular chamada API blockchain
        
        var supportedCurrencies = new[] { "USDT", "BTC", "ETH" };
        if (!supportedCurrencies.Contains(currency.ToUpper()))
        {
            return BadRequest(new { message = $"Criptomoeda {currency} não suportada" });
        }
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            txHash = $"0x{Guid.NewGuid().ToString("N")}",
            currency = currency.ToUpper(),
            network = currency.ToUpper() == "BTC" ? "Bitcoin" : "Ethereum",
            provider = "crypto-exchange"
        });
    }

    /// <summary>
    /// Stark Bank - Gerar QR Code PIX (POST /brcodes)
    /// </summary>
    [HttpPost("stark-bank/brcodes")]
    public async Task<IActionResult> StarkBankQrCode([FromBody] object request)
    {
        _logger.LogInformation("Gerando QR Code PIX via Stark Bank");

        // Mock da integração Stark Bank QR Code
        await Task.Delay(200); // Simular chamada API

        return Ok(new {
            success = true,
            id = Guid.NewGuid().ToString(),
            brcode = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426614174000520400005303986540510.005802BR5913FINTECH PSP6009SAO PAULO62070503***6304A1B2",
            qrCodeUrl = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            provider = "stark-bank",
            type = "static"
        });
    }

    /// <summary>
    /// Sicoob - Gerar QR Code PIX (Cobrança V3)
    /// </summary>
    [HttpPost("sicoob/cobranca/v3/qrcode")]
    public async Task<IActionResult> SicoobQrCode([FromBody] object request)
    {
        _logger.LogInformation("Gerando QR Code PIX via Sicoob Cobrança V3");

        // Mock da integração Sicoob QR Code
        await Task.Delay(250); // Simular chamada API

        return Ok(new {
            success = true,
            txid = Guid.NewGuid().ToString("N")[..25],
            pixCopiaECola = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426614174000520400005303986540510.005802BR5913FINTECH PSP6009SAO PAULO62070503***6304B2C3",
            qrcode = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            provider = "sicoob",
            type = "dynamic"
        });
    }

    /// <summary>
    /// Banco Genial - Gerar QR Code PIX (Open Finance)
    /// </summary>
    [HttpPost("banco-genial/openfinance/pix/qrcode")]
    public async Task<IActionResult> BancoGenialQrCode([FromBody] object request)
    {
        _logger.LogInformation("Gerando QR Code PIX via Banco Genial Open Finance");

        // Mock da integração Banco Genial QR Code
        await Task.Delay(180); // Simular chamada API

        return Ok(new {
            success = true,
            transactionId = Guid.NewGuid(),
            emvCode = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426614174000520400005303986540510.005802BR5913FINTECH PSP6009SAO PAULO62070503***6304C3D4",
            qrCodeImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            provider = "banco-genial",
            expiresAt = DateTime.UtcNow.AddMinutes(5)
        });
    }

    /// <summary>
    /// Efí (ex-Gerencianet) - Gerar QR Code PIX
    /// </summary>
    [HttpPost("efi/pix/qrcode")]
    public async Task<IActionResult> EfiQrCode([FromBody] object request)
    {
        _logger.LogInformation("Gerando QR Code PIX via Efí");

        // Mock da integração Efí QR Code
        await Task.Delay(220); // Simular chamada API

        return Ok(new {
            success = true,
            txid = Guid.NewGuid().ToString("N")[..32],
            qrcode = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426614174000520400005303986540510.005802BR5913FINTECH PSP6009SAO PAULO62070503***6304D4E5",
            imagemQrcode = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            provider = "efi",
            status = "ATIVA"
        });
    }

    /// <summary>
    /// Celcoin - Gerar QR Code PIX
    /// </summary>
    [HttpPost("celcoin/pix/qrcode")]
    public async Task<IActionResult> CelcoinQrCode([FromBody] object request)
    {
        _logger.LogInformation("Gerando QR Code PIX via Celcoin");

        // Mock da integração Celcoin QR Code
        await Task.Delay(190); // Simular chamada API

        return Ok(new {
            success = true,
            transactionId = Guid.NewGuid(),
            emvqrcps = "00020126580014br.gov.bcb.pix0136123e4567-e12b-12d1-a456-426614174000520400005303986540510.005802BR5913FINTECH PSP6009SAO PAULO62070503***6304E5F6",
            qrcodeImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==",
            provider = "celcoin",
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Health check das integrações
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> Health()
    {
        var integrations = new Dictionary<string, object>
        {
            ["stark-bank"] = new { status = "healthy", latency = "95ms", qrCodeSupport = true },
            ["sicoob"] = new { status = "healthy", latency = "120ms", qrCodeSupport = true },
            ["banco-genial"] = new { status = "healthy", latency = "110ms", qrCodeSupport = true },
            ["efi"] = new { status = "healthy", latency = "105ms", qrCodeSupport = true },
            ["celcoin"] = new { status = "healthy", latency = "125ms", qrCodeSupport = true },
            ["crypto"] = new { status = "healthy", latency = "250ms", qrCodeSupport = false }
        };

        return Ok(new {
            status = "healthy",
            service = "IntegrationService",
            timestamp = DateTime.UtcNow,
            integrations = integrations,
            qrCodeEndpoints = new[] {
                "stark-bank/brcodes",
                "sicoob/cobranca/v3/qrcode",
                "banco-genial/openfinance/pix/qrcode",
                "efi/pix/qrcode",
                "celcoin/pix/qrcode"
            }
        });
    }

    private Guid GetCurrentClientId()
    {
        var clientIdClaim = User.FindFirst("clienteId")?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(clientIdClaim, out var clienteId) ? clienteId : Guid.Empty;
    }
}

/// <summary>
/// Request para seleção de conta
/// </summary>
public class SelectAccountRequest
{
    public string? BankCode { get; set; }
    public decimal? Amount { get; set; }
}
