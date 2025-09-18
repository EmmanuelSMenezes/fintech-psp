using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.AuthService.Models;
using Npgsql;

namespace FintechPSP.AuthService.Repositories;

/// <summary>
/// Repositório para usuários do sistema
/// </summary>
public class SystemUserRepository : ISystemUserRepository
{
    private readonly string _connectionString;

    public SystemUserRepository(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    /// <inheritdoc />
    public async Task<SystemUser?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT id, email, password_hash as PasswordHash, name, role, 
                   is_active as IsActive, is_master as IsMaster, 
                   last_login_at as LastLoginAt, created_at as CreatedAt, 
                   updated_at as UpdatedAt
            FROM system_users 
            WHERE email = @Email AND is_active = true";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<SystemUser>(sql, new { Email = email });
    }

    /// <inheritdoc />
    public async Task<SystemUser?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, email, password_hash as PasswordHash, name, role, 
                   is_active as IsActive, is_master as IsMaster, 
                   last_login_at as LastLoginAt, created_at as CreatedAt, 
                   updated_at as UpdatedAt
            FROM system_users 
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstOrDefaultAsync<SystemUser>(sql, new { Id = id });
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SystemUser>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, email, password_hash as PasswordHash, name, role, 
                   is_active as IsActive, is_master as IsMaster, 
                   last_login_at as LastLoginAt, created_at as CreatedAt, 
                   updated_at as UpdatedAt
            FROM system_users 
            ORDER BY created_at DESC";

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryAsync<SystemUser>(sql);
    }

    /// <inheritdoc />
    public async Task<SystemUser> CreateAsync(SystemUser user)
    {
        const string sql = @"
            INSERT INTO system_users (id, email, password_hash, name, role, is_active, is_master, created_at)
            VALUES (@Id, @Email, @PasswordHash, @Name, @Role, @IsActive, @IsMaster, @CreatedAt)
            RETURNING id, email, password_hash as PasswordHash, name, role, 
                      is_active as IsActive, is_master as IsMaster, 
                      last_login_at as LastLoginAt, created_at as CreatedAt, 
                      updated_at as UpdatedAt";

        user.Id = Guid.NewGuid();
        user.CreatedAt = DateTime.UtcNow;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<SystemUser>(sql, user);
    }

    /// <inheritdoc />
    public async Task<SystemUser> UpdateAsync(SystemUser user)
    {
        const string sql = @"
            UPDATE system_users 
            SET email = @Email, password_hash = @PasswordHash, name = @Name, 
                role = @Role, is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING id, email, password_hash as PasswordHash, name, role, 
                      is_active as IsActive, is_master as IsMaster, 
                      last_login_at as LastLoginAt, created_at as CreatedAt, 
                      updated_at as UpdatedAt";

        user.UpdatedAt = DateTime.UtcNow;

        using var connection = new NpgsqlConnection(_connectionString);
        return await connection.QueryFirstAsync<SystemUser>(sql, user);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = @"
            DELETE FROM system_users 
            WHERE id = @Id AND is_master = false";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UpdateLastLoginAsync(Guid id)
    {
        const string sql = @"
            UPDATE system_users 
            SET last_login_at = @LastLoginAt 
            WHERE id = @Id";

        using var connection = new NpgsqlConnection(_connectionString);
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, LastLoginAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }
}
