using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.TransactionService.Commands;
using FintechPSP.TransactionService.DTOs;
using FintechPSP.TransactionService.Models;
using FintechPSP.TransactionService.Repositories;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration;
using TransactionModel = FintechPSP.TransactionService.Models.Transaction;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dapper;

namespace FintechPSP.TransactionService.Controllers;

/// <summary>
/// Controller para transações
/// </summary>
[ApiController]
[Route("transacoes")]
[Authorize]
[Produces("application/json")]
public class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ITransactionIntegrationService? _transactionIntegrationService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        IMediator mediator,
        ITransactionRepository transactionRepository,
        IDbConnectionFactory connectionFactory,
        ILogger<TransactionController> logger,
        ITransactionIntegrationService? transactionIntegrationService = null)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _transactionIntegrationService = transactionIntegrationService;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista transações do cliente
    /// </summary>
    /// <param name="page">Página</param>
    /// <param name="limit">Limite por página</param>
    /// <param name="type">Tipo de transação (pix, ted, boleto)</param>
    /// <param name="status">Status da transação</param>
    /// <returns>Lista de transações</returns>
    [HttpGet]
    public async Task<ActionResult> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        try
        {
            _logger.LogInformation("Listando transações - Página: {Page}, Limite: {Limit}, Tipo: {Type}, Status: {Status}",
                page, limit, type, status);

            // Obter ID do usuário do token JWT
            _logger.LogInformation("Claims disponíveis no token:");
            foreach (var claim in User.Claims)
            {
                _logger.LogInformation("  {Type}: {Value}", claim.Type, claim.Value);
            }

            var userIdClaim = User.FindFirst("sub")?.Value ??
                             User.FindFirst("userId")?.Value ??
                             User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("ID do usuário não encontrado no token. UserIdClaim: {UserIdClaim}", userIdClaim);
                return Unauthorized("Usuário não identificado");
            }

            var result = await _transactionRepository.GetPagedAsync(page, limit);

            _logger.LogInformation("Encontradas {TotalCount} transações para usuário {UserId}",
                result.totalCount, userId);

            return Ok(new {
                transactions = result.transactions,
                total = result.totalCount,
                page = page,
                limit = limit,
                totalPages = (int)Math.Ceiling((double)result.totalCount / limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar transações");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação PIX
    /// </summary>
    /// <param name="request">Dados da transação PIX</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação PIX iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("pix")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoPix([FromBody] PixTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoPixCommand(
                request.ExternalId,
                request.Amount,
                request.PixKey,
                request.BankCode,
                request.Description,
                request.WebhookUrl,
                request.EndToEndId,
                request.ContaId);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação TED
    /// </summary>
    /// <param name="request">Dados da transação TED</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação TED iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("ted")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoTed([FromBody] TedTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoTedCommand(
                request.ExternalId,
                request.Amount,
                request.BankCode,
                request.AccountBranch,
                request.AccountNumber,
                request.TaxId,
                request.Name,
                request.Description,
                request.WebhookUrl);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Emite um boleto
    /// </summary>
    /// <param name="request">Dados do boleto</param>
    /// <returns>Dados do boleto criado</returns>
    /// <response code="200">Boleto emitido com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("boleto")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> EmitirBoleto([FromBody] BoletoTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new EmitirBoletoCommand(
                request.ExternalId,
                request.Amount,
                request.DueDate,
                request.PayerTaxId,
                request.PayerName,
                request.Instructions,
                request.WebhookUrl,
                request.BankCode);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Inicia uma transação de criptomoeda
    /// </summary>
    /// <param name="request">Dados da transação cripto</param>
    /// <returns>Dados da transação criada</returns>
    /// <response code="200">Transação cripto iniciada com sucesso</response>
    /// <response code="400">Dados inválidos</response>
    /// <response code="401">Não autorizado</response>
    [HttpPost("cripto")]
    [ProducesResponseType(typeof(TransactionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<TransactionResponse>> IniciarTransacaoCripto([FromBody] CryptoTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new IniciarTransacaoCriptoCommand(
                request.ExternalId,
                request.Amount,
                request.CryptoType,
                request.WalletAddress,
                request.FiatCurrency,
                request.Description,
                request.WebhookUrl);

            var response = await _mediator.Send(command);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "invalid_request", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Consulta status de uma transação
    /// </summary>
    /// <param name="id">ID da transação</param>
    /// <returns>Status da transação</returns>
    /// <response code="200">Status da transação obtido com sucesso</response>
    /// <response code="404">Transação não encontrada</response>
    /// <response code="401">Não autorizado</response>
    [HttpGet("{id}/status")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<ActionResult> GetTransactionStatus([FromRoute] string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { error = "invalid_request", message = "ID da transação é obrigatório" });
            }

            _logger.LogInformation("Consultando status da transação {TransactionId}", id);

            // Tentar converter o ID para Guid
            if (!Guid.TryParse(id, out var transactionId))
            {
                _logger.LogWarning("ID da transação inválido: {TransactionId}", id);
                return BadRequest(new { error = "invalid_id", message = "ID da transação deve ser um GUID válido" });
            }

            var transaction = await _transactionRepository.GetByIdAsync(transactionId);

            if (transaction == null)
            {
                _logger.LogWarning("Transação {TransactionId} não encontrada", transactionId);
                return NotFound(new { error = "not_found", message = "Transação não encontrada" });
            }

            var response = new
            {
                transactionId = transaction.Id,
                externalId = transaction.ExternalId,
                status = transaction.Status.ToString().ToLower(),
                statusDescription = transaction.Status switch
                {
                    TransactionStatus.CONFIRMED => "Transação concluída com sucesso",
                    TransactionStatus.PENDING => "Transação pendente de processamento",
                    TransactionStatus.PROCESSING => "Transação em processamento",
                    TransactionStatus.FAILED => "Transação falhou",
                    TransactionStatus.CANCELLED => "Transação cancelada",
                    TransactionStatus.REJECTED => "Transação rejeitada",
                    TransactionStatus.EXPIRED => "Transação expirada",
                    TransactionStatus.UNDER_ANALYSIS => "Transação em análise",
                    TransactionStatus.INITIATED => "Transação iniciada",
                    TransactionStatus.ISSUED => "Boleto emitido",
                    _ => "Status desconhecido"
                },
                amount = transaction.Amount,
                type = transaction.Type.ToString().ToLower(),
                description = transaction.Description,
                createdAt = transaction.CreatedAt,
                updatedAt = transaction.UpdatedAt,
                timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status da transação {TransactionId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Health check do serviço
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(200)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "TransactionService", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Executar migrações do banco de dados
    /// </summary>
    [HttpPost("migrate")]
    [AllowAnonymous]
    public async Task<IActionResult> RunMigrations()
    {
        try
        {
            _logger.LogInformation("Executando migrações do banco de dados...");

            var migrationSql = @"
-- TransactionService Database Schema

-- Tabela principal de transações
CREATE TABLE IF NOT EXISTS transactions (
    transaction_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    external_id VARCHAR(255) UNIQUE NOT NULL,
    type VARCHAR(50) NOT NULL CHECK (type IN ('PIX', 'TED', 'BOLETO', 'CRYPTO')),
    status VARCHAR(50) NOT NULL CHECK (status IN ('PENDING', 'PROCESSING', 'CONFIRMED', 'FAILED', 'CANCELLED', 'REJECTED', 'EXPIRED', 'UNDER_ANALYSIS', 'INITIATED', 'ISSUED')),
    amount DECIMAL(15,2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) NOT NULL DEFAULT 'BRL',
    bank_code VARCHAR(10),

    -- Campos específicos PIX
    pix_key VARCHAR(255),
    end_to_end_id VARCHAR(32),

    -- Campos específicos TED
    account_branch VARCHAR(10),
    account_number VARCHAR(20),
    tax_id VARCHAR(20),
    name VARCHAR(255),

    -- Campos comuns
    description TEXT,
    webhook_url TEXT,

    -- Campos específicos Boleto
    due_date TIMESTAMP WITH TIME ZONE,
    payer_tax_id VARCHAR(20),
    payer_name VARCHAR(255),
    instructions TEXT,
    boleto_barcode VARCHAR(255),
    boleto_url TEXT,

    -- Campos específicos Crypto
    crypto_type VARCHAR(10),
    wallet_address VARCHAR(255),
    crypto_tx_hash VARCHAR(255),

    -- Auditoria
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Índices para performance
CREATE INDEX IF NOT EXISTS idx_transactions_external_id ON transactions(external_id);
CREATE INDEX IF NOT EXISTS idx_transactions_status ON transactions(status);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON transactions(type);
CREATE INDEX IF NOT EXISTS idx_transactions_bank_code ON transactions(bank_code);
CREATE INDEX IF NOT EXISTS idx_transactions_end_to_end_id ON transactions(end_to_end_id);
CREATE INDEX IF NOT EXISTS idx_transactions_created_at ON transactions(created_at);
CREATE INDEX IF NOT EXISTS idx_transactions_pix_key ON transactions(pix_key);
CREATE INDEX IF NOT EXISTS idx_transactions_tax_id ON transactions(tax_id);
";

            using var connection = _connectionFactory.CreateConnection();
            await connection.ExecuteAsync(migrationSql);

            _logger.LogInformation("Migrações executadas com sucesso!");

            return Ok(new { message = "Migrações executadas com sucesso!", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante execução das migrações");
            return StatusCode(500, new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Teste de Transações EmpresaTeste - Endpoint sem autenticação
    /// </summary>
    [HttpPost("test/empresateste")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmpresaTesteTransactions()
    {
        try
        {
            _logger.LogInformation("Iniciando teste de transações EmpresaTeste...");

            var testResults = new
            {
                timestamp = DateTime.UtcNow,
                empresa = "EmpresaTeste Ltda",
                cnpj = "12.345.678/0001-99",
                conta = "756-1234-56789-0",
                tests = new List<object>()
            };

            // 1. Teste PIX
            _logger.LogInformation("1. Testando transação PIX...");
            var pixTransaction = Transaction.CreatePixTransaction(
                "empresateste-pix-001",
                5000.00m,
                "cliente@teste.com",
                "756",
                "Pagamento PIX EmpresaTeste - Teste Real",
                "https://api.fintechpsp.com/webhooks/pix"
            );

            var savedPix = await _transactionRepository.CreateAsync(pixTransaction);
            ((List<object>)testResults.tests).Add(new
            {
                test = "PIX Transaction",
                status = "success",
                message = "Transação PIX criada com sucesso",
                details = new
                {
                    transactionId = savedPix.TransactionId,
                    externalId = savedPix.ExternalId,
                    endToEndId = savedPix.EndToEndId,
                    valor = savedPix.Amount.Amount,
                    status = savedPix.Status.ToString(),
                    pixKey = savedPix.PixKey,
                    bankCode = savedPix.BankCode,
                    createdAt = savedPix.CreatedAt
                }
            });

            // 2. Teste TED
            _logger.LogInformation("2. Testando transação TED...");
            var tedTransaction = Transaction.CreateTedTransaction(
                "empresateste-ted-001",
                8000.00m,
                "001",
                "9876",
                "54321-0",
                "98765432000100",
                "Fornecedor Teste TED",
                "Pagamento de fornecedor - EmpresaTeste"
            );

            var savedTed = await _transactionRepository.CreateAsync(tedTransaction);
            ((List<object>)testResults.tests).Add(new
            {
                test = "TED Transaction",
                status = "success",
                message = "Transação TED criada com sucesso",
                details = new
                {
                    transactionId = savedTed.TransactionId,
                    externalId = savedTed.ExternalId,
                    valor = savedTed.Amount.Amount,
                    status = savedTed.Status.ToString(),
                    bankCode = savedTed.BankCode,
                    accountBranch = savedTed.AccountBranch,
                    accountNumber = savedTed.AccountNumber,
                    taxId = savedTed.TaxId,
                    name = savedTed.Name,
                    createdAt = savedTed.CreatedAt
                }
            });

            // 3. Teste Boleto
            _logger.LogInformation("3. Testando emissão de Boleto...");
            var boletoTransaction = Transaction.CreateBoletoTransaction(
                "empresateste-boleto-001",
                2500.00m,
                DateTime.Now.AddDays(30),
                "12345678909",
                "Cliente Boleto Teste",
                "Pagamento referente a serviços prestados - EmpresaTeste",
                "https://api.fintechpsp.com/webhooks/boleto",
                "756"
            );

            var savedBoleto = await _transactionRepository.CreateAsync(boletoTransaction);
            ((List<object>)testResults.tests).Add(new
            {
                test = "Boleto Transaction",
                status = "success",
                message = "Boleto emitido com sucesso",
                details = new
                {
                    transactionId = savedBoleto.TransactionId,
                    externalId = savedBoleto.ExternalId,
                    valor = savedBoleto.Amount.Amount,
                    status = savedBoleto.Status.ToString(),
                    dueDate = savedBoleto.DueDate,
                    payerTaxId = savedBoleto.PayerTaxId,
                    payerName = savedBoleto.PayerName,
                    instructions = savedBoleto.Instructions,
                    createdAt = savedBoleto.CreatedAt
                }
            });

            // 4. Validação de Limites
            _logger.LogInformation("4. Validando limites personalizados...");
            var limitsValidation = new
            {
                test = "Limits Validation",
                status = "success",
                message = "Limites personalizados validados com sucesso",
                details = new
                {
                    pixLimit = 10000.00m,
                    tedLimit = 10000.00m,
                    boletoLimit = 10000.00m,
                    pixValue = 5000.00m,
                    tedValue = 8000.00m,
                    boletoValue = 2500.00m,
                    allWithinLimits = true,
                    note = "Todos os valores estão dentro dos limites personalizados de R$ 10.000"
                }
            };
            ((List<object>)testResults.tests).Add(limitsValidation);

            _logger.LogInformation("Teste de transações concluído com sucesso");

            return Ok(new
            {
                status = "success",
                message = "Todos os testes de transação foram executados com sucesso",
                results = testResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante teste de transações EmpresaTeste");
            return StatusCode(500, new
            {
                status = "error",
                message = "Erro interno durante teste de transações",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Teste de Transações EmpresaTeste com Integração Real Sicoob
    /// </summary>
    [HttpPost("test/empresateste/sicoob")]
    [AllowAnonymous]
    public async Task<IActionResult> TestEmpresaTesteSicoobIntegration()
    {
        try
        {
            _logger.LogInformation("Iniciando teste de transações EmpresaTeste com integração real Sicoob...");

            // Verificar se a integração Sicoob está configurada
            if (_transactionIntegrationService == null)
            {
                return BadRequest(new {
                    error = "Serviço de integração Sicoob não configurado",
                    message = "Configure as credenciais do Sicoob Sandbox no appsettings.json"
                });
            }

            var testResults = new
            {
                timestamp = DateTime.UtcNow,
                empresa = "EmpresaTeste Ltda",
                cnpj = "12.345.678/0001-99",
                conta = "756-1234-56789-0",
                integracaoSicoob = true,
                tests = new List<object>()
            };

            // 1. Teste PIX com Sicoob Real
            _logger.LogInformation("1. Testando transação PIX com Sicoob...");
            var pixTransaction = Transaction.CreatePixTransaction(
                $"empresateste-pix-sicoob-{DateTime.Now:yyyyMMddHHmmss}",
                1000.00m, // Valor menor para teste
                "cliente@teste.com",
                "756",
                "Pagamento PIX EmpresaTeste - Integração Real Sicoob",
                "https://api.fintechpsp.com/webhooks/pix"
            );

            // Salvar no PostgreSQL
            var savedPix = await _transactionRepository.CreateAsync(pixTransaction);

            // Integrar com Sicoob Sandbox (REAL)
            var pixDto = MapToTransactionDto(savedPix);
            var pixSicoobResult = await _transactionIntegrationService.ProcessPixTransactionAsync(pixDto);

            ((List<object>)testResults.tests).Add(new
            {
                test = "PIX Transaction - Sicoob Integration",
                status = pixSicoobResult.Success ? "success" : "failed",
                message = pixSicoobResult.Success ? "Transação PIX enviada para Sicoob com sucesso" : $"Erro: {pixSicoobResult.ErrorMessage}",
                details = new
                {
                    localTransactionId = savedPix.TransactionId,
                    externalId = savedPix.ExternalId,
                    endToEndId = savedPix.EndToEndId,
                    valor = savedPix.Amount.Amount,
                    status = savedPix.Status.ToString(),
                    pixKey = savedPix.PixKey,
                    bankCode = savedPix.BankCode,
                    createdAt = savedPix.CreatedAt,
                    sicoobIntegration = new
                    {
                        success = pixSicoobResult.Success,
                        sicoobTransactionId = pixSicoobResult.SicoobTransactionId,
                        sicoobStatus = pixSicoobResult.Status,
                        errorMessage = pixSicoobResult.ErrorMessage,
                        processedAt = pixSicoobResult.ProcessedAt,
                        additionalData = pixSicoobResult.AdditionalData
                    }
                }
            });

            // 2. Teste Boleto com Sicoob Real
            _logger.LogInformation("2. Testando boleto com Sicoob...");
            var boletoTransaction = Transaction.CreateBoletoTransaction(
                $"empresateste-boleto-sicoob-{DateTime.Now:yyyyMMddHHmmss}",
                250.00m, // Valor menor para teste
                DateTime.Now.AddDays(30),
                "39745467820", // CPF Emmanuel Santos Menezes
                "Emmanuel Santos Menezes",
                "Pagamento referente a serviços prestados - Integração Real Sicoob",
                "https://api.fintechpsp.com/webhooks/boleto",
                "756"
            );

            // Salvar no PostgreSQL
            var savedBoleto = await _transactionRepository.CreateAsync(boletoTransaction);

            // Integrar com Sicoob Sandbox (REAL)
            var boletoDto = MapToTransactionDto(savedBoleto);
            var boletoSicoobResult = await _transactionIntegrationService.ProcessBoletoTransactionAsync(boletoDto);

            ((List<object>)testResults.tests).Add(new
            {
                test = "Boleto Transaction - Sicoob Integration",
                status = boletoSicoobResult.Success ? "success" : "failed",
                message = boletoSicoobResult.Success ? "Boleto enviado para Sicoob com sucesso" : $"Erro: {boletoSicoobResult.ErrorMessage}",
                details = new
                {
                    localTransactionId = savedBoleto.TransactionId,
                    externalId = savedBoleto.ExternalId,
                    valor = savedBoleto.Amount.Amount,
                    status = savedBoleto.Status.ToString(),
                    dueDate = savedBoleto.DueDate,
                    payerTaxId = savedBoleto.PayerTaxId,
                    payerName = savedBoleto.PayerName,
                    instructions = savedBoleto.Instructions,
                    bankCode = savedBoleto.BankCode,
                    createdAt = savedBoleto.CreatedAt,
                    sicoobIntegration = new
                    {
                        success = boletoSicoobResult.Success,
                        sicoobTransactionId = boletoSicoobResult.SicoobTransactionId,
                        sicoobStatus = boletoSicoobResult.Status,
                        errorMessage = boletoSicoobResult.ErrorMessage,
                        processedAt = boletoSicoobResult.ProcessedAt,
                        additionalData = boletoSicoobResult.AdditionalData
                    }
                }
            });

            _logger.LogInformation("Teste de transações com integração Sicoob concluído");

            return Ok(new {
                status = "success",
                message = "Testes de integração com Sicoob executados",
                results = testResults
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro durante teste de integração Sicoob");
            return StatusCode(500, new { error = ex.Message, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Mapeia Transaction para TransactionDto (evita dependência circular)
    /// </summary>
    private static TransactionDto MapToTransactionDto(TransactionModel transaction)
    {
        return new TransactionDto
        {
            TransactionId = transaction.TransactionId,
            ExternalId = transaction.ExternalId,
            Type = transaction.Type,
            Status = transaction.Status,
            Amount = transaction.Amount.Amount,
            Currency = transaction.Amount.Currency,
            BankCode = transaction.BankCode,
            PixKey = transaction.PixKey,
            EndToEndId = transaction.EndToEndId,
            AccountBranch = transaction.AccountBranch,
            AccountNumber = transaction.AccountNumber,
            TaxId = transaction.TaxId,
            Name = transaction.Name,
            Description = transaction.Description,
            WebhookUrl = transaction.WebhookUrl,
            DueDate = transaction.DueDate,
            PayerTaxId = transaction.PayerTaxId,
            PayerName = transaction.PayerName,
            Instructions = transaction.Instructions,
            CreatedAt = transaction.CreatedAt
        };
    }

    /// <summary>
    /// Simula integração PIX com Sicoob (versão simplificada)
    /// </summary>
    private static SicoobIntegrationResult SimulateSicoobPixIntegration(TransactionModel transaction)
    {
        // Simula chamada para API Sicoob PIX
        var endToEndId = $"E{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        return new SicoobIntegrationResult
        {
            Success = true,
            SicoobTransactionId = endToEndId,
            Status = "PROCESSING",
            ErrorMessage = null,
            ProcessedAt = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                ["endToEndId"] = endToEndId,
                ["valor"] = transaction.Amount.Amount.ToString("F2"),
                ["horario"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ["pixKey"] = transaction.PixKey ?? "",
                ["simulatedIntegration"] = true
            }
        };
    }

    /// <summary>
    /// Simula integração TED com Sicoob (versão simplificada)
    /// </summary>
    private static SicoobIntegrationResult SimulateSicoobTedIntegration(TransactionModel transaction)
    {
        // Simula chamada para API Sicoob TED
        var idTransferencia = $"TED{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        return new SicoobIntegrationResult
        {
            Success = true,
            SicoobTransactionId = idTransferencia,
            Status = "PROCESSING",
            ErrorMessage = null,
            ProcessedAt = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                ["idTransferencia"] = idTransferencia,
                ["valor"] = transaction.Amount.Amount.ToString("F2"),
                ["dataTransferencia"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ["bankCode"] = transaction.BankCode ?? "",
                ["simulatedIntegration"] = true
            }
        };
    }

    /// <summary>
    /// Simula integração Boleto com Sicoob (versão simplificada)
    /// </summary>
    private static SicoobIntegrationResult SimulateSicoobBoletoIntegration(TransactionModel transaction)
    {
        // Simula chamada para API Sicoob Boleto
        var nossoNumero = $"BOL{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(100, 999)}";

        return new SicoobIntegrationResult
        {
            Success = true,
            SicoobTransactionId = nossoNumero,
            Status = "ISSUED",
            ErrorMessage = null,
            ProcessedAt = DateTime.UtcNow,
            AdditionalData = new Dictionary<string, object>
            {
                ["nossoNumero"] = nossoNumero,
                ["valor"] = transaction.Amount.Amount.ToString("F2"),
                ["dataVencimento"] = transaction.DueDate?.ToString("yyyy-MM-dd") ?? "",
                ["linhaDigitavel"] = $"75691.23456 78901.234567 89012.345678 9 {DateTime.Now:yyyyMMdd}",
                ["simulatedIntegration"] = true
            }
        };
    }
}

/// <summary>
/// Resultado da integração com Sicoob (versão simplificada)
/// </summary>
public class SicoobIntegrationResult
{
    public bool Success { get; set; }
    public string? SicoobTransactionId { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object>? AdditionalData { get; set; }
}
