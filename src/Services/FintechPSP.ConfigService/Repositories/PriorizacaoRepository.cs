using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Dapper;
using FintechPSP.ConfigService.Models;
using FintechPSP.Shared.Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace FintechPSP.ConfigService.Repositories;

/// <summary>
/// Implementação do repositório de configurações de priorização
/// </summary>
public class PriorizacaoRepository : IPriorizacaoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<PriorizacaoRepository> _logger;

    public PriorizacaoRepository(IDbConnectionFactory connectionFactory, ILogger<PriorizacaoRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ConfiguracaoPriorizacao> UpsertAsync(ConfiguracaoPriorizacao configuracao)
    {
        const string upsertSql = @"
            INSERT INTO configuracoes_priorizacao (id, cliente_id, prioridades_json, total_percentual, is_valid, created_at, updated_at)
            VALUES (@Id, @ClienteId, @PrioridadesJson, @TotalPercentual, @IsValid, @CreatedAt, @UpdatedAt)
            ON CONFLICT (cliente_id) 
            DO UPDATE SET 
                prioridades_json = @PrioridadesJson,
                total_percentual = @TotalPercentual,
                is_valid = @IsValid,
                updated_at = @UpdatedAt
            RETURNING id, cliente_id, prioridades_json, total_percentual, is_valid, created_at, updated_at";

        if (configuracao.Id == Guid.Empty)
        {
            configuracao.Id = Guid.NewGuid();
            configuracao.CreatedAt = DateTime.UtcNow;
        }
        configuracao.UpdatedAt = DateTime.UtcNow;

        var prioridadesJson = JsonSerializer.Serialize(configuracao.Prioridades);

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(upsertSql, new
        {
            configuracao.Id,
            configuracao.ClienteId,
            PrioridadesJson = prioridadesJson,
            configuracao.TotalPercentual,
            configuracao.IsValid,
            configuracao.CreatedAt,
            configuracao.UpdatedAt
        });

        return new ConfiguracaoPriorizacao
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            Prioridades = JsonSerializer.Deserialize<List<Models.PrioridadeConta>>(result.prioridades_json) ?? new List<Models.PrioridadeConta>(),
            TotalPercentual = result.total_percentual,
            IsValid = result.is_valid,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<ConfiguracaoPriorizacao?> GetByClienteIdAsync(Guid clienteId)
    {
        const string sql = @"
            SELECT id, cliente_id, prioridades_json, total_percentual, is_valid, created_at, updated_at
            FROM configuracoes_priorizacao
            WHERE cliente_id = @ClienteId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { ClienteId = clienteId });

        if (result == null) return null;

        return new ConfiguracaoPriorizacao
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            Prioridades = JsonSerializer.Deserialize<List<Models.PrioridadeConta>>(result.prioridades_json) ?? new List<Models.PrioridadeConta>(),
            TotalPercentual = result.total_percentual,
            IsValid = result.is_valid,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<bool> DeleteByClienteIdAsync(Guid clienteId)
    {
        const string sql = @"
            DELETE FROM configuracoes_priorizacao
            WHERE cliente_id = @ClienteId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { ClienteId = clienteId });

        return rowsAffected > 0;
    }
}

/// <summary>
/// Implementação do repositório de bancos personalizados
/// </summary>
public class BancoPersonalizadoRepository : IBancoPersonalizadoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<BancoPersonalizadoRepository> _logger;

    public BancoPersonalizadoRepository(IDbConnectionFactory connectionFactory, ILogger<BancoPersonalizadoRepository> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BancoPersonalizado> CreateAsync(BancoPersonalizado banco)
    {
        const string sql = @"
            INSERT INTO bancos_personalizados (id, cliente_id, bank_code, endpoint, credentials_template, created_at)
            VALUES (@Id, @ClienteId, @BankCode, @Endpoint, @CredentialsTemplate, @CreatedAt)
            RETURNING id, cliente_id, bank_code, endpoint, credentials_template, created_at, updated_at";

        banco.Id = Guid.NewGuid();
        banco.CreatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, banco);

        return new BancoPersonalizado
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            Endpoint = result.endpoint,
            CredentialsTemplate = result.credentials_template,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<BancoPersonalizado?> GetByIdAsync(Guid bankId)
    {
        const string sql = @"
            SELECT id, cliente_id, bank_code, endpoint, credentials_template, created_at, updated_at
            FROM bancos_personalizados
            WHERE id = @BankId";

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { BankId = bankId });

        if (result == null) return null;

        return new BancoPersonalizado
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            Endpoint = result.endpoint,
            CredentialsTemplate = result.credentials_template,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<IEnumerable<BancoPersonalizado>> GetByClienteIdAsync(Guid clienteId)
    {
        const string sql = @"
            SELECT id, cliente_id, bank_code, endpoint, credentials_template, created_at, updated_at
            FROM bancos_personalizados
            WHERE cliente_id = @ClienteId
            ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var results = await connection.QueryAsync<dynamic>(sql, new { ClienteId = clienteId });

        return results.Select(result => new BancoPersonalizado
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            Endpoint = result.endpoint,
            CredentialsTemplate = result.credentials_template,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        });
    }

    public async Task<BancoPersonalizado> UpdateAsync(BancoPersonalizado banco)
    {
        const string sql = @"
            UPDATE bancos_personalizados
            SET endpoint = @Endpoint, credentials_template = @CredentialsTemplate, updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING id, cliente_id, bank_code, endpoint, credentials_template, created_at, updated_at";

        banco.UpdatedAt = DateTime.UtcNow;

        using var connection = _connectionFactory.CreateConnection();
        var result = await connection.QuerySingleAsync<dynamic>(sql, banco);

        return new BancoPersonalizado
        {
            Id = result.id,
            ClienteId = result.cliente_id,
            BankCode = result.bank_code,
            Endpoint = result.endpoint,
            CredentialsTemplate = result.credentials_template,
            CreatedAt = result.created_at,
            UpdatedAt = result.updated_at
        };
    }

    public async Task<bool> DeleteAsync(Guid bankId)
    {
        const string sql = @"
            DELETE FROM bancos_personalizados
            WHERE id = @BankId";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { BankId = bankId });

        return rowsAffected > 0;
    }

    public async Task<bool> IsBancoOwnedByClienteAsync(Guid bankId, Guid clienteId)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM bancos_personalizados
            WHERE id = @BankId AND cliente_id = @ClienteId";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { BankId = bankId, ClienteId = clienteId });

        return count > 0;
    }

    public async Task<bool> ExistsAsync(Guid clienteId, string bankCode)
    {
        const string sql = @"
            SELECT COUNT(1)
            FROM bancos_personalizados
            WHERE cliente_id = @ClienteId AND bank_code = @BankCode";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.QuerySingleAsync<int>(sql, new { ClienteId = clienteId, BankCode = bankCode });

        return count > 0;
    }
}
