using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FintechPSP.IntegrationService.Services;

/// <summary>
/// Interface para serviço de dados de contas bancárias
/// </summary>
public interface IAccountDataService
{
    /// <summary>
    /// Obtém uma conta por ID
    /// </summary>
    Task<AccountData?> GetAccountByIdAsync(Guid contaId);

    /// <summary>
    /// Obtém todas as contas de um cliente
    /// </summary>
    Task<List<AccountData>> GetAccountsByClienteIdAsync(Guid clienteId);

    /// <summary>
    /// Obtém credenciais descriptografadas de uma conta
    /// </summary>
    Task<AccountCredentials?> GetAccountCredentialsAsync(string credentialsTokenId);
}

/// <summary>
/// Interface para serviço de configuração de prioridade
/// </summary>
public interface IPriorityConfigService
{
    /// <summary>
    /// Obtém a configuração de prioridade de um cliente
    /// </summary>
    Task<PriorityConfiguration?> GetPriorityConfigAsync(Guid clienteId);
}

/// <summary>
/// Dados de uma conta bancária
/// </summary>
public class AccountData
{
    public Guid ContaId { get; set; }
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CredentialsTokenId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Credenciais de uma conta bancária
/// </summary>
public class AccountCredentials
{
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? MtlsCert { get; set; }
    public Dictionary<string, string>? AdditionalData { get; set; }
}

/// <summary>
/// Configuração de prioridade de um cliente
/// </summary>
public class PriorityConfiguration
{
    public Guid ConfigId { get; set; }
    public Guid ClienteId { get; set; }
    public List<AccountPriority> Prioridades { get; set; } = new();
    public decimal TotalPercentual { get; set; }
    public bool IsValid { get; set; }
}

/// <summary>
/// Prioridade de uma conta específica
/// </summary>
public class AccountPriority
{
    public Guid ContaId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}
