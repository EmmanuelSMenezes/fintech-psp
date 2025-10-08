using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FintechPSP.Shared.Domain.Events;
using MassTransit;

namespace FintechPSP.IntegrationService.Controllers;

/// <summary>
/// Controller para receber webhooks do Sicoob
/// </summary>
[ApiController]
[Route("webhooks")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly HttpClient _httpClient;

    public WebhookController(
        ILogger<WebhookController> logger,
        IPublishEndpoint publishEndpoint,
        HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Webhook para notificações PIX do Sicoob
    /// </summary>
    /// <param name="webhook">Dados do webhook PIX</param>
    /// <returns>Confirmação de recebimento</returns>
    [HttpPost("sicoob/pix")]
    [AllowAnonymous] // Sicoob não envia autenticação JWT
    public async Task<IActionResult> ReceivePixWebhook([FromBody] SicoobPixWebhook webhook)
    {
        try
        {
            _logger.LogInformation("Webhook PIX recebido - TxId: {TxId}, Status: {Status}", 
                webhook.TxId, webhook.Status);

            // Validar webhook (opcional: verificar assinatura)
            if (string.IsNullOrEmpty(webhook.TxId))
            {
                _logger.LogWarning("Webhook PIX inválido - TxId vazio");
                return BadRequest(new { error = "TxId é obrigatório" });
            }

            // Processar notificação baseada no status
            switch (webhook.Status?.ToUpper())
            {
                case "CONFIRMED":
                case "CONCLUIDA":
                    await ProcessPixConfirmed(webhook);
                    break;
                
                case "REJECTED":
                case "REJEITADA":
                    await ProcessPixRejected(webhook);
                    break;
                
                case "EXPIRED":
                case "EXPIRADA":
                    await ProcessPixExpired(webhook);
                    break;
                
                default:
                    _logger.LogInformation("Status PIX não processado: {Status}", webhook.Status);
                    break;
            }

            return Ok(new { message = "Webhook processado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook PIX: {TxId}", webhook.TxId);
            return StatusCode(500, new { error = "Erro interno no processamento" });
        }
    }

    /// <summary>
    /// Webhook para notificações de Boleto do Sicoob
    /// </summary>
    /// <param name="webhook">Dados do webhook Boleto</param>
    /// <returns>Confirmação de recebimento</returns>
    [HttpPost("sicoob/boleto")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveBoletoWebhook([FromBody] SicoobBoletoWebhook webhook)
    {
        try
        {
            _logger.LogInformation("Webhook Boleto recebido - NossoNumero: {NossoNumero}, Status: {Status}", 
                webhook.NossoNumero, webhook.Status);

            if (string.IsNullOrEmpty(webhook.NossoNumero))
            {
                _logger.LogWarning("Webhook Boleto inválido - NossoNumero vazio");
                return BadRequest(new { error = "NossoNumero é obrigatório" });
            }

            // Processar notificação baseada no status
            switch (webhook.Status?.ToUpper())
            {
                case "PAID":
                case "LIQUIDADO":
                    await ProcessBoletoConfirmed(webhook);
                    break;
                
                case "EXPIRED":
                case "VENCIDO":
                    await ProcessBoletoExpired(webhook);
                    break;
                
                case "CANCELLED":
                case "CANCELADO":
                    await ProcessBoletoCancelled(webhook);
                    break;
                
                default:
                    _logger.LogInformation("Status Boleto não processado: {Status}", webhook.Status);
                    break;
            }

            return Ok(new { message = "Webhook processado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook Boleto: {NossoNumero}", webhook.NossoNumero);
            return StatusCode(500, new { error = "Erro interno no processamento" });
        }
    }

    /// <summary>
    /// Webhook genérico para outras notificações do Sicoob
    /// </summary>
    /// <param name="webhook">Dados do webhook</param>
    /// <returns>Confirmação de recebimento</returns>
    [HttpPost("sicoob/generic")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveGenericWebhook([FromBody] JsonElement webhook)
    {
        try
        {
            var webhookJson = JsonSerializer.Serialize(webhook, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Webhook genérico Sicoob recebido: {Webhook}", webhookJson);

            // Processar webhook genérico se necessário
            // Por enquanto, apenas logar para análise

            return Ok(new { message = "Webhook genérico recebido" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar webhook genérico Sicoob");
            return StatusCode(500, new { error = "Erro interno no processamento" });
        }
    }

    /// <summary>
    /// Endpoint de health check para webhooks
    /// </summary>
    /// <returns>Status dos webhooks</returns>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            endpoints = new[]
            {
                "/webhooks/sicoob/pix",
                "/webhooks/sicoob/boleto",
                "/webhooks/sicoob/generic"
            }
        });
    }

    #region Private Methods

    private async Task ProcessPixConfirmed(SicoobPixWebhook webhook)
    {
        _logger.LogInformation("Processando PIX confirmado - TxId: {TxId}, Valor: {Amount}",
            webhook.TxId, webhook.Amount);

        // 1. Publicar evento de PIX confirmado (mantém para outros consumers)
        var pixConfirmadoEvent = new PixConfirmado
        {
            TxId = webhook.TxId,
            EndToEndId = webhook.EndToEndId,
            Amount = webhook.Amount ?? 0,
            PayerDocument = webhook.PayerDocument,
            PayerName = webhook.PayerName,
            ConfirmedAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756" // Sicoob
        };

        await _publishEndpoint.Publish(pixConfirmadoEvent);
        _logger.LogInformation("Evento PixConfirmado publicado - TxId: {TxId}", webhook.TxId);

        // 2. Chamar BalanceService diretamente via HTTP
        try
        {
            var notification = new
            {
                TxId = webhook.TxId,
                Amount = webhook.Amount ?? 0,
                PayerName = webhook.PayerName,
                PayerDocument = webhook.PayerDocument,
                ConfirmedAt = webhook.Timestamp ?? DateTime.UtcNow,
                PayerInfo = $"PIX via Sicoob - EndToEndId: {webhook.EndToEndId}"
            };

            var json = JsonSerializer.Serialize(notification);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // URL do BalanceService (ajustar conforme necessário)
            var balanceServiceUrl = "http://balance-service:8080/saldo/pix-confirmado";

            _logger.LogInformation("🚀 Chamando BalanceService via HTTP - URL: {Url}, TxId: {TxId}",
                balanceServiceUrl, webhook.TxId);

            var response = await _httpClient.PostAsync(balanceServiceUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ BalanceService respondeu com sucesso - TxId: {TxId}, Response: {Response}",
                    webhook.TxId, responseContent);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Erro ao chamar BalanceService - TxId: {TxId}, Status: {Status}, Error: {Error}",
                    webhook.TxId, response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Exceção ao chamar BalanceService via HTTP - TxId: {TxId}", webhook.TxId);
        }
    }

    private async Task ProcessPixRejected(SicoobPixWebhook webhook)
    {
        _logger.LogInformation("Processando PIX rejeitado - TxId: {TxId}, Motivo: {Reason}", 
            webhook.TxId, webhook.RejectionReason);

        var pixRejeitadoEvent = new PixRejeitado
        {
            TxId = webhook.TxId,
            RejectionReason = webhook.RejectionReason ?? "Motivo não informado",
            RejectedAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756"
        };

        await _publishEndpoint.Publish(pixRejeitadoEvent);
        _logger.LogInformation("Evento PixRejeitado publicado - TxId: {TxId}", webhook.TxId);
    }

    private async Task ProcessPixExpired(SicoobPixWebhook webhook)
    {
        _logger.LogInformation("Processando PIX expirado - TxId: {TxId}", webhook.TxId);

        var pixExpiradoEvent = new PixExpirado
        {
            TxId = webhook.TxId,
            ExpiredAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756"
        };

        await _publishEndpoint.Publish(pixExpiradoEvent);
        _logger.LogInformation("Evento PixExpirado publicado - TxId: {TxId}", webhook.TxId);
    }

    private async Task ProcessBoletoConfirmed(SicoobBoletoWebhook webhook)
    {
        _logger.LogInformation("Processando Boleto confirmado - NossoNumero: {NossoNumero}, Valor: {Amount}", 
            webhook.NossoNumero, webhook.Amount);

        var boletoConfirmadoEvent = new BoletoConfirmado
        {
            NossoNumero = webhook.NossoNumero,
            Amount = webhook.Amount ?? 0,
            PayerDocument = webhook.PayerDocument,
            PayerName = webhook.PayerName,
            PaidAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756"
        };

        await _publishEndpoint.Publish(boletoConfirmadoEvent);
        _logger.LogInformation("Evento BoletoConfirmado publicado - NossoNumero: {NossoNumero}", webhook.NossoNumero);
    }

    private async Task ProcessBoletoExpired(SicoobBoletoWebhook webhook)
    {
        _logger.LogInformation("Processando Boleto expirado - NossoNumero: {NossoNumero}", webhook.NossoNumero);

        var boletoExpiradoEvent = new BoletoExpirado
        {
            NossoNumero = webhook.NossoNumero,
            ExpiredAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756"
        };

        await _publishEndpoint.Publish(boletoExpiradoEvent);
        _logger.LogInformation("Evento BoletoExpirado publicado - NossoNumero: {NossoNumero}", webhook.NossoNumero);
    }

    private async Task ProcessBoletoCancelled(SicoobBoletoWebhook webhook)
    {
        _logger.LogInformation("Processando Boleto cancelado - NossoNumero: {NossoNumero}", webhook.NossoNumero);

        var boletoCanceladoEvent = new BoletoCancelado
        {
            NossoNumero = webhook.NossoNumero,
            CancelledAt = webhook.Timestamp ?? DateTime.UtcNow,
            BankCode = "756"
        };

        await _publishEndpoint.Publish(boletoCanceladoEvent);
        _logger.LogInformation("Evento BoletoCancelado publicado - NossoNumero: {NossoNumero}", webhook.NossoNumero);
    }

    #endregion
}
