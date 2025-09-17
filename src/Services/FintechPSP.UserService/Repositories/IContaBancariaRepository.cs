using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.UserService.Models;

namespace FintechPSP.UserService.Repositories;

/// <summary>
/// Interface para repositório de contas bancárias
/// </summary>
public interface IContaBancariaRepository
{
    /// <summary>
    /// Cria uma nova conta bancária
    /// </summary>
    Task<ContaBancaria> CreateAsync(ContaBancaria conta);

    /// <summary>
    /// Obtém uma conta bancária por ID
    /// </summary>
    Task<ContaBancaria?> GetByIdAsync(Guid contaId);

    /// <summary>
    /// Obtém todas as contas de um cliente
    /// </summary>
    Task<IEnumerable<ContaBancaria>> GetByClienteIdAsync(Guid clienteId);

    /// <summary>
    /// Atualiza uma conta bancária
    /// </summary>
    Task<ContaBancaria> UpdateAsync(ContaBancaria conta);

    /// <summary>
    /// Remove uma conta bancária (soft delete)
    /// </summary>
    Task<bool> DeleteAsync(Guid contaId);

    /// <summary>
    /// Verifica se uma conta pertence a um cliente
    /// </summary>
    Task<bool> IsContaOwnedByClienteAsync(Guid contaId, Guid clienteId);

    /// <summary>
    /// Verifica se já existe uma conta com o mesmo bankCode e accountNumber para o cliente
    /// </summary>
    Task<bool> ExistsAsync(Guid clienteId, string bankCode, string accountNumber);
}

/// <summary>
/// Interface para repositório de tokens de credenciais
/// </summary>
public interface ICredentialsTokenRepository
{
    /// <summary>
    /// Cria um novo token de credenciais
    /// </summary>
    Task<ContaCredentialsToken> CreateAsync(ContaCredentialsToken token);

    /// <summary>
    /// Obtém um token por ID
    /// </summary>
    Task<ContaCredentialsToken?> GetByTokenIdAsync(string tokenId);

    /// <summary>
    /// Atualiza um token de credenciais
    /// </summary>
    Task<ContaCredentialsToken> UpdateAsync(ContaCredentialsToken token);

    /// <summary>
    /// Remove um token de credenciais
    /// </summary>
    Task<bool> DeleteAsync(string tokenId);

    /// <summary>
    /// Obtém o token associado a uma conta
    /// </summary>
    Task<ContaCredentialsToken?> GetByContaIdAsync(Guid contaId);
}
