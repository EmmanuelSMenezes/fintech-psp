using Dapper;
using FintechPSP.UserService.Models;
using Npgsql;
using System.Text.Json;

namespace FintechPSP.UserService.Repositories;

/// <summary>
/// Implementação do repositório de acessos usando Dapper
/// </summary>
public class AcessoRepository : IAcessoRepository
{
    private readonly string _connectionString;

    public AcessoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<Acesso> CreateAsync(Acesso acesso)
    {
        const string sql = @"
            INSERT INTO acessos (acesso_id, user_id, parent_user_id, email, role, permissions, is_active, created_at, created_by)
            VALUES (@AcessoId, @UserId, @ParentUserId, @Email, @Role, @PermissionsJson, @IsActive, @CreatedAt, @CreatedBy)
            RETURNING *";

        using var connection = new NpgsqlConnection(_connectionString);
        
        var parameters = new
        {
            acesso.AcessoId,
            acesso.UserId,
            acesso.ParentUserId,
            acesso.Email,
            acesso.Role,
            PermissionsJson = JsonSerializer.Serialize(acesso.Permissions),
            acesso.IsActive,
            acesso.CreatedAt,
            acesso.CreatedBy
        };

        var result = await connection.QuerySingleAsync<dynamic>(sql, parameters);
        return MapToAcesso(result);
    }

    public async Task<Acesso?> GetByIdAsync(Guid acessoId)
    {
        const string sql = "SELECT * FROM acessos WHERE acesso_id = @AcessoId";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { AcessoId = acessoId });
        
