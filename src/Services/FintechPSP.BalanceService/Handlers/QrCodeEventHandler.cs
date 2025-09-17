using FintechPSP.BalanceService.Repositories;
using FintechPSP.Shared.Domain.Events;
using MassTransit;

namespace FintechPSP.BalanceService.Handlers;

/// <summary>
/// Handler para eventos de QR Code PIX
/// </summary>
public class QrCodeEventHandler : 
    IConsumer<QrCodeGerado>,
    IConsumer<QrCodeExpirado>
{
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;
    private readonly ILogger<QrCodeEventHandler> _logger;

    public QrCodeEventHandler(
        ITransactionHistoryRepository transactionHistoryRepository,
        ILogger<QrCodeEventHandler> logger)
    {
        _transactionHistoryRepository = transactionHistoryRepository ?? throw new ArgumentNullException(nameof(transactionHistoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processa evento de QR Code gerado
    /// </summary>
    public async Task Consume(ConsumeContext<QrCodeGerado> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation("Processando evento QrCodeGerado para transactionId {TransactionId}, tipo {Type}", 
            @event.TransactionId, @event.Type);

        try
        {
            // Criar entrada no histórico de transações para QR Code gerado
            var transactionHistory = new
            {
                Id = Guid.NewGuid(),
                TransactionId = @event.TransactionId,
                ExternalId = @event.ExternalId,
                Type = "QR_CODE_GENERATED",
                SubType = @event.Type.ToUpper(), // "STATIC" ou "DYNAMIC"
                Amount = @event.Amount ?? 0m,
                Currency = "BRL",
                Status = "ACTIVE",
                Description = $"QR Code {(@event.Type == "static" ? "estático" : "dinâmico")} gerado - {(@event.Description ?? "Sem descrição")}",
                PixKey = @event.PixKey,
                BankCode = @event.BankCode,
                QrcodePayload = @event.QrcodePayload,
                ExpiresAt = @event.ExpiresAt,
                CreatedAt = @event.OccurredAt,
                UpdatedAt = @event.OccurredAt,
                Metadata = new Dictionary<string, object>
                {
                    ["qrCodeType"] = @event.Type,
                    ["pixKey"] = @event.PixKey,
                    ["bankCode"] = @event.BankCode,
                    ["hasImage"] = !string.IsNullOrEmpty(@event.QrcodeImage),
                    ["payloadLength"] = @event.QrcodePayload.Length,
                    ["isExpirable"] = @event.ExpiresAt.HasValue
                }
            };

            await _transactionHistoryRepository.AddTransactionHistoryAsync(transactionHistory);

            _logger.LogInformation("QR Code {Type} registrado no histórico para transactionId {TransactionId}", 
                @event.Type, @event.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento QrCodeGerado para transactionId {TransactionId}", 
                @event.TransactionId);
            throw;
        }
    }

    /// <summary>
    /// Processa evento de QR Code expirado
    /// </summary>
    public async Task Consume(ConsumeContext<QrCodeExpirado> context)
    {
        var @event = context.Message;
        
        _logger.LogInformation("Processando evento QrCodeExpirado para transactionId {TransactionId}", 
            @event.TransactionId);

        try
        {
            // Atualizar status do QR Code para expirado
            await _transactionHistoryRepository.UpdateTransactionStatusAsync(
                @event.TransactionId, 
                "EXPIRED", 
                "QR Code dinâmico expirado");

            // Criar entrada adicional no histórico para o evento de expiração
            var expirationHistory = new
            {
                Id = Guid.NewGuid(),
                TransactionId = @event.TransactionId,
                ExternalId = @event.ExternalId,
                Type = "QR_CODE_EXPIRED",
                SubType = "DYNAMIC",
                Amount = 0m,
                Currency = "BRL",
                Status = "EXPIRED",
                Description = "QR Code dinâmico expirado automaticamente",
                QrcodePayload = @event.QrcodePayload,
                CreatedAt = @event.OccurredAt,
                UpdatedAt = @event.OccurredAt,
                Metadata = new Dictionary<string, object>
                {
                    ["expiredAt"] = @event.OccurredAt,
                    ["reason"] = "automatic_expiration",
                    ["originalTransactionId"] = @event.TransactionId
                }
            };

            await _transactionHistoryRepository.AddTransactionHistoryAsync(expirationHistory);

            _logger.LogInformation("QR Code expirado registrado no histórico para transactionId {TransactionId}", 
                @event.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento QrCodeExpirado para transactionId {TransactionId}", 
                @event.TransactionId);
            throw;
        }
    }
}
