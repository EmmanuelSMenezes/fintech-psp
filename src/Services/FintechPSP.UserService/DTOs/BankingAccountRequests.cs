using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.DTOs;

/// <summary>
/// Request para criar conta bancária no escopo banking (sem clienteId)
/// </summary>
public class CreateMyBankAccountRequest
{
    [Required]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Credentials Credentials { get; set; } = new();
}

/// <summary>
/// Request para atualizar conta bancária no escopo banking
/// </summary>
public class UpdateMyBankAccountRequest
{
    public string? BankCode { get; set; }
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public Credentials? Credentials { get; set; }
}

/// <summary>
/// Response para conta bancária
/// </summary>
public class BankAccountResponse
{
    public Guid ContaId { get; set; }
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CredentialsTokenId { get; set; }
}
