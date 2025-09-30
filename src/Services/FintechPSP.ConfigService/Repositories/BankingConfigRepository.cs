using Dapper;
using FintechPSP.ConfigService.Models;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.ConfigService.Repositories;

/// <summary>
/// Implementação do repositório para configurações bancárias
/// </summary>
public class BankingConfigRepository : IBankingConfigRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public BankingConfigRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public async Task<BankingConfig?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by
            FROM banking_configs 
            WHERE id = @Id";

        return await connection.QueryFirstOrDefaultAsync<BankingConfig>(sql, new { Id = id });
    }

    public async Task<BankingConfig?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by
            FROM banking_configs 
            WHERE LOWER(name) = LOWER(@Name)";

        return await connection.QueryFirstOrDefaultAsync<BankingConfig>(sql, new { Name = name });
    }

    public async Task<IEnumerable<BankingConfig>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by
            FROM banking_configs 
            ORDER BY name";

        return await connection.QueryAsync<BankingConfig>(sql);
    }

    public async Task<IEnumerable<BankingConfig>> GetPagedAsync(int page = 1, int pageSize = 50, bool? enabled = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = enabled.HasValue ? "WHERE enabled = @Enabled" : "";
        var sql = $@"
            SELECT id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by
            FROM banking_configs 
            {whereClause}
            ORDER BY name
            LIMIT @PageSize OFFSET @Offset";

        var offset = (page - 1) * pageSize;
        var parameters = new { PageSize = pageSize, Offset = offset, Enabled = enabled };

        return await connection.QueryAsync<BankingConfig>(sql, parameters);
    }

    public async Task<int> CountAsync(bool? enabled = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = enabled.HasValue ? "WHERE enabled = @Enabled" : "";
        var sql = $"SELECT COUNT(*) FROM banking_configs {whereClause}";

        return await connection.QuerySingleAsync<int>(sql, new { Enabled = enabled });
    }

    public async Task<IEnumerable<BankingConfig>> GetByTypeAsync(string type)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by
            FROM banking_configs 
            WHERE LOWER(type) = LOWER(@Type)
            ORDER BY name";

        return await connection.QueryAsync<BankingConfig>(sql, new { Type = type });
    }

    public async Task<BankingConfig> CreateAsync(BankingConfig config)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO banking_configs (id, name, type, enabled, settings, created_at, created_by)
            VALUES (@Id, @Name, @Type, @Enabled, @Settings, @CreatedAt, @CreatedBy)
            RETURNING id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by";

        var result = await connection.QuerySingleAsync<BankingConfig>(sql, config);
        return result;
    }

    public async Task<BankingConfig> UpdateAsync(BankingConfig config)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE banking_configs 
            SET name = @Name, type = @Type, enabled = @Enabled, settings = @Settings, 
                updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id
            RETURNING id, name, type, enabled, settings, created_at, updated_at, created_by, updated_by";

        var result = await connection.QuerySingleAsync<BankingConfig>(sql, config);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM banking_configs WHERE id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var whereClause = excludeId.HasValue 
            ? "WHERE LOWER(name) = LOWER(@Name) AND id != @ExcludeId"
            : "WHERE LOWER(name) = LOWER(@Name)";
            
        var sql = $"SELECT COUNT(*) FROM banking_configs {whereClause}";
        
        var count = await connection.QuerySingleAsync<int>(sql, new { Name = name, ExcludeId = excludeId });
        return count > 0;
    }

    public async Task<bool> SetEnabledAsync(Guid id, bool enabled, string? updatedBy = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE banking_configs 
            SET enabled = @Enabled, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            Enabled = enabled, 
            UpdatedAt = DateTime.UtcNow, 
            UpdatedBy = updatedBy 
        });
        
        return rowsAffected > 0;
    }
}
