using System.Text.Json.Serialization;

namespace FintechPSP.TransactionService.DTOs;

/// <summary>
/// Request para gerar QR Code PIX estático
/// </summary>
public class QrCodeEstaticoRequest
{
    [JsonPropertyName("externalId")]
    public string ExternalId { get; set; } = string.Empty;
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Request para gerar QR Code PIX dinâmico
/// </summary>
public class QrCodeDinamicoRequest
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
}

/// <summary>
/// Response para QR Code PIX gerado
/// </summary>
public class QrCodeResponse
{
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }
    
    [JsonPropertyName("qrcodePayload")]
    public string QrcodePayload { get; set; } = string.Empty;
    
    [JsonPropertyName("qrcodeImage")]
    public string QrcodeImage { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "static" ou "dynamic"
    
    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }
    
    [JsonPropertyName("pixKey")]
    public string PixKey { get; set; } = string.Empty;
    
    [JsonPropertyName("amount")]
    public decimal? Amount { get; set; }
    
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }
}
