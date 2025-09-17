using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintechPSP.IntegrationService.Services;

/// <summary>
/// Interface para serviço de roteamento ponderado
/// </summary>
public interface IRoutingService
{
    /// <summary>
    /// Seleciona a melhor conta para uma transação baseada na priorização configurada
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="bankCode">Código do banco (opcional - se especificado, filtra apenas contas deste banco)</param>
    /// <param name="amount">Valor da transação (para validações futuras)</param>
    /// <returns>Informações da conta selecionada</returns>
    Task<SelectedAccountInfo?> SelectAccountForTransactionAsync(Guid clienteId, string? bankCode = null, decimal? amount = null);

    /// <summary>
    /// Obtém todas as contas disponíveis para um cliente com suas prioridades
    /// </summary>
    /// <param name="clienteId">ID do cliente</param>
    /// <returns>Lista de contas com prioridades</returns>
    Task<List<AccountWithPriority>> GetAccountsWithPriorityAsync(Guid clienteId);

    /// <summary>
    /// Valida se uma conta específica pode ser usada para uma transação
    /// </summary>
    /// <param name="contaId">ID da conta</param>
    /// <param name="clienteId">ID do cliente</param>
    /// <param name="amount">Valor da transação</param>
    /// <returns>True se a conta pode ser usada</returns>
    Task<bool> ValidateAccountForTransactionAsync(Guid contaId, Guid clienteId, decimal amount);
}

/// <summary>
/// Informações da conta selecionada para roteamento
/// </summary>
public class SelectedAccountInfo
{
    public Guid ContaId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CredentialsTokenId { get; set; } = string.Empty;
    public decimal Priority { get; set; }
    public string SelectionReason { get; set; } = string.Empty; // Para auditoria
}

/// <summary>
/// Conta com informações de prioridade
/// </summary>
public class AccountWithPriority
{
    public Guid ContaId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Priority { get; set; } = 0; // 0 se não configurado
    public bool HasPriorityConfig { get; set; } = false;
}
