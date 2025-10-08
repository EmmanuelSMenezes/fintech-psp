using System;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.BalanceService.Repositories;
using FintechPSP.BalanceService.Models;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FintechPSP.BalanceService.Consumers;

/// <summary>
/// Consumer para processar eventos de PIX confirmado e atualizar saldo
/// </summary>
public class PixConfirmadoConsumer : IConsumer<PixConfirmado>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<PixConfirmadoConsumer> _logger;

    public PixConfirmadoConsumer(
        IAccountRepository accountRepository,
        ITransactionHistoryRepository transactionHistoryRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<PixConfirmadoConsumer> logger)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _transactionHistoryRepository = transactionHistoryRepository ?? throw new ArgumentNullException(nameof(transactionHistoryRepository));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PixConfirmado> context)
    {
        var evento = context.Message;
        
        _logger.LogInformation("💰 Processando PIX confirmado - TxId: {TxId}, Valor: R$ {Amount}, Pagador: {PayerName}", 
            evento.TxId, evento.Amount, evento.PayerName);

        try
        {
            // 1. Buscar a transação original pelo TxId para obter o ClientId
            // Primeiro tenta buscar pelo TxId exato
            _logger.LogInformation("🔍 Buscando transação existente para TxId: {TxId}", evento.TxId);

            TransactionHistory? transactionHistory = null;
            if (!string.IsNullOrEmpty(evento.TxId))
            {
                transactionHistory = await _transactionHistoryRepository.GetByExternalIdAsync(evento.TxId);
            }
            _logger.LogInformation("🔍 Transação encontrada: {Found}", transactionHistory != null);

            // Se não encontrar, pode ser que o TxId seja diferente do ExternalId
            // Vamos buscar por transações QR_CODE_GENERATED recentes que ainda não foram confirmadas
            if (transactionHistory == null)
            {
                _logger.LogInformation("🔍 TxId {TxId} não encontrado diretamente, buscando QR Code relacionado...", evento.TxId);

                // Por enquanto, vamos usar um ClientId padrão para teste
                // TODO: Implementar busca mais inteligente baseada em valor e timestamp
                var defaultClientId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"); // Cliente de teste

                // Buscar conta do cliente padrão
                var defaultAccount = await _accountRepository.GetByClientIdAsync(defaultClientId);
                if (defaultAccount == null)
                {
                    _logger.LogError("❌ Conta padrão não encontrada para cliente: {ClientId}", defaultClientId);
                    return;
                }

                // Criar entrada de histórico para o PIX recebido
                var newTransactionId = Guid.NewGuid();
                transactionHistory = new TransactionHistory
                {
                    TransactionId = newTransactionId,
                    ClientId = defaultClientId,
                    AccountId = defaultAccount.AccountId, // ✅ Usar AccountId da conta encontrada
                    ExternalId = evento.TxId,
                    Type = "PIX_RECEIVED",
                    Amount = evento.Amount,
                    Description = $"PIX recebido de {evento.PayerName ?? "N/A"}",
                    Status = "PENDING",
                    Operation = "CREDIT",
                    CreatedAt = evento.ConfirmedAt
                };

                _logger.LogInformation("💾 Salvando nova transação - ID: {TransactionId}, AccountId: {AccountId}",
                    transactionHistory.TransactionId, transactionHistory.AccountId);
                await _transactionHistoryRepository.AddTransactionAsync(transactionHistory);
                _logger.LogInformation("✅ Transação salva com sucesso");
                _logger.LogInformation("📝 Nova entrada de histórico criada para PIX recebido - TxId: {TxId}", evento.TxId);
            }

            if (transactionHistory == null)
            {
                _logger.LogWarning("⚠️ Não foi possível processar PIX para TxId: {TxId}", evento.TxId);
                return;
            }

            // 2. Buscar a conta do cliente (pode já ter sido buscada acima)
            var account = await _accountRepository.GetByClientIdAsync(transactionHistory.ClientId);
            if (account == null)
            {
                _logger.LogError("❌ Conta não encontrada para cliente: {ClientId}", transactionHistory.ClientId);
                return;
            }

            // 3. Creditar o valor na conta
            var oldBalance = account.AvailableBalance.Amount;
            account.Credit(evento.Amount, $"PIX recebido - TxId: {evento.TxId}", transactionHistory.TransactionId.ToString());
            
            // 4. Salvar a conta atualizada
            await _accountRepository.UpdateAsync(account);

            // 5. Atualizar o histórico da transação
            await _transactionHistoryRepository.UpdateTransactionStatusAsync(
                transactionHistory.TransactionId,
                "CONFIRMED",
                $"PIX confirmado - Pagador: {evento.PayerName ?? "N/A"}");

            // 6. Publicar evento de saldo creditado
            var saldoCreditadoEvent = new SaldoCreditado(
                account.ClientId,
                account.AccountId,
                evento.Amount,
                oldBalance,
                account.AvailableBalance.Amount,
                $"PIX recebido de {evento.PayerName ?? "N/A"}",
                transactionHistory.TransactionId.ToString()
            );

            await _publishEndpoint.Publish(saldoCreditadoEvent);

            _logger.LogInformation("✅ PIX processado com sucesso - Cliente: {ClientId}, Valor: R$ {Amount}, Saldo anterior: R$ {OldBalance}, Novo saldo: R$ {NewBalance}", 
                account.ClientId, evento.Amount, oldBalance, account.AvailableBalance.Amount);

            // 7. Publicar evento de notificação para o frontend
            var notificacaoEvent = new NotificacaoPixRecebido
            {
                ClientId = account.ClientId,
                Amount = evento.Amount,
                PayerName = evento.PayerName ?? "Pagador não identificado",
                PayerDocument = evento.PayerDocument,
                TxId = evento.TxId,
                NewBalance = account.AvailableBalance.Amount,
                ReceivedAt = evento.ConfirmedAt
            };

            await _publishEndpoint.Publish(notificacaoEvent);

            _logger.LogInformation("🔔 Notificação de PIX recebido enviada para cliente: {ClientId}", account.ClientId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Erro ao processar PIX confirmado - TxId: {TxId}", evento.TxId);
            throw; // Re-throw para que o MassTransit possa fazer retry
        }
    }
}
