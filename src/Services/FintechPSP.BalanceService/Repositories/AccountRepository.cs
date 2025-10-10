using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.BalanceService.Models;
using FintechPSP.Shared.Domain.ValueObjects;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.BalanceService.Repositories;

// DTO para mapeamento explícito do Dapper
public class AccountDto
{
    public Guid ClientId { get; set; }
    public string AccountId { get; set; } = string.Empty;
    public decimal AvailableBalance { get; set; }
    public decimal BlockedBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastUpdated { get; set; }
}

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
        Console.WriteLine($"🔍 DEBUG AccountRepository.GetByClientIdAsync - ClientId: {clientId}, AccountId: {accountId}");

        var sql = @"
            SELECT client_id as ClientId, account_id as AccountId,
                   available_balance as AvailableBalance, blocked_balance as BlockedBalance,
                   currency, created_at as CreatedAt, last_updated as LastUpdated
            FROM accounts
            WHERE client_id = @ClientId";

        object parameters = new { ClientId = clientId };

        if (!string.IsNullOrEmpty(accountId))
        {
            sql += " AND account_id = @AccountId";
            parameters = new { ClientId = clientId, AccountId = accountId };
        }

        sql += " LIMIT 1";

        Console.WriteLine($"🔍 DEBUG SQL: {sql}");
        Console.WriteLine($"🔍 DEBUG Parameters: ClientId={clientId}");

        using var connection = _connectionFactory.CreateConnection();

        Console.WriteLine($"🔍 DEBUG ANTES QuerySingleOrDefaultAsync<AccountDto>");

        // Usar mapeamento explícito em vez de dynamic
        var result = await connection.QuerySingleOrDefaultAsync<AccountDto>(sql, parameters);

        Console.WriteLine($"🔍 DEBUG DEPOIS QuerySingleOrDefaultAsync - Result: {(result != null ? "FOUND" : "NOT FOUND")}");

        if (result != null)
        {
            Console.WriteLine($"🔍 DEBUG AccountDto - ClientId: {result.ClientId}, AccountId: '{result.AccountId}'");

            var account = MapFromDto(result);

            Console.WriteLine($"🔍 DEBUG Account final - AccountId: '{account.AccountId}'");
            return account;
        }

        return null;
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
        var results = await connection.QueryAsync<AccountDto>(sql, new { ClientId = clientId });

        return results.Select(MapFromDto);
    }

    public async Task<bool> DebitAsync(Guid clientId, decimal amount, string description)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                UPDATE accounts
                SET available_balance = available_balance - @Amount,
                    last_updated = @UpdatedAt
                WHERE client_id = @ClientId";

            var parameters = new
            {
                ClientId = clientId,
                Amount = amount,
                UpdatedAt = DateTime.UtcNow
            };

            var rowsAffected = await connection.ExecuteAsync(sql, parameters);

            Console.WriteLine($"💸 DEBUG DebitAsync - ClientId: {clientId}, Amount: {amount}, Rows affected: {rowsAffected}");

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR DebitAsync - ClientId: {clientId}, Amount: {amount}, Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> CreditAsync(Guid clientId, decimal amount, string description)
    {
        try
        {
            using var connection = _connectionFactory.CreateConnection();

            var sql = @"
                UPDATE accounts
                SET available_balance = available_balance + @Amount,
                    last_updated = @UpdatedAt
                WHERE client_id = @ClientId";

            var parameters = new
            {
                ClientId = clientId,
                Amount = amount,
                UpdatedAt = DateTime.UtcNow
            };

            var rowsAffected = await connection.ExecuteAsync(sql, parameters);

            Console.WriteLine($"💰 DEBUG CreditAsync - ClientId: {clientId}, Amount: {amount}, Rows affected: {rowsAffected}");

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"💥 ERROR CreditAsync - ClientId: {clientId}, Amount: {amount}, Error: {ex.Message}");
            throw;
        }
    }

    private static Account MapFromDto(AccountDto dto)
    {
        Console.WriteLine($"🔍 DEBUG MapFromDto - ClientId: {dto.ClientId}, AccountId: '{dto.AccountId}'");

        // Usar reflection para criar instância privada
        var account = (Account)Activator.CreateInstance(typeof(Account), true)!;

        // Mapear propriedades usando reflection
        var type = typeof(Account);

        type.GetProperty("ClientId")?.SetValue(account, dto.ClientId);
        type.GetProperty("AccountId")?.SetValue(account, dto.AccountId);
        type.GetProperty("AvailableBalance")?.SetValue(account, new Money(dto.AvailableBalance, dto.Currency));
        type.GetProperty("BlockedBalance")?.SetValue(account, new Money(dto.BlockedBalance, dto.Currency));
        type.GetProperty("CreatedAt")?.SetValue(account, dto.CreatedAt);
        type.GetProperty("LastUpdated")?.SetValue(account, dto.LastUpdated);

        Console.WriteLine($"🔍 DEBUG Account criado - AccountId: '{account.AccountId}'");
        return account;
    }
}
