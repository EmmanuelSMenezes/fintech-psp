using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para iniciar transação TED
/// </summary>
public class IniciarTransacaoTedCommand : IRequest<TransactionResponse>
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountBranch { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebhookUrl { get; set; }

    public IniciarTransacaoTedCommand(
        string externalId,
        decimal amount,
        string bankCode,
        string accountBranch,
        string accountNumber,
        string taxId,
        string name,
        string? description = null,
        string? webhookUrl = null)
    {
        ExternalId = externalId;
        Amount = amount;
        BankCode = bankCode;
        AccountBranch = accountBranch;
        AccountNumber = accountNumber;
        TaxId = taxId;
        Name = name;
        Description = description;
        WebhookUrl = webhookUrl;
    }
}
