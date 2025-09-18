using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    public UserController(ILogger<UserController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Lista usuários
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Listando usuários - página {Page}", page);
        
        await Task.Delay(50); // Simular consulta DB
        
        var users = new[]
        {
            new UserResponse { Id = Guid.NewGuid(), Name = "João Silva", Email = "joao@exemplo.com", Document = "12345678901", Active = true, CreatedAt = DateTime.UtcNow.AddDays(-30) },
            new UserResponse { Id = Guid.NewGuid(), Name = "Maria Santos", Email = "maria@exemplo.com", Document = "98765432100", Active = true, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new UserResponse { Id = Guid.NewGuid(), Name = "Pedro Costa", Email = "pedro@exemplo.com", Document = "11122233344", Active = false, CreatedAt = DateTime.UtcNow.AddDays(-5) }
        };

        return Ok(new { users, totalCount = 3, page, pageSize });
    }

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Obtendo usuário {UserId}", id);
        
        await Task.Delay(30); // Simular consulta DB
        
        var user = new UserResponse 
        { 
            Id = id, 
            Name = "João Silva", 
            Email = "joao@exemplo.com", 
            Document = "12345678901", 
            Active = true, 
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            Phone = "+5511999887766",
            Address = "Rua das Flores, 123 - São Paulo/SP"
        };

        return Ok(user);
    }

    /// <summary>
    /// Cria novo usuário
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Criando usuário {Email}", request.Email);
        
        // Validações básicas
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Nome é obrigatório" });
        
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email é obrigatório" });
        
        if (string.IsNullOrWhiteSpace(request.Document))
            return BadRequest(new { message = "Documento é obrigatório" });
        
        await Task.Delay(100); // Simular criação no DB
        
        var user = new UserResponse
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Document = request.Document,
            Phone = request.Phone,
            Address = request.Address,
            Active = true,
            CreatedAt = DateTime.UtcNow
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    /// <summary>
    /// Atualiza usuário
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Atualizando usuário {UserId}", id);
        
        await Task.Delay(80); // Simular atualização no DB
        
        var user = new UserResponse
        {
            Id = id,
            Name = request.Name ?? "João Silva",
            Email = request.Email ?? "joao@exemplo.com",
            Document = "12345678901", // Documento não pode ser alterado
            Phone = request.Phone,
            Address = request.Address,
            Active = request.Active ?? true,
            CreatedAt = DateTime.UtcNow.AddDays(-30)
        };

        return Ok(user);
    }

    /// <summary>
    /// Desativa usuário
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Desativando usuário {UserId}", id);
        
        await Task.Delay(50); // Simular desativação no DB
        
        return NoContent();
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "UserService", timestamp = DateTime.UtcNow });
    }
}

public class UserResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("document")]
    public string Document { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("lastLoginAt")]
    public DateTime? LastLoginAt { get; set; }
}

public class CreateUserRequest
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("document")]
    public string Document { get; set; } = string.Empty;
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class UpdateUserRequest
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("email")]
    public string? Email { get; set; }
    
    [JsonPropertyName("phone")]
    public string? Phone { get; set; }
    
    [JsonPropertyName("address")]
    public string? Address { get; set; }
    
    [JsonPropertyName("active")]
    public bool? Active { get; set; }
}
