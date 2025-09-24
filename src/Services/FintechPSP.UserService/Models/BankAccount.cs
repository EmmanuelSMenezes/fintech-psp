using System;

namespace FintechPSP.UserService.Models;

public class BankAccount
{
    public Guid ContaId { get; set; }
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CredentialsTokenId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

