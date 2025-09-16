using System;
using System.Threading.Tasks;
using FintechPSP.AuthService.Models;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Repository para clientes OAuth
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Busca cliente por client_id
    /// </summary>
    Task<Client?> GetByClientIdAsync(string clientId);

    /// <summary>
    /// Busca cliente por ID
    /// </summary>
    Task<Client?> GetByIdAsync(Guid id);

    /// <summary>
    /// Cria um novo cliente
    /// </summary>
    Task<Client> CreateAsync(Client client);

    /// <summary>
    /// Atualiza um cliente
    /// </summary>
    Task<Client> UpdateAsync(Client client);

    /// <summary>
    /// Desativa um cliente
    /// </summary>
    Task DeactivateAsync(Guid id);
}
