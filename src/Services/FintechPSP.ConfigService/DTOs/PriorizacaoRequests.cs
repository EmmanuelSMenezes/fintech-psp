using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.ConfigService.DTOs;

/// <summary>
/// Request para configurar priorização de contas
/// </summary>
public class ConfigurarPriorizacaoRequest
{
    [JsonPropertyName("clienteId")]
    public Guid? ClienteId { get; set; } // Opcional para banking (vem do JWT), obrigatório para admin

    [JsonPropertyName("prioridades")]
    [Required(ErrorMessage = "Lista de prioridades é obrigatória")]
    public List<PrioridadeContaRequest> Prioridades { get; set; } = new();
}

/// <summary>
/// Prioridade de uma conta específica
/// </summary>
public class PrioridadeContaRequest
{
    [JsonPropertyName("contaId")]
    [Required(ErrorMessage = "ID da conta é obrigatório")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("bankCode")]
    [Required(ErrorMessage = "Código do banco é obrigatório")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("percentual")]
    [Required(ErrorMessage = "Percentual é obrigatório")]
    [Range(0.01, 100.0, ErrorMessage = "Percentual deve estar entre 0.01 e 100")]
    public decimal Percentual { get; set; }
}

/// <summary>
/// Response para operações de priorização
/// </summary>
public class PriorizacaoResponse
{
    [JsonPropertyName("configId")]
    public Guid ConfigId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("validationErrors")]
    public List<string>? ValidationErrors { get; set; }
}

/// <summary>
/// Response detalhada da configuração de priorização
/// </summary>
public class PriorizacaoDetalhesResponse
{
    [JsonPropertyName("configId")]
    public Guid ConfigId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("prioridades")]
    public List<PrioridadeContaResponse> Prioridades { get; set; } = new();

    [JsonPropertyName("totalPercentual")]
    public decimal TotalPercentual { get; set; }

    [JsonPropertyName("isValid")]
    public bool IsValid { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Response de prioridade de conta
/// </summary>
public class PrioridadeContaResponse
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("percentual")]
    public decimal Percentual { get; set; }

    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; } // Para exibição

    [JsonPropertyName("description")]
    public string? Description { get; set; } // Para exibição
}

/// <summary>
/// Request para configurar banco personalizado
/// </summary>
public class ConfigurarBancoRequest
{
    [JsonPropertyName("clienteId")]
    public Guid? ClienteId { get; set; } // Opcional para banking (vem do JWT), obrigatório para admin

    [JsonPropertyName("bankCode")]
    [Required(ErrorMessage = "Código do banco é obrigatório")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("credentialsTemplate")]
    public string? CredentialsTemplate { get; set; }
}

/// <summary>
/// Response para operações de banco
/// </summary>
public class BancoResponse
{
    [JsonPropertyName("bankId")]
    public Guid BankId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Response detalhada de banco personalizado
/// </summary>
public class BancoDetalhesResponse
{
    [JsonPropertyName("bankId")]
    public Guid BankId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("credentialsTemplate")]
    public string? CredentialsTemplate { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Response para listar bancos disponíveis
/// </summary>
public class ListarBancosResponse
{
    [JsonPropertyName("bancosDefault")]
    public List<BancoDefaultResponse> BancosDefault { get; set; } = new();

    [JsonPropertyName("bancosPersonalizados")]
    public List<BancoDetalhesResponse> BancosPersonalizados { get; set; } = new();
}

/// <summary>
/// Banco padrão do sistema
/// </summary>
public class BancoDefaultResponse
{
    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isSupported")]
    public bool IsSupported { get; set; }
}
