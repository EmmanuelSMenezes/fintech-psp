namespace FintechPSP.Shared.Domain.Enums;

/// <summary>
/// Tipos de transação suportados
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Transferência PIX
    /// </summary>
    PIX,
    
    /// <summary>
    /// Transferência Eletrônica Disponível (TED)
    /// </summary>
    TED,
    
    /// <summary>
    /// Boleto bancário
    /// </summary>
    BOLETO,
    
    /// <summary>
    /// Transferência entre contas (DOC)
    /// </summary>
    DOC,
    
    /// <summary>
    /// Transação com criptomoedas
    /// </summary>
    CRIPTO
}
