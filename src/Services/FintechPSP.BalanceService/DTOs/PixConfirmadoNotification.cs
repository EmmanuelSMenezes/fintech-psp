namespace FintechPSP.BalanceService.DTOs;

/// <summary>
/// DTO para notificação de PIX confirmado via HTTP
/// </summary>
public class PixConfirmadoNotification
{
    /// <summary>
    /// ID da transação
    /// </summary>
    public string TxId { get; set; } = string.Empty;

    /// <summary>
    /// Valor do PIX
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Nome do pagador
    /// </summary>
    public string? PayerName { get; set; }

    /// <summary>
    /// Documento do pagador
    /// </summary>
    public string? PayerDocument { get; set; }

    /// <summary>
    /// Data/hora da confirmação
    /// </summary>
    public DateTime ConfirmedAt { get; set; }

    /// <summary>
    /// Dados adicionais do pagador
    /// </summary>
    public string? PayerInfo { get; set; }
}
