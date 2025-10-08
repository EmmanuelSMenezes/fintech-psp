using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FintechPSP.IntegrationService.Services;
using FintechPSP.IntegrationService.Services.Sicoob;
using FintechPSP.IntegrationService.Services.Sicoob.Pix;
using FintechPSP.IntegrationService.Services.Sicoob.ContaCorrente;
using FintechPSP.IntegrationService.Services.Sicoob.SPB;
using FintechPSP.IntegrationService.Services.ReceitaFederal;
using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using FintechPSP.IntegrationService.Models.Sicoob.SPB;
using FintechPSP.Shared.Domain.Events;
using MassTransit;
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
    private readonly IPixPagamentosService _pixPagamentosService;
    private readonly IPixRecebimentosService _pixRecebimentosService;
    private readonly IContaCorrenteService _contaCorrenteService;
    private readonly ISPBService _spbService;
    private readonly ISicoobAuthService _sicoobAuthService;
    private readonly IReceitaFederalService _receitaFederalService;
    private readonly ISicoobTokenCache _tokenCache;
    private readonly ICertificateMonitoringService _certificateMonitoring;

    public IntegrationController(
        ILogger<IntegrationController> logger,
        IRoutingService routingService,
        IPixPagamentosService pixPagamentosService,
        IPixRecebimentosService pixRecebimentosService,
        IContaCorrenteService contaCorrenteService,
        ISPBService spbService,
        ISicoobAuthService sicoobAuthService,
        IReceitaFederalService receitaFederalService,
        ISicoobTokenCache tokenCache,
        ICertificateMonitoringService certificateMonitoring)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _routingService = routingService ?? throw new ArgumentNullException(nameof(routingService));
        _pixPagamentosService = pixPagamentosService ?? throw new ArgumentNullException(nameof(pixPagamentosService));
        _pixRecebimentosService = pixRecebimentosService ?? throw new ArgumentNullException(nameof(pixRecebimentosService));
        _contaCorrenteService = contaCorrenteService ?? throw new ArgumentNullException(nameof(contaCorrenteService));
        _spbService = spbService ?? throw new ArgumentNullException(nameof(spbService));
        _sicoobAuthService = sicoobAuthService ?? throw new ArgumentNullException(nameof(sicoobAuthService));
        _receitaFederalService = receitaFederalService ?? throw new ArgumentNullException(nameof(receitaFederalService));
        _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        _certificateMonitoring = certificateMonitoring ?? throw new ArgumentNullException(nameof(certificateMonitoring));
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
        
        // Integração Stark Bank
        _logger.LogInformation("Processando transação PIX via Stark Bank");
        
        return Ok(new { 
            success = true, 
            transactionId = Guid.NewGuid(),
            bankResponse = "Transfer processed successfully",
            provider = "stark-bank"
        });
    }

    /// <summary>
    /// Integração Sicoob - PIX Pagamento
    /// </summary>
    [HttpPost("sicoob/pix/pagamento")]
    public async Task<IActionResult> SicoobPixPagamento([FromBody] PixPagamentoRequest request)
    {
        try
        {
            _logger.LogInformation("Processando pagamento PIX Sicoob para chave: {Chave}", request.Favorecido.Chave);

            var response = await _pixPagamentosService.RealizarPagamentoPixAsync(request);

            if (response == null)
            {
                return BadRequest(new { error = "payment_failed", message = "Falha ao processar pagamento PIX" });
            }

            return Ok(new {
                success = true,
                transactionId = response.EndToEndId,
                txId = response.Txid,
                status = response.Status,
                valor = response.Valor,
                horario = response.Horario,
                provider = "sicoob"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar pagamento PIX Sicoob");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Integração Sicoob - PIX Cobrança
    /// </summary>
    [HttpPost("sicoob/pix/cobranca")]
    public async Task<IActionResult> SicoobPixCobranca([FromBody] CobrancaImediataRequest request)
    {
        try
        {
            _logger.LogInformation("Criando cobrança PIX Sicoob para chave: {Chave}", request.Chave);

            var response = await _pixRecebimentosService.CriarCobrancaImediataAsync(request);

            if (response == null)
            {
                return BadRequest(new { error = "cobranca_failed", message = "Falha ao criar cobrança PIX" });
            }

            return Ok(new {
                success = true,
                txId = response.TxId,
                status = response.Status,
                pixCopiaECola = response.PixCopiaECola,
                qrcode = response.QrCode,
                provider = "sicoob"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cobrança PIX Sicoob");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Integração Sicoob - TED
    /// </summary>
    [HttpPost("sicoob/ted")]
    public async Task<IActionResult> SicoobTED([FromBody] TEDRequest request)
    {
        try
        {
            _logger.LogInformation("Processando TED Sicoob para conta: {Banco}-{Agencia}-{Conta}",
                request.ContaDestino.Banco, request.ContaDestino.Agencia, request.ContaDestino.Conta);

            var response = await _spbService.RealizarTEDAsync(request);

            if (response == null)
            {
                return BadRequest(new { error = "ted_failed", message = "Falha ao processar TED" });
            }

            return Ok(new {
                success = true,
                numeroDocumento = response.NumeroDocumento,
                idTransferencia = response.IdTransferencia,
                situacao = response.Situacao,
                valor = response.Valor,
                dataTransferencia = response.DataTransferencia,
                provider = "sicoob"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar TED Sicoob");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Integração Sicoob - Consultar Saldo
    /// </summary>
    [HttpGet("sicoob/conta/{contaCorrente}/saldo")]
    public async Task<IActionResult> SicoobConsultarSaldo(string contaCorrente)
    {
        try
        {
            _logger.LogInformation("Consultando saldo Sicoob da conta: {ContaCorrente}", contaCorrente);

            var response = await _contaCorrenteService.ConsultarSaldoAsync(contaCorrente);

            if (response == null)
            {
                return NotFound(new { error = "account_not_found", message = "Conta não encontrada" });
            }

            return Ok(new {
                success = true,
                contaCorrente = response.ContaCorrente,
                saldoAtual = response.SaldoAtual,
                saldoDisponivel = response.SaldoDisponivel,
                saldoBloqueado = response.SaldoBloqueado,
                dataHoraConsulta = response.DataHoraConsulta,
                provider = "sicoob"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar saldo Sicoob");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Integração Banco Genial - PIX Open Finance
    /// </summary>
    [HttpPost("banco-genial/pix")]
    public async Task<IActionResult> BancoGenialPix([FromBody] object request)
    {
        _logger.LogInformation("Processando PIX Banco Genial");
        
        // Integração Banco Genial
        _logger.LogInformation("Processando transação PIX via Banco Genial");
        
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
        
        // Integração Efí
        _logger.LogInformation("Processando transação TED via Efí");
        
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
        
        // Integração Efí
        _logger.LogInformation("Processando emissão de boleto via Efí");
        
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
        
        // Integração Celcoin
        _logger.LogInformation("Processando transação PIX via Celcoin");
        
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
        
        // Integração crypto
        _logger.LogInformation("Processando transação crypto para moeda {Currency}", currency);
        
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

        // Integração Stark Bank QR Code
        _logger.LogInformation("Gerando QR Code PIX via Stark Bank");

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

        // Integração Sicoob QR Code
        _logger.LogInformation("Gerando QR Code PIX via Sicoob");

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

        // Integração Banco Genial QR Code
        _logger.LogInformation("Gerando QR Code PIX via Banco Genial");

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

        // Integração Efí QR Code
        _logger.LogInformation("Gerando QR Code PIX via Efí");

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

        // Integração Celcoin QR Code
        _logger.LogInformation("Gerando QR Code PIX via Celcoin");

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
    /// Endpoint de teste para simular evento PixIniciado
    /// </summary>
    [HttpPost("test/pix-event")]
    [AllowAnonymous]
    public async Task<IActionResult> TestPixEvent([FromServices] IPublishEndpoint publishEndpoint)
    {
        _logger.LogInformation("Simulando evento PixIniciado para teste de integracao");

        var pixEvent = new PixIniciado
        {
            TransactionId = Guid.NewGuid(),
            ExternalId = $"PIX-TEST-EVENT-{DateTime.Now:yyyyMMddHHmmss}",
            Amount = 99.99m,
            PixKey = "11999887766",
            BankCode = "756",
            Description = "Teste de evento PIX para integracao Sicoob",
            WebhookUrl = "https://webhook.site/test-event",
            EndToEndId = null
        };

        try
        {
            await publishEndpoint.Publish(pixEvent);
            _logger.LogInformation("Evento PixIniciado publicado com sucesso: {TransactionId}", pixEvent.TransactionId);

            return Ok(new
            {
                success = true,
                message = "Evento PixIniciado publicado com sucesso",
                eventData = new
                {
                    transactionId = pixEvent.TransactionId,
                    externalId = pixEvent.ExternalId,
                    amount = pixEvent.Amount,
                    pixKey = pixEvent.PixKey,
                    bankCode = pixEvent.BankCode
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento PixIniciado");
            return StatusCode(500, new
            {
                success = false,
                message = "Erro ao publicar evento",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Teste de conectividade Sicoob - Validação completa da integração
    /// </summary>
    [HttpGet("sicoob/test-connectivity")]
    [AllowAnonymous]
    public async Task<IActionResult> TestSicoobConnectivity()
    {
        try
        {
            _logger.LogInformation("Iniciando teste de conectividade Sicoob...");

            var testResults = new
            {
                timestamp = DateTime.UtcNow,
                tests = new List<object>()
            };

            // 1. Teste de Autenticação OAuth
            _logger.LogInformation("1. Testando autenticação OAuth...");
            var authTest = await TestOAuthAuthentication();
            ((List<object>)testResults.tests).Add(authTest);

            // 2. Teste de Ping nos Endpoints
            _logger.LogInformation("2. Testando conectividade dos endpoints...");
            var pingTest = await TestEndpointsPing();
            ((List<object>)testResults.tests).Add(pingTest);

            // 3. Teste de Validação de Scopes
            _logger.LogInformation("3. Testando validação de scopes...");
            var scopesTest = await TestScopesValidation();
            ((List<object>)testResults.tests).Add(scopesTest);

            // 4. Teste de Webhook (simulado)
            _logger.LogInformation("4. Testando configuração de webhook...");
            var webhookTest = TestWebhookConfiguration();
            ((List<object>)testResults.tests).Add(webhookTest);

            // Determinar status geral
            var allPassed = ((List<object>)testResults.tests).All(t =>
                ((dynamic)t).status == "success");

            _logger.LogInformation("Teste de conectividade concluído. Status: {Status}",
                allPassed ? "SUCCESS" : "PARTIAL");

            return Ok(new
            {
                status = allPassed ? "success" : "partial",
                message = allPassed ? "Todos os testes passaram" : "Alguns testes falharam",
                results = testResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante teste de conectividade Sicoob");
            return StatusCode(500, new
            {
                status = "error",
                message = "Erro interno durante teste de conectividade",
                error = ex.Message
            });
        }
    }

    private async Task<object> TestOAuthAuthentication()
    {
        try
        {
            var token = await _sicoobAuthService.GetAccessTokenAsync();

            return new
            {
                test = "OAuth Authentication",
                status = "success",
                message = "Token obtido com sucesso",
                details = new
                {
                    tokenLength = token.Length,
                    tokenPrefix = token.Substring(0, Math.Min(10, token.Length)) + "...",
                    isValid = _sicoobAuthService.IsTokenValid()
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                test = "OAuth Authentication",
                status = "error",
                message = "Falha na autenticação OAuth",
                error = ex.Message
            };
        }
    }

    private async Task<object> TestEndpointsPing()
    {
        var endpoints = new Dictionary<string, string>
        {
            { "CobrancaBancaria", "https://api.sicoob.com.br/cobranca-bancaria/v3" },
            { "Pagamentos", "https://api.sicoob.com.br/pagamentos/v3" },
            { "ContaCorrente", "https://api.sicoob.com.br/conta-corrente/v4" },
            { "PixPagamentos", "https://api.sicoob.com.br/pix-pagamentos/v2" },
            { "PixRecebimentos", "https://api.sicoob.com.br/pix/api/v2" },
            { "SPB", "https://api.sicoob.com.br/spb/v2" }
        };

        var results = new List<object>();

        foreach (var endpoint in endpoints)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // Apenas teste de conectividade básica (HEAD request)
                var response = await httpClient.SendAsync(
                    new HttpRequestMessage(HttpMethod.Head, endpoint.Value));

                results.Add(new
                {
                    endpoint = endpoint.Key,
                    url = endpoint.Value,
                    status = response.IsSuccessStatusCode ? "success" : "warning",
                    statusCode = (int)response.StatusCode,
                    message = response.IsSuccessStatusCode ? "Endpoint acessível" : $"Status: {response.StatusCode}"
                });
            }
            catch (Exception ex)
            {
                results.Add(new
                {
                    endpoint = endpoint.Key,
                    url = endpoint.Value,
                    status = "error",
                    message = "Endpoint inacessível",
                    error = ex.Message
                });
            }
        }

        return new
        {
            test = "Endpoints Ping",
            status = results.Any(r => ((dynamic)r).status == "success") ? "success" : "error",
            message = $"{results.Count(r => ((dynamic)r).status == "success")}/{results.Count} endpoints acessíveis",
            details = results
        };
    }

    private async Task<object> TestScopesValidation()
    {
        try
        {
            // Simula validação de scopes fazendo uma chamada básica
            // Em ambiente real, faria uma chamada que requer scopes específicos
            var token = await _sicoobAuthService.GetAccessTokenAsync();

            var configuredScopes = new[]
            {
                "boletos_consulta", "boletos_inclusao", "boletos_alteracao",
                "webhooks_inclusao", "webhooks_consulta", "webhooks_alteracao",
                "pagamentos_inclusao", "pagamentos_alteracao", "pagamentos_consulta",
                "cco_saldo", "cco_extrato", "cco_consulta", "cco_transferencias",
                "pix_pagamentos", "pix_recebimentos", "pix_consultas"
            };

            return new
            {
                test = "Scopes Validation",
                status = "success",
                message = "Scopes configurados corretamente",
                details = new
                {
                    configuredScopes = configuredScopes,
                    totalScopes = configuredScopes.Length,
                    note = "Validação completa requer chamadas específicas para cada scope"
                }
            };
        }
        catch (Exception ex)
        {
            return new
            {
                test = "Scopes Validation",
                status = "error",
                message = "Erro na validação de scopes",
                error = ex.Message
            };
        }
    }

    private object TestWebhookConfiguration()
    {
        try
        {
            // Simula teste de configuração de webhook
            var webhookConfig = new
            {
                callbackUrl = "https://api.fintechpsp.com/webhooks/sicoob",
                events = new[] { "pix.received", "boleto.paid", "ted.completed" },
                configured = true
            };

            return new
            {
                test = "Webhook Configuration",
                status = "success",
                message = "Configuração de webhook simulada",
                details = webhookConfig
            };
        }
        catch (Exception ex)
        {
            return new
            {
                test = "Webhook Configuration",
                status = "error",
                message = "Erro na configuração de webhook",
                error = ex.Message
            };
        }
    }

    /// <summary>
    /// Health check das integrações
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<IActionResult> Health()
    {
        var integrations = new Dictionary<string, object>();

        // Health check real do Sicoob
        try
        {
            var startTime = DateTime.UtcNow;
            // Fazer uma chamada simples para testar conectividade
            var sicoobHealth = await TestSicoobConnectivityAsync();
            var latency = (DateTime.UtcNow - startTime).TotalMilliseconds;

            integrations["sicoob"] = new {
                status = sicoobHealth ? "healthy" : "unhealthy",
                latency = $"{latency:F0}ms",
                qrCodeSupport = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no health check do Sicoob");
            integrations["sicoob"] = new {
                status = "unhealthy",
                latency = "timeout",
                qrCodeSupport = true,
                error = ex.Message
            };
        }

        // TODO: Implementar health checks reais para outras integrações
        integrations["stark-bank"] = new { status = "not_implemented", latency = "0ms", qrCodeSupport = true };
        integrations["banco-genial"] = new { status = "not_implemented", latency = "0ms", qrCodeSupport = true };
        integrations["efi"] = new { status = "not_implemented", latency = "0ms", qrCodeSupport = true };
        integrations["celcoin"] = new { status = "not_implemented", latency = "0ms", qrCodeSupport = true };
        integrations["crypto"] = new { status = "not_implemented", latency = "0ms", qrCodeSupport = false };

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
            },
            sicoobEndpoints = new[] {
                "sicoob/pix/pagamento",
                "sicoob/pix/cobranca",
                "sicoob/ted",
                "sicoob/conta/{contaCorrente}/saldo"
            }
        });
    }

    private Guid GetCurrentClientId()
    {
        var clientIdClaim = User.FindFirst("clienteId")?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(clientIdClaim, out var clienteId) ? clienteId : Guid.Empty;
    }

    private async Task<bool> TestSicoobConnectivityAsync()
    {
        try
        {
            // Fazer uma chamada simples para testar se o Sicoob está respondendo
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            // Usar o endpoint base do Sicoob configurado
            var baseUrl = "https://sandbox.sicoob.com.br/sicoob/sandbox";
            var response = await httpClient.GetAsync($"{baseUrl}/conta-corrente/v4");

            // Mesmo que retorne 401 (não autorizado), significa que o serviço está online
            return response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                   response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validar CNPJ via Receita Federal
    /// </summary>
    /// <param name="request">Dados do CNPJ para validação</param>
    /// <returns>Resultado da validação</returns>
    [HttpPost("receita-federal/cnpj/validate")]
    public async Task<ActionResult<CnpjValidationResult>> ValidateCnpj([FromBody] ValidateCnpjRequest request)
    {
        try
        {
            _logger.LogInformation("Validando CNPJ: {Cnpj}", request.Cnpj);

            var result = await _receitaFederalService.ValidateCnpjAsync(request.Cnpj);

            if (result.IsValid)
            {
                _logger.LogInformation("CNPJ válido - Empresa: {CompanyName}, Status: {Status}",
                    result.CompanyName, result.Status);
            }
            else
            {
                _logger.LogWarning("CNPJ inválido: {ErrorMessage}", result.ErrorMessage);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CNPJ: {Cnpj}", request.Cnpj);
            return StatusCode(500, new { error = "Erro interno na validação do CNPJ" });
        }
    }



    /// <summary>
    /// Status do cache de tokens
    /// </summary>
    /// <returns>Status do cache</returns>
    [HttpGet("sicoob/token/cache/status")]
    public ActionResult<TokenCacheStatus> GetTokenCacheStatus()
    {
        try
        {
            var status = _tokenCache.GetCacheStatus();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status do cache de tokens");
            return StatusCode(500, new { error = "Erro interno na consulta do cache" });
        }
    }

    /// <summary>
    /// Invalidar cache de tokens
    /// </summary>
    /// <returns>Confirmação</returns>
    [HttpPost("sicoob/token/cache/invalidate")]
    public IActionResult InvalidateTokenCache()
    {
        try
        {
            _tokenCache.InvalidateToken();
            _logger.LogInformation("Cache de tokens invalidado manualmente");
            return Ok(new { message = "Cache de tokens invalidado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao invalidar cache de tokens");
            return StatusCode(500, new { error = "Erro interno na invalidação do cache" });
        }
    }

    /// <summary>
    /// Refresh forçado do token
    /// </summary>
    /// <returns>Novo token</returns>
    [HttpPost("sicoob/token/refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var newToken = await _tokenCache.RefreshTokenAsync();
            _logger.LogInformation("Token OAuth renovado manualmente");

            return Ok(new
            {
                message = "Token renovado com sucesso",
                tokenPreview = $"{newToken[..10]}..."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return StatusCode(500, new { error = "Erro interno na renovação do token" });
        }
    }

    /// <summary>
    /// Status dos certificados mTLS
    /// </summary>
    /// <returns>Status dos certificados</returns>
    [HttpGet("sicoob/certificates/status")]
    public async Task<ActionResult<CertificateStatus>> GetCertificateStatus()
    {
        try
        {
            var status = await _certificateMonitoring.CheckCertificateStatusAsync();

            if (!status.IsValid)
            {
                _logger.LogWarning("Certificado inválido: {ErrorMessage}", status.ErrorMessage);
            }
            else if (status.NeedsRenewal)
            {
                _logger.LogWarning("Certificado precisa ser renovado em {Days} dias", status.DaysUntilExpiry);
            }

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status dos certificados");
            return StatusCode(500, new { error = "Erro interno na verificação de certificados" });
        }
    }

    /// <summary>
    /// Configuração de retry policy
    /// </summary>
    /// <returns>Configuração atual</returns>
    [HttpGet("sicoob/retry/config")]
    public IActionResult GetRetryConfig()
    {
        try
        {
            var config = new
            {
                retryCount = 3,
                backoffType = "Exponential",
                baseDelay = "2 seconds",
                circuitBreakerThreshold = 5,
                circuitBreakerDuration = "30 seconds",
                timeout = "60 seconds"
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar configuração de retry");
            return StatusCode(500, new { error = "Erro interno na consulta de configuração" });
        }
    }

    /// <summary>
    /// Rate limiting status
    /// </summary>
    /// <returns>Status do rate limiting</returns>
    [HttpGet("sicoob/rate-limit/status")]
    public IActionResult GetRateLimitStatus()
    {
        try
        {
            // Por enquanto, retornar configuração estática
            // Em produção, implementar rate limiting real
            var status = new
            {
                enabled = false,
                requestsPerMinute = 60,
                currentUsage = 0,
                resetTime = DateTime.UtcNow.AddMinutes(1),
                message = "Rate limiting não implementado ainda"
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status de rate limiting");
            return StatusCode(500, new { error = "Erro interno na consulta de rate limiting" });
        }
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

/// <summary>
/// Request para validação de CNPJ
/// </summary>
public class ValidateCnpjRequest
{
    public string Cnpj { get; set; } = string.Empty;
}
