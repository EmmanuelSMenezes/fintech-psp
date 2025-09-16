using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para iniciar transação de criptomoeda
/// </summary>
public class IniciarTransacaoCriptoCommand : IRequest<TransactionResponse>
{
    public string ExternalId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CryptoType { get; set; } = string.Empty;
    public string WalletAddress { get; set; } = string.Empty;
    public string FiatCurrency { get; set; } = "BRL";
    public string? Description { get; set; }
    public string? WebhookUrl { get; set; }

    public IniciarTransacaoCriptoCommand(
        string externalId,
        decimal amount,
        string cryptoType,
        string walletAddress,
        string fiatCurrency = "BRL",
        string? description = null,
        string? webhookUrl = null)
    {
        ExternalId = externalId;
        Amount = amount;
        CryptoType = cryptoType;
        WalletAddress = walletAddress;
        FiatCurrency = fiatCurrency;
        Description = description;
        WebhookUrl = webhookUrl;
    }
}
