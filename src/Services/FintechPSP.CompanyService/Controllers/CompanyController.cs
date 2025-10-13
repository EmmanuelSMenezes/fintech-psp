using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.CompanyService.Models;
using FintechPSP.CompanyService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dapper;

namespace FintechPSP.CompanyService.Controllers;

/// <summary>
/// Controller para gerenciamento de empresas clientes
/// </summary>
[ApiController]
[Route("admin/companies")]
[Produces("application/json")]
public class CompanyController : ControllerBase
{
    private readonly ILogger<CompanyController> _logger;
    private readonly ICompanyRepository _companyRepository;
    private readonly HttpClient _httpClient;

    public CompanyController(
        ILogger<CompanyController> logger,
        ICompanyRepository companyRepository,
        HttpClient httpClient)
    {
        _logger = logger;
        _companyRepository = companyRepository;
        _httpClient = httpClient;
    }

    /// <summary>
    /// Endpoint de teste sem autenticação
    /// </summary>
    [HttpGet("test")]
    [AllowAnonymous]
    public IActionResult Test()
    {
        return Ok(new { message = "CompanyService está funcionando!", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Endpoint de debug para testar consulta SQL direta
    /// </summary>
    [HttpGet("debugsql")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugSql()
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection("Host=localhost:5433;Database=fintech_psp;Username=postgres;Password=postgres");
            await connection.OpenAsync();

            var sql = "SELECT id, razao_social, cnpj, status FROM company_service.companies ORDER BY created_at DESC";
            var companies = await connection.QueryAsync(sql);

            return Ok(new {
                message = "Consulta SQL direta executada com sucesso",
                count = companies.Count(),
                companies = companies.ToList()
            });
        }
        catch (Exception ex)
        {
            return Ok(new {
                error = ex.Message,
                stackTrace = ex.StackTrace
            });
        }
    }



    /// <summary>
    /// Debug do token JWT
    /// </summary>
    [HttpGet("debug-token")]
    [AllowAnonymous]
    public IActionResult DebugToken()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            return Ok(new {
                message = "Token recebido",
                tokenLength = token.Length,
                tokenStart = token.Substring(0, Math.Min(50, token.Length)),
                timestamp = DateTime.UtcNow
            });
        }
        return Ok(new { message = "Nenhum token encontrado", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Lista todas as empresas
    /// </summary>
    [HttpGet]
    // [Authorize(Policy = "AdminScope")] // Temporariamente desabilitado - problema no API Gateway
    public async Task<IActionResult> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] CompanyStatus? status = null)
    {
        try
        {
            _logger.LogInformation("Listando empresas - Página: {Page}, Limite: {Limit}, Busca: {Search}, Status: {Status}",
                page, limit, search, status);

            var result = await _companyRepository.GetPagedAsync(page, limit, search, status);

            var totalPages = (int)Math.Ceiling((double)result.TotalCount / limit);

            _logger.LogInformation("Encontradas {TotalCount} empresas (página {Page}/{TotalPages})",
                result.TotalCount, page, totalPages);

            return Ok(new
            {
                companies = result.Companies,
                total = result.TotalCount,
                page = page,
                limit = limit,
                totalPages = totalPages
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar empresas");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém empresa por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Obtendo empresa {CompanyId}", id);

            var company = await _companyRepository.GetByIdAsync(id);

            if (company == null)
            {
                _logger.LogWarning("Empresa {CompanyId} não encontrada", id);
                return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
            }

            return Ok(company);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter empresa {CompanyId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria nova empresa
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
    {
        try
        {
            _logger.LogInformation("Criando nova empresa: {RazaoSocial}", request.Company.RazaoSocial);

            // Verificar se CNPJ já existe
            var existingCompany = await _companyRepository.GetByCnpjAsync(request.Company.Cnpj);
            if (existingCompany != null)
            {
                _logger.LogWarning("CNPJ {Cnpj} já existe", request.Company.Cnpj);
                return BadRequest(new { error = "cnpj_exists", message = "CNPJ já cadastrado" });
            }

            // Validar CNPJ via Receita Federal
            var cnpjValidation = await ValidateCnpjAsync(request.Company.Cnpj);
            if (!cnpjValidation.IsValid)
            {
                _logger.LogWarning("CNPJ inválido: {Cnpj} - {Error}", request.Company.Cnpj, cnpjValidation.ErrorMessage);
                return BadRequest(new {
                    error = "invalid_cnpj",
                    message = cnpjValidation.ErrorMessage ?? "CNPJ inválido"
                });
            }

            var company = new Company
            {
                Id = Guid.NewGuid(),
                RazaoSocial = request.Company.RazaoSocial,
                NomeFantasia = request.Company.NomeFantasia,
                Cnpj = request.Company.Cnpj,
                InscricaoEstadual = request.Company.InscricaoEstadual,
                InscricaoMunicipal = request.Company.InscricaoMunicipal,
                Address = request.Company.Address ?? new CompanyAddress(),
                Telefone = request.Company.Telefone,
                Email = request.Company.Email,
                Website = request.Company.Website,
                ContractData = request.Company.ContractData ?? new ContractData(),
                Status = CompanyStatus.PendingDocuments,
                CreatedAt = DateTime.UtcNow,
                Observacoes = request.Company.Observacoes
            };

            var createdCompany = await _companyRepository.CreateAsync(company);

            _logger.LogInformation("Empresa criada com sucesso: {CompanyId}", createdCompany.Id);

            return CreatedAtAction(nameof(GetCompany), new { id = createdCompany.Id }, createdCompany);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar empresa");
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza empresa
    /// </summary>
    [HttpPut("{id}")]
    // [Authorize(Policy = "AdminScope")] // Temporariamente desabilitado - problema no API Gateway
    public async Task<IActionResult> UpdateCompany([FromRoute] Guid id, [FromBody] CompanyData request)
    {
        try
        {
            _logger.LogInformation("🔄 Iniciando atualização da empresa {CompanyId}", id);

            // Log detalhado dos dados recebidos
            _logger.LogInformation("📋 Dados recebidos - RazaoSocial: {RazaoSocial}, Cnpj: {Cnpj}",
                request?.RazaoSocial, request?.Cnpj);

            // Log do ModelState
            _logger.LogInformation("🔍 ModelState.IsValid: {IsValid}", ModelState.IsValid);

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                    .ToList();

                _logger.LogWarning("❌ Modelo inválido para empresa {CompanyId}. Erros detalhados: {Errors}",
                    id, System.Text.Json.JsonSerializer.Serialize(errors));

                return BadRequest(ModelState);
            }

            // Verificar se a empresa existe
            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
            {
                _logger.LogWarning("Empresa {CompanyId} não encontrada", id);
                return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
            }

            // Verificar se CNPJ já existe para outra empresa
            if (existingCompany.Cnpj != request.Cnpj)
            {
                var cnpjExists = await _companyRepository.CnpjExistsAsync(request.Cnpj);
                if (cnpjExists)
                {
                    _logger.LogWarning("CNPJ {Cnpj} já existe para outra empresa", request.Cnpj);
                    return BadRequest(new { error = "cnpj_exists", message = "CNPJ já cadastrado para outra empresa" });
                }
            }

            // Atualizar os dados
            existingCompany.RazaoSocial = request.RazaoSocial;
            existingCompany.NomeFantasia = request.NomeFantasia;
            existingCompany.Cnpj = request.Cnpj;
            existingCompany.InscricaoEstadual = request.InscricaoEstadual;
            existingCompany.InscricaoMunicipal = request.InscricaoMunicipal;
            existingCompany.Address = request.Address ?? new CompanyAddress();
            existingCompany.Telefone = request.Telefone;
            existingCompany.Email = request.Email;
            existingCompany.Website = request.Website;
            existingCompany.ContractData = request.ContractData ?? new ContractData();
            existingCompany.Observacoes = request.Observacoes;

            // Atualizar dados do solicitante se fornecidos
            if (request.Applicant != null)
            {
                existingCompany.Applicant = request.Applicant;
                _logger.LogInformation("📝 Dados do solicitante atualizados: {ApplicantName}", request.Applicant.NomeCompleto);
            }

            var updatedCompany = await _companyRepository.UpdateAsync(existingCompany);

            _logger.LogInformation("✅ Empresa {CompanyId} atualizada com sucesso", id);
            return Ok(updatedCompany);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar empresa {CompanyId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza status da empresa
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateCompanyStatus([FromRoute] Guid id, [FromBody] UpdateStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Atualizando status da empresa {CompanyId} para {Status}", id, request.Status);

            // Verificar se a empresa existe
            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
            {
                _logger.LogWarning("Empresa {CompanyId} não encontrada", id);
                return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
            }

            var updatedCompany = await _companyRepository.UpdateStatusAsync(id, request.Status, GetCurrentUserId());

            _logger.LogInformation("Status da empresa {CompanyId} atualizado para {Status}", id, request.Status);

            return Ok(updatedCompany);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status da empresa {CompanyId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Exclui empresa
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCompany([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Excluindo empresa {CompanyId}", id);

            // Verificar se a empresa existe
            var existingCompany = await _companyRepository.GetByIdAsync(id);
            if (existingCompany == null)
            {
                _logger.LogWarning("Empresa {CompanyId} não encontrada para exclusão", id);
                return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
            }

            var deleted = await _companyRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Falha ao excluir empresa {CompanyId}", id);
                return StatusCode(500, new { error = "delete_failed", message = "Falha ao excluir empresa" });
            }

            _logger.LogInformation("Empresa {CompanyId} excluída com sucesso", id);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir empresa {CompanyId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Valida CNPJ via Receita Federal
    /// </summary>
    private async Task<CnpjValidationResult> ValidateCnpjAsync(string cnpj)
    {
        try
        {
            // Limpar CNPJ (remover pontos, barras, etc.)
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());

            _logger.LogInformation("Validando CNPJ via Receita Federal: {Cnpj}", cnpjLimpo);

            // Validação básica de formato
            if (!IsValidCnpjFormat(cnpjLimpo))
            {
                return new CnpjValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Formato de CNPJ inválido",
                    ValidationSource = "FORMAT_CHECK"
                };
            }

            // Consulta na Receita Federal (API pública)
            var url = $"https://receitaws.com.br/v1/cnpj/{cnpjLimpo}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erro na consulta Receita Federal - Status: {StatusCode}", response.StatusCode);

                // Fallback para validação de formato apenas
                return new CnpjValidationResult
                {
                    IsValid = true, // Se formato OK, considera válido
                    ErrorMessage = "Consulta Receita Federal indisponível, validação por formato",
                    ValidationSource = "FORMAT_FALLBACK",
                    CompanyName = "Não disponível",
                    Status = "Não verificado"
                };
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var receitaData = System.Text.Json.JsonSerializer.Deserialize<ReceitaFederalResponse>(jsonContent);

            if (receitaData?.Status == "ERROR")
            {
                return new CnpjValidationResult
                {
                    IsValid = false,
                    ErrorMessage = receitaData.Message ?? "CNPJ não encontrado na Receita Federal",
                    ValidationSource = "RECEITA_FEDERAL"
                };
            }

            _logger.LogInformation("CNPJ validado com sucesso - Empresa: {Nome}, Situação: {Situacao}",
                receitaData?.Nome, receitaData?.Situacao);

            return new CnpjValidationResult
            {
                IsValid = receitaData?.Situacao?.ToUpper() == "ATIVA",
                CompanyName = receitaData?.Nome,
                TradeName = receitaData?.Fantasia,
                Status = receitaData?.Situacao,
                ValidationSource = "RECEITA_FEDERAL"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CNPJ: {Cnpj}", cnpj);

            // Em caso de erro, fazer validação básica de formato
            var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
            return new CnpjValidationResult
            {
                IsValid = IsValidCnpjFormat(cnpjLimpo),
                ErrorMessage = "Erro na validação online, validação por formato apenas",
                ValidationSource = "FORMAT_FALLBACK"
            };
        }
    }

    private static bool IsValidCnpjFormat(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14)
            return false;

        // Verificar se todos os dígitos são iguais
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        // Validação dos dígitos verificadores
        var digits = cnpj.Select(c => int.Parse(c.ToString())).ToArray();

        // Primeiro dígito verificador
        var sum = 0;
        var multiplier = 5;
        for (int i = 0; i < 12; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (digits[12] != digit1)
            return false;

        // Segundo dígito verificador
        sum = 0;
        multiplier = 6;
        for (int i = 0; i < 13; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return digits[13] == digit2;
    }
}

/// <summary>
/// Request para atualizar status
/// </summary>
public class UpdateStatusRequest
{
    public CompanyStatus Status { get; set; }
    public string? Observacoes { get; set; }
}

/// <summary>
/// Resultado da validação de CNPJ
/// </summary>
public class CnpjValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CompanyName { get; set; }
    public string? TradeName { get; set; }
    public string? Status { get; set; }
    public string ValidationSource { get; set; } = string.Empty;
}

/// <summary>
/// Response da API da Receita Federal
/// </summary>
public class ReceitaFederalResponse
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public string? Nome { get; set; }
    public string? Fantasia { get; set; }
    public string? Situacao { get; set; }
    public string? Abertura { get; set; }
    public string? Logradouro { get; set; }
    public string? Numero { get; set; }
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Municipio { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
    public List<AtividadeEconomica>? AtividadePrincipal { get; set; }
}

/// <summary>
/// Atividade econômica da empresa
/// </summary>
public class AtividadeEconomica
{
    public string? Code { get; set; }
    public string? Text { get; set; }
}
