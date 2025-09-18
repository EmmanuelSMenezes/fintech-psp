using System;
using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.Models;

/// <summary>
/// Modelo para usuários do sistema
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
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hash da senha
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Nome completo do usuário
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Role/Função do usuário
    /// </summary>
    [Required]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Se o usuário está ativo
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Se é usuário master (não pode ser excluído)
    /// </summary>
    public bool IsMaster { get; set; } = false;

    /// <summary>
    /// Último login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Documento (CPF/CNPJ) - opcional
    /// </summary>
    public string? Document { get; set; }

    /// <summary>
    /// Telefone - opcional
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Endereço - opcional
    /// </summary>
    public string? Address { get; set; }
}
