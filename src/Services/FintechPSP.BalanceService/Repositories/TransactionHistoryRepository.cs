using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.BalanceService.Repositories;

/// <summary>
/// Implementação do repository de histórico de transações usando Dapper
/// </summary>
public class TransactionHistoryRepository : ITransactionHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<IEnumerable<TransactionHistory>> GetTransactionHistoryAsync(
        Guid clientId,
        string? accountId,
        DateTime startDate,
        DateTime endDate,
        int page = 1,
        int pageSize = 50)
    {
        var sql = @"
            SELECT transaction_id as TransactionId, client_id as ClientId, account_id as AccountId,
                   external_id as ExternalId, type as Type, amount as Amount, description as Description,
                   status as Status, created_at as CreatedAt, operation as Operation
            FROM transaction_history
            WHERE client_id = @ClientId
              AND created_at >= @StartDate
              AND created_at <= @EndDate";

        object parameters = new
        {
            ClientId = clientId,
            StartDate = startDate,
            EndDate = endDate,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };

        if (!string.IsNullOrEmpty(accountId))
        {
            sql += " AND account_id = @AccountId";
            parameters = new
            {
                ClientId = clientId,
                AccountId = accountId,
                StartDate = startDate,
                EndDate = endDate,
                Offset = (page - 1) * pageSize,
                PageSize = pageSize
            };
        }

        sql += " ORDER BY created_at DESC OFFSET @Offset LIMIT @PageSize";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<TransactionHistory>(sql, parameters);
        
        return results;
    }

    public async Task<int> GetTransactionCountAsync(
        Guid clientId,
        string? accountId,
        DateTime startDate,
        DateTime endDate)
    {
        var sql = @"
            SELECT COUNT(*) 
            FROM transaction_history 
            WHERE client_id = @ClientId 
              AND created_at >= @StartDate 
              AND created_at <= @EndDate";

        object parameters = new { ClientId = clientId, StartDate = startDate, EndDate = endDate };

        if (!string.IsNullOrEmpty(accountId))
        {
            sql += " AND account_id = @AccountId";
            parameters = new { ClientId = clientId, AccountId = accountId, StartDate = startDate, EndDate = endDate };
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QuerySingleAsync<int>(sql, parameters);
    }

    public async Task AddTransactionAsync(TransactionHistory transaction)
    {
        Console.WriteLine($"🔍 DEBUG INICIO AddTransactionAsync - AccountId: {transaction.AccountId}");

        // Usar parâmetros nomeados com DynamicParameters
        const string sql = @"
            INSERT INTO transaction_history
            (transaction_id, external_id, type, amount, description, status, created_at, operation, client_id, account_id)
            VALUES
            (@TransactionId, @ExternalId, @Type, @Amount, @Description, @Status, @CreatedAt, @Operation, @ClientId, @AccountId)";

        Console.WriteLine($"🔍 DEBUG SQL: {sql}");
        Console.WriteLine($"🔍 DEBUG AccountId value: '{transaction.AccountId}'");

        var parameters = new DynamicParameters();
        parameters.Add("TransactionId", transaction.TransactionId);
        parameters.Add("ExternalId", transaction.ExternalId);
        parameters.Add("Type", transaction.Type);
        parameters.Add("Amount", transaction.Amount);
        parameters.Add("Description", transaction.Description);
        parameters.Add("Status", transaction.Status);
        parameters.Add("CreatedAt", transaction.CreatedAt);
        parameters.Add("Operation", transaction.Operation);
        parameters.Add("ClientId", transaction.ClientId);
        parameters.Add("AccountId", transaction.AccountId);

        using var connection = _connectionFactory.CreateConnection();
        Console.WriteLine($"🔍 DEBUG Conexão criada, executando query...");

        await connection.ExecuteAsync(sql, parameters);

        Console.WriteLine($"🔍 DEBUG Query executada com sucesso!");
    }

    /// <summary>
    /// Adiciona entrada no histórico de transações (versão genérica para QR Codes)
    /// </summary>
    public async Task AddTransactionHistoryAsync(object transactionHistory)
    {
        const string sql = @"
            INSERT INTO transaction_history
            (id, transaction_id, external_id, type, sub_type, amount, currency, status, description,
             pix_key, bank_code, qrcode_payload, expires_at, created_at, updated_at, metadata)
            VALUES
            (@Id, @TransactionId, @ExternalId, @Type, @SubType, @Amount, @Currency, @Status, @Description,
             @PixKey, @BankCode, @QrcodePayload, @ExpiresAt, @CreatedAt, @UpdatedAt, @Metadata::jsonb)";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, transactionHistory);
    }

    /// <summary>
    /// Atualiza status de uma transação
    /// </summary>
    public async Task UpdateTransactionStatusAsync(Guid transactionId, string status, string? description = null)
    {
        var sql = @"
            UPDATE transaction_history
            SET status = @Status";

        if (!string.IsNullOrEmpty(description))
        {
            sql += ", description = @Description";
        }

        sql += " WHERE transaction_id = @TransactionId";

        var parameters = new
        {
            TransactionId = transactionId,
            Status = status,
            Description = description
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<TransactionHistory?> GetByExternalIdAsync(string externalId)
    {
        Console.WriteLine($"🔍 DEBUG GetByExternalIdAsync - externalId: '{externalId}'");

        var sql = @"
            SELECT transaction_id as TransactionId, client_id as ClientId, account_id as AccountId,
                   external_id as ExternalId, type as Type, amount as Amount, description as Description,
                   status as Status, created_at as CreatedAt, operation as Operation
            FROM transaction_history
            WHERE external_id = @ExternalId
            LIMIT 1";

        Console.WriteLine($"🔍 DEBUG SQL GetByExternalIdAsync: {sql}");

        var parameters = new DynamicParameters();
        parameters.Add("ExternalId", externalId);

        Console.WriteLine($"🔍 DEBUG Parameters GetByExternalIdAsync: ExternalId='{externalId}'");

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QueryFirstOrDefaultAsync<TransactionHistory>(sql, parameters);

        Console.WriteLine($"🔍 DEBUG Result GetByExternalIdAsync: {(result != null ? "FOUND" : "NOT FOUND")}");
        return result;
    }
}
