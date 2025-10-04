using Dapper;
using FintechPSP.CompanyService.Models;
using FintechPSP.Shared.Infrastructure.Database;
using System.Text;
using Microsoft.Extensions.Logging;

namespace FintechPSP.CompanyService.Repositories;

/// <summary>
/// Repositório para empresas
/// </summary>
public class CompanyRepository : ICompanyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<CompanyRepository> _logger;

    public CompanyRepository(IDbConnectionFactory connectionFactory, ILogger<CompanyRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    public async Task<Company?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, razao_social as RazaoSocial, nome_fantasia as NomeFantasia, 
                cnpj, inscricao_estadual as InscricaoEstadual, inscricao_municipal as InscricaoMunicipal,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                telefone, email, website,
                capital_social as CapitalSocial, atividade_principal as AtividadePrincipal,
                status, observacoes,
                created_at as CreatedAt, updated_at as UpdatedAt, 
                approved_at as ApprovedAt, approved_by as ApprovedBy
            FROM company_service.companies 
            WHERE id = @Id";

        var company = await connection.QueryFirstOrDefaultAsync<Company>(sql, new { Id = id });
        
        if (company != null)
        {
            // Map address data
            company.Address = new CompanyAddress
            {
                Cep = company.Address?.Cep ?? "",
                Logradouro = company.Address?.Logradouro ?? "",
                Numero = company.Address?.Numero ?? "",
                Complemento = company.Address?.Complemento,
                Bairro = company.Address?.Bairro ?? "",
                Cidade = company.Address?.Cidade ?? "",
                Estado = company.Address?.Estado ?? "",
                Pais = company.Address?.Pais ?? "Brasil"
            };

            // Map contract data
            company.ContractData = new ContractData
            {
                CapitalSocial = company.ContractData?.CapitalSocial ?? 0,
                AtividadePrincipal = company.ContractData?.AtividadePrincipal
            };
        }

        return company;
    }

    public async Task<Company?> GetByCnpjAsync(string cnpj)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            SELECT 
                id, razao_social as RazaoSocial, nome_fantasia as NomeFantasia, 
                cnpj, inscricao_estadual as InscricaoEstadual, inscricao_municipal as InscricaoMunicipal,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                telefone, email, website,
                capital_social as CapitalSocial, atividade_principal as AtividadePrincipal,
                status, observacoes,
                created_at as CreatedAt, updated_at as UpdatedAt, 
                approved_at as ApprovedAt, approved_by as ApprovedBy
            FROM company_service.companies 
            WHERE cnpj = @Cnpj";

        return await connection.QueryFirstOrDefaultAsync<Company>(sql, new { Cnpj = cnpj });
    }

    public async Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(
        int page, int limit, string? search = null, CompanyStatus? status = null)
    {
        using var connection = _connectionFactory.CreateConnection();

        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        _logger.LogInformation("[DEBUG] GetPagedAsync called with: page={Page}, limit={Limit}, search='{Search}', status={Status}", page, limit, search, status);

        if (!string.IsNullOrEmpty(search))
        {
            whereConditions.Add("(razao_social ILIKE @Search OR nome_fantasia ILIKE @Search OR cnpj LIKE @Search)");
            parameters.Add("Search", $"%{search}%");
            _logger.LogInformation("[DEBUG] Added search condition with parameter: %{SearchParam}%", search);
        }

        if (status.HasValue)
        {
            whereConditions.Add("status = @Status");
            parameters.Add("Status", status.Value.ToString());
            _logger.LogInformation("[DEBUG] Added status condition: {Status}", status.Value);
        }

        var whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
        _logger.LogInformation("[DEBUG] WHERE clause: {WhereClause}", whereClause);

        // Count query
        var countSql = $@"
            SELECT COUNT(*)
            FROM company_service.companies
            {whereClause}";

        _logger.LogInformation("[DEBUG] Count SQL: {CountSql}", countSql);
        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);
        _logger.LogInformation("[DEBUG] Total count result: {TotalCount}", totalCount);

        // Data query
        var offset = (page - 1) * limit;
        parameters.Add("Limit", limit);
        parameters.Add("Offset", offset);

        var dataSql = $@"
            SELECT 
                id, razao_social as RazaoSocial, nome_fantasia as NomeFantasia, 
                cnpj, inscricao_estadual as InscricaoEstadual, inscricao_municipal as InscricaoMunicipal,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                telefone, email, website,
                status, observacoes,
                created_at as CreatedAt, updated_at as UpdatedAt, 
                approved_at as ApprovedAt, approved_by as ApprovedBy
            FROM company_service.companies 
            {whereClause}
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        _logger.LogInformation("[DEBUG] Data SQL: {DataSql}", dataSql);
        var companies = await connection.QueryAsync<Company>(dataSql, parameters);
        _logger.LogInformation("[DEBUG] Companies result count: {CompaniesCount}", companies.Count());

        return (companies, totalCount);
    }

    public async Task<Company> CreateAsync(Company company)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            INSERT INTO company_service.companies (
                id, razao_social, nome_fantasia, cnpj, inscricao_estadual, inscricao_municipal,
                cep, logradouro, numero, complemento, bairro, cidade, estado, pais,
                telefone, email, website,
                capital_social, atividade_principal,
                status, observacoes, created_at
            ) VALUES (
                @Id, @RazaoSocial, @NomeFantasia, @Cnpj, @InscricaoEstadual, @InscricaoMunicipal,
                @Cep, @Logradouro, @Numero, @Complemento, @Bairro, @Cidade, @Estado, @Pais,
                @Telefone, @Email, @Website,
                @CapitalSocial, @AtividadePrincipal,
                @Status, @Observacoes, @CreatedAt
            )
            RETURNING 
                id, razao_social as RazaoSocial, nome_fantasia as NomeFantasia, 
                cnpj, inscricao_estadual as InscricaoEstadual, inscricao_municipal as InscricaoMunicipal,
                status, created_at as CreatedAt";

        var parameters = new
        {
            company.Id,
            company.RazaoSocial,
            company.NomeFantasia,
            company.Cnpj,
            company.InscricaoEstadual,
            company.InscricaoMunicipal,
            Cep = company.Address.Cep,
            Logradouro = company.Address.Logradouro,
            Numero = company.Address.Numero,
            Complemento = company.Address.Complemento,
            Bairro = company.Address.Bairro,
            Cidade = company.Address.Cidade,
            Estado = company.Address.Estado,
            Pais = company.Address.Pais,
            company.Telefone,
            company.Email,
            company.Website,
            CapitalSocial = company.ContractData.CapitalSocial,
            AtividadePrincipal = company.ContractData.AtividadePrincipal,
            Status = company.Status.ToString(),
            company.Observacoes,
            company.CreatedAt
        };

        var result = await connection.QuerySingleAsync<Company>(sql, parameters);
        return result;
    }

    public async Task<Company> UpdateAsync(Company company)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE company_service.companies SET
                razao_social = @RazaoSocial,
                nome_fantasia = @NomeFantasia,
                cnpj = @Cnpj,
                inscricao_estadual = @InscricaoEstadual,
                inscricao_municipal = @InscricaoMunicipal,
                cep = @Cep,
                logradouro = @Logradouro,
                numero = @Numero,
                complemento = @Complemento,
                bairro = @Bairro,
                cidade = @Cidade,
                estado = @Estado,
                pais = @Pais,
                telefone = @Telefone,
                email = @Email,
                website = @Website,
                observacoes = @Observacoes,
                updated_at = @UpdatedAt
            WHERE id = @Id
            RETURNING 
                id, razao_social as RazaoSocial, nome_fantasia as NomeFantasia, 
                cnpj, status, updated_at as UpdatedAt";

        var parameters = new
        {
            company.Id,
            company.RazaoSocial,
            company.NomeFantasia,
            company.Cnpj,
            company.InscricaoEstadual,
            company.InscricaoMunicipal,
            Cep = company.Address.Cep,
            Logradouro = company.Address.Logradouro,
            Numero = company.Address.Numero,
            Complemento = company.Address.Complemento,
            Bairro = company.Address.Bairro,
            Cidade = company.Address.Cidade,
            Estado = company.Address.Estado,
            Pais = company.Address.Pais,
            company.Telefone,
            company.Email,
            company.Website,
            company.Observacoes,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await connection.QuerySingleAsync<Company>(sql, parameters);
        return result;
    }

    public async Task<bool> UpdateStatusAsync(Guid id, CompanyStatus status, Guid? approvedBy = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = @"
            UPDATE company_service.companies SET
                status = @Status,
                updated_at = @UpdatedAt,
                approved_at = CASE WHEN @Status = 'Approved' THEN @UpdatedAt ELSE approved_at END,
                approved_by = CASE WHEN @Status = 'Approved' THEN @ApprovedBy ELSE approved_by END
            WHERE id = @Id";

        var parameters = new
        {
            Id = id,
            Status = status.ToString(),
            UpdatedAt = DateTime.UtcNow,
            ApprovedBy = approvedBy
        };

        var rowsAffected = await connection.ExecuteAsync(sql, parameters);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        const string sql = "DELETE FROM company_service.companies WHERE id = @Id";
        
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
        return rowsAffected > 0;
    }

    public async Task<bool> CnpjExistsAsync(string cnpj, Guid? excludeId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT COUNT(*) FROM company_service.companies WHERE cnpj = @Cnpj";
        var parameters = new DynamicParameters();
        parameters.Add("Cnpj", cnpj);

        if (excludeId.HasValue)
        {
            sql += " AND id != @ExcludeId";
            parameters.Add("ExcludeId", excludeId.Value);
        }

        var count = await connection.QuerySingleAsync<int>(sql, parameters);
        return count > 0;
    }
}
