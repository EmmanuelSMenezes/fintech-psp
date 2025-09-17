using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.DTOs;

/// <summary>
/// Request para criar um novo acesso (scope banking)
/// </summary>
public class CreateSubUserAccessRequest
{
    [Required(ErrorMessage = "Email do sub-usuário é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter formato válido")]
    public string SubUserEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role é obrigatório")]
    [RegularExpression("^(sub-cliente)$", ErrorMessage = "Role deve ser 'sub-cliente'")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Permissões são obrigatórias")]
    [MinLength(1, ErrorMessage = "Pelo menos uma permissão deve ser especificada")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Request para criar um novo acesso (scope admin)
/// </summary>
public class CreateUserAccessRequest
{
    [Required(ErrorMessage = "UserId é obrigatório")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Role é obrigatório")]
    [RegularExpression("^(admin|sub-admin|cliente|sub-cliente)$", 
        ErrorMessage = "Role deve ser 'admin', 'sub-admin', 'cliente' ou 'sub-cliente'")]
    public string Role { get; set; } = string.Empty;

    [Required(ErrorMessage = "Permissões são obrigatórias")]
    [MinLength(1, ErrorMessage = "Pelo menos uma permissão deve ser especificada")]
    public List<string> Permissions { get; set; } = new();
}

/// <summary>
/// Request para atualizar um acesso existente
/// </summary>
public class UpdateAccessRequest
{
    [RegularExpression("^(admin|sub-admin|cliente|sub-cliente)$", 
        ErrorMessage = "Role deve ser 'admin', 'sub-admin', 'cliente' ou 'sub-cliente'")]
    public string? Role { get; set; }

    [MinLength(1, ErrorMessage = "Pelo menos uma permissão deve ser especificada")]
    public List<string>? Permissions { get; set; }
}

/// <summary>
/// Response para operações de acesso
/// </summary>
public class AccessResponse
{
    public Guid AcessoId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentUserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string CreatedByEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response para listagem de acessos
/// </summary>
public class AccessListResponse
{
    public List<AccessResponse> Acessos { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}

/// <summary>
/// Request para busca de acessos com filtros
/// </summary>
public class GetAccessesRequest
{
    public Guid? UserId { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}

/// <summary>
/// Response com detalhes de permissões disponíveis
/// </summary>
public class PermissionsResponse
{
    public Dictionary<string, List<string>> PermissionsByRole { get; set; } = new();
    public List<string> AllPermissions { get; set; } = new();
}

/// <summary>
/// Request para validação de permissões
/// </summary>
public class ValidatePermissionRequest
{
    [Required(ErrorMessage = "UserId é obrigatório")]
    public Guid UserId { get; set; }

    [Required(ErrorMessage = "Permission é obrigatória")]
    public string Permission { get; set; } = string.Empty;
}

/// <summary>
/// Response para validação de permissões
/// </summary>
public class ValidatePermissionResponse
{
    public bool HasPermission { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> UserPermissions { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
