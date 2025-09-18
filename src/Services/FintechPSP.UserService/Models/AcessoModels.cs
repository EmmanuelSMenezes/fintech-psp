using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FintechPSP.UserService.Models;

/// <summary>
/// Entidade para controle de acesso RBAC
/// </summary>
public class Acesso
{
    public Guid AcessoId { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? ParentUserId { get; set; } // Para sub-usuários
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // admin, sub-admin, cliente, sub-cliente
    public List<string> Permissions { get; set; } = new(); // Permissões específicas
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedBy { get; set; } // Quem criou este acesso
}

/// <summary>
/// Enum para roles do sistema
/// </summary>
public enum SystemRole
{
    Admin,
    SubAdmin,
    Cliente,
    SubCliente
}

/// <summary>
/// Enum para permissões do sistema
/// </summary>
public enum SystemPermission
{
    // Visualização
    ViewDashboard,
    ViewTransacoes,
    ViewContas,
    ViewClientes,
    ViewRelatorios,
    ViewExtratos,
    ViewSaldo,
    
    // Edição
    EditContas,
    EditClientes,
    EditConfiguracoes,
    EditAcessos,
    
    // Transações
    TransacionarPix,
    TransacionarTed,
    TransacionarBoleto,
    TransacionarCripto,
    GerarQrCode,
    
    // Administração
    ManageUsers,
    ManagePermissions,
    ManageSystem,
    ViewAuditLogs,
    
    // Configurações
    ConfigurarPriorizacao,
    ConfigurarBancos,
    ConfigurarIntegracoes
}

/// <summary>
/// Mapeamento de roles para permissões padrão
/// </summary>
public static class RolePermissions
{
    public static readonly Dictionary<SystemRole, List<SystemPermission>> DefaultPermissions = new()
    {
        [SystemRole.Admin] = new List<SystemPermission>
        {
            // Admin tem todas as permissões
            SystemPermission.ViewDashboard,
            SystemPermission.ViewTransacoes,
            SystemPermission.ViewContas,
            SystemPermission.ViewClientes,
            SystemPermission.ViewRelatorios,
            SystemPermission.ViewExtratos,
            SystemPermission.ViewSaldo,
            SystemPermission.EditContas,
            SystemPermission.EditClientes,
            SystemPermission.EditConfiguracoes,
            SystemPermission.EditAcessos,
            SystemPermission.TransacionarPix,
            SystemPermission.TransacionarTed,
            SystemPermission.TransacionarBoleto,
            SystemPermission.TransacionarCripto,
            SystemPermission.GerarQrCode,
            SystemPermission.ManageUsers,
            SystemPermission.ManagePermissions,
            SystemPermission.ManageSystem,
            SystemPermission.ViewAuditLogs,
            SystemPermission.ConfigurarPriorizacao,
            SystemPermission.ConfigurarBancos,
            SystemPermission.ConfigurarIntegracoes
        },
        
        [SystemRole.SubAdmin] = new List<SystemPermission>
        {
            // Sub-admin: apenas visualização e relatórios
            SystemPermission.ViewDashboard,
            SystemPermission.ViewTransacoes,
            SystemPermission.ViewContas,
            SystemPermission.ViewClientes,
            SystemPermission.ViewRelatorios,
            SystemPermission.ViewExtratos,
            SystemPermission.ViewSaldo
        },
        
        [SystemRole.Cliente] = new List<SystemPermission>
        {
            // Cliente: gerenciamento completo de suas próprias contas
            SystemPermission.ViewDashboard,
            SystemPermission.ViewTransacoes,
            SystemPermission.ViewContas,
            SystemPermission.ViewExtratos,
            SystemPermission.ViewSaldo,
            SystemPermission.EditContas,
            SystemPermission.EditAcessos,
            SystemPermission.TransacionarPix,
            SystemPermission.TransacionarTed,
            SystemPermission.TransacionarBoleto,
            SystemPermission.TransacionarCripto,
            SystemPermission.GerarQrCode,
            SystemPermission.ConfigurarPriorizacao
        },
        
        [SystemRole.SubCliente] = new List<SystemPermission>
        {
            // Sub-cliente: permissões limitadas definidas pelo cliente principal
            SystemPermission.ViewDashboard,
            SystemPermission.ViewSaldo,
            SystemPermission.TransacionarPix // Padrão mínimo
        }
    };
    
    /// <summary>
    /// Obtém as permissões padrão para um role
    /// </summary>
    public static List<string> GetDefaultPermissions(string role)
    {
        if (Enum.TryParse<SystemRole>(role, true, out var systemRole) &&
            DefaultPermissions.TryGetValue(systemRole, out var permissions))
        {
            return permissions.Select(p => p.ToString()).ToList();
        }

        return new List<string>();
    }

    /// <summary>
    /// Obtém as permissões para um role (alias para GetDefaultPermissions)
    /// </summary>
    public static List<string> GetPermissionsForRole(string role)
    {
        return GetDefaultPermissions(role);
    }
    
    /// <summary>
    /// Verifica se um role pode gerenciar outro role
    /// </summary>
    public static bool CanManageRole(string managerRole, string targetRole)
    {
        return managerRole switch
        {
            "Admin" => true, // Admin pode gerenciar todos
            "SubAdmin" => false, // Sub-admin não pode gerenciar ninguém
            "Cliente" => targetRole == "SubCliente", // Cliente pode gerenciar apenas sub-clientes
            "SubCliente" => false, // Sub-cliente não pode gerenciar ninguém
            _ => false
        };
    }
}
