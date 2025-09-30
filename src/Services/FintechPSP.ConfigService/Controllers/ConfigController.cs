using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace FintechPSP.ConfigService.Controllers;

/// <summary>
/// Controller para configurações e taxas
/// </summary>
[ApiController]
[Route("config")]
[Authorize]
public class ConfigController : ControllerBase
{
    private readonly ILogger<ConfigController> _logger;

    public ConfigController(ILogger<ConfigController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém taxas por tipo de transação
    /// </summary>
    [HttpGet("fees/{transactionType}")]
    public async Task<IActionResult> GetFees([FromRoute] string transactionType)
    {
        _logger.LogInformation("Obtendo taxas para {TransactionType}", transactionType);
        
        // Configurações de taxas (dados estáticos do sistema)
        
        var fees = transactionType.ToLower() switch
        {
            "pix" => new FeeResponse { Type = "PIX", FixedFee = 0.00m, PercentageFee = 0.50m, MinFee = 0.00m, MaxFee = 5.00m },
            "ted" => new FeeResponse { Type = "TED", FixedFee = 8.50m, PercentageFee = 0.00m, MinFee = 8.50m, MaxFee = 8.50m },
            "boleto" => new FeeResponse { Type = "Boleto", FixedFee = 2.90m, PercentageFee = 0.00m, MinFee = 2.90m, MaxFee = 2.90m },
            "crypto" => new FeeResponse { Type = "Crypto", FixedFee = 0.00m, PercentageFee = 1.50m, MinFee = 5.00m, MaxFee = 50.00m },
            _ => null
        };

        if (fees == null)
            return NotFound(new { message = "Tipo de transação não encontrado" });

        return Ok(fees);
    }

    /// <summary>
    /// Lista todas as taxas
    /// </summary>
    [HttpGet("fees")]
    public async Task<IActionResult> GetAllFees()
    {
        _logger.LogInformation("Listando todas as taxas");
        
        // Lista completa de taxas (dados estáticos do sistema)
        
        var fees = new[]
        {
            new FeeResponse { Type = "PIX", FixedFee = 0.00m, PercentageFee = 0.50m, MinFee = 0.00m, MaxFee = 5.00m },
            new FeeResponse { Type = "TED", FixedFee = 8.50m, PercentageFee = 0.00m, MinFee = 8.50m, MaxFee = 8.50m },
            new FeeResponse { Type = "Boleto", FixedFee = 2.90m, PercentageFee = 0.00m, MinFee = 2.90m, MaxFee = 2.90m },
            new FeeResponse { Type = "Crypto", FixedFee = 0.00m, PercentageFee = 1.50m, MinFee = 5.00m, MaxFee = 50.00m }
        };

        return Ok(new { fees });
    }

    /// <summary>
    /// Obtém configurações de banco
    /// </summary>
    [HttpGet("banks/{bankCode}")]
    public async Task<IActionResult> GetBankConfig([FromRoute] string bankCode)
    {
        _logger.LogInformation("Obtendo configuração do banco {BankCode}", bankCode);
        
        // Configurações bancárias (dados estáticos do sistema)
        
        var bankConfig = bankCode switch
        {
            "341" => new BankConfigResponse { Code = "341", Name = "Itaú", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            "033" => new BankConfigResponse { Code = "033", Name = "Santander", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            "001" => new BankConfigResponse { Code = "001", Name = "Banco do Brasil", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            "104" => new BankConfigResponse { Code = "104", Name = "Caixa", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            "237" => new BankConfigResponse { Code = "237", Name = "Bradesco", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            _ => null
        };

        if (bankConfig == null)
            return NotFound(new { message = "Banco não encontrado" });

        return Ok(bankConfig);
    }

    /// <summary>
    /// Lista todos os bancos configurados
    /// </summary>
    [HttpGet("banks")]
    public async Task<IActionResult> GetAllBanks()
    {
        _logger.LogInformation("Listando todos os bancos");
        
        // Lista de bancos suportados (dados estáticos do sistema)
        
        var banks = new[]
        {
            new BankConfigResponse { Code = "341", Name = "Itaú", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            new BankConfigResponse { Code = "033", Name = "Santander", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            new BankConfigResponse { Code = "001", Name = "Banco do Brasil", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            new BankConfigResponse { Code = "104", Name = "Caixa", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true },
            new BankConfigResponse { Code = "237", Name = "Bradesco", Active = true, PixEnabled = true, TedEnabled = true, BoletoEnabled = true }
        };

        return Ok(new { banks });
    }

    /// <summary>
    /// Obtém limites por tipo de transação
    /// </summary>
    [HttpGet("limits/{transactionType}")]
    public async Task<IActionResult> GetLimits([FromRoute] string transactionType)
    {
        _logger.LogInformation("Obtendo limites para {TransactionType}", transactionType);
        
        // Limites de transação (dados estáticos do sistema)
        
        var limits = transactionType.ToLower() switch
        {
            "pix" => new LimitResponse { Type = "PIX", MinAmount = 0.01m, MaxAmount = 20000.00m, DailyLimit = 50000.00m, MonthlyLimit = 200000.00m },
            "ted" => new LimitResponse { Type = "TED", MinAmount = 1.00m, MaxAmount = 100000.00m, DailyLimit = 500000.00m, MonthlyLimit = 2000000.00m },
            "boleto" => new LimitResponse { Type = "Boleto", MinAmount = 5.00m, MaxAmount = 50000.00m, DailyLimit = 100000.00m, MonthlyLimit = 500000.00m },
            "crypto" => new LimitResponse { Type = "Crypto", MinAmount = 10.00m, MaxAmount = 10000.00m, DailyLimit = 25000.00m, MonthlyLimit = 100000.00m },
            _ => null
        };

        if (limits == null)
            return NotFound(new { message = "Tipo de transação não encontrado" });

        return Ok(limits);
    }

    /// <summary>
    /// Obtém configurações gerais do sistema
    /// </summary>
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemConfig()
    {
        _logger.LogInformation("Obtendo configurações do sistema");
        
        // Configurações gerais do sistema (dados estáticos)
        
        var config = new SystemConfigResponse
        {
            MaintenanceMode = false,
            PixEnabled = true,
            TedEnabled = true,
            BoletoEnabled = true,
            CryptoEnabled = true,
            MaxRetryAttempts = 3,
            TimeoutSeconds = 30,
            RateLimitPerMinute = 100
        };

        return Ok(config);
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ConfigService", timestamp = DateTime.UtcNow });
    }
}

public class FeeResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("fixedFee")]
    public decimal FixedFee { get; set; }
    
    [JsonPropertyName("percentageFee")]
    public decimal PercentageFee { get; set; }
    
    [JsonPropertyName("minFee")]
    public decimal MinFee { get; set; }
    
    [JsonPropertyName("maxFee")]
    public decimal MaxFee { get; set; }
}

public class BankConfigResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    
    [JsonPropertyName("pixEnabled")]
    public bool PixEnabled { get; set; }
    
    [JsonPropertyName("tedEnabled")]
    public bool TedEnabled { get; set; }
    
    [JsonPropertyName("boletoEnabled")]
    public bool BoletoEnabled { get; set; }
}

public class LimitResponse
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("minAmount")]
    public decimal MinAmount { get; set; }
    
    [JsonPropertyName("maxAmount")]
    public decimal MaxAmount { get; set; }
    
    [JsonPropertyName("dailyLimit")]
    public decimal DailyLimit { get; set; }
    
    [JsonPropertyName("monthlyLimit")]
    public decimal MonthlyLimit { get; set; }
}

public class SystemConfigResponse
{
    [JsonPropertyName("maintenanceMode")]
    public bool MaintenanceMode { get; set; }
    
    [JsonPropertyName("pixEnabled")]
    public bool PixEnabled { get; set; }
    
    [JsonPropertyName("tedEnabled")]
    public bool TedEnabled { get; set; }
    
    [JsonPropertyName("boletoEnabled")]
    public bool BoletoEnabled { get; set; }
    
    [JsonPropertyName("cryptoEnabled")]
    public bool CryptoEnabled { get; set; }
    
    [JsonPropertyName("maxRetryAttempts")]
    public int MaxRetryAttempts { get; set; }
    
    [JsonPropertyName("timeoutSeconds")]
    public int TimeoutSeconds { get; set; }
    
    [JsonPropertyName("rateLimitPerMinute")]
    public int RateLimitPerMinute { get; set; }
}
