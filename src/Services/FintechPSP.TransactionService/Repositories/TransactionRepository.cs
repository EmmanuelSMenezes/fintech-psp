using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.Shared.Domain.Enums;
using FintechPSP.Shared.Domain.ValueObjects;
using FintechPSP.Shared.Infrastructure.Database;
using FintechPSP.TransactionService.Models;

namespace FintechPSP.TransactionService.Repositories;

/// <summary>
/// Implementação do repository de transações usando Dapper
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TransactionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<Transaction?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status, 
                   amount, currency, bank_code as BankCode, pix_key as PixKey, 
                   end_to_end_id as EndToEndId, account_branch as AccountBranch, 
                   account_number as AccountNumber, tax_id as TaxId, name, description, 
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId, 
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode, 
                   boleto_url as BoletoUrl, crypto_type as CryptoType, 
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions 
            WHERE transaction_id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
        
        return result != null ? MapToTransaction(result) : null;
    }

    public async Task<Transaction?> GetByExternalIdAsync(string externalId)
    {
        const string sql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status, 
                   amount, currency, bank_code as BankCode, pix_key as PixKey, 
                   end_to_end_id as EndToEndId, account_branch as AccountBranch, 
                   account_number as AccountNumber, tax_id as TaxId, name, description, 
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId, 
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode, 
                   boleto_url as BoletoUrl, crypto_type as CryptoType, 
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions 
            WHERE external_id = @ExternalId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync(sql, new { ExternalId = externalId });
        
        return result != null ? MapToTransaction(result) : null;
    }

    public async Task<Transaction?> GetByEndToEndIdAsync(string endToEndId)
    {
        const string sql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status, 
                   amount, currency, bank_code as BankCode, pix_key as PixKey, 
                   end_to_end_id as EndToEndId, account_branch as AccountBranch, 
                   account_number as AccountNumber, tax_id as TaxId, name, description, 
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId, 
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode, 
                   boleto_url as BoletoUrl, crypto_type as CryptoType, 
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions 
            WHERE end_to_end_id = @EndToEndId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync(sql, new { EndToEndId = endToEndId });
        
        return result != null ? MapToTransaction(result) : null;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        const string sql = @"
            INSERT INTO transactions (
                transaction_id, external_id, type, status, amount, currency, bank_code, 
                pix_key, end_to_end_id, account_branch, account_number, tax_id, name, 
                description, webhook_url, due_date, payer_tax_id, payer_name, instructions, 
                crypto_type, wallet_address, created_at
            ) VALUES (
                @TransactionId, @ExternalId, @Type, @Status, @Amount, @Currency, @BankCode, 
                @PixKey, @EndToEndId, @AccountBranch, @AccountNumber, @TaxId, @Name, 
                @Description, @WebhookUrl, @DueDate, @PayerTaxId, @PayerName, @Instructions, 
                @CryptoType, @WalletAddress, @CreatedAt
            )";

        var parameters = new
        {
            transaction.TransactionId,
            transaction.ExternalId,
            Type = transaction.Type.ToString(),
            Status = transaction.Status.ToString(),
            Amount = transaction.Amount.Amount,
            Currency = transaction.Amount.Currency,
            transaction.BankCode,
            transaction.PixKey,
            transaction.EndToEndId,
            transaction.AccountBranch,
            transaction.AccountNumber,
            transaction.TaxId,
            transaction.Name,
            transaction.Description,
            transaction.WebhookUrl,
            transaction.DueDate,
            transaction.PayerTaxId,
            transaction.PayerName,
            transaction.Instructions,
            transaction.CryptoType,
            transaction.WalletAddress,
            transaction.CreatedAt
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return transaction;
    }

    public async Task<Transaction> UpdateAsync(Transaction transaction)
    {
        const string sql = @"
            UPDATE transactions SET 
                status = @Status, boleto_barcode = @BoletoBarcode, boleto_url = @BoletoUrl, 
                crypto_tx_hash = @CryptoTxHash, updated_at = @UpdatedAt
            WHERE transaction_id = @TransactionId";

        var parameters = new
        {
            Status = transaction.Status.ToString(),
            transaction.BoletoBarcode,
            transaction.BoletoUrl,
            transaction.CryptoTxHash,
            transaction.UpdatedAt,
            transaction.TransactionId
        };

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, parameters);
        
        return transaction;
    }

    public async Task<IEnumerable<Transaction>> GetByStatusAsync(TransactionStatus status)
    {
        const string sql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status, 
                   amount, currency, bank_code as BankCode, pix_key as PixKey, 
                   end_to_end_id as EndToEndId, account_branch as AccountBranch, 
                   account_number as AccountNumber, tax_id as TaxId, name, description, 
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId, 
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode, 
                   boleto_url as BoletoUrl, crypto_type as CryptoType, 
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions 
            WHERE status = @Status
            ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, new { Status = status.ToString() });
        
        return results.Select(MapToTransaction);
    }

    public async Task<IEnumerable<Transaction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status, 
                   amount, currency, bank_code as BankCode, pix_key as PixKey, 
                   end_to_end_id as EndToEndId, account_branch as AccountBranch, 
                   account_number as AccountNumber, tax_id as TaxId, name, description, 
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId, 
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode, 
                   boleto_url as BoletoUrl, crypto_type as CryptoType, 
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash, 
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions 
            WHERE created_at >= @StartDate AND created_at <= @EndDate
            ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, new { StartDate = startDate, EndDate = endDate });
        
        return results.Select(MapToTransaction);
    }

    private static Transaction MapToTransaction(dynamic result)
    {
        // Usar reflection para criar instância privada
        var transaction = (Transaction)Activator.CreateInstance(typeof(Transaction), true)!;
        
        // Mapear propriedades usando reflection
        var type = typeof(Transaction);
        
        type.GetProperty("TransactionId")?.SetValue(transaction, (Guid)result.TransactionId);
        type.GetProperty("ExternalId")?.SetValue(transaction, (string)result.ExternalId);
        type.GetProperty("Type")?.SetValue(transaction, Enum.Parse<TransactionType>((string)result.type));
        type.GetProperty("Status")?.SetValue(transaction, Enum.Parse<TransactionStatus>((string)result.status));
        type.GetProperty("Amount")?.SetValue(transaction, new Money((decimal)result.amount, (string)result.currency));
        type.GetProperty("BankCode")?.SetValue(transaction, result.BankCode);
        type.GetProperty("PixKey")?.SetValue(transaction, result.PixKey);
        type.GetProperty("EndToEndId")?.SetValue(transaction, result.EndToEndId);
        type.GetProperty("AccountBranch")?.SetValue(transaction, result.AccountBranch);
        type.GetProperty("AccountNumber")?.SetValue(transaction, result.AccountNumber);
        type.GetProperty("TaxId")?.SetValue(transaction, result.TaxId);
        type.GetProperty("Name")?.SetValue(transaction, result.name);
        type.GetProperty("Description")?.SetValue(transaction, result.description);
        type.GetProperty("WebhookUrl")?.SetValue(transaction, result.WebhookUrl);
        type.GetProperty("DueDate")?.SetValue(transaction, result.DueDate);
        type.GetProperty("PayerTaxId")?.SetValue(transaction, result.PayerTaxId);
        type.GetProperty("PayerName")?.SetValue(transaction, result.PayerName);
        type.GetProperty("Instructions")?.SetValue(transaction, result.instructions);
        type.GetProperty("BoletoBarcode")?.SetValue(transaction, result.BoletoBarcode);
        type.GetProperty("BoletoUrl")?.SetValue(transaction, result.BoletoUrl);
        type.GetProperty("CryptoType")?.SetValue(transaction, result.CryptoType);
        type.GetProperty("WalletAddress")?.SetValue(transaction, result.WalletAddress);
        type.GetProperty("CryptoTxHash")?.SetValue(transaction, result.CryptoTxHash);
        type.GetProperty("CreatedAt")?.SetValue(transaction, (DateTime)result.CreatedAt);
        type.GetProperty("UpdatedAt")?.SetValue(transaction, result.UpdatedAt);
        
        return transaction;
    }

    public async Task<(IEnumerable<Transaction> transactions, int totalCount)> GetPagedAsync(int page, int pageSize)
    {
        const string countSql = "SELECT COUNT(*) FROM transactions";
        const string dataSql = @"
            SELECT transaction_id as TransactionId, external_id as ExternalId, type, status,
                   amount, currency, bank_code as BankCode, pix_key as PixKey,
                   end_to_end_id as EndToEndId, account_branch as AccountBranch,
                   account_number as AccountNumber, tax_id as TaxId, name, description,
                   webhook_url as WebhookUrl, due_date as DueDate, payer_tax_id as PayerTaxId,
                   payer_name as PayerName, instructions, boleto_barcode as BoletoBarcode,
                   boleto_url as BoletoUrl, crypto_type as CryptoType,
                   wallet_address as WalletAddress, crypto_tx_hash as CryptoTxHash,
                   created_at as CreatedAt, updated_at as UpdatedAt
            FROM transactions
            ORDER BY created_at DESC
            LIMIT @PageSize OFFSET @Offset";

        using var connection = _connectionFactory.CreateConnection();

        var totalCount = await connection.QuerySingleAsync<int>(countSql);
        var offset = (page - 1) * pageSize;
        var results = await connection.QueryAsync(dataSql, new { PageSize = pageSize, Offset = offset });

        var transactions = results.Select(MapToTransaction);

        return (transactions, totalCount);
    }

    public async Task<IEnumerable<object>> GetReconciliationHistoryAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT transaction_id, external_id, status, amount, created_at, updated_at
            FROM transactions
            WHERE created_at >= @StartDate AND created_at <= @EndDate
            ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync(sql, new { StartDate = startDate, EndDate = endDate });

        return results;
    }

    public async Task<IEnumerable<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await GetByDateRangeAsync(startDate, endDate);
    }

    public async Task SaveReconciliationResultAsync(object reconciliationResult)
    {
        // Implementação básica - pode ser expandida conforme necessário
        const string sql = @"
            INSERT INTO reconciliation_history (data, created_at)
            VALUES (@Data, @CreatedAt)";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new {
            Data = System.Text.Json.JsonSerializer.Serialize(reconciliationResult),
            CreatedAt = DateTime.UtcNow
        });
    }
}
