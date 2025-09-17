using MediatR;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Repositories;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Handlers;

/// <summary>
/// Handler para queries de acesso
/// </summary>
public class AcessoQueryHandlers :
    IRequestHandler<GetAccessesQuery, AccessListResponse>,
    IRequestHandler<GetAccessByIdQuery, AccessResponse?>,
    IRequestHandler<GetUserAccessesQuery, List<AccessResponse>>,
    IRequestHandler<GetSubUsersQuery, List<AccessResponse>>,
    IRequestHandler<ValidatePermissionQuery, ValidatePermissionResponse>,
    IRequestHandler<GetAvailablePermissionsQuery, PermissionsResponse>,
    IRequestHandler<CheckEmailAccessQuery, AccessResponse?>
{
    private readonly IAcessoRepository _acessoRepository;
    private readonly ILogger<AcessoQueryHandlers> _logger;

    public AcessoQueryHandlers(
        IAcessoRepository acessoRepository,
        ILogger<AcessoQueryHandlers> logger)
    {
        _acessoRepository = acessoRepository;
        _logger = logger;
    }

    public async Task<AccessListResponse> Handle(GetAccessesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando acessos com filtros - UserId: {UserId}, Role: {Role}, Page: {Page}", 
            request.UserId, request.Role, request.Page);

        var (acessos, total) = await _acessoRepository.GetWithFiltersAsync(
            request.UserId,
            request.ParentUserId,
            request.Role,
            request.IsActive,
            request.Page,
            request.Limit);

        return new AccessListResponse
        {
            Acessos = acessos.Select(MapToAccessResponse).ToList(),
            Total = total,
            Page = request.Page,
            Limit = request.Limit
        };
    }

    public async Task<AccessResponse?> Handle(GetAccessByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando acesso por ID: {AcessoId}", request.AcessoId);

        var acesso = await _acessoRepository.GetByIdAsync(request.AcessoId);
        return acesso != null ? MapToAccessResponse(acesso) : null;
    }

    public async Task<List<AccessResponse>> Handle(GetUserAccessesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando acessos do usuário: {UserId}", request.UserId);

        var acesso = await _acessoRepository.GetByUserIdAsync(request.UserId);
        return acesso != null ? new List<AccessResponse> { MapToAccessResponse(acesso) } : new List<AccessResponse>();
    }

    public async Task<List<AccessResponse>> Handle(GetSubUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando sub-usuários do usuário pai: {ParentUserId}", request.ParentUserId);

        var subUsers = await _acessoRepository.GetByParentUserIdAsync(request.ParentUserId);
        return subUsers.Select(MapToAccessResponse).ToList();
    }

    public async Task<ValidatePermissionResponse> Handle(ValidatePermissionQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validando permissão '{Permission}' para usuário: {UserId}", 
            request.Permission, request.UserId);

        var acesso = await _acessoRepository.GetByUserIdAsync(request.UserId);
        
        if (acesso == null || !acesso.IsActive)
        {
            return new ValidatePermissionResponse
            {
                HasPermission = false,
                Role = string.Empty,
                UserPermissions = new List<string>(),
                Message = "Usuário não encontrado ou inativo"
            };
        }

        // Admin sempre tem todas as permissões
        var hasPermission = acesso.Role == "admin" || acesso.Permissions.Contains(request.Permission);

        return new ValidatePermissionResponse
        {
            HasPermission = hasPermission,
            Role = acesso.Role,
            UserPermissions = acesso.Permissions,
            Message = hasPermission ? "Permissão concedida" : "Permissão negada"
        };
    }

    public async Task<PermissionsResponse> Handle(GetAvailablePermissionsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Buscando permissões disponíveis por role");

        await Task.CompletedTask; // Para satisfazer o async

        var permissionsByRole = new Dictionary<string, List<string>>();
        var allPermissions = new List<string>();

        foreach (SystemRole role in Enum.GetValues<SystemRole>())
        {
            var roleString = role.ToString().ToLower().Replace("_", "-");
            var permissions = RolePermissions.GetPermissionsForRole(roleString);
            permissionsByRole[roleString] = permissions;
            
            foreach (var permission in permissions)
            {
                if (!allPermissions.Contains(permission))
                {
                    allPermissions.Add(permission);
                }
            }
        }

        return new PermissionsResponse
        {
            PermissionsByRole = permissionsByRole,
            AllPermissions = allPermissions.OrderBy(p => p).ToList()
        };
    }

    public async Task<AccessResponse?> Handle(CheckEmailAccessQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Verificando se email possui acesso: {Email}", request.Email);

        var acesso = await _acessoRepository.GetByEmailAsync(request.Email);
        return acesso != null ? MapToAccessResponse(acesso) : null;
    }

    private static AccessResponse MapToAccessResponse(Acesso acesso)
    {
        return new AccessResponse
        {
            AcessoId = acesso.AcessoId,
            UserId = acesso.UserId,
            ParentUserId = acesso.ParentUserId,
            Email = acesso.Email,
            Role = acesso.Role,
            Permissions = acesso.Permissions,
            IsActive = acesso.IsActive,
            CreatedAt = acesso.CreatedAt,
            UpdatedAt = acesso.UpdatedAt,
            CreatedByEmail = "system" // TODO: Buscar email do criador
        };
    }
}
