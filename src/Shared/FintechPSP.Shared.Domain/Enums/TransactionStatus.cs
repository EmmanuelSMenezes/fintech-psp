namespace FintechPSP.Shared.Domain.Enums;

/// <summary>
/// Status das transações
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transação iniciada
    /// </summary>
    INITIATED,
    
    /// <summary>
    /// Transação em processamento
    /// </summary>
    PROCESSING,
    
    /// <summary>
    /// Transação confirmada/aprovada
    /// </summary>
    CONFIRMED,
    
    /// <summary>
    /// Transação rejeitada
    /// </summary>
    REJECTED,
    
    /// <summary>
    /// Transação cancelada
    /// </summary>
    CANCELLED,
    
    /// <summary>
    /// Transação expirada
    /// </summary>
    EXPIRED,
    
    /// <summary>
    /// Transação em análise
    /// </summary>
    UNDER_ANALYSIS,
    
    /// <summary>
    /// Transação pendente
    /// </summary>
    PENDING,
    
    /// <summary>
    /// Boleto emitido
    /// </summary>
    ISSUED,

    /// <summary>
    /// Transação falhou
    /// </summary>
    FAILED
}
