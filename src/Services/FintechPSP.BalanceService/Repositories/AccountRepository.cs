using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.BalanceService.Models;
using FintechPSP.Shared.Domain.ValueObjects;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.BalanceService.Repositories;

/// <summary>
/// Implementação do repository de contas usando Dapper
/// </summary>
public class AccountRepository : IAccountRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AccountRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Account?> GetByClientIdAsync(Guid clientId, string? accountId = null)
    {
        var sql = @"
            SELECT client_id as ClientId, account_id as AccountId,
                   balance as AvailableBalance, 0.00 as BlockedBalance,
                   currency, created_at as CreatedAt, updated_at as LastUpdated
            FROM accounts
            WHERE client_id = @ClientId";

        object parameters = new { ClientId = clientId };

        if (!string.IsNullOrEmpty(accountId))
        {
            sql += " AND account_id = @AccountId";
            parameters = new { ClientId = clientId, AccountId = accountId };
        }

        sql += " LIMIT 1";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync(sql, parameters);
        
        return result != null ? MapToAccount(result) : null;
    }

    public async Task<Account> CreateAsync(Account account)
    {
        const string sql = @"
            INSERT INTO accounts (client_id, account_id, balance, currency, created_at, updated_at)
            VALUES (@ClientId, @AccountId, @Balance, @Currency, @CreatedAt, @UpdatedAt)";

        var parameters = new
        {
            account.ClientId,
            account.AccountId,
            Balance = account.AvailableBalance.Amount,
            Currency = account.AvailableBalance.Currency,
            account.CreatedAt,
            UpdatedAt = account.LastUpdated
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return account;
    }

    public async Task<Account> UpdateAsync(Account account)
    {
        const string sql = @"
            UPDATE accounts
            SET balance = @Balance, updated_at = @UpdatedAt
            WHERE client_id = @ClientId AND account_id = @AccountId";

        var parameters = new
        {
            Balance = account.AvailableBalance.Amount,
            UpdatedAt = account.LastUpdated,
            account.ClientId,
            account.AccountId
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return account;
    }

    public async Task<IEnumerable<Account>> GetAccountsByClientIdAsync(Guid clientId)
    {
        const string sql = @"
            SELECT client_id as ClientId, account_id as AccountId,
                   balance as AvailableBalance, 0.00 as BlockedBalance,
                   currency, created_at as CreatedAt, updated_at as LastUpdated
            FROM accounts
            WHERE client_id = @ClientId
            ORDER BY created_at";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, new { ClientId = clientId });
        
        return results.Select(MapToAccount);
    }

    private static Account MapToAccount(dynamic result)
    {
        // Usar reflection para criar instância privada
        var account = (Account)Activator.CreateInstance(typeof(Account), true)!;

        // Mapear propriedades usando reflection
        var type = typeof(Account);

        // Verificar se os valores não são null antes de converter
        if (result.ClientId != null)
            type.GetProperty("ClientId")?.SetValue(account, (Guid)result.ClientId);

        if (result.AccountId != null)
            type.GetProperty("AccountId")?.SetValue(account, (string)result.AccountId);

        if (result.AvailableBalance != null && result.currency != null)
            type.GetProperty("AvailableBalance")?.SetValue(account, new Money((decimal)result.AvailableBalance, (string)result.currency));

        if (result.BlockedBalance != null && result.currency != null)
            type.GetProperty("BlockedBalance")?.SetValue(account, new Money((decimal)result.BlockedBalance, (string)result.currency));

        if (result.CreatedAt != null)
            type.GetProperty("CreatedAt")?.SetValue(account, (DateTime)result.CreatedAt);

        if (result.LastUpdated != null)
            type.GetProperty("LastUpdated")?.SetValue(account, (DateTime)result.LastUpdated);

        return account;
    }
}
