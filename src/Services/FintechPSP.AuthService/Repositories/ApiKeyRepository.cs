using Dapper;
using FintechPSP.AuthService.Models;
using System.Text.Json;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Repositório para API Keys
/// </summary>
public class ApiKeyRepository : IApiKeyRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ApiKeyRepository> _logger;

    public ApiKeyRepository(IConfiguration configuration, ILogger<ApiKeyRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger;
    }

    public async Task<ApiKey> CreateAsync(ApiKey apiKey)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = @"
            INSERT INTO api_keys (
                id, public_key, secret_hash, company_id, name, scopes, 
                is_active, created_at, expires_at, allowed_ip, 
                rate_limit_per_minute, created_by
            ) VALUES (
                @Id, @PublicKey, @SecretHash, @CompanyId, @Name, @Scopes,
                @IsActive, @CreatedAt, @ExpiresAt, @AllowedIp,
                @RateLimitPerMinute, @CreatedBy
            )
            RETURNING *";

        var parameters = new
        {
            apiKey.Id,
            apiKey.PublicKey,
            apiKey.SecretHash,
            apiKey.CompanyId,
            apiKey.Name,
            Scopes = JsonSerializer.Serialize(apiKey.Scopes),
            apiKey.IsActive,
            apiKey.CreatedAt,
            apiKey.ExpiresAt,
            apiKey.AllowedIp,
            apiKey.RateLimitPerMinute,
            apiKey.CreatedBy
        };

        var result = await connection.QuerySingleAsync<dynamic>(sql, parameters);
        
        _logger.LogInformation("API Key criada: {PublicKey} para empresa {CompanyId}", 
            apiKey.PublicKey, apiKey.CompanyId);

        return MapToApiKey(result);
    }

    public async Task<ApiKey?> GetByPublicKeyAsync(string publicKey)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = @"
            SELECT * FROM api_keys 
            WHERE public_key = @PublicKey AND is_active = true";

        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { PublicKey = publicKey });
        
        return result != null ? MapToApiKey(result) : null;
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = "SELECT * FROM api_keys WHERE id = @Id";

        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
        
        return result != null ? MapToApiKey(result) : null;
    }

    public async Task<(List<ApiKey> apiKeys, int total)> GetByCompanyAsync(Guid companyId, int page = 1, int limit = 10)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        // Count query
        const string countSql = "SELECT COUNT(*) FROM api_keys WHERE company_id = @CompanyId";
        var total = await connection.QuerySingleAsync<int>(countSql, new { CompanyId = companyId });

        // Data query
        var offset = (page - 1) * limit;
        const string dataSql = @"
            SELECT * FROM api_keys 
            WHERE company_id = @CompanyId 
            ORDER BY created_at DESC 
            LIMIT @Limit OFFSET @Offset";

        var results = await connection.QueryAsync<dynamic>(dataSql, new 
        { 
            CompanyId = companyId, 
            Limit = limit, 
            Offset = offset 
        });

        var apiKeys = results.Select(MapToApiKey).ToList();
        
        return (apiKeys, total);
    }

    public async Task UpdateLastUsedAsync(Guid id, DateTime lastUsed)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = "UPDATE api_keys SET last_used_at = @LastUsed WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, new { Id = id, LastUsed = lastUsed });
    }

    public async Task UpdateStatusAsync(Guid id, bool isActive)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = "UPDATE api_keys SET is_active = @IsActive WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, new { Id = id, IsActive = isActive });
        
        _logger.LogInformation("API Key {Id} status atualizado para {Status}", id, isActive ? "ativa" : "inativa");
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = "DELETE FROM api_keys WHERE id = @Id";
        
        await connection.ExecuteAsync(sql, new { Id = id });
        
        _logger.LogInformation("API Key {Id} deletada", id);
    }

    public async Task<bool> PublicKeyExistsAsync(string publicKey)
    {
        using var connection = new Npgsql.NpgsqlConnection(_connectionString);
        
        const string sql = "SELECT COUNT(*) FROM api_keys WHERE public_key = @PublicKey";
        
        var count = await connection.QuerySingleAsync<int>(sql, new { PublicKey = publicKey });
        
        return count > 0;
    }

    private static ApiKey MapToApiKey(dynamic row)
    {
        var scopes = new List<string>();
        if (!string.IsNullOrEmpty(row.scopes))
        {
            try
            {
                scopes = JsonSerializer.Deserialize<List<string>>(row.scopes) ?? new List<string>();
            }
            catch
            {
                // Se falhar na deserialização, usar lista vazia
            }
        }

        return new ApiKey
        {
            Id = row.id,
            PublicKey = row.public_key,
            SecretHash = row.secret_hash,
            CompanyId = row.company_id,
            Name = row.name,
            Scopes = scopes,
            IsActive = row.is_active,
            CreatedAt = row.created_at,
            ExpiresAt = row.expires_at,
            LastUsedAt = row.last_used_at,
            AllowedIp = row.allowed_ip,
            RateLimitPerMinute = row.rate_limit_per_minute,
            CreatedBy = row.created_by
        };
    }
}