        return result != null ? MapToAcesso(result) : null;
    }

    public async Task<Acesso?> GetByUserIdAsync(Guid userId)
    {
        const string sql = "SELECT * FROM acessos WHERE user_id = @UserId";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { UserId = userId });
        
        return result != null ? MapToAcesso(result) : null;
    }

    public async Task<Acesso?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM acessos WHERE email = @Email";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Email = email });
        
        return result != null ? MapToAcesso(result) : null;
    }

    public async Task<List<Acesso>> GetByParentUserIdAsync(Guid parentUserId)
    {
        const string sql = "SELECT * FROM acessos WHERE parent_user_id = @ParentUserId ORDER BY created_at DESC";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql, new { ParentUserId = parentUserId });
        
        return results.Select(MapToAcesso).ToList();
    }

    public async Task<(List<Acesso> acessos, int total)> GetWithFiltersAsync(
        Guid? userId = null,
        Guid? parentUserId = null,
        string? role = null,
        bool? isActive = null,
        int page = 1,
        int limit = 50)
    {
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (userId.HasValue)
        {
            whereConditions.Add("user_id = @UserId");
            parameters.Add("UserId", userId.Value);
        }

        if (parentUserId.HasValue)
        {
            whereConditions.Add("parent_user_id = @ParentUserId");
            parameters.Add("ParentUserId", parentUserId.Value);
        }

        if (!string.IsNullOrEmpty(role))
        {
            whereConditions.Add("role = @Role");
            parameters.Add("Role", role);
        }

        if (isActive.HasValue)
        {
            whereConditions.Add("is_active = @IsActive");
            parameters.Add("IsActive", isActive.Value);
        }

        var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        
        var offset = (page - 1) * limit;
        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var countSql = $"SELECT COUNT(*) FROM acessos {whereClause}";
        var dataSql = $@"
            SELECT * FROM acessos 
            {whereClause} 
            ORDER BY created_at DESC 
            LIMIT @Limit OFFSET @Offset";

        using var connection = new NpgsqlConnection(_connectionString);
        
        var total = await connection.QuerySingleAsync<int>(countSql, parameters);
        var results = await connection.QueryAsync<dynamic>(dataSql, parameters);
        
        var acessos = results.Select(MapToAcesso).ToList();
        
        return (acessos, total);
    }

    public async Task<List<Acesso>> GetByRoleAsync(string role)
    {
        const string sql = "SELECT * FROM acessos WHERE role = @Role ORDER BY created_at DESC";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql, new { Role = role });
        
        return results.Select(MapToAcesso).ToList();
    }

    public async Task<Acesso> UpdateAsync(Acesso acesso)
    {
        const string sql = @"
            UPDATE acessos 
            SET role = @Role, 
                permissions = @PermissionsJson, 
                is_active = @IsActive, 
                updated_at = @UpdatedAt
            WHERE acesso_id = @AcessoId
            RETURNING *";

        using var connection = new NpgsqlConnection(_connectionString);
        
        var parameters = new
        {
            acesso.AcessoId,
            acesso.Role,
            PermissionsJson = JsonSerializer.Serialize(acesso.Permissions),
            acesso.IsActive,
            acesso.UpdatedAt
        };

        var result = await connection.QuerySingleAsync<dynamic>(sql, parameters);
        return MapToAcesso(result);
    }

    public async Task<bool> DeleteAsync(Guid acessoId)
    {
        const string sql = "DELETE FROM acessos WHERE acesso_id = @AcessoId";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { AcessoId = acessoId });
        
        return rowsAffected > 0;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission)
    {
        const string sql = "SELECT permissions FROM acessos WHERE user_id = @UserId AND is_active = true";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var permissionsJson = await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
        
        if (string.IsNullOrEmpty(permissionsJson))
            return false;

        var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
        return permissions.Contains(permission);
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        const string sql = "SELECT permissions FROM acessos WHERE user_id = @UserId AND is_active = true";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var permissionsJson = await connection.QuerySingleOrDefaultAsync<string>(sql, new { UserId = userId });
        
        if (string.IsNullOrEmpty(permissionsJson))
            return new List<string>();

        return JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(*) FROM acessos WHERE email = @Email";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var count = await connection.QuerySingleAsync<int>(sql, new { Email = email });
        
        return count > 0;
    }

    public async Task<Dictionary<string, int>> GetAccessCountByRoleAsync()
    {
        const string sql = "SELECT role, COUNT(*) as count FROM acessos WHERE is_active = true GROUP BY role";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql);
        
        return results.ToDictionary(
            r => (string)r.role,
            r => (int)r.count
        );
    }

    public async Task<List<Acesso>> GetActiveAccessesAsync()
    {
        const string sql = "SELECT * FROM acessos WHERE is_active = true ORDER BY created_at DESC";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql);
        
        return results.Select(MapToAcesso).ToList();
    }

    public async Task<List<Acesso>> GetInactiveAccessesAsync()
    {
        const string sql = "SELECT * FROM acessos WHERE is_active = false ORDER BY created_at DESC";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql);
        
        return results.Select(MapToAcesso).ToList();
    }

    public async Task<List<Acesso>> GetAccessesCreatedBetweenAsync(DateTime startDate, DateTime endDate)
    {
        const string sql = @"
            SELECT * FROM acessos 
            WHERE created_at >= @StartDate AND created_at <= @EndDate 
            ORDER BY created_at DESC";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql, new { StartDate = startDate, EndDate = endDate });
        
        return results.Select(MapToAcesso).ToList();
    }

    public async Task<bool> IsSubUserOfAsync(Guid subUserId, Guid parentUserId)
    {
        const string sql = "SELECT COUNT(*) FROM acessos WHERE user_id = @SubUserId AND parent_user_id = @ParentUserId";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var count = await connection.QuerySingleAsync<int>(sql, new { SubUserId = subUserId, ParentUserId = parentUserId });
        
        return count > 0;
    }

    public async Task<List<Acesso>> GetUserHierarchyAsync(Guid userId)
    {
        const string sql = @"
            WITH RECURSIVE user_hierarchy AS (
                SELECT * FROM acessos WHERE user_id = @UserId
                UNION ALL
                SELECT a.* FROM acessos a
                INNER JOIN user_hierarchy uh ON a.parent_user_id = uh.user_id
            )
            SELECT * FROM user_hierarchy ORDER BY created_at";
        
        using var connection = new NpgsqlConnection(_connectionString);
        var results = await connection.QueryAsync<dynamic>(sql, new { UserId = userId });
        
        return results.Select(MapToAcesso).ToList();
    }

    private static Acesso MapToAcesso(dynamic row)
    {
        var permissions = new List<string>();
        if (!string.IsNullOrEmpty(row.permissions))
        {
            permissions = JsonSerializer.Deserialize<List<string>>(row.permissions) ?? new List<string>();
        }

        return new Acesso
        {
            AcessoId = row.acesso_id,
            UserId = row.user_id,
            ParentUserId = row.parent_user_id,
            Email = row.email ?? string.Empty,
            Role = row.role ?? string.Empty,
            Permissions = permissions,
            IsActive = row.is_active,
            CreatedAt = row.created_at,
            UpdatedAt = row.updated_at,
            CreatedBy = row.created_by
        };
    }
}
