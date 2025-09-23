using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.CompanyService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

    // Armazenamento temporário em memória (substituir por banco de dados real)
    private static readonly List<Company> _companies = new();
    private static readonly object _lock = new object();

    public CompanyController(ILogger<CompanyController> logger)
    {
        _logger = logger;

        // Inicializar com dados de exemplo se estiver vazio
        lock (_lock)
        {
            if (_companies.Count == 0)
            {
                _companies.AddRange(new[]
                {
                    new Company
                    {
                        Id = Guid.Parse("b79fda6d-1642-4c05-b81f-7d065a2e28a1"),
                        RazaoSocial = "Tech Solutions Ltda",
                        NomeFantasia = "TechSol",
                        Cnpj = "12.345.678/0001-90",
                        InscricaoEstadual = "123.456.789.012",
                        Email = "contato@techsol.com.br",
                        Telefone = "(11) 3000-0000",
                        Status = CompanyStatus.Active,
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        Address = new CompanyAddress
                        {
                            Cep = "01310-100",
                            Logradouro = "Av. Paulista",
                            Numero = "1000",
                            Bairro = "Bela Vista",
                            Cidade = "São Paulo",
                            Estado = "SP",
                            Pais = "Brasil"
                        },
                        ContractData = new ContractData
                        {
                            NumeroContrato = "123456789",
                            DataContrato = DateTime.UtcNow.AddDays(-30),
                            CapitalSocial = 100000.00m,
                            AtividadePrincipal = "6201-5/00 - Desenvolvimento de programas de computador sob encomenda",
                            AtividadesSecundarias = new List<string>()
                        },
                        Applicant = new ApplicantData
                        {
                            NomeCompleto = "João Silva Santos",
                            Cpf = "123.456.789-01",
                            Email = "joao.silva@techsol.com.br",
                            Telefone = "(11) 99999-8888",
                            Address = new ApplicantAddress
                            {
                                Cep = "01310-100",
                                Logradouro = "Av. Paulista",
                                Numero = "1000",
                                Bairro = "Bela Vista",
                                Cidade = "São Paulo",
                                Estado = "SP",
                                Pais = "Brasil"
                            },
                            IsMainRepresentative = true
                        }
                    },
                    new Company
                    {
                        Id = Guid.NewGuid(),
                        RazaoSocial = "Inovação Digital S.A.",
                        NomeFantasia = "InovaDigital",
                        Cnpj = "98.765.432/0001-10",
                        Status = CompanyStatus.UnderReview,
                        CreatedAt = DateTime.UtcNow.AddDays(-15),
                        Address = new CompanyAddress
                        {
                            Cep = "20040-020",
                            Logradouro = "Rua das Flores, 500",
                            Cidade = "Rio de Janeiro",
                            Estado = "RJ",
                            Pais = "Brasil"
                        },
                        ContractData = new ContractData
                        {
                            AtividadesSecundarias = new List<string>()
                        },
                        Applicant = new ApplicantData
                        {
                            NomeCompleto = "Maria Oliveira Costa",
                            Cpf = "987.654.321-00",
                            Email = "maria.oliveira@inovadigital.com.br",
                            Telefone = "(21) 98888-7777",
                            Address = new ApplicantAddress
                            {
                                Cep = "20040-020",
                                Logradouro = "Rua das Flores",
                                Numero = "500",
                                Bairro = "Centro",
                                Cidade = "Rio de Janeiro",
                                Estado = "RJ",
                                Pais = "Brasil"
                            },
                            IsMainRepresentative = true
                        }
                    }
                });
            }
        }
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
        _logger.LogInformation("Listando empresas - Página: {Page}, Limite: {Limit}, Busca: {Search}, Status: {Status}",
            page, limit, search, status);

        await Task.Delay(50); // Simular consulta DB

        List<Company> companies;
        lock (_lock)
        {
            companies = _companies.ToList(); // Criar cópia para evitar problemas de concorrência
        }

        var filteredCompanies = companies;
        if (!string.IsNullOrEmpty(search))
        {
            filteredCompanies = companies.Where(c => 
                c.RazaoSocial.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.NomeFantasia?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
                c.Cnpj.Contains(search)).ToList();
        }

        if (status.HasValue)
        {
            filteredCompanies = filteredCompanies.Where(c => c.Status == status.Value).ToList();
        }

        var total = filteredCompanies.Count;
        var pagedCompanies = filteredCompanies
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToList();

        return Ok(new
        {
            companies = pagedCompanies,
            total,
            page,
            limit,
            totalPages = (int)Math.Ceiling((double)total / limit)
        });
    }

    /// <summary>
    /// Obtém empresa por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetCompany([FromRoute] Guid id)
    {
        _logger.LogInformation("Obtendo empresa {CompanyId}", id);

        await Task.Delay(30); // Simular consulta DB

        Company? company;
        lock (_lock)
        {
            company = _companies.FirstOrDefault(c => c.Id == id);
        }

        if (company == null)
        {
            _logger.LogWarning("Empresa {CompanyId} não encontrada", id);
            return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
        }

        return Ok(company);
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

            await Task.Delay(100); // Simular criação no DB

            var company = new Company
            {
                Id = Guid.NewGuid(),
                RazaoSocial = request.Company.RazaoSocial,
                NomeFantasia = request.Company.NomeFantasia,
                Cnpj = request.Company.Cnpj,
                InscricaoEstadual = request.Company.InscricaoEstadual,
                InscricaoMunicipal = request.Company.InscricaoMunicipal,
                Address = request.Company.Address,
                Telefone = request.Company.Telefone,
                Email = request.Company.Email,
                Website = request.Company.Website,
                ContractData = request.Company.ContractData,
                Status = CompanyStatus.PendingDocuments,
                CreatedAt = DateTime.UtcNow,
                Observacoes = request.Company.Observacoes
            };

            // Adicionar à lista em memória
            lock (_lock)
            {
                _companies.Add(company);
            }

            _logger.LogInformation("Empresa criada com sucesso: {CompanyId}", company.Id);

            return CreatedAtAction(nameof(GetCompany), new { id = company.Id }, company);
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

            await Task.Delay(80); // Simular atualização no DB

            Company company;
            lock (_lock)
            {
                // Encontrar a empresa existente
                var existingCompany = _companies.FirstOrDefault(c => c.Id == id);
                if (existingCompany == null)
                {
                    _logger.LogWarning("Empresa {CompanyId} não encontrada", id);
                    return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
                }

                // Atualizar os dados
                existingCompany.RazaoSocial = request.RazaoSocial;
                existingCompany.NomeFantasia = request.NomeFantasia;
                existingCompany.Cnpj = request.Cnpj;
                existingCompany.InscricaoEstadual = request.InscricaoEstadual;
                existingCompany.InscricaoMunicipal = request.InscricaoMunicipal;
                existingCompany.Address = request.Address;
                existingCompany.Telefone = request.Telefone;
                existingCompany.Email = request.Email;
                existingCompany.Website = request.Website;
                existingCompany.ContractData = request.ContractData;
                existingCompany.UpdatedAt = DateTime.UtcNow;
                existingCompany.Observacoes = request.Observacoes;

                // Atualizar dados do solicitante se fornecidos
                if (request.Applicant != null)
                {
                    existingCompany.Applicant = request.Applicant;
                    _logger.LogInformation("📝 Dados do solicitante atualizados: {ApplicantName}", request.Applicant.NomeCompleto);
                }

                company = existingCompany;
            }

            _logger.LogInformation("✅ Empresa {CompanyId} atualizada com sucesso", id);
            return Ok(company);
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

            await Task.Delay(50); // Simular atualização no DB

            var company = new Company
            {
                Id = id,
                RazaoSocial = "Tech Solutions Ltda",
                Status = request.Status,
                UpdatedAt = DateTime.UtcNow,
                ApprovedAt = request.Status == CompanyStatus.Approved ? DateTime.UtcNow : null,
                ApprovedBy = request.Status == CompanyStatus.Approved ? GetCurrentUserId() : null
            };

            return Ok(company);
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

            // Buscar e remover da lista em memória
            lock (_lock)
            {
                var company = _companies.FirstOrDefault(c => c.Id == id);
                if (company == null)
                {
                    _logger.LogWarning("Empresa {CompanyId} não encontrada para exclusão", id);
                    return NotFound(new { error = "not_found", message = "Empresa não encontrada" });
                }

                _companies.Remove(company);
                _logger.LogInformation("Empresa {CompanyId} removida da lista. Total restante: {Count}", id, _companies.Count);
            }

            await Task.Delay(50); // Simular exclusão no DB

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
}

/// <summary>
/// Request para atualizar status
/// </summary>
public class UpdateStatusRequest
{
    public CompanyStatus Status { get; set; }
    public string? Observacoes { get; set; }
}
