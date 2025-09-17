using MediatR;
using FintechPSP.UserService.DTOs;

namespace FintechPSP.UserService.Commands;

/// <summary>
/// Command para criar um novo acesso de sub-usuário (scope banking)
/// </summary>
public class CreateSubUserAccessCommand : IRequest<AccessResponse>
{
    public Guid ParentUserId { get; set; }
    public string SubUserEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// Command para criar um novo acesso de usuário (scope admin)
/// </summary>
public class CreateUserAccessCommand : IRequest<AccessResponse>
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// Command para atualizar um acesso existente
/// </summary>
public class UpdateAccessCommand : IRequest<AccessResponse>
{
    public Guid AcessoId { get; set; }
    public string? Role { get; set; }
    public List<string>? Permissions { get; set; }
    public Guid UpdatedBy { get; set; }
}

/// <summary>
/// Command para remover um acesso
/// </summary>
public class RemoveAccessCommand : IRequest<bool>
{
    public Guid AcessoId { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public Guid RemovedBy { get; set; }
}

/// <summary>
/// Command para ativar/desativar um acesso
/// </summary>
public class ToggleAccessCommand : IRequest<AccessResponse>
{
    public Guid AcessoId { get; set; }
    public bool IsActive { get; set; }
    public Guid UpdatedBy { get; set; }
}

/// <summary>
/// Query para obter acessos com filtros
/// </summary>
public class GetAccessesQuery : IRequest<AccessListResponse>
{
    public Guid? UserId { get; set; }
    public Guid? ParentUserId { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
}

/// <summary>
/// Query para obter um acesso específico
/// </summary>
public class GetAccessByIdQuery : IRequest<AccessResponse?>
{
    public Guid AcessoId { get; set; }
}

/// <summary>
/// Query para obter acessos de um usuário específico
/// </summary>
public class GetUserAccessesQuery : IRequest<List<AccessResponse>>
{
    public Guid UserId { get; set; }
}

/// <summary>
/// Query para obter sub-usuários de um usuário pai
/// </summary>
public class GetSubUsersQuery : IRequest<List<AccessResponse>>
{
    public Guid ParentUserId { get; set; }
}

/// <summary>
/// Query para validar se um usuário tem uma permissão específica
/// </summary>
public class ValidatePermissionQuery : IRequest<ValidatePermissionResponse>
{
    public Guid UserId { get; set; }
    public string Permission { get; set; } = string.Empty;
}

/// <summary>
/// Query para obter todas as permissões disponíveis por role
/// </summary>
public class GetAvailablePermissionsQuery : IRequest<PermissionsResponse>
{
}

/// <summary>
/// Query para verificar se um email já possui acesso
/// </summary>
public class CheckEmailAccessQuery : IRequest<AccessResponse?>
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Command para sincronizar permissões de um usuário com base no role
/// </summary>
public class SyncUserPermissionsCommand : IRequest<AccessResponse>
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public Guid UpdatedBy { get; set; }
}
