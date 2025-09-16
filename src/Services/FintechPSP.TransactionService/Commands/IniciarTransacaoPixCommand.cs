using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para iniciar transação PIX
/// </summary>
public class IniciarTransacaoPixCommand : IRequest<TransactionResponse>
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? WebhookUrl { get; set; }
    public string? EndToEndId { get; set; }

    public IniciarTransacaoPixCommand(
        string externalId,
        decimal amount,
        string pixKey,
        string bankCode,
        string? description = null,
        string? webhookUrl = null,
        string? endToEndId = null)
    {
        ExternalId = externalId;
        Amount = amount;
        PixKey = pixKey;
        BankCode = bankCode;
        Description = description;
        WebhookUrl = webhookUrl;
        EndToEndId = endToEndId;
    }
}
