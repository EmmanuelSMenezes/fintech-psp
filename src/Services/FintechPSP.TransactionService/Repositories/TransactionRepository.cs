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
        try
        {
            // Usar reflection para criar instância privada
            var transaction = (Transaction)Activator.CreateInstance(typeof(Transaction), true)!;

            // Mapear propriedades usando reflection com tratamento de erro
            var type = typeof(Transaction);

            // Helper para mapear com segurança
            void SafeSetProperty(string propertyName, object? value)
            {
                try
                {
                    if (value != null)
                        type.GetProperty(propertyName)?.SetValue(transaction, value);
                }
                catch (Exception ex)
                {
                    // Log do erro mas não falha a operação
                    Console.WriteLine($"Erro ao mapear {propertyName}: {ex.Message}");
                }
            }

            // Mapear campos obrigatórios
            SafeSetProperty("TransactionId", result.TransactionId);
            SafeSetProperty("ExternalId", result.ExternalId);

            // Mapear enums com tratamento de erro
            try
            {
                if (result.type != null)
                    SafeSetProperty("Type", Enum.Parse<TransactionType>((string)result.type));
            }
            catch { /* Ignora erro de enum */ }

            try
            {
                if (result.status != null)
                    SafeSetProperty("Status", Enum.Parse<TransactionStatus>((string)result.status));
            }
            catch { /* Ignora erro de enum */ }

            // Mapear Money com tratamento de erro
            try
            {
                if (result.amount != null)
                {
                    var currency = result.currency?.ToString() ?? "BRL";
                    SafeSetProperty("Amount", new Money((decimal)result.amount, currency));
                }
            }
            catch { /* Ignora erro de Money */ }

            // Mapear campos opcionais
            SafeSetProperty("BankCode", result.BankCode);
            SafeSetProperty("PixKey", result.PixKey);
            SafeSetProperty("EndToEndId", result.EndToEndId);
            SafeSetProperty("AccountBranch", result.AccountBranch);
            SafeSetProperty("AccountNumber", result.AccountNumber);
            SafeSetProperty("TaxId", result.TaxId);
            SafeSetProperty("Name", result.name);
            SafeSetProperty("Description", result.description);
            SafeSetProperty("WebhookUrl", result.WebhookUrl);
            SafeSetProperty("DueDate", result.DueDate);
            SafeSetProperty("PayerTaxId", result.PayerTaxId);
            SafeSetProperty("PayerName", result.PayerName);
            SafeSetProperty("Instructions", result.instructions);
            SafeSetProperty("BoletoBarcode", result.BoletoBarcode);
            SafeSetProperty("BoletoUrl", result.BoletoUrl);
            SafeSetProperty("CryptoType", result.CryptoType);
            SafeSetProperty("WalletAddress", result.WalletAddress);
            SafeSetProperty("CryptoTxHash", result.CryptoTxHash);
            SafeSetProperty("CreatedAt", result.CreatedAt);
            SafeSetProperty("UpdatedAt", result.UpdatedAt);

            return transaction;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro geral no mapeamento: {ex.Message}");
            // Retorna uma transação básica em caso de erro
            var fallbackTransaction = (Transaction)Activator.CreateInstance(typeof(Transaction), true)!;
            return fallbackTransaction;
        }
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
