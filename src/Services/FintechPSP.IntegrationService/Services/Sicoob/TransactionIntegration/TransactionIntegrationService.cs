using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using FintechPSP.IntegrationService.Models.Sicoob.SPB;
using FintechPSP.IntegrationService.Models.Sicoob.CobrancaBancaria;
using FintechPSP.IntegrationService.Services.Sicoob.Pix;
using FintechPSP.IntegrationService.Services.Sicoob.SPB;
using FintechPSP.IntegrationService.Services.Sicoob.CobrancaBancaria;
using FintechPSP.Shared.Domain.Enums;

namespace FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration;

/// <summary>
/// Serviço para integração de transações com o Sicoob
/// </summary>
public class TransactionIntegrationService : ITransactionIntegrationService
{
    private readonly IPixPagamentosService _pixService;
    private readonly IPixRecebimentosService _pixRecebimentosService;
    private readonly IPixQrCodeService _pixQrCodeService;
    private readonly ICobrancaBancariaService _cobrancaBancariaService;
    private readonly ISPBService _spbService;
    private readonly ILogger<TransactionIntegrationService> _logger;
    private readonly SicoobSettings _settings;

    public TransactionIntegrationService(
        IPixPagamentosService pixService,
        IPixRecebimentosService pixRecebimentosService,
        IPixQrCodeService pixQrCodeService,
        ICobrancaBancariaService cobrancaBancariaService,
        ISPBService spbService,
        IOptions<SicoobSettings> settings,
        ILogger<TransactionIntegrationService> logger)
    {
        _pixService = pixService ?? throw new ArgumentNullException(nameof(pixService));
        _pixRecebimentosService = pixRecebimentosService ?? throw new ArgumentNullException(nameof(pixRecebimentosService));
        _pixQrCodeService = pixQrCodeService ?? throw new ArgumentNullException(nameof(pixQrCodeService));
        _cobrancaBancariaService = cobrancaBancariaService ?? throw new ArgumentNullException(nameof(cobrancaBancariaService));
        _spbService = spbService ?? throw new ArgumentNullException(nameof(spbService));
        _settings = settings.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SicoobTransactionResult> ProcessPixTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Criando cobrança PIX (QR Code dinâmico) no Sicoob: {ExternalId}", transaction.ExternalId);

            // Criar request de cobrança PIX (QR Code dinâmico) - usando CNPJ OWAYPAY como chave PIX
            var cobrancaRequest = new CobrancaImediataRequest
            {
                Calendario = new CalendarioCobranca
                {
                    Expiracao = 3600 // 1 hora
                },
                Valor = new ValorCobranca
                {
                    Original = transaction.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                },
                Chave = "a59b3ad1-c78a-4382-9216-01376298b153", // Chave PIX aleatória OWAYPAY
                SolicitacaoPagador = $"Cobrança PIX R$ {transaction.Amount:F2} - Emmanuel Santos Menezes"
            };

            // Criar cobrança no Sicoob
            var response = await _pixRecebimentosService.CriarCobrancaImediataAsync(cobrancaRequest, cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Cobrança PIX criada com sucesso no Sicoob. TxId: {TxId}, Status: {Status}",
                    response.TxId, response.Status);

                // Consultar a cobrança para obter o QR Code e dados completos
                var cobrancaCompleta = await _pixRecebimentosService.ConsultarCobrancaAsync(response.TxId!, cancellationToken);

                if (cobrancaCompleta != null)
                {
                    _logger.LogInformation("Dados completos da cobrança obtidos. Location: {Location}, QrCode: {QrCode}, PixCopiaECola: {PixCopiaECola}",
                        cobrancaCompleta.Location ?? cobrancaCompleta.Loc?.Location ?? "N/A",
                        string.IsNullOrEmpty(cobrancaCompleta.QrCode) ? "VAZIO" : "PRESENTE",
                        string.IsNullOrEmpty(cobrancaCompleta.PixCopiaECola) ? "VAZIO" : "PRESENTE");

                    // Usar os dados da consulta que são mais completos
                    response = cobrancaCompleta;

                    // Obter QR Code do Sicoob usando o loc.id
                    if (response.Loc?.Id > 0)
                    {
                        try
                        {
                            var qrCodeResponse = await _pixRecebimentosService.ConsultarQrCodeAsync(response.Loc.Id, cancellationToken);

                            if (qrCodeResponse != null)
                            {
                                _logger.LogInformation("QR Code PIX obtido do Sicoob com sucesso. PIX Copia e Cola: {Tamanho} caracteres",
                                    qrCodeResponse.QrCode?.Length ?? 0);

                                // Atualizar response com os dados do QR Code do Sicoob
                                response.QrCode = qrCodeResponse.ImagemQrCode ?? "";
                                response.PixCopiaECola = qrCodeResponse.QrCode ?? "";
                            }
                            else
                            {
                                _logger.LogWarning("QR Code não retornado pelo Sicoob para Location ID: {LocationId}", response.Loc.Id);
                            }
                        }
                        catch (Exception qrEx)
                        {
                            _logger.LogWarning(qrEx, "Erro ao obter QR Code do Sicoob para Location ID: {LocationId}, tentando gerar localmente", response.Loc.Id);

                            // Fallback: gerar QR Code localmente
                            try
                            {
                                var qrCodeResult = await _pixQrCodeService.GerarQrCodeAsync(response);
                                response.QrCode = qrCodeResult.QrCodeBase64;
                                response.PixCopiaECola = qrCodeResult.PixCopiaECola;
                                _logger.LogInformation("QR Code PIX gerado localmente como fallback");
                            }
                            catch (Exception fallbackEx)
                            {
                                _logger.LogWarning(fallbackEx, "Erro ao gerar QR Code PIX localmente, continuando sem QR Code");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Location ID não disponível, tentando gerar QR Code localmente");

                        // Gerar QR Code localmente se não tiver loc.id
                        try
                        {
                            var qrCodeResult = await _pixQrCodeService.GerarQrCodeAsync(response);
                            response.QrCode = qrCodeResult.QrCodeBase64;
                            response.PixCopiaECola = qrCodeResult.PixCopiaECola;
                            _logger.LogInformation("QR Code PIX gerado localmente");
                        }
                        catch (Exception qrEx)
                        {
                            _logger.LogWarning(qrEx, "Erro ao gerar QR Code PIX localmente, continuando sem QR Code");
                        }
                    }
                }

                return new SicoobTransactionResult
                {
                    Success = true,
                    SicoobTransactionId = response.TxId,
                    Status = response.Status,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["txId"] = response.TxId ?? "",
                        ["qrCode"] = response.QrCode ?? "",
                        ["pixCopiaECola"] = response.PixCopiaECola ?? "",
                        ["location"] = response.Location ?? response.Loc?.Location ?? "",
                        ["valor"] = response.Valor?.Original ?? "",
                        ["status"] = response.Status ?? "",
                        ["chave"] = response.Chave ?? "",
                        ["revisao"] = response.Revisao,
                        ["calendario"] = response.Calendario != null ? new Dictionary<string, object>
                        {
                            ["criacao"] = response.Calendario.Criacao,
                            ["expiracao"] = response.Calendario.Expiracao
                        } : null,
                        ["loc"] = response.Loc != null ? new Dictionary<string, object>
                        {
                            ["id"] = response.Loc.Id,
                            ["location"] = response.Loc.Location ?? "",
                            ["tipoCob"] = response.Loc.TipoCob ?? ""
                        } : null,
                        ["recebedor"] = response.Recebedor != null ? new Dictionary<string, object>
                        {
                            ["nome"] = response.Recebedor.Nome ?? "",
                            ["cnpj"] = response.Recebedor.Cnpj ?? "",
                            ["cidade"] = response.Recebedor.Cidade ?? "",
                            ["uf"] = response.Recebedor.Uf ?? ""
                        } : null
                    }
                };
            }

            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = "Resposta vazia do Sicoob"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar PIX no Sicoob: {ExternalId}", transaction.ExternalId);
            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "PIX_PROCESSING_ERROR"
            };
        }
    }

    public async Task<SicoobTransactionResult> ProcessTedTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processando transação TED no Sicoob: {ExternalId}", transaction.ExternalId);

            // Mapear TransactionDto para TEDRequest
            var tedRequest = new TEDRequest
            {
                Valor = transaction.Amount.ToString("F2"),
                ContaOrigem = new ContaTED
                {
                    Banco = "756", // Sicoob
                    Agencia = "1234",
                    Conta = "56789",
                    Digito = "0",
                    TipoConta = "CC",
                    Titular = new TitularConta
                    {
                        Cnpj = "12345678000199",
                        Nome = "EmpresaTeste Ltda"
                    }
                },
                ContaDestino = new ContaTED
                {
                    Banco = transaction.BankCode ?? "",
                    Agencia = transaction.AccountBranch ?? "",
                    Conta = transaction.AccountNumber ?? "",
                    TipoConta = "CC",
                    Titular = new TitularConta
                    {
                        Nome = transaction.Name ?? "",
                        Cpf = transaction.TaxId?.Length == 11 ? transaction.TaxId : null,
                        Cnpj = transaction.TaxId?.Length == 14 ? transaction.TaxId : null
                    }
                },
                Finalidade = "1",
                CodigoFinalidade = "01",
                Descricao = transaction.Description
            };

            // Enviar para o Sicoob
            var response = await _spbService.RealizarTEDAsync(tedRequest, cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("TED processada com sucesso no Sicoob. ID: {IdTransferencia}", response.IdTransferencia);

                return new SicoobTransactionResult
                {
                    Success = true,
                    SicoobTransactionId = response.IdTransferencia,
                    Status = response.Situacao,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["idTransferencia"] = response.IdTransferencia ?? "",
                        ["valor"] = response.Valor.ToString("F2"),
                        ["dataTransferencia"] = response.DataTransferencia ?? "",
                        ["numeroDocumento"] = response.NumeroDocumento ?? ""
                    }
                };
            }

            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = "Resposta vazia do Sicoob"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar TED no Sicoob: {ExternalId}", transaction.ExternalId);
            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "TED_PROCESSING_ERROR"
            };
        }
    }

    public async Task<SicoobTransactionResult> ProcessBoletoTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Criando boleto bancário no Sicoob: {ExternalId}", transaction.ExternalId);

            // Criar request de boleto bancário
            var boletoRequest = new BoletoRequest
            {
                NumeroCliente = 25546454, // Número do cliente no Sicoob (exemplo do Postman)
                CodigoModalidade = 1, // Simples
                NumeroContaCorrente = 0, // Conta corrente (exemplo do Postman)
                CodigoEspecieDocumento = "DM", // Duplicata Mercantil
                DataEmissao = DateTime.Now.ToString("yyyy-MM-dd"),
                SeuNumero = transaction.ExternalId,
                IdentificacaoBoletoEmpresa = transaction.ExternalId,
                IdentificacaoEmissaoBoleto = 2, // Banco emite
                IdentificacaoDistribuicaoBoleto = 2, // Cliente via internet
                Valor = transaction.Amount,
                DataVencimento = DateTime.Now.AddDays(30).ToString("yyyy-MM-dd"), // 30 dias
                Pagador = new PagadorBoleto
                {
                    NumeroCpfCnpj = "39745467820", // CPF Emmanuel Santos Menezes
                    Nome = "Emmanuel Santos Menezes",
                    Endereco = "Rua Exemplo, 123",
                    Bairro = "Centro",
                    Cidade = "São Paulo",
                    Cep = "01234567",
                    Uf = "SP",
                    Email = "emmanuel@exemplo.com"
                    // Telefone = "(11) 99999-9999"
                },
                // NumeroContratoCobranca = 1,
                Aceite = true,
                NumeroParcela = 1,
                MensagensInstrucao = new List<string>
                {
                    "Pagamento referente a serviços prestados",
                    "Após o vencimento cobrar multa de 2%",
                    "Após o vencimento cobrar juros de 1% ao mês"
                }
            };

            // Criar boleto no Sicoob
            var response = await _cobrancaBancariaService.IncluirBoletoAsync(boletoRequest, cancellationToken);

            if (response != null)
            {
                _logger.LogInformation("Boleto criado com sucesso no Sicoob. NossoNumero: {NossoNumero}, CodigoBarras: {CodigoBarras}",
                    response.NossoNumero,
                    string.IsNullOrEmpty(response.CodigoBarras) ? "N/A" : "PRESENTE");

                return new SicoobTransactionResult
                {
                    Success = true,
                    SicoobTransactionId = response.NossoNumero,
                    Status = response.Situacao ?? "ATIVO",
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["nossoNumero"] = response.NossoNumero,
                        ["codigoBarras"] = response.CodigoBarras ?? "",
                        ["linhaDigitavel"] = response.LinhaDigitavel ?? "",
                        ["valor"] = response.Valor,
                        ["dataVencimento"] = response.DataVencimento,
                        ["situacao"] = response.Situacao ?? "",
                        ["urlBoleto"] = response.UrlBoleto ?? "",
                        ["pagador"] = new Dictionary<string, object>
                        {
                            ["nome"] = response.Pagador.Nome,
                            ["documento"] = response.Pagador.NumeroCpfCnpj,
                            ["email"] = response.Pagador.Email ?? ""
                            // ["telefone"] = response.Pagador.Telefone ?? ""
                        }
                    }
                };
            }

            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = "Resposta vazia do Sicoob"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar boleto no Sicoob: {ExternalId}", transaction.ExternalId);
            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "BOLETO_PROCESSING_ERROR"
            };
        }
    }

    public async Task<SicoobTransactionResult> GetTransactionStatusAsync(string externalId, string transactionType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Consultando status da transação no Sicoob: {ExternalId}, Tipo: {Type}", externalId, transactionType);

            // Implementar consulta baseada no tipo
            switch (transactionType.ToUpper())
            {
                case "PIX":
                    // Consultar PIX por E2E ID
                    // var pixResponse = await _pixService.ConsultarPagamentoAsync(externalId, cancellationToken);
                    break;
                case "TED":
                    // Consultar TED por número do documento
                    // var tedResponse = await _spbService.ConsultarTEDAsync(externalId, cancellationToken);
                    break;
                case "BOLETO":
                    // Consultar boleto
                    break;
            }

            // Por enquanto, retorna status genérico
            return new SicoobTransactionResult
            {
                Success = true,
                Status = "PROCESSING"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar status no Sicoob: {ExternalId}", externalId);
            return new SicoobTransactionResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ErrorCode = "STATUS_QUERY_ERROR"
            };
        }
    }
}
