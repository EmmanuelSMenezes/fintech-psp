using Dapper;
using FintechPSP.CompanyService.Models;
using FintechPSP.Shared.Infrastructure.Database;

namespace FintechPSP.CompanyService.Repositories;

/// <summary>
/// Repositório para representantes legais
/// </summary>
public class LegalRepresentativeRepository : ILegalRepresentativeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LegalRepresentativeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<LegalRepresentative?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, company_id as CompanyId, nome_completo as NomeCompleto, cpf, rg, 
                orgao_expedidor as OrgaoExpedidor, data_nascimento as DataNascimento,
                estado_civil as EstadoCivil, nacionalidade, profissao,
                email, telefone, celular,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                cargo, type, percentual_participacao as PercentualParticipacao,
                poderes_representacao as PoderesRepresentacao, pode_assinar_sozinho as PodeAssinarSozinho,
                limite_alcada as LimiteAlcada, is_active as IsActive,
                created_at as CreatedAt, updated_at as UpdatedAt
            FROM company_service.legal_representatives 
            WHERE id = @Id";

        var representative = await connection.QueryFirstOrDefaultAsync<LegalRepresentative>(sql, new { Id = id });
        
        if (representative != null)
        {
            // Map address data
            representative.Address = new RepresentativeAddress
            {
                Cep = representative.Address?.Cep ?? "",
                Logradouro = representative.Address?.Logradouro ?? "",
                Numero = representative.Address?.Numero ?? "",
                Complemento = representative.Address?.Complemento,
                Bairro = representative.Address?.Bairro ?? "",
                Cidade = representative.Address?.Cidade ?? "",
                Estado = representative.Address?.Estado ?? "",
                Pais = representative.Address?.Pais ?? "Brasil"
            };
        }

        return representative;
    }

    public async Task<IEnumerable<LegalRepresentative>> GetByCompanyIdAsync(Guid companyId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, company_id as CompanyId, nome_completo as NomeCompleto, cpf, rg, 
                orgao_expedidor as OrgaoExpedidor, data_nascimento as DataNascimento,
                estado_civil as EstadoCivil, nacionalidade, profissao,
                email, telefone, celular,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                cargo, type, percentual_participacao as PercentualParticipacao,
                poderes_representacao as PoderesRepresentacao, pode_assinar_sozinho as PodeAssinarSozinho,
                limite_alcada as LimiteAlcada, is_active as IsActive,
                created_at as CreatedAt, updated_at as UpdatedAt
            FROM company_service.legal_representatives 
            WHERE company_id = @CompanyId AND is_active = true
            ORDER BY created_at DESC";

        var representatives = await connection.QueryAsync<LegalRepresentative>(sql, new { CompanyId = companyId });
        
        // Map address data for each representative
        foreach (var representative in representatives)
        {
            representative.Address = new RepresentativeAddress
            {
                Cep = representative.Address?.Cep ?? "",
                Logradouro = representative.Address?.Logradouro ?? "",
                Numero = representative.Address?.Numero ?? "",
                Complemento = representative.Address?.Complemento,
                Bairro = representative.Address?.Bairro ?? "",
                Cidade = representative.Address?.Cidade ?? "",
                Estado = representative.Address?.Estado ?? "",
                Pais = representative.Address?.Pais ?? "Brasil"
            };
        }

        return representatives;
    }

    public async Task<LegalRepresentative?> GetByCpfAndCompanyAsync(string cpf, Guid companyId)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, company_id as CompanyId, nome_completo as NomeCompleto, cpf, rg, 
                orgao_expedidor as OrgaoExpedidor, data_nascimento as DataNascimento,
                estado_civil as EstadoCivil, nacionalidade, profissao,
                email, telefone, celular,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                cargo, type, percentual_participacao as PercentualParticipacao,
                poderes_representacao as PoderesRepresentacao, pode_assinar_sozinho as PodeAssinarSozinho,
                limite_alcada as LimiteAlcada, is_active as IsActive,
                created_at as CreatedAt, updated_at as UpdatedAt
            FROM company_service.legal_representatives 
            WHERE cpf = @Cpf AND company_id = @CompanyId";

        return await connection.QueryFirstOrDefaultAsync<LegalRepresentative>(sql, new { Cpf = cpf, CompanyId = companyId });
    }

    public async Task<LegalRepresentative> CreateAsync(LegalRepresentative representative)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO company_service.legal_representatives (
                id, company_id, nome_completo, cpf, rg, orgao_expedidor, data_nascimento,
                estado_civil, nacionalidade, profissao, email, telefone, celular,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                cargo, type, percentual_participacao, poderes_representacao, 
                pode_assinar_sozinho, limite_alcada, is_active, created_at
            ) VALUES (
                @Id, @CompanyId, @NomeCompleto, @Cpf, @Rg, @OrgaoExpedidor, @DataNascimento,
                @EstadoCivil, @Nacionalidade, @Profissao, @Email, @Telefone, @Celular,
                @Cep, @Logradouro, @Numero, @Complemento, @Bairro, @Cidade, @Estado, @Pais,
                @Cargo, @Type, @PercentualParticipacao, @PoderesRepresentacao,
                @PodeAssinarSozinho, @LimiteAlcada, @IsActive, @CreatedAt
            )
            RETURNING 
                id, company_id as CompanyId, nome_completo as NomeCompleto, cpf, cargo, type,
                is_active as IsActive, created_at as CreatedAt";

        var parameters = new
        {
            representative.Id,
            representative.CompanyId,
            representative.NomeCompleto,
            representative.Cpf,
            representative.Rg,
            representative.OrgaoExpedidor,
            representative.DataNascimento,
            representative.EstadoCivil,
            representative.Nacionalidade,
            representative.Profissao,
            representative.Email,
            representative.Telefone,
            representative.Celular,
            Cep = representative.Address.Cep,
            Logradouro = representative.Address.Logradouro,
            Numero = representative.Address.Numero,
            Complemento = representative.Address.Complemento,
            Bairro = representative.Address.Bairro,
            Cidade = representative.Address.Cidade,
            Estado = representative.Address.Estado,
            Pais = representative.Address.Pais,
            representative.Cargo,
            Type = representative.Type.ToString(),
            representative.PercentualParticipacao,
            representative.PoderesRepresentacao,
            representative.PodeAssinarSozinho,
            representative.LimiteAlcada,
            representative.IsActive,
            representative.CreatedAt
        };

        var result = await connection.QuerySingleAsync<LegalRepresentative>(sql, parameters);
        return result;
    }

    public async Task<LegalRepresentative> UpdateAsync(LegalRepresentative representative)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE company_service.legal_representatives SET
                nome_completo = @NomeCompleto,
                cpf = @Cpf,
                rg = @Rg,
                orgao_expedidor = @OrgaoExpedidor,
                data_nascimento = @DataNascimento,
                estado_civil = @EstadoCivil,
                nacionalidade = @Nacionalidade,
                profissao = @Profissao,
                email = @Email,
                telefone = @Telefone,
                celular = @Celular,
                cep = @Cep,
                logradouro = @Logradouro,
                numero = @Numero,
                complemento = @Complemento,
                bairro = @Bairro,
                cidade = @Cidade,
                estado = @Estado,
                pais = @Pais,
                cargo = @Cargo,
                type = @Type,
                percentual_participacao = @PercentualParticipacao,
                poderes_representacao = @PoderesRepresentacao,
                pode_assinar_sozinho = @PodeAssinarSozinho,
                limite_alcada = @LimiteAlcada,
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING 
                id, company_id as CompanyId, nome_completo as NomeCompleto, cpf, cargo, type,
                is_active as IsActive, updated_at as UpdatedAt";

        var parameters = new
        {
            representative.Id,
            representative.NomeCompleto,
            representative.Cpf,
            representative.Rg,
            representative.OrgaoExpedidor,
            representative.DataNascimento,
            representative.EstadoCivil,
            representative.Nacionalidade,
            representative.Profissao,
            representative.Email,
            representative.Telefone,
            representative.Celular,
            Cep = representative.Address.Cep,
            Logradouro = representative.Address.Logradouro,
            Numero = representative.Address.Numero,
            Complemento = representative.Address.Complemento,
            Bairro = representative.Address.Bairro,
            Cidade = representative.Address.Cidade,
            Estado = representative.Address.Estado,
            Pais = representative.Address.Pais,
            representative.Cargo,
            Type = representative.Type.ToString(),
            representative.PercentualParticipacao,
            representative.PoderesRepresentacao,
            representative.PodeAssinarSozinho,
            representative.LimiteAlcada,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await connection.QuerySingleAsync<LegalRepresentative>(sql, parameters);
        return result;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM company_service.legal_representatives WHERE id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> CpfExistsForCompanyAsync(string cpf, Guid companyId, Guid? excludeId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT COUNT(*) FROM company_service.legal_representatives WHERE cpf = @Cpf AND company_id = @CompanyId";
        var parameters = new DynamicParameters();
        parameters.Add("Cpf", cpf);
        parameters.Add("CompanyId", companyId);

        if (excludeId.HasValue)
        {
            sql += " AND id != @ExcludeId";
            parameters.Add("ExcludeId", excludeId.Value);
        }

        var count = await connection.QuerySingleAsync<int>(sql, parameters);
        return count > 0;
    }

    public async Task<bool> SetActiveStatusAsync(Guid id, bool isActive)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE company_service.legal_representatives SET
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var parameters = new
        {
            Id = id,
            IsActive = isActive,
            UpdatedAt = DateTime.UtcNow
        };

        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }
}
