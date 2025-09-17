using System.Text.Json.Serialization;
using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para gerar QR Code PIX dinâmico
/// </summary>
public class GerarQrDinamicoCommand : IRequest<QrCodeResponse>
{
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; set; } = 300; // 5 minutos padrão
    
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public GerarQrDinamicoCommand() { }

    public GerarQrDinamicoCommand(string externalId, decimal amount, string pixKey, 
        string bankCode, string? description, int expiresIn, Guid clientId)
    {
        ExternalId = externalId;
        Amount = amount;
        PixKey = pixKey;
        BankCode = bankCode;
        Description = description;
        ExpiresIn = expiresIn;
        ClientId = clientId;
    }
}
