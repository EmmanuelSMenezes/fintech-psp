using System;
using System.Collections.Generic;
using FintechPSP.ConfigService.DTOs;
using FintechPSP.Shared.Domain.Events;
using MediatR;

namespace FintechPSP.ConfigService.Commands;

/// <summary>
/// Comando para configurar priorização de contas
/// </summary>
public class ConfigurarPriorizacaoCommand : IRequest<PriorizacaoResponse>
{
    public Guid ClienteId { get; set; }
    public List<PrioridadeContaRequest> Prioridades { get; set; } = new();

    public ConfigurarPriorizacaoCommand() { }

    public ConfigurarPriorizacaoCommand(Guid clienteId, List<PrioridadeContaRequest> prioridades)
    {
        ClienteId = clienteId;
        Prioridades = prioridades;
    }
}

/// <summary>
/// Comando para configurar banco personalizado
/// </summary>
public class ConfigurarBancoCommand : IRequest<BancoResponse>
{
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string? Endpoint { get; set; }
    public string? CredentialsTemplate { get; set; }

    public ConfigurarBancoCommand() { }

    public ConfigurarBancoCommand(Guid clienteId, string bankCode, string? endpoint = null, string? credentialsTemplate = null)
    {
        ClienteId = clienteId;
        BankCode = bankCode;
        Endpoint = endpoint;
        CredentialsTemplate = credentialsTemplate;
    }
}

/// <summary>
/// Comando para atualizar banco personalizado
/// </summary>
public class AtualizarBancoCommand : IRequest<BancoResponse>
{
    public Guid BankId { get; set; }
    public Guid ClienteId { get; set; }
    public string? Endpoint { get; set; }
    public string? CredentialsTemplate { get; set; }

    public AtualizarBancoCommand() { }

    public AtualizarBancoCommand(Guid bankId, Guid clienteId, string? endpoint = null, string? credentialsTemplate = null)
    {
        BankId = bankId;
        ClienteId = clienteId;
        Endpoint = endpoint;
        CredentialsTemplate = credentialsTemplate;
    }
}

/// <summary>
/// Comando para remover banco personalizado
/// </summary>
public class RemoverBancoCommand : IRequest<BancoResponse>
{
    public Guid BankId { get; set; }
    public Guid ClienteId { get; set; }

    public RemoverBancoCommand() { }

    public RemoverBancoCommand(Guid bankId, Guid clienteId)
    {
        BankId = bankId;
        ClienteId = clienteId;
    }
}
