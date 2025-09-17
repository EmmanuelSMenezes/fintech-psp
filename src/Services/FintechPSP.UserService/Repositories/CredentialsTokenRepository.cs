using System;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.UserService.Models;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Repositories;

/// <summary>
/// Implementação do repositório de tokens de credenciais
/// </summary>
public class CredentialsTokenRepository : ICredentialsTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CredentialsTokenRepository> _logger;

    public CredentialsTokenRepository(IDbConnectionFactory connectionFactory, ILogger<CredentialsTokenRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContaCredentialsToken> CreateAsync(ContaCredentialsToken token)
    {
        const string sql = @"
            INSERT INTO conta_credentials_tokens (token_id, conta_id, encrypted_credentials, created_at)
            VALUES (@TokenId, @ContaId, @EncryptedCredentials, @CreatedAt)
            RETURNING token_id, conta_id, encrypted_credentials, created_at, updated_at";

        token.CreatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, token);

        return new ContaCredentialsToken
        {
            TokenId = result.token_id,
            ContaId = result.conta_id,
            EncryptedCredentials = result.encrypted_credentials,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<ContaCredentialsToken?> GetByTokenIdAsync(string tokenId)
    {
        const string sql = @"
            SELECT token_id, conta_id, encrypted_credentials, created_at, updated_at
            FROM conta_credentials_tokens
            WHERE token_id = @TokenId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { TokenId = tokenId });

        if (result == null) return null;

        return new ContaCredentialsToken
        {
            TokenId = result.token_id,
            ContaId = result.conta_id,
            EncryptedCredentials = result.encrypted_credentials,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<ContaCredentialsToken> UpdateAsync(ContaCredentialsToken token)
    {
        const string sql = @"
            UPDATE conta_credentials_tokens
            SET encrypted_credentials = @EncryptedCredentials, updated_at = @UpdatedAt
            WHERE token_id = @TokenId
            RETURNING token_id, conta_id, encrypted_credentials, created_at, updated_at";

        token.UpdatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, token);

        return new ContaCredentialsToken
        {
            TokenId = result.token_id,
            ContaId = result.conta_id,
            EncryptedCredentials = result.encrypted_credentials,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<bool> DeleteAsync(string tokenId)
    {
        const string sql = @"
            DELETE FROM conta_credentials_tokens
            WHERE token_id = @TokenId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { TokenId = tokenId });

        return rowsAffected > 0;
    }

    public async Task<ContaCredentialsToken?> GetByContaIdAsync(Guid contaId)
    {
        const string sql = @"
            SELECT token_id, conta_id, encrypted_credentials, created_at, updated_at
            FROM conta_credentials_tokens
            WHERE conta_id = @ContaId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { ContaId = contaId });

        if (result == null) return null;

        return new ContaCredentialsToken
        {
            TokenId = result.token_id,
            ContaId = result.conta_id,
            EncryptedCredentials = result.encrypted_credentials,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }
}
