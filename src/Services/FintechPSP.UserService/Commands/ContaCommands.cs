using System;
using FintechPSP.UserService.DTOs;
using MediatR;

namespace FintechPSP.UserService.Commands;

/// <summary>
/// Comando para criar uma nova conta bancária
/// </summary>
public class CriarContaCommand : IRequest<ContaResponse>
{
    public Guid ClienteId { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public ContaCredentials Credentials { get; set; } = new();
    public string Description { get; set; } = string.Empty;

    public CriarContaCommand() { }

    public CriarContaCommand(Guid clienteId, string bankCode, string accountNumber, 
        ContaCredentials credentials, string description)
    {
        ClienteId = clienteId;
        BankCode = bankCode;
        AccountNumber = accountNumber;
        Credentials = credentials;
        Description = description;
    }
}

/// <summary>
/// Comando para atualizar uma conta bancária
/// </summary>
public class AtualizarContaCommand : IRequest<ContaResponse>
{
    public Guid ContaId { get; set; }
    public Guid ClienteId { get; set; }
    public ContaCredentials? Credentials { get; set; }
    public string? Description { get; set; }

    public AtualizarContaCommand() { }

    public AtualizarContaCommand(Guid contaId, Guid clienteId, ContaCredentials? credentials = null, 
        string? description = null)
    {
        ContaId = contaId;
        ClienteId = clienteId;
        Credentials = credentials;
        Description = description;
    }
}

/// <summary>
/// Comando para remover uma conta bancária
/// </summary>
public class RemoverContaCommand : IRequest<ContaResponse>
{
    public Guid ContaId { get; set; }
    public Guid ClienteId { get; set; }

    public RemoverContaCommand() { }

    public RemoverContaCommand(Guid contaId, Guid clienteId)
    {
        ContaId = contaId;
        ClienteId = clienteId;
    }
}
