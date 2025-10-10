using System.ComponentModel.DataAnnotations;

namespace FintechPSP.BalanceService.DTOs;

/// <summary>
/// Request específico para saque via PIX
/// </summary>
public class PixCashOutRequest
{
    /// <summary>
    /// Valor do saque
    /// </summary>
    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Chave PIX de destino
    /// </summary>
    [Required(ErrorMessage = "Chave PIX é obrigatória")]
    public string PixKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome do beneficiário
    /// </summary>
    [Required(ErrorMessage = "Nome do beneficiário é obrigatório")]
    public string BeneficiaryName { get; set; } = string.Empty;

    /// <summary>
    /// Documento do beneficiário
    /// </summary>
    [Required(ErrorMessage = "Documento do beneficiário é obrigatório")]
    public string BeneficiaryDocument { get; set; } = string.Empty;

    /// <summary>
    /// Banco de destino (código)
    /// </summary>
    public string? BankCode { get; set; }

    /// <summary>
    /// ID da transação externa (opcional)
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// URL de webhook para notificação (opcional)
    /// </summary>
    public string? WebhookUrl { get; set; }
}

/// <summary>
/// Request para débito administrativo
/// </summary>
public class AdminDebitRequest
{
    /// <summary>
    /// ID do cliente a ser debitado
    /// </summary>
    [Required(ErrorMessage = "ID do cliente é obrigatório")]
    public Guid ClientId { get; set; }

    /// <summary>
    /// Valor do débito
    /// </summary>
    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Motivo do débito administrativo
    /// </summary>
    [Required(ErrorMessage = "Motivo é obrigatório")]
    [StringLength(500, ErrorMessage = "Motivo deve ter no máximo 500 caracteres")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID da transação externa (opcional)
    /// </summary>
    public string? ExternalTransactionId { get; set; }

    /// <summary>
    /// Observações adicionais
    /// </summary>
    public string? Notes { get; set; }
}
