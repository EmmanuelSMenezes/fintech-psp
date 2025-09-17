using System;

namespace FintechPSP.UserService.Models;

/// <summary>
/// Modelo de conta bancária
/// </summary>
public class ContaBancaria
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CredentialsTokenId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Credenciais tokenizadas de uma conta bancária
/// </summary>
public class ContaCredentialsToken
{
    public string TokenId { get; set; } = string.Empty;
    public Guid ContaId { get; set; }
    public string EncryptedCredentials { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
