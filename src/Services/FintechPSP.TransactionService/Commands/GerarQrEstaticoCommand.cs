using System.Text.Json.Serialization;
using FintechPSP.TransactionService.DTOs;
using MediatR;

namespace FintechPSP.TransactionService.Commands;

/// <summary>
/// Comando para gerar QR Code PIX estático
/// </summary>
public class GerarQrEstaticoCommand : IRequest<QrCodeResponse>
{
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("clientId")]
    public Guid ClientId { get; set; }

    public GerarQrEstaticoCommand() { }

    public GerarQrEstaticoCommand(string externalId, string pixKey, string bankCode, 
        string? description, Guid clientId)
    {
        ExternalId = externalId;
        PixKey = pixKey;
        BankCode = bankCode;
        Description = description;
        ClientId = clientId;
    }
}
