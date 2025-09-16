using System;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando um usuário é criado
/// </summary>
public class UserCreated : DomainEvent
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    
    [JsonPropertyName("accountId")]
    public Guid? AccountId { get; set; }

    public UserCreated() { }

    public UserCreated(Guid userId, string email, string role, Guid? accountId = null)
    {
        UserId = userId;
        Email = email;
        Role = role;
        AccountId = accountId;
    }
}

/// <summary>
/// Evento disparado quando um usuário é atualizado
/// </summary>
public class UserUpdated : DomainEvent
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    
    [JsonPropertyName("isActive")]
    public bool? IsActive { get; set; }

    public UserUpdated() { }

    public UserUpdated(Guid userId, string? email = null, string? role = null, bool? isActive = null)
    {
        UserId = userId;
        Email = email;
        Role = role;
        IsActive = isActive;
    }
}
