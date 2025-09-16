using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para emitir boleto
/// </summary>
public class EmitirBoletoCommand : IRequest<TransactionResponse>
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public string PayerTaxId { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public string? WebhookUrl { get; set; }
    public string? BankCode { get; set; }

    public EmitirBoletoCommand(
        string externalId,
        decimal amount,
        DateTime dueDate,
        string payerTaxId,
        string payerName,
        string? instructions = null,
        string? webhookUrl = null,
        string? bankCode = null)
    {
        ExternalId = externalId;
        Amount = amount;
        DueDate = dueDate;
        PayerTaxId = payerTaxId;
        PayerName = payerName;
        Instructions = instructions;
        WebhookUrl = webhookUrl;
        BankCode = bankCode;
    }
}
