namespace FintechPSP.UserService.DTOs;

public class UpdateBankAccountRequest
{
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public Credentials? Credentials { get; set; }
}

