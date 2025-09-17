using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FintechPSP.UserService.DTOs;

/// <summary>
/// Request para criar uma nova conta bancária
/// </summary>
public class CriarContaRequest
{
    [JsonPropertyName("clienteId")]
    public Guid? ClienteId { get; set; } // Opcional para banking (vem do JWT), obrigatório para admin

    [JsonPropertyName("bankCode")]
    [Required(ErrorMessage = "Código do banco é obrigatório")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    [Required(ErrorMessage = "Número da conta é obrigatório")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("credentials")]
    [Required(ErrorMessage = "Credenciais são obrigatórias")]
    public ContaCredentials Credentials { get; set; } = new();

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Request para atualizar uma conta bancária
/// </summary>
public class AtualizarContaRequest
{
    [JsonPropertyName("credentials")]
    public ContaCredentials? Credentials { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>
/// Credenciais de uma conta bancária
/// </summary>
public class ContaCredentials
{
    [JsonPropertyName("clientId")]
    public string? ClientId { get; set; }

    [JsonPropertyName("clientSecret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("mtlsCert")]
    public string? MtlsCert { get; set; }

    [JsonPropertyName("additionalData")]
    public Dictionary<string, string>? AdditionalData { get; set; }
}

/// <summary>
/// Response para operações de conta
/// </summary>
public class ContaResponse
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}

/// <summary>
/// Response detalhada de uma conta
/// </summary>
public class ContaDetalhesResponse
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("accountNumber")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }

    [JsonPropertyName("credentialsTokenId")]
    public string? CredentialsTokenId { get; set; } // Apenas para admin
}

/// <summary>
/// Response para listar contas de um cliente
/// </summary>
public class ListarContasResponse
{
    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("contas")]
    public List<ContaResumo> Contas { get; set; } = new();
}

/// <summary>
/// Resumo de uma conta (sem dados sensíveis)
/// </summary>
public class ContaResumo
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }
}
