using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.DTOs;

public class CreateBankAccountRequest
{
    [Required]
    public string ClienteId { get; set; } = string.Empty;

    [Required]
    public string BankCode { get; set; } = string.Empty;

    [Required]
    public string AccountNumber { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public Credentials Credentials { get; set; } = new();
}

public class Credentials
{
    [Required]
    public string ClientId { get; set; } = string.Empty;
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
    public string Environment { get; set; } = "sandbox";
    public string? MtlsCert { get; set; }
}

