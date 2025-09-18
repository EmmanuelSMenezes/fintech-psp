using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.AuthService.Models;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Interface para repositório de usuários do sistema
/// </summary>
public interface ISystemUserRepository
{
    /// <summary>
    /// Busca usuário por email
    /// </summary>
    /// <param name="email">Email do usuário</param>
    /// <returns>Usuário encontrado ou null</returns>
    Task<SystemUser?> GetByEmailAsync(string email);

    /// <summary>
    /// Busca usuário por ID
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>Usuário encontrado ou null</returns>
    Task<SystemUser?> GetByIdAsync(Guid id);

    /// <summary>
    /// Lista todos os usuários
    /// </summary>
    /// <returns>Lista de usuários</returns>
    Task<IEnumerable<SystemUser>> GetAllAsync();

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    /// <param name="user">Dados do usuário</param>
    /// <returns>Usuário criado</returns>
    Task<SystemUser> CreateAsync(SystemUser user);

    /// <summary>
    /// Atualiza um usuário existente
    /// </summary>
    /// <param name="user">Dados do usuário</param>
    /// <returns>Usuário atualizado</returns>
    Task<SystemUser> UpdateAsync(SystemUser user);

    /// <summary>
    /// Exclui um usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se excluído com sucesso</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Atualiza a data do último login
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se atualizado com sucesso</returns>
    Task<bool> UpdateLastLoginAsync(Guid id);
}
