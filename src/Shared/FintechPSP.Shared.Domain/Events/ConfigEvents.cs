using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Domain.Events;

/// <summary>
/// Evento disparado quando a priorização de contas é atualizada
/// </summary>
public class PriorizacaoAtualizada : DomainEvent
{
    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("configId")]
    public Guid ConfigId { get; set; }

    [JsonPropertyName("prioridades")]
    public List<PrioridadeConta> Prioridades { get; set; } = new();

    public PriorizacaoAtualizada() { }

    public PriorizacaoAtualizada(Guid clienteId, Guid configId, List<PrioridadeConta> prioridades)
    {
        ClienteId = clienteId;
        ConfigId = configId;
        Prioridades = prioridades;
    }
}

/// <summary>
/// Evento disparado quando um banco personalizado é adicionado
/// </summary>
public class BancoPersonalizadoAdicionado : DomainEvent
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

    public BancoPersonalizadoAdicionado() { }

    public BancoPersonalizadoAdicionado(Guid bankId, Guid clienteId, string bankCode, 
        string? endpoint = null, string? credentialsTemplate = null)
    {
        BankId = bankId;
        ClienteId = clienteId;
        BankCode = bankCode;
        Endpoint = endpoint;
        CredentialsTemplate = credentialsTemplate;
    }
}

/// <summary>
/// Evento disparado quando um banco personalizado é atualizado
/// </summary>
public class BancoPersonalizadoAtualizado : DomainEvent
{
    [JsonPropertyName("bankId")]
    public Guid BankId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("endpoint")]
    public string? Endpoint { get; set; }

    [JsonPropertyName("credentialsTemplate")]
    public string? CredentialsTemplate { get; set; }

    public BancoPersonalizadoAtualizado() { }

    public BancoPersonalizadoAtualizado(Guid bankId, Guid clienteId, string? endpoint = null, 
        string? credentialsTemplate = null)
    {
        BankId = bankId;
        ClienteId = clienteId;
        Endpoint = endpoint;
        CredentialsTemplate = credentialsTemplate;
    }
}

/// <summary>
/// Evento disparado quando um banco personalizado é removido
/// </summary>
public class BancoPersonalizadoRemovido : DomainEvent
{
    [JsonPropertyName("bankId")]
    public Guid BankId { get; set; }

    [JsonPropertyName("clienteId")]
    public Guid ClienteId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    public BancoPersonalizadoRemovido() { }

    public BancoPersonalizadoRemovido(Guid bankId, Guid clienteId, string bankCode)
    {
        BankId = bankId;
        ClienteId = clienteId;
        BankCode = bankCode;
    }
}

/// <summary>
/// Representa uma prioridade de conta na configuração de roteamento
/// </summary>
public class PrioridadeConta
{
    [JsonPropertyName("contaId")]
    public Guid ContaId { get; set; }

    [JsonPropertyName("bankCode")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("percentual")]
    public decimal Percentual { get; set; }

    public PrioridadeConta() { }

    public PrioridadeConta(Guid contaId, string bankCode, decimal percentual)
    {
        ContaId = contaId;
        BankCode = bankCode;
        Percentual = percentual;
    }
}
