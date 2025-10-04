using System;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.CompanyService.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dapper;

namespace FintechPSP.CompanyService.Controllers;

/// <summary>
/// Controller para debug e testes
/// </summary>
[ApiController]
[Route("debug")]
[Produces("application/json")]
public class DebugController : ControllerBase
{
    private readonly ILogger<DebugController> _logger;
    private readonly ICompanyRepository _companyRepository;

    public DebugController(
        ILogger<DebugController> logger,
        ICompanyRepository companyRepository)
    {
        _logger = logger;
        _companyRepository = companyRepository;
    }

    /// <summary>
    /// Debug endpoint para testar consulta direta
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> DebugSearch([FromQuery] string? search = null)
    {
        try
        {
            _logger.LogInformation("Debug - Testando consulta com search: {Search}", search);
            
            var result = await _companyRepository.GetPagedAsync(1, 10, search, null);
            
            _logger.LogInformation("Debug - Resultado: {Count} empresas encontradas", result.TotalCount);
            
            return Ok(new
            {
                search = search,
                totalCount = result.TotalCount,
                companies = result.Companies.Select(c => new { c.Id, c.RazaoSocial, c.Cnpj, c.Status }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debug - Erro na consulta");
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Endpoint de debug para testar consulta SQL direta
    /// </summary>
    [HttpGet("sql")]
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
}
