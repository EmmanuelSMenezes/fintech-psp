using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.UserService.Models;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Repositories;

/// <summary>
/// Implementação do repositório de contas bancárias
/// </summary>
public class ContaBancariaRepository : IContaBancariaRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<ContaBancariaRepository> _logger;

    public ContaBancariaRepository(IDbConnectionFactory connectionFactory, ILogger<ContaBancariaRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContaBancaria> CreateAsync(ContaBancaria conta)
    {
        const string sql = @"
            INSERT INTO contas_bancarias (id, cliente_id, bank_code, account_number, description, 
                                        credentials_token_id, is_active, created_at)
            VALUES (@Id, @ClienteId, @BankCode, @AccountNumber, @Description, 
                    @CredentialsTokenId, @IsActive, @CreatedAt)
            RETURNING id, cliente_id, bank_code, account_number, description, 
                      credentials_token_id, is_active, created_at, updated_at";

        conta.Id = Guid.NewGuid();
        conta.CreatedAt = DateTime.UtcNow;
        conta.IsActive = true;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, conta);

        return new ContaBancaria
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            AccountNumber = result.account_number,
            Description = result.description,
            CredentialsTokenId = result.credentials_token_id,
            IsActive = result.is_active,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<ContaBancaria?> GetByIdAsync(Guid contaId)
    {
        const string sql = @"
            SELECT id, cliente_id, bank_code, account_number, description, 
                   credentials_token_id, is_active, created_at, updated_at
            FROM contas_bancarias
            WHERE id = @ContaId AND is_active = true";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { ContaId = contaId });

        if (result == null) return null;

        return new ContaBancaria
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            AccountNumber = result.account_number,
            Description = result.description,
            CredentialsTokenId = result.credentials_token_id,
            IsActive = result.is_active,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<IEnumerable<ContaBancaria>> GetByClienteIdAsync(Guid clienteId)
    {
        const string sql = @"
            SELECT id, cliente_id, bank_code, account_number, description, 
                   credentials_token_id, is_active, created_at, updated_at
            FROM contas_bancarias
            WHERE cliente_id = @ClienteId AND is_active = true
            ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<dynamic>(sql, new { ClienteId = clienteId });

        return results.Select(result => new ContaBancaria
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            AccountNumber = result.account_number,
            Description = result.description,
            CredentialsTokenId = result.credentials_token_id,
            IsActive = result.is_active,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        });
    }

    public async Task<ContaBancaria> UpdateAsync(ContaBancaria conta)
    {
        const string sql = @"
            UPDATE contas_bancarias
            SET description = @Description, credentials_token_id = @CredentialsTokenId, 
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING id, cliente_id, bank_code, account_number, description, 
                      credentials_token_id, is_active, created_at, updated_at";

        conta.UpdatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, conta);

        return new ContaBancaria
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            AccountNumber = result.account_number,
            Description = result.description,
            CredentialsTokenId = result.credentials_token_id,
            IsActive = result.is_active,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<bool> DeleteAsync(Guid contaId)
    {
        const string sql = @"
            UPDATE contas_bancarias
            SET is_active = false, updated_at = @UpdatedAt
            WHERE id = @ContaId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { ContaId = contaId, UpdatedAt = DateTime.UtcNow });

        return rowsAffected > 0;
    }

    public async Task<bool> IsContaOwnedByClienteAsync(Guid contaId, Guid clienteId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM contas_bancarias
            WHERE id = @ContaId AND cliente_id = @ClienteId AND is_active = true";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ContaId = contaId, ClienteId = clienteId });

        return count > 0;
    }

    public async Task<bool> ExistsAsync(Guid clienteId, string bankCode, string accountNumber)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM contas_bancarias
            WHERE cliente_id = @ClienteId AND bank_code = @BankCode 
                  AND account_number = @AccountNumber AND is_active = true";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ClienteId = clienteId, BankCode = bankCode, AccountNumber = accountNumber });

        return count > 0;
    }
}
