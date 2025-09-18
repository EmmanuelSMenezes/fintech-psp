using System;

namespace FintechPSP.AuthService.Models;

/// <summary>
/// Modelo de usuário do sistema
/// </summary>
public class SystemUser
{
    /// <summary>
    /// ID único do usuário
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Email do usuário (único)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role do usuário (Admin, SubAdmin, etc.)
    /// </summary>
    public string Role { get; set; } = "Admin";

    /// <summary>
    /// Se o usuário está ativo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Se é usuário master (não pode ser excluído)
    /// </summary>
    public bool IsMaster { get; set; } = false;

    /// <summary>
    /// Data do último login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
