using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Services;

namespace FintechPSP.UserService.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ICredentialsProtector _protector;

    private const string AccountsTable = "contas_bancarias";
    private const string TokensTable = "conta_credentials_tokens";

    public AccountRepository(IDbConnectionFactory connectionFactory, ICredentialsProtector protector)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _protector = protector ?? throw new ArgumentNullException(nameof(protector));
    }

    public async Task<(IReadOnlyList<BankAccount> contas, int total)> GetPagedAsync(int page, int limit, Guid? clienteId)
    {
        var offset = (page - 1) * limit;
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);

        string where = clienteId.HasValue ? "WHERE cliente_id = @ClienteId" : string.Empty;
        var sql = $@"SELECT id as ContaId, cliente_id as ClienteId, bank_code as BankCode, account_number as AccountNumber,
                            description as Description, credentials_token_id as CredentialsTokenId, is_active as IsActive,
                            created_at as CreatedAt, updated_at as UpdatedAt
                     FROM {AccountsTable} {where}
                     ORDER BY created_at DESC
                     LIMIT @Limit OFFSET @Offset;
                     SELECT COUNT(*) FROM {AccountsTable} {where};";

        using var multi = await conn.QueryMultipleAsync(sql, new { ClienteId = clienteId, Limit = limit, Offset = offset });
        var items = (await multi.ReadAsync<BankAccount>()).ToList();
        var total = await multi.ReadFirstAsync<int>();
        return (items, total);
    }

    public async Task<IReadOnlyList<BankAccount>> GetByClientAsync(Guid clienteId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);
        var sql = $@"SELECT id as ContaId, cliente_id as ClienteId, bank_code as BankCode, account_number as AccountNumber,
                            description as Description, credentials_token_id as CredentialsTokenId, is_active as IsActive,
                            created_at as CreatedAt, updated_at as UpdatedAt
                     FROM {AccountsTable}
                     WHERE cliente_id = @ClienteId
                     ORDER BY created_at DESC";
        var items = await conn.QueryAsync<BankAccount>(sql, new { ClienteId = clienteId });
        return items.ToList();
    }

    public async Task<BankAccount?> GetByIdAsync(Guid contaId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);
        var sql = $@"SELECT id as ContaId, cliente_id as ClienteId, bank_code as BankCode, account_number as AccountNumber,
                            description as Description, credentials_token_id as CredentialsTokenId, is_active as IsActive,
                            created_at as CreatedAt, updated_at as UpdatedAt
                     FROM {AccountsTable}
                     WHERE id = @ContaId";
        return await conn.QueryFirstOrDefaultAsync<BankAccount>(sql, new { ContaId = contaId });
    }

    public async Task<BankAccount> CreateAsync(CreateBankAccountRequest request)
    {
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);
        using var tx = conn.BeginTransaction();

        // Generate IDs first
        var contaId = Guid.NewGuid();
        var tokenId = $"acct_{Guid.NewGuid():N}";

        // Store credentials token
        var credentialsJson = JsonSerializer.Serialize(request.Credentials);
        var cipher = _protector.Encrypt(credentialsJson, out var keyId);

        var insertToken = $@"INSERT INTO {TokensTable} (token_id, conta_id, encrypted_credentials, created_at)
                             VALUES (@TokenId, @ContaId, @Encrypted, NOW())";
        await conn.ExecuteAsync(insertToken, new { TokenId = tokenId, ContaId = contaId, Encrypted = cipher }, tx);

        // Insert account
        var insertAccount = $@"INSERT INTO {AccountsTable} (id, cliente_id, bank_code, account_number, description, credentials_token_id, is_active, created_at)
                               VALUES (@ContaId, @ClienteId, @BankCode, @AccountNumber, @Description, @CredentialsTokenId, true, NOW())";
        await conn.ExecuteAsync(insertAccount, new {
            ContaId = contaId,
            ClienteId = Guid.Parse(request.ClienteId),
            request.BankCode,
            request.AccountNumber,
            request.Description,
            CredentialsTokenId = tokenId
        }, tx);

        var created = await conn.QueryFirstAsync<BankAccount>($@"SELECT id as ContaId, cliente_id as ClienteId, bank_code as BankCode, account_number as AccountNumber,
                                                                       description as Description, credentials_token_id as CredentialsTokenId, is_active as IsActive,
                                                                       created_at as CreatedAt, updated_at as UpdatedAt
                                                                FROM {AccountsTable}
                                                                WHERE id = @ContaId", new { ContaId = contaId }, tx);

        tx.Commit();
        return created;
    }

    public async Task<BankAccount> UpdateAsync(Guid contaId, UpdateBankAccountRequest request)
    {
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);
        using var tx = conn.BeginTransaction();

        // Optionally rotate credentials
        if (request.Credentials is not null)
        {
            var tokenId = $"acct_{Guid.NewGuid():N}";
            var credentialsJson = System.Text.Json.JsonSerializer.Serialize(request.Credentials);
            var cipher = _protector.Encrypt(credentialsJson, out var keyId);
            var insertToken = $@"INSERT INTO {TokensTable} (token_id, conta_id, encrypted_credentials, created_at)
                                 VALUES (@TokenId, @ContaId, @Encrypted, NOW())";
            await conn.ExecuteAsync(insertToken, new { TokenId = tokenId, ContaId = contaId, Encrypted = cipher }, tx);

            var updateToken = $@"UPDATE {AccountsTable} SET credentials_token_id = @TokenId, updated_at = NOW() WHERE id = @ContaId";
            await conn.ExecuteAsync(updateToken, new { TokenId = tokenId, ContaId = contaId }, tx);
        }

        if (request.Description is not null)
        {
            await conn.ExecuteAsync($@"UPDATE {AccountsTable} SET description = @Description, updated_at = NOW() WHERE id = @ContaId",
                new { request.Description, ContaId = contaId }, tx);
        }
        if (request.IsActive.HasValue)
        {
            await conn.ExecuteAsync($@"UPDATE {AccountsTable} SET is_active = @IsActive, updated_at = NOW() WHERE id = @ContaId",
                new { IsActive = request.IsActive.Value, ContaId = contaId }, tx);
        }

        var updated = await conn.QueryFirstAsync<BankAccount>($@"SELECT id as ContaId, cliente_id as ClienteId, bank_code as BankCode, account_number as AccountNumber,
                                                                       description as Description, credentials_token_id as CredentialsTokenId, is_active as IsActive,
                                                                       created_at as CreatedAt, updated_at as UpdatedAt
                                                                FROM {AccountsTable}
                                                                WHERE id = @ContaId", new { ContaId = contaId }, tx);
        tx.Commit();
        return updated;
    }

    public async Task<bool> DeleteAsync(Guid contaId)
    {
        using var conn = _connectionFactory.CreateConnection();
        await EnsureOpenAsync(conn);
        var sql = $"DELETE FROM {AccountsTable} WHERE id = @ContaId";
        var rows = await conn.ExecuteAsync(sql, new { ContaId = contaId });
        return rows > 0;
    }

    private static async Task EnsureOpenAsync(IDbConnection conn)
    {
        if (conn.State != ConnectionState.Open)
        {
            if (conn is Npgsql.NpgsqlConnection npg)
                await npg.OpenAsync();
            else
                conn.Open();
        }
    }
}

