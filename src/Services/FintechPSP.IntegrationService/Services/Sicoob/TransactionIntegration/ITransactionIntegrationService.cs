using FintechPSP.Shared.Domain.Enums;

namespace FintechPSP.IntegrationService.Services.Sicoob.TransactionIntegration;

/// <summary>
/// Interface para integração de transações com o Sicoob
/// </summary>
public interface ITransactionIntegrationService
{
    /// <summary>
    /// Processa uma transação PIX no Sicoob
    /// </summary>
    Task<SicoobTransactionResult> ProcessPixTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa uma transação TED no Sicoob
    /// </summary>
    Task<SicoobTransactionResult> ProcessTedTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa um boleto no Sicoob
    /// </summary>
    Task<SicoobTransactionResult> ProcessBoletoTransactionAsync(TransactionDto transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o status de uma transação no Sicoob
    /// </summary>
    Task<SicoobTransactionResult> GetTransactionStatusAsync(string externalId, string transactionType, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO para transação (evita dependência circular)
/// </summary>
public class TransactionDto
{
    public Guid TransactionId { get; set; }
    public string ExternalId { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BRL";
    public string? BankCode { get; set; }
    public string? PixKey { get; set; }
    public string? EndToEndId { get; set; }
    public string? AccountBranch { get; set; }
    public string? AccountNumber { get; set; }
    public string? TaxId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? WebhookUrl { get; set; }
    public DateTime? DueDate { get; set; }
    public string? PayerTaxId { get; set; }
    public string? PayerName { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Resultado da integração com o Sicoob
/// </summary>
public class SicoobTransactionResult
{
    public bool Success { get; set; }
    public string? SicoobTransactionId { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
