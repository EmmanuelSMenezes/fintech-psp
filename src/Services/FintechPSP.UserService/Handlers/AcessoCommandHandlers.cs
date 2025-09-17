using MediatR;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Repositories;
using FintechPSP.Shared.Domain.Events;
using Marten;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Handlers;

/// <summary>
/// Handler para comandos de acesso
/// </summary>
public class AcessoCommandHandlers :
    IRequestHandler<CreateSubUserAccessCommand, AccessResponse>,
    IRequestHandler<CreateUserAccessCommand, AccessResponse>,
    IRequestHandler<UpdateAccessCommand, AccessResponse>,
    IRequestHandler<RemoveAccessCommand, bool>,
    IRequestHandler<ToggleAccessCommand, AccessResponse>,
    IRequestHandler<SyncUserPermissionsCommand, AccessResponse>
{
    private readonly IAcessoRepository _acessoRepository;
    private readonly IDocumentStore _documentStore;
    private readonly ILogger<AcessoCommandHandlers> _logger;

    public AcessoCommandHandlers(
        IAcessoRepository acessoRepository,
        IDocumentStore documentStore,
        ILogger<AcessoCommandHandlers> logger)
    {
        _acessoRepository = acessoRepository;
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task<AccessResponse> Handle(CreateSubUserAccessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando acesso para sub-usuário: {Email}", request.SubUserEmail);

        // Verificar se o email já possui acesso
        var existingAccess = await _acessoRepository.GetByEmailAsync(request.SubUserEmail);
        if (existingAccess != null)
        {
            throw new InvalidOperationException($"Email {request.SubUserEmail} já possui acesso no sistema");
        }

        // Validar permissões baseadas no role
        var validPermissions = RolePermissions.GetPermissionsForRole(request.Role);
        var invalidPermissions = request.Permissions.Except(validPermissions).ToList();
        if (invalidPermissions.Any())
        {
            throw new ArgumentException($"Permissões inválidas para o role '{request.Role}': {string.Join(", ", invalidPermissions)}");
        }

        // Criar novo acesso
        var novoAcesso = new Acesso
        {
            AcessoId = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // Novo UserId para sub-usuário
            ParentUserId = request.ParentUserId,
            Email = request.SubUserEmail,
            Role = request.Role,
            Permissions = request.Permissions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        var acessoCriado = await _acessoRepository.CreateAsync(novoAcesso);

        // Publicar evento
        await PublishEventAsync(new AcessoCriado(
            acessoCriado.AcessoId,
            acessoCriado.UserId,
            acessoCriado.Email,
            acessoCriado.Role,
            acessoCriado.Permissions,
            acessoCriado.CreatedBy,
            acessoCriado.ParentUserId
        ));

        _logger.LogInformation("Acesso criado com sucesso para sub-usuário: {Email} com ID: {AcessoId}", 
            request.SubUserEmail, acessoCriado.AcessoId);

        return MapToAccessResponse(acessoCriado);
    }

    public async Task<AccessResponse> Handle(CreateUserAccessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Criando acesso para usuário: {UserId}", request.UserId);

        // Verificar se o usuário já possui acesso
        var existingAccess = await _acessoRepository.GetByUserIdAsync(request.UserId);
        if (existingAccess != null)
        {
            throw new InvalidOperationException($"Usuário {request.UserId} já possui acesso no sistema");
        }

        // Validar permissões baseadas no role
        var validPermissions = RolePermissions.GetPermissionsForRole(request.Role);
        var invalidPermissions = request.Permissions.Except(validPermissions).ToList();
        if (invalidPermissions.Any())
        {
            throw new ArgumentException($"Permissões inválidas para o role '{request.Role}': {string.Join(", ", invalidPermissions)}");
        }

        // Criar novo acesso
        var novoAcesso = new Acesso
        {
            AcessoId = Guid.NewGuid(),
            UserId = request.UserId,
            Email = $"user_{request.UserId}@system.local", // Email temporário para usuários admin
            Role = request.Role,
            Permissions = request.Permissions,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.CreatedBy
        };

        var acessoCriado = await _acessoRepository.CreateAsync(novoAcesso);

        // Publicar evento
        await PublishEventAsync(new AcessoCriado(
            acessoCriado.AcessoId,
            acessoCriado.UserId,
            acessoCriado.Email,
            acessoCriado.Role,
            acessoCriado.Permissions,
            acessoCriado.CreatedBy
        ));

        _logger.LogInformation("Acesso criado com sucesso para usuário: {UserId} com ID: {AcessoId}", 
            request.UserId, acessoCriado.AcessoId);

        return MapToAccessResponse(acessoCriado);
    }

    public async Task<AccessResponse> Handle(UpdateAccessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Atualizando acesso: {AcessoId}", request.AcessoId);

        var acesso = await _acessoRepository.GetByIdAsync(request.AcessoId);
        if (acesso == null)
        {
            throw new ArgumentException($"Acesso {request.AcessoId} não encontrado");
        }

        var permissoesAnteriores = new List<string>(acesso.Permissions);

        // Atualizar campos se fornecidos
        if (!string.IsNullOrEmpty(request.Role))
        {
            // Validar permissões baseadas no novo role
            if (request.Permissions != null)
            {
                var validPermissions = RolePermissions.GetPermissionsForRole(request.Role);
                var invalidPermissions = request.Permissions.Except(validPermissions).ToList();
                if (invalidPermissions.Any())
                {
                    throw new ArgumentException($"Permissões inválidas para o role '{request.Role}': {string.Join(", ", invalidPermissions)}");
                }
            }

            acesso.Role = request.Role;
        }

        if (request.Permissions != null)
        {
            acesso.Permissions = request.Permissions;
        }

        acesso.UpdatedAt = DateTime.UtcNow;

        var acessoAtualizado = await _acessoRepository.UpdateAsync(acesso);

        // Publicar evento
        await PublishEventAsync(new AcessoAtualizado(
            acessoAtualizado.AcessoId,
            acessoAtualizado.UserId,
            acessoAtualizado.Role,
            acessoAtualizado.Permissions,
            permissoesAnteriores,
            request.UpdatedBy
        ));

        _logger.LogInformation("Acesso atualizado com sucesso: {AcessoId}", request.AcessoId);

        return MapToAccessResponse(acessoAtualizado);
    }

    public async Task<bool> Handle(RemoveAccessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removendo acesso: {AcessoId}", request.AcessoId);

        var acesso = await _acessoRepository.GetByIdAsync(request.AcessoId);
        if (acesso == null)
        {
            throw new ArgumentException($"Acesso {request.AcessoId} não encontrado");
        }

        var removido = await _acessoRepository.DeleteAsync(request.AcessoId);

        if (removido)
        {
            // Publicar evento
            await PublishEventAsync(new AcessoRemovido(
                acesso.AcessoId,
                acesso.UserId,
                acesso.Email,
                acesso.Role,
                request.Motivo,
                request.RemovedBy
            ));

            _logger.LogInformation("Acesso removido com sucesso: {AcessoId}", request.AcessoId);
        }

        return removido;
    }

    public async Task<AccessResponse> Handle(ToggleAccessCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Alterando status do acesso: {AcessoId} para {IsActive}", request.AcessoId, request.IsActive);

        var acesso = await _acessoRepository.GetByIdAsync(request.AcessoId);
        if (acesso == null)
        {
            throw new ArgumentException($"Acesso {request.AcessoId} não encontrado");
        }

        acesso.IsActive = request.IsActive;
        acesso.UpdatedAt = DateTime.UtcNow;

        var acessoAtualizado = await _acessoRepository.UpdateAsync(acesso);

        _logger.LogInformation("Status do acesso alterado com sucesso: {AcessoId}", request.AcessoId);

        return MapToAccessResponse(acessoAtualizado);
    }

    public async Task<AccessResponse> Handle(SyncUserPermissionsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sincronizando permissões do usuário: {UserId} com role: {Role}", request.UserId, request.Role);

        var acesso = await _acessoRepository.GetByUserIdAsync(request.UserId);
        if (acesso == null)
        {
            throw new ArgumentException($"Acesso para usuário {request.UserId} não encontrado");
        }

        var permissoesAnteriores = new List<string>(acesso.Permissions);
        var novasPermissoes = RolePermissions.GetPermissionsForRole(request.Role);

        acesso.Role = request.Role;
        acesso.Permissions = novasPermissoes;
        acesso.UpdatedAt = DateTime.UtcNow;

        var acessoAtualizado = await _acessoRepository.UpdateAsync(acesso);

        // Publicar evento
        await PublishEventAsync(new AcessoAtualizado(
            acessoAtualizado.AcessoId,
            acessoAtualizado.UserId,
            acessoAtualizado.Role,
            acessoAtualizado.Permissions,
            permissoesAnteriores,
            request.UpdatedBy
        ));

        _logger.LogInformation("Permissões sincronizadas com sucesso para usuário: {UserId}", request.UserId);

        return MapToAccessResponse(acessoAtualizado);
    }

    private async Task PublishEventAsync<T>(T domainEvent) where T : class
    {
        try
        {
            using var session = _documentStore.LightweightSession();
            session.Events.Append(Guid.NewGuid(), domainEvent);
            await session.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento: {EventType}", typeof(T).Name);
            // Não falhar a operação por causa do evento
        }
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
