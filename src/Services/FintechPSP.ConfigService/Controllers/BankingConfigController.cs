using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    public BankingConfigController(ILogger<BankingConfigController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista todas as configurações bancárias (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBankingConfigs()
    {
        _logger.LogInformation("Admin listando configurações bancárias");
        
        await Task.Delay(50); // Simular consulta
        
        var configs = new
        {
            configs = new[]
            {
                new
                {
                    id = Guid.NewGuid(),
                    name = "PIX Configuration",
                    type = "PIX",
                    enabled = true,
                    settings = new
                    {
                        maxAmount = 50000.00m,
                        minAmount = 0.01m,
                        dailyLimit = 100000.00m,
                        monthlyLimit = 500000.00m
                    },
                    createdAt = DateTime.UtcNow.AddDays(-30),
                    updatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new
                {
                    id = Guid.NewGuid(),
                    name = "TED Configuration",
                    type = "TED",
                    enabled = true,
                    settings = new
                    {
                        maxAmount = 1000000.00m,
                        minAmount = 1.00m,
                        dailyLimit = 2000000.00m,
                        monthlyLimit = 10000000.00m
                    },
                    createdAt = DateTime.UtcNow.AddDays(-25),
                    updatedAt = DateTime.UtcNow.AddDays(-2)
                }
            },
            total = 2,
            page = 1,
            pageSize = 50
        };

        return Ok(configs);
    }

    /// <summary>
    /// Obtém configuração específica (admin)
    /// </summary>
    [HttpGet("{configId}")]
    public async Task<IActionResult> GetBankingConfig([FromRoute] Guid configId)
    {
        _logger.LogInformation("Admin obtendo configuração {ConfigId}", configId);
        
        await Task.Delay(30); // Simular consulta
        
        var config = new
        {
            id = configId,
            name = "PIX Configuration",
            type = "PIX",
            enabled = true,
            settings = new
            {
                maxAmount = 50000.00m,
                minAmount = 0.01m,
                dailyLimit = 100000.00m,
                monthlyLimit = 500000.00m,
                allowedHours = "00:00-23:59",
                weekendEnabled = true
            },
            createdAt = DateTime.UtcNow.AddDays(-30),
            updatedAt = DateTime.UtcNow.AddDays(-1),
            createdBy = "admin@fintechpsp.com",
            updatedBy = "admin@fintechpsp.com"
        };

        return Ok(config);
    }

    /// <summary>
    /// Cria nova configuração bancária (admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBankingConfig([FromBody] CreateBankingConfigRequest request)
    {
        _logger.LogInformation("Admin criando configuração bancária {Name}", request.Name);
        
        await Task.Delay(100); // Simular criação
        
        var config = new
        {
            id = Guid.NewGuid(),
            name = request.Name,
            type = request.Type,
            enabled = request.Enabled,
            settings = request.Settings,
            createdAt = DateTime.UtcNow,
            updatedAt = DateTime.UtcNow,
            createdBy = "admin@fintechpsp.com"
        };

        return CreatedAtAction(nameof(GetBankingConfig), new { configId = config.id }, config);
    }

    /// <summary>
    /// Atualiza configuração bancária (admin)
    /// </summary>
    [HttpPut("{configId}")]
    public async Task<IActionResult> UpdateBankingConfig([FromRoute] Guid configId, [FromBody] UpdateBankingConfigRequest request)
    {
        _logger.LogInformation("Admin atualizando configuração {ConfigId}", configId);
        
        await Task.Delay(80); // Simular atualização
        
        var config = new
        {
            id = configId,
            name = request.Name,
            type = request.Type,
            enabled = request.Enabled,
            settings = request.Settings,
            createdAt = DateTime.UtcNow.AddDays(-30),
            updatedAt = DateTime.UtcNow,
            updatedBy = "admin@fintechpsp.com"
        };

        return Ok(config);
    }

    /// <summary>
    /// Remove configuração bancária (admin)
    /// </summary>
    [HttpDelete("{configId}")]
    public async Task<IActionResult> DeleteBankingConfig([FromRoute] Guid configId)
    {
        _logger.LogInformation("Admin removendo configuração {ConfigId}", configId);
        
        await Task.Delay(50); // Simular remoção
        
        return NoContent();
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
