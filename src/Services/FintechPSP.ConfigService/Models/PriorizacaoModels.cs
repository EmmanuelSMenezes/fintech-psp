using System;
using System.Collections.Generic;

namespace FintechPSP.ConfigService.Models;

/// <summary>
/// Configuração de priorização de contas
/// </summary>
public class ConfiguracaoPriorizacao
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public List<PrioridadeConta> Prioridades { get; set; } = new();
    public decimal TotalPercentual { get; set; }
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Prioridade de uma conta específica
/// </summary>
public class PrioridadeConta
{
    public Guid ContaId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public decimal Percentual { get; set; }
}

/// <summary>
/// Banco personalizado configurado pelo cliente
/// </summary>
public class BancoPersonalizado
{
    public Guid Id { get; set; }
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? CredentialsTemplate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Banco padrão do sistema
/// </summary>
public class BancoDefault
{
    public string BankCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSupported { get; set; }

    public static List<BancoDefault> GetBancosDefault()
    {
        return new List<BancoDefault>
        {
            new() { BankCode = "STARK", Name = "Stark Bank", IsSupported = true },
            new() { BankCode = "SICOOB", Name = "Sicoob", IsSupported = true },
            new() { BankCode = "GENIAL", Name = "Banco Genial", IsSupported = true },
            new() { BankCode = "EFI", Name = "Efí (Gerencianet)", IsSupported = true },
            new() { BankCode = "CELCOIN", Name = "Celcoin", IsSupported = true }
        };
    }
}
