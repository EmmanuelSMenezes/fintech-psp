using FintechPSP.Shared.Domain.Common;
using FintechPSP.Shared.Domain.Events;

namespace FintechPSP.TransactionService.Models;

/// <summary>
/// Agregado para QR Code PIX
/// </summary>
public class QrCode : AggregateRoot
{
    public string ExternalId { get; private set; } = string.Empty;
    public Guid TransactionId { get; private set; }
    public string PixKey { get; private set; } = string.Empty;
    public decimal? Amount { get; private set; }
    public string QrcodePayload { get; private set; } = string.Empty;
    public string QrcodeImage { get; private set; } = string.Empty;
    public string Type { get; private set; } = string.Empty; // "static" ou "dynamic"
    public DateTime? ExpiresAt { get; private set; }
    public string BankCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    private QrCode() { }

    public static QrCode CreateStatic(string externalId, Guid transactionId, string pixKey, 
        string bankCode, string qrcodePayload, string qrcodeImage, string? description = null)
    {
        var qrCode = new QrCode();
        
        var @event = new QrCodeGerado(
            transactionId,
            externalId,
            pixKey,
            null, // QR estático não tem valor fixo
            qrcodePayload,
            qrcodeImage,
            "static",
            null, // QR estático não expira
            bankCode,
            description);

        qrCode.ApplyEvent(@event);
        return qrCode;
    }

    public static QrCode CreateDynamic(string externalId, Guid transactionId, string pixKey, 
        decimal amount, string bankCode, string qrcodePayload, string qrcodeImage, 
        DateTime expiresAt, string? description = null)
    {
        var qrCode = new QrCode();
        
        var @event = new QrCodeGerado(
            transactionId,
            externalId,
            pixKey,
            amount,
            qrcodePayload,
            qrcodeImage,
            "dynamic",
            expiresAt,
            bankCode,
            description);

        qrCode.ApplyEvent(@event);
        return qrCode;
    }

    public void MarkAsExpired()
    {
        if (Type != "dynamic")
            throw new InvalidOperationException("Apenas QR Codes dinâmicos podem expirar");

        if (IsExpired)
            return; // Já expirado

        var @event = new QrCodeExpirado(TransactionId, ExternalId, QrcodePayload);
        ApplyEvent(@event);
    }

    public void Apply(QrCodeGerado @event)
    {
        Id = @event.TransactionId;
        TransactionId = @event.TransactionId;
        ExternalId = @event.ExternalId;
        PixKey = @event.PixKey;
        Amount = @event.Amount;
        QrcodePayload = @event.QrcodePayload;
        QrcodeImage = @event.QrcodeImage;
        Type = @event.Type;
        ExpiresAt = @event.ExpiresAt;
        BankCode = @event.BankCode;
        Description = @event.Description;
    }

    public void Apply(QrCodeExpirado @event)
    {
        // QR Code foi marcado como expirado
        // Não há mudança de estado específica, apenas o evento foi registrado
    }
}
