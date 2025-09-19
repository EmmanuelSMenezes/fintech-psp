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
/// Controller para gerenciamento de representantes legais
/// </summary>
[ApiController]
[Route("admin/companies/{companyId}/representatives")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class LegalRepresentativeController : ControllerBase
{
    private readonly ILogger<LegalRepresentativeController> _logger;

    public LegalRepresentativeController(ILogger<LegalRepresentativeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista representantes legais de uma empresa
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRepresentatives([FromRoute] Guid companyId)
    {
        _logger.LogInformation("Listando representantes legais da empresa {CompanyId}", companyId);

        await Task.Delay(30); // Simular consulta DB

        var representatives = new List<LegalRepresentative>
        {
            new LegalRepresentative
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                NomeCompleto = "João Silva Santos",
                Cpf = "123.456.789-01",
                Rg = "12.345.678-9",
                OrgaoExpedidor = "SSP/SP",
                DataNascimento = new DateTime(1980, 5, 15),
                EstadoCivil = "Casado",
                Nacionalidade = "Brasileira",
                Profissao = "Empresário",
                Email = "joao@techsol.com.br",
                Telefone = "(11) 99999-9999",
                Cargo = "Diretor Presidente",
                Type = RepresentationType.President,
                PercentualParticipacao = 60.0m,
                PodeAssinarSozinho = true,
                LimiteAlcada = 1000000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Address = new RepresentativeAddress
                {
                    Cep = "01310-100",
                    Logradouro = "Rua das Palmeiras",
                    Numero = "123",
                    Bairro = "Jardins",
                    Cidade = "São Paulo",
                    Estado = "SP"
                }
            },
            new LegalRepresentative
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                NomeCompleto = "Maria Oliveira Costa",
                Cpf = "987.654.321-09",
                Rg = "98.765.432-1",
                OrgaoExpedidor = "SSP/SP",
                DataNascimento = new DateTime(1985, 8, 22),
                EstadoCivil = "Solteira",
                Nacionalidade = "Brasileira",
                Profissao = "Administradora",
                Email = "maria@techsol.com.br",
                Telefone = "(11) 88888-8888",
                Cargo = "Diretora Financeira",
                Type = RepresentationType.Director,
                PercentualParticipacao = 40.0m,
                PodeAssinarSozinho = false,
                LimiteAlcada = 500000.00m,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Address = new RepresentativeAddress
                {
                    Cep = "04567-890",
                    Logradouro = "Av. Brigadeiro Faria Lima",
                    Numero = "456",
                    Bairro = "Itaim Bibi",
                    Cidade = "São Paulo",
                    Estado = "SP"
                }
            }
        };

        return Ok(representatives);
    }

    /// <summary>
    /// Obtém representante legal por ID
    /// </summary>
    [HttpGet("{representativeId}")]
    public async Task<IActionResult> GetRepresentative([FromRoute] Guid companyId, [FromRoute] Guid representativeId)
    {
        _logger.LogInformation("Obtendo representante legal {RepresentativeId} da empresa {CompanyId}", 
            representativeId, companyId);

        await Task.Delay(30); // Simular consulta DB

        var representative = new LegalRepresentative
        {
            Id = representativeId,
            CompanyId = companyId,
            NomeCompleto = "João Silva Santos",
            Cpf = "123.456.789-01",
            Rg = "12.345.678-9",
            OrgaoExpedidor = "SSP/SP",
            DataNascimento = new DateTime(1980, 5, 15),
            EstadoCivil = "Casado",
            Nacionalidade = "Brasileira",
            Profissao = "Empresário",
            Email = "joao@techsol.com.br",
            Telefone = "(11) 99999-9999",
            Celular = "(11) 99999-9999",
            Cargo = "Diretor Presidente",
            Type = RepresentationType.President,
            PercentualParticipacao = 60.0m,
            PoderesRepresentacao = "Poderes gerais de administração",
            PodeAssinarSozinho = true,
            LimiteAlcada = 1000000.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Address = new RepresentativeAddress
            {
                Cep = "01310-100",
                Logradouro = "Rua das Palmeiras",
                Numero = "123",
                Complemento = "Apto 45",
                Bairro = "Jardins",
                Cidade = "São Paulo",
                Estado = "SP",
                Pais = "Brasil"
            }
        };

        return Ok(representative);
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

            await Task.Delay(80); // Simular criação no DB

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
                Address = request.Address,
                Cargo = request.Cargo,
                Type = request.Type,
                PercentualParticipacao = request.PercentualParticipacao,
                PoderesRepresentacao = request.PoderesRepresentacao,
                PodeAssinarSozinho = request.PodeAssinarSozinho,
                LimiteAlcada = request.LimiteAlcada,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Representante legal criado com sucesso: {RepresentativeId}", representative.Id);

            return CreatedAtAction(nameof(GetRepresentative), 
                new { companyId, representativeId = representative.Id }, representative);
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

            await Task.Delay(80); // Simular atualização no DB

            var representative = new LegalRepresentative
            {
                Id = representativeId,
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
                Address = request.Address,
                Cargo = request.Cargo,
                Type = request.Type,
                PercentualParticipacao = request.PercentualParticipacao,
                PoderesRepresentacao = request.PoderesRepresentacao,
                PodeAssinarSozinho = request.PodeAssinarSozinho,
                LimiteAlcada = request.LimiteAlcada,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(representative);
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

            await Task.Delay(50); // Simular exclusão no DB

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir representante legal {RepresentativeId}", representativeId);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }
}
