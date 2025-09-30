using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.CompanyService.Models;
using FintechPSP.CompanyService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.CompanyService.Controllers;

/// <summary>
/// Controller para gerenciamento de representantes legais
/// </summary>
[ApiController]
[Route("admin/companies/{companyId}/representatives")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class LegalRepresentativeController : ControllerBase
{
    private readonly ILogger<LegalRepresentativeController> _logger;
    private readonly ILegalRepresentativeRepository _representativeRepository;

    public LegalRepresentativeController(
        ILogger<LegalRepresentativeController> logger,
        ILegalRepresentativeRepository representativeRepository)
    {
        _logger = logger;
        _representativeRepository = representativeRepository;
    }

    /// <summary>
    /// Lista representantes legais de uma empresa
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRepresentatives([FromRoute] Guid companyId)
    {
        try
        {
            _logger.LogInformation("Listando representantes legais da empresa {CompanyId}", companyId);

            var representatives = await _representativeRepository.GetByCompanyIdAsync(companyId);

            _logger.LogInformation("Encontrados {Count} representantes legais para empresa {CompanyId}",
                representatives.Count(), companyId);

            return Ok(representatives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar representantes legais da empresa {CompanyId}", companyId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém representante legal por ID
    /// </summary>
    [HttpGet("{representativeId}")]
    public async Task<IActionResult> GetRepresentative([FromRoute] Guid companyId, [FromRoute] Guid representativeId)
    {
        try
        {
            _logger.LogInformation("Obtendo representante legal {RepresentativeId} da empresa {CompanyId}",
                representativeId, companyId);

            var representative = await _representativeRepository.GetByIdAsync(representativeId);

            if (representative == null || representative.CompanyId != companyId)
            {
                _logger.LogWarning("Representante legal {RepresentativeId} não encontrado para empresa {CompanyId}",
                    representativeId, companyId);
                return NotFound(new { error = "not_found", message = "Representante legal não encontrado" });
            }

            return Ok(representative);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter representante legal {RepresentativeId}", representativeId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria novo representante legal
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRepresentative([FromRoute] Guid companyId, [FromBody] LegalRepresentativeData request)
    {
        try
        {
            _logger.LogInformation("Criando representante legal para empresa {CompanyId}: {Nome}",
                companyId, request.NomeCompleto);

            // Verificar se CPF já existe para esta empresa
            var existingRepresentative = await _representativeRepository.GetByCpfAndCompanyAsync(request.Cpf, companyId);
            if (existingRepresentative != null)
            {
                _logger.LogWarning("CPF {Cpf} já existe para empresa {CompanyId}", request.Cpf, companyId);
                return BadRequest(new { error = "cpf_exists", message = "CPF já cadastrado para esta empresa" });
            }

            var representative = new LegalRepresentative
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                NomeCompleto = request.NomeCompleto,
                Cpf = request.Cpf,
                Rg = request.Rg,
                OrgaoExpedidor = request.OrgaoExpedidor,
                DataNascimento = request.DataNascimento,
                EstadoCivil = request.EstadoCivil,
                Nacionalidade = request.Nacionalidade,
                Profissao = request.Profissao,
                Email = request.Email,
                Telefone = request.Telefone,
                Celular = request.Celular,
                Address = request.Address ?? new RepresentativeAddress(),
                Cargo = request.Cargo,
                Type = request.Type,
                PercentualParticipacao = request.PercentualParticipacao,
                PoderesRepresentacao = request.PoderesRepresentacao,
                PodeAssinarSozinho = request.PodeAssinarSozinho,
                LimiteAlcada = request.LimiteAlcada,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createdRepresentative = await _representativeRepository.CreateAsync(representative);

            _logger.LogInformation("Representante legal criado com sucesso: {RepresentativeId}", createdRepresentative.Id);

            return CreatedAtAction(nameof(GetRepresentative),
                new { companyId, representativeId = createdRepresentative.Id }, createdRepresentative);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar representante legal para empresa {CompanyId}", companyId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza representante legal
    /// </summary>
    [HttpPut("{representativeId}")]
    public async Task<IActionResult> UpdateRepresentative([FromRoute] Guid companyId, [FromRoute] Guid representativeId,
        [FromBody] LegalRepresentativeData request)
    {
        try
        {
            _logger.LogInformation("Atualizando representante legal {RepresentativeId} da empresa {CompanyId}",
                representativeId, companyId);

            // Verificar se o representante existe
            var existingRepresentative = await _representativeRepository.GetByIdAsync(representativeId);
            if (existingRepresentative == null || existingRepresentative.CompanyId != companyId)
            {
                _logger.LogWarning("Representante legal {RepresentativeId} não encontrado para empresa {CompanyId}",
                    representativeId, companyId);
                return NotFound(new { error = "not_found", message = "Representante legal não encontrado" });
            }

            // Verificar se CPF já existe para outro representante da mesma empresa
            if (existingRepresentative.Cpf != request.Cpf)
            {
                var cpfExists = await _representativeRepository.CpfExistsForCompanyAsync(request.Cpf, companyId, representativeId);
                if (cpfExists)
                {
                    _logger.LogWarning("CPF {Cpf} já existe para outro representante da empresa {CompanyId}", request.Cpf, companyId);
                    return BadRequest(new { error = "cpf_exists", message = "CPF já cadastrado para outro representante desta empresa" });
                }
            }

            // Atualizar dados
            existingRepresentative.NomeCompleto = request.NomeCompleto;
            existingRepresentative.Cpf = request.Cpf;
            existingRepresentative.Rg = request.Rg;
            existingRepresentative.OrgaoExpedidor = request.OrgaoExpedidor;
            existingRepresentative.DataNascimento = request.DataNascimento;
            existingRepresentative.EstadoCivil = request.EstadoCivil;
            existingRepresentative.Nacionalidade = request.Nacionalidade;
            existingRepresentative.Profissao = request.Profissao;
            existingRepresentative.Email = request.Email;
            existingRepresentative.Telefone = request.Telefone;
            existingRepresentative.Celular = request.Celular;
            existingRepresentative.Address = request.Address ?? new RepresentativeAddress();
            existingRepresentative.Cargo = request.Cargo;
            existingRepresentative.Type = request.Type;
            existingRepresentative.PercentualParticipacao = request.PercentualParticipacao;
            existingRepresentative.PoderesRepresentacao = request.PoderesRepresentacao;
            existingRepresentative.PodeAssinarSozinho = request.PodeAssinarSozinho;
            existingRepresentative.LimiteAlcada = request.LimiteAlcada;

            var updatedRepresentative = await _representativeRepository.UpdateAsync(existingRepresentative);

            _logger.LogInformation("Representante legal {RepresentativeId} atualizado com sucesso", representativeId);

            return Ok(updatedRepresentative);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar representante legal {RepresentativeId}", representativeId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui representante legal
    /// </summary>
    [HttpDelete("{representativeId}")]
    public async Task<IActionResult> DeleteRepresentative([FromRoute] Guid companyId, [FromRoute] Guid representativeId)
    {
        try
        {
            _logger.LogInformation("Excluindo representante legal {RepresentativeId} da empresa {CompanyId}",
                representativeId, companyId);

            // Verificar se o representante existe e pertence à empresa
            var existingRepresentative = await _representativeRepository.GetByIdAsync(representativeId);
            if (existingRepresentative == null || existingRepresentative.CompanyId != companyId)
            {
                _logger.LogWarning("Representante legal {RepresentativeId} não encontrado para empresa {CompanyId}",
                    representativeId, companyId);
                return NotFound(new { error = "not_found", message = "Representante legal não encontrado" });
            }

            var deleted = await _representativeRepository.DeleteAsync(representativeId);
            if (!deleted)
            {
                _logger.LogWarning("Falha ao excluir representante legal {RepresentativeId}", representativeId);
                return StatusCode(500, new { error = "delete_failed", message = "Falha ao excluir representante legal" });
            }

            _logger.LogInformation("Representante legal {RepresentativeId} excluído com sucesso", representativeId);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir representante legal {RepresentativeId}", representativeId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }
}
