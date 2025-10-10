namespace FintechPSP.BalanceService.DTOs;

/// <summary>
/// Response para operação de cash-out
/// </summary>
public class CashOutResponse
{
    /// <summary>
    /// ID da transação gerada
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// ID externo da transação (se aplicável)
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Status da operação
    /// </summary>
    public CashOutStatus Status { get; set; }

    /// <summary>
    /// Valor debitado
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Saldo anterior
    /// </summary>
    public decimal PreviousBalance { get; set; }

    /// <summary>
    /// Novo saldo após o débito
    /// </summary>
    public decimal NewBalance { get; set; }

    /// <summary>
    /// Tipo de operação realizada
    /// </summary>
    public CashOutType Type { get; set; }

    /// <summary>
    /// Descrição da operação
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data/hora da operação
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Mensagem adicional (se houver)
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Dados específicos do processamento (se aplicável)
    /// </summary>
    public Dictionary<string, object>? ProcessingData { get; set; }

    /// <summary>
    /// Indica se a operação foi bem-sucedida
    /// </summary>
    public bool Success => Status == CashOutStatus.COMPLETED || Status == CashOutStatus.PROCESSING;

    /// <summary>
    /// Taxa aplicada (se houver)
    /// </summary>
    public decimal? Fee { get; set; }

    /// <summary>
    /// Valor líquido (amount - fee)
    /// </summary>
    public decimal NetAmount => Amount - (Fee ?? 0);
}

/// <summary>
/// Status possíveis para operação de cash-out
/// </summary>
public enum CashOutStatus
{
    /// <summary>
    /// Operação iniciada
    /// </summary>
    INITIATED,

    /// <summary>
    /// Em processamento
    /// </summary>
    PROCESSING,

    /// <summary>
    /// Concluída com sucesso
    /// </summary>
    COMPLETED,

    /// <summary>
    /// Falhou
    /// </summary>
    FAILED,

    /// <summary>
    /// Cancelada
    /// </summary>
    CANCELLED,

    /// <summary>
    /// Rejeitada (saldo insuficiente, etc.)
    /// </summary>
    REJECTED,

    /// <summary>
    /// Em análise
    /// </summary>
    UNDER_ANALYSIS
}

/// <summary>
/// Notification para cash-out processado (para webhooks)
/// </summary>
public class CashOutNotification
{
    /// <summary>
    /// ID da transação
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// ID externo da transação
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Status da operação
    /// </summary>
    public CashOutStatus Status { get; set; }

    /// <summary>
    /// Valor da operação
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo de operação
    /// </summary>
    public CashOutType Type { get; set; }

    /// <summary>
    /// ID do cliente
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Data/hora do processamento
    /// </summary>
    public DateTime ProcessedAt { get; set; }

    /// <summary>
    /// Mensagem de erro (se houver)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Dados adicionais do processamento
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
