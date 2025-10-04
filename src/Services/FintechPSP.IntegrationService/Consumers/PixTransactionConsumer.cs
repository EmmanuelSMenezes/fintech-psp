using System;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.IntegrationService.Services.Sicoob.Pix;
using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FintechPSP.IntegrationService.Consumers;

/// <summary>
/// Consumer para processar eventos de transação PIX e integrar com o Sicoob
/// </summary>
public class PixTransactionConsumer : IConsumer<PixIniciado>
{
    private readonly IPixPagamentosService _pixPagamentosService;
    private readonly ILogger<PixTransactionConsumer> _logger;

    public PixTransactionConsumer(
        IPixPagamentosService pixPagamentosService,
        ILogger<PixTransactionConsumer> logger)
    {
        _pixPagamentosService = pixPagamentosService ?? throw new ArgumentNullException(nameof(pixPagamentosService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PixIniciado> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation("🎯 Processando evento PixIniciado - TransactionId: {TransactionId}, ExternalId: {ExternalId}, Valor: {Amount}, Chave: {PixKey}, Banco: {BankCode}", 
            evento.TransactionId, evento.ExternalId, evento.Amount, evento.PixKey, evento.BankCode);

        try
        {
            // Verificar se é para o Sicoob (código 756)
            if (evento.BankCode != "756")
            {
                _logger.LogInformation("⚠️ Transação não é para Sicoob (BankCode: {BankCode}). Ignorando.", evento.BankCode);
                return;
            }

            // Criar request para o Sicoob
            var pixRequest = new PixPagamentoRequest
            {
                Valor = evento.Amount.ToString("F2"),
                Pagador = new PagadorPix
                {
                    Nome = "EmpresaTeste Ltda", // TODO: Buscar dados reais da empresa
                    Cnpj = "12345678000199", // CNPJ da EmpresaTeste
                    ContaCorrente = "12345" // TODO: Buscar conta real
                },
                Favorecido = new FavorecidoPix
                {
                    Nome = "Destinatário PIX", // TODO: Resolver nome do destinatário
                    Chave = evento.PixKey
                },
                InfoPagador = evento.Description ?? "Pagamento PIX via FintechPSP"
            };

            _logger.LogInformation("🏦 Enviando pagamento PIX para Sicoob - Valor: R$ {Valor}, Chave: {Chave}", 
                pixRequest.Valor, pixRequest.Favorecido.Chave);

            // Realizar pagamento no Sicoob
            var response = await _pixPagamentosService.RealizarPagamentoPixAsync(pixRequest);

            if (response != null)
            {
                _logger.LogInformation("✅ Pagamento PIX realizado com sucesso no Sicoob - E2E: {EndToEndId}, TxId: {Txid}, Status: {Status}",
                    response.EndToEndId, response.Txid, response.Status);

                // TODO: Publicar evento de confirmação de volta para o TransactionService
                // Exemplo: PixConfirmado, TransacaoProcessando, etc.
            }
            else
            {
                _logger.LogError("❌ Falha ao processar pagamento PIX no Sicoob - TransactionId: {TransactionId}", 
                    evento.TransactionId);

                // TODO: Publicar evento de erro de volta para o TransactionService
                // Exemplo: PixFalhou, TransacaoFalhou, etc.
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Erro ao processar evento PixIniciado - TransactionId: {TransactionId}", 
                evento.TransactionId);

            // TODO: Implementar retry policy ou dead letter queue
            throw; // Re-throw para que o MassTransit possa fazer retry
        }
    }
}
