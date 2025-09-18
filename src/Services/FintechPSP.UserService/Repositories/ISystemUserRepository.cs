using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.UserService.Models;

namespace FintechPSP.UserService.Repositories;

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
    /// Lista usuários com paginação
    /// </summary>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Lista de usuários e total</returns>
    Task<(IEnumerable<SystemUser> users, int total)> GetPagedAsync(int page, int pageSize);

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
    /// Atualiza último login do usuário
    /// </summary>
    /// <param name="id">ID do usuário</param>
    /// <returns>True se atualizado com sucesso</returns>
    Task<bool> UpdateLastLoginAsync(Guid id);

    /// <summary>
    /// Verifica se email já existe
    /// </summary>
    /// <param name="email">Email para verificar</param>
    /// <returns>True se email já existe</returns>
    Task<bool> EmailExistsAsync(string email);
}
