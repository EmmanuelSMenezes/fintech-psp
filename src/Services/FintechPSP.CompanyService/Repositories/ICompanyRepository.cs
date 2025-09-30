using FintechPSP.CompanyService.Models;

namespace FintechPSP.CompanyService.Repositories;

/// <summary>
/// Interface para repositório de empresas
/// </summary>
public interface ICompanyRepository
{
    /// <summary>
    /// Obtém empresa por ID
    /// </summary>
    Task<Company?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém empresa por CNPJ
    /// </summary>
    Task<Company?> GetByCnpjAsync(string cnpj);

    /// <summary>
    /// Lista empresas com paginação e filtros
    /// </summary>
    Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(
        int page, 
        int limit, 
        string? search = null, 
        CompanyStatus? status = null);

    /// <summary>
    /// Cria nova empresa
    /// </summary>
    Task<Company> CreateAsync(Company company);

    /// <summary>
    /// Atualiza empresa existente
    /// </summary>
    Task<Company> UpdateAsync(Company company);

    /// <summary>
    /// Atualiza status da empresa
    /// </summary>
    Task<bool> UpdateStatusAsync(Guid id, CompanyStatus status, Guid? approvedBy = null);

    /// <summary>
    /// Exclui empresa
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se CNPJ já existe
    /// </summary>
    Task<bool> CnpjExistsAsync(string cnpj, Guid? excludeId = null);
}
