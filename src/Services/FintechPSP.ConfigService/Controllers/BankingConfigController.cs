using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FintechPSP.ConfigService.Repositories;
using FintechPSP.ConfigService.Models;
using System.Text.Json;

namespace FintechPSP.ConfigService.Controllers;

/// <summary>
/// Controller para configurações bancárias (admin)
/// </summary>
[ApiController]
[Route("banking/configs")]
[Authorize(Policy = "AdminScope")]
public class BankingConfigController : ControllerBase
{
    private readonly ILogger<BankingConfigController> _logger;
    private readonly IBankingConfigRepository _bankingConfigRepository;

    public BankingConfigController(
        ILogger<BankingConfigController> logger,
        IBankingConfigRepository bankingConfigRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _bankingConfigRepository = bankingConfigRepository ?? throw new ArgumentNullException(nameof(bankingConfigRepository));
    }

    /// <summary>
    /// Lista todas as configurações bancárias (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBankingConfigs()
    {
        try
        {
            _logger.LogInformation("Admin listando configurações bancárias");

            // Busca todas as configurações no banco de dados
            var allConfigs = await _bankingConfigRepository.GetAllAsync();
            var configsList = allConfigs.ToList();

            var configs = new
            {
                configs = configsList.Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    type = c.Type,
                    enabled = c.Enabled,
                    settings = c.Settings != null ? JsonSerializer.Deserialize<object>(c.Settings) : null,
                    createdAt = c.CreatedAt,
                    updatedAt = c.UpdatedAt
                }).ToArray(),
                total = configsList.Count,
                page = 1,
                pageSize = 50
            };

            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar configurações bancárias");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém configuração específica (admin)
    /// </summary>
    [HttpGet("{configId}")]
    public async Task<IActionResult> GetBankingConfig([FromRoute] Guid configId)
    {
        try
        {
            _logger.LogInformation("Admin obtendo configuração {ConfigId}", configId);

            // Busca configuração específica no banco de dados
            var config = await _bankingConfigRepository.GetByIdAsync(configId);

            if (config == null)
            {
                return NotFound(new { message = "Configuração bancária não encontrada" });
            }

            var result = new
            {
                id = config.Id,
                name = config.Name,
                type = config.Type,
                enabled = config.Enabled,
                settings = config.Settings != null ? JsonSerializer.Deserialize<object>(config.Settings) : null,
                createdAt = config.CreatedAt,
                updatedAt = config.UpdatedAt,
                createdBy = config.CreatedBy,
                updatedBy = config.UpdatedBy
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração bancária {ConfigId}", configId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria nova configuração bancária (admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBankingConfig([FromBody] CreateBankingConfigRequest request)
    {
        try
        {
            _logger.LogInformation("Admin criando configuração bancária {Name}", request.Name);

            // Validações
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Nome da configuração é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new { message = "Tipo da configuração é obrigatório" });
            }

            // Verifica se já existe configuração com o mesmo nome
            if (await _bankingConfigRepository.ExistsByNameAsync(request.Name))
            {
                return Conflict(new { message = "Já existe uma configuração com este nome" });
            }

            // Serializa as configurações se fornecidas
            string? settingsJson = null;
            if (request.Settings != null)
            {
                settingsJson = JsonSerializer.Serialize(request.Settings);
            }

            // Cria a configuração no banco de dados
            var config = new BankingConfig(
                request.Name,
                request.Type,
                request.Enabled,
                settingsJson,
                "admin"
            );

            var createdConfig = await _bankingConfigRepository.CreateAsync(config);

            var result = new
            {
                id = createdConfig.Id,
                name = createdConfig.Name,
                type = createdConfig.Type,
                enabled = createdConfig.Enabled,
                settings = createdConfig.Settings != null ? JsonSerializer.Deserialize<object>(createdConfig.Settings) : null,
                createdAt = createdConfig.CreatedAt,
                updatedAt = createdConfig.UpdatedAt,
                createdBy = createdConfig.CreatedBy
            };

            return CreatedAtAction(nameof(GetBankingConfig), new { configId = createdConfig.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar configuração bancária {Name}", request.Name);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza configuração bancária (admin)
    /// </summary>
    [HttpPut("{configId}")]
    public async Task<IActionResult> UpdateBankingConfig([FromRoute] Guid configId, [FromBody] UpdateBankingConfigRequest request)
    {
        try
        {
            _logger.LogInformation("Admin atualizando configuração {ConfigId}", configId);

            // Validações
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Nome da configuração é obrigatório" });
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new { message = "Tipo da configuração é obrigatório" });
            }

            // Busca a configuração existente
            var existingConfig = await _bankingConfigRepository.GetByIdAsync(configId);
            if (existingConfig == null)
            {
                return NotFound(new { message = "Configuração bancária não encontrada" });
            }

            // Verifica se já existe configuração com o mesmo nome (excluindo a atual)
            if (await _bankingConfigRepository.ExistsByNameAsync(request.Name, configId))
            {
                return Conflict(new { message = "Já existe uma configuração com este nome" });
            }

            // Serializa as configurações se fornecidas
            string? settingsJson = null;
            if (request.Settings != null)
            {
                settingsJson = JsonSerializer.Serialize(request.Settings);
            }

            // Atualiza a configuração
            existingConfig.Update(request.Name, request.Type, request.Enabled, settingsJson, "admin");

            var updatedConfig = await _bankingConfigRepository.UpdateAsync(existingConfig);

            var result = new
            {
                id = updatedConfig.Id,
                name = updatedConfig.Name,
                type = updatedConfig.Type,
                enabled = updatedConfig.Enabled,
                settings = updatedConfig.Settings != null ? JsonSerializer.Deserialize<object>(updatedConfig.Settings) : null,
                createdAt = updatedConfig.CreatedAt,
                updatedAt = updatedConfig.UpdatedAt,
                createdBy = updatedConfig.CreatedBy,
                updatedBy = updatedConfig.UpdatedBy
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar configuração bancária {ConfigId}", configId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Remove configuração bancária (admin)
    /// </summary>
    [HttpDelete("{configId}")]
    public async Task<IActionResult> DeleteBankingConfig([FromRoute] Guid configId)
    {
        try
        {
            _logger.LogInformation("Admin removendo configuração {ConfigId}", configId);

            // Verifica se a configuração existe
            var existingConfig = await _bankingConfigRepository.GetByIdAsync(configId);
            if (existingConfig == null)
            {
                return NotFound(new { message = "Configuração bancária não encontrada" });
            }

            // Remove a configuração do banco de dados
            var deleted = await _bankingConfigRepository.DeleteAsync(configId);

            if (!deleted)
            {
                return StatusCode(500, new { message = "Erro ao remover configuração bancária" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover configuração bancária {ConfigId}", configId);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }
}

public class CreateBankingConfigRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public object? Settings { get; set; }
}

public class UpdateBankingConfigRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public object? Settings { get; set; }
}
