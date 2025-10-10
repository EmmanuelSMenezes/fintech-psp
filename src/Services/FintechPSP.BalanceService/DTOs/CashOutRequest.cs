using System.ComponentModel.DataAnnotations;

namespace FintechPSP.BalanceService.DTOs;

/// <summary>
/// Request para operação de cash-out (saque/débito)
/// </summary>
public class CashOutRequest
{
    /// <summary>
    /// Valor do saque
    /// </summary>
    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Descrição da operação
    /// </summary>
    [Required(ErrorMessage = "Descrição é obrigatória")]
    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de operação de cash-out
    /// </summary>
    [Required(ErrorMessage = "Tipo de operação é obrigatório")]
    public CashOutType Type { get; set; }

    /// <summary>
    /// Dados específicos para PIX (se aplicável)
    /// </summary>
    public PixCashOutData? PixData { get; set; }

    /// <summary>
    /// Dados específicos para TED (se aplicável)
    /// </summary>
    public TedCashOutData? TedData { get; set; }

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
/// Tipos de operação de cash-out
/// </summary>
public enum CashOutType
{
    /// <summary>
    /// Saque via PIX
    /// </summary>
    PIX,

    /// <summary>
    /// Transferência TED
    /// </summary>
    TED,

    /// <summary>
    /// Saque em dinheiro
    /// </summary>
    CASH,

    /// <summary>
    /// Débito administrativo
    /// </summary>
    ADMIN_DEBIT,

    /// <summary>
    /// Estorno
    /// </summary>
    REFUND
}

/// <summary>
/// Dados específicos para cash-out via PIX
/// </summary>
public class PixCashOutData
{
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
}

/// <summary>
/// Dados específicos para cash-out via TED
/// </summary>
public class TedCashOutData
{
    /// <summary>
    /// Banco de destino
    /// </summary>
    [Required(ErrorMessage = "Banco de destino é obrigatório")]
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// Agência de destino
    /// </summary>
    [Required(ErrorMessage = "Agência é obrigatória")]
    public string Agency { get; set; } = string.Empty;

    /// <summary>
    /// Conta de destino
    /// </summary>
    [Required(ErrorMessage = "Conta é obrigatória")]
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// Dígito da conta
    /// </summary>
    public string? AccountDigit { get; set; }

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
    /// Tipo de conta (C = Corrente, P = Poupança)
    /// </summary>
    [Required(ErrorMessage = "Tipo de conta é obrigatório")]
    public string AccountType { get; set; } = "C";
}
