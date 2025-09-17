using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.ConfigService.Models;

namespace FintechPSP.ConfigService.Repositories;

/// <summary>
/// Interface para repositório de configurações de priorização
/// </summary>
public interface IPriorizacaoRepository
{
    /// <summary>
    /// Cria ou atualiza uma configuração de priorização
    /// </summary>
    Task<ConfiguracaoPriorizacao> UpsertAsync(ConfiguracaoPriorizacao configuracao);

    /// <summary>
    /// Obtém a configuração de priorização de um cliente
    /// </summary>
    Task<ConfiguracaoPriorizacao?> GetByClienteIdAsync(Guid clienteId);

    /// <summary>
    /// Remove a configuração de priorização de um cliente
    /// </summary>
    Task<bool> DeleteByClienteIdAsync(Guid clienteId);
}

/// <summary>
/// Interface para repositório de bancos personalizados
/// </summary>
public interface IBancoPersonalizadoRepository
{
    /// <summary>
    /// Cria um novo banco personalizado
    /// </summary>
    Task<BancoPersonalizado> CreateAsync(BancoPersonalizado banco);

    /// <summary>
    /// Obtém um banco personalizado por ID
    /// </summary>
    Task<BancoPersonalizado?> GetByIdAsync(Guid bankId);

    /// <summary>
    /// Obtém todos os bancos personalizados de um cliente
    /// </summary>
    Task<IEnumerable<BancoPersonalizado>> GetByClienteIdAsync(Guid clienteId);

    /// <summary>
    /// Atualiza um banco personalizado
    /// </summary>
    Task<BancoPersonalizado> UpdateAsync(BancoPersonalizado banco);

    /// <summary>
    /// Remove um banco personalizado
    /// </summary>
    Task<bool> DeleteAsync(Guid bankId);

    /// <summary>
    /// Verifica se um banco pertence a um cliente
    /// </summary>
    Task<bool> IsBancoOwnedByClienteAsync(Guid bankId, Guid clienteId);

    /// <summary>
    /// Verifica se já existe um banco com o mesmo código para o cliente
    /// </summary>
    Task<bool> ExistsAsync(Guid clienteId, string bankCode);
}
