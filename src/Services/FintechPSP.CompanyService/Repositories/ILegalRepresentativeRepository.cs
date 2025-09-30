using FintechPSP.CompanyService.Models;

namespace FintechPSP.CompanyService.Repositories;

/// <summary>
/// Interface para repositório de representantes legais
/// </summary>
public interface ILegalRepresentativeRepository
{
    /// <summary>
    /// Obtém representante legal por ID
    /// </summary>
    Task<LegalRepresentative?> GetByIdAsync(Guid id);

    /// <summary>
    /// Lista representantes legais de uma empresa
    /// </summary>
    Task<IEnumerable<LegalRepresentative>> GetByCompanyIdAsync(Guid companyId);

    /// <summary>
    /// Obtém representante legal por CPF e empresa
    /// </summary>
    Task<LegalRepresentative?> GetByCpfAndCompanyAsync(string cpf, Guid companyId);

    /// <summary>
    /// Cria novo representante legal
    /// </summary>
    Task<LegalRepresentative> CreateAsync(LegalRepresentative representative);

    /// <summary>
    /// Atualiza representante legal existente
    /// </summary>
    Task<LegalRepresentative> UpdateAsync(LegalRepresentative representative);

    /// <summary>
    /// Exclui representante legal
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Verifica se CPF já existe para a empresa
    /// </summary>
    Task<bool> CpfExistsForCompanyAsync(string cpf, Guid companyId, Guid? excludeId = null);

    /// <summary>
    /// Ativa/desativa representante legal
    /// </summary>
    Task<bool> SetActiveStatusAsync(Guid id, bool isActive);
}
