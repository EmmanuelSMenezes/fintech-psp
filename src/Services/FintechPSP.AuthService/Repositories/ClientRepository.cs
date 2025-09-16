using System;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.AuthService.Models;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Implementação do repository de clientes usando Dapper
/// </summary>
public class ClientRepository : IClientRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ClientRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Client?> GetByClientIdAsync(string clientId)
    {
        const string sql = @"
            SELECT id, client_id as ClientId, client_secret as ClientSecret, 
                   name, allowed_scopes as AllowedScopes, is_active as IsActive, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM clients 
            WHERE client_id = @ClientId AND is_active = true";

        using var connection = _connectionFactory.CreateConnection();
        var client = await connection.QuerySingleOrDefaultAsync<Client>(sql, new { ClientId = clientId });
        
        if (client != null)
        {
            // Converter string JSON para array (simplificado)
            client.AllowedScopes = client.AllowedScopes?.FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
        }

        return client;
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, client_id as ClientId, client_secret as ClientSecret, 
                   name, allowed_scopes as AllowedScopes, is_active as IsActive, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM clients 
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var client = await connection.QuerySingleOrDefaultAsync<Client>(sql, new { Id = id });
        
        if (client != null)
        {
            client.AllowedScopes = client.AllowedScopes?.FirstOrDefault()?.Split(',') ?? Array.Empty<string>();
        }

        return client;
    }

    public async Task<Client> CreateAsync(Client client)
    {
        const string sql = @"
            INSERT INTO clients (id, client_id, client_secret, name, allowed_scopes, is_active, created_at)
            VALUES (@Id, @ClientId, @ClientSecret, @Name, @AllowedScopes, @IsActive, @CreatedAt)
            RETURNING id, client_id as ClientId, client_secret as ClientSecret, 
                      name, allowed_scopes as AllowedScopes, is_active as IsActive, 
                      created_at as CreatedAt, updated_at as UpdatedAt";

        client.Id = Guid.NewGuid();
        client.CreatedAt = DateTime.UtcNow;
        client.IsActive = true;

        var parameters = new
        {
            client.Id,
            client.ClientId,
            client.ClientSecret,
            client.Name,
            AllowedScopes = string.Join(",", client.AllowedScopes),
            client.IsActive,
            client.CreatedAt
        };

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<Client>(sql, parameters);
        
        result.AllowedScopes = client.AllowedScopes;
        return result;
    }

    public async Task<Client> UpdateAsync(Client client)
    {
        const string sql = @"
            UPDATE clients 
            SET client_secret = @ClientSecret, name = @Name, allowed_scopes = @AllowedScopes, 
                is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING id, client_id as ClientId, client_secret as ClientSecret, 
                      name, allowed_scopes as AllowedScopes, is_active as IsActive, 
                      created_at as CreatedAt, updated_at as UpdatedAt";

        client.UpdatedAt = DateTime.UtcNow;

        var parameters = new
        {
            client.Id,
            client.ClientSecret,
            client.Name,
            AllowedScopes = string.Join(",", client.AllowedScopes),
            client.IsActive,
            client.UpdatedAt
        };

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<Client>(sql, parameters);
        
        result.AllowedScopes = client.AllowedScopes;
        return result;
    }

    public async Task DeactivateAsync(Guid id)
    {
        const string sql = @"
            UPDATE clients 
            SET is_active = false, updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
    }
}
