using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.Models;
using BCrypt.Net;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários
/// </summary>
[ApiController]
[Route("client-users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly ISystemUserRepository _systemUserRepository;

    public UserController(ILogger<UserController> logger, ISystemUserRepository systemUserRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _systemUserRepository = systemUserRepository ?? throw new ArgumentNullException(nameof(systemUserRepository));
    }

    /// <summary>
    /// Lista usuários
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Listando usuários - página {Page}", page);

        try
        {
            var (users, totalCount) = await _systemUserRepository.GetPagedAsync(page, pageSize);

            var userResponses = users.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Document = u.Document ?? "",
                Active = u.IsActive,
                CreatedAt = u.CreatedAt
            }).ToArray();

            return Ok(new { users = userResponses, totalCount, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém usuário por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Obtendo usuário {UserId}", id);

        try
        {
            var user = await _systemUserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var userResponse = new UserResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Document = user.Document ?? "",
                Active = user.IsActive,
                CreatedAt = user.CreatedAt,
                Phone = user.Phone ?? "",
                Address = user.Address ?? ""
            };

            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário {UserId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Cria novo usuário
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateClientUserRequest request)
    {
        _logger.LogInformation("🚨 USER CONTROLLER - POST /client-users CHAMADO!");
        _logger.LogInformation("Criando usuário {Email}", request.Email);
        
        // Validações básicas
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { message = "Nome é obrigatório" });
        
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email é obrigatório" });
        
        if (string.IsNullOrWhiteSpace(request.Document))
            return BadRequest(new { message = "Documento é obrigatório" });

        try
        {
            // Verificar se email já existe
            var existingUser = await _systemUserRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Email já está em uso" });
            }

            var newUser = new SystemUser
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Document = request.Document,
                Phone = request.Phone,
                Address = request.Address,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Role = "cliente",
                PasswordHash = "" // Senha será definida posteriormente
            };

            var createdUser = await _systemUserRepository.CreateAsync(newUser);

            var userResponse = new UserResponse
            {
                Id = createdUser.Id,
                Name = createdUser.Name,
                Email = createdUser.Email,
                Document = createdUser.Document ?? "",
                Phone = createdUser.Phone ?? "",
                Address = createdUser.Address ?? "",
                Active = createdUser.IsActive,
                CreatedAt = createdUser.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser), new { id = userResponse.Id }, userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário {Email}", request.Email);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Atualiza usuário
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateClientUserRequest request)
    {
        _logger.LogInformation("Atualizando usuário {UserId}", id);

        try
        {
            var existingUser = await _systemUserRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Atualizar apenas campos fornecidos
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingUser.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                // Verificar se o novo email já está em uso por outro usuário
                var emailUser = await _systemUserRepository.GetByEmailAsync(request.Email);
                if (emailUser != null && emailUser.Id != id)
                {
                    return BadRequest(new { message = "Email já está em uso" });
                }
                existingUser.Email = request.Email;
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
                existingUser.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Address))
                existingUser.Address = request.Address;

            if (request.Active.HasValue)
                existingUser.IsActive = request.Active.Value;

            var updatedUser = await _systemUserRepository.UpdateAsync(existingUser);

            var userResponse = new UserResponse
            {
                Id = updatedUser.Id,
                Name = updatedUser.Name,
                Email = updatedUser.Email,
                Document = updatedUser.Document ?? "",
                Phone = updatedUser.Phone ?? "",
                Address = updatedUser.Address ?? "",
                Active = updatedUser.IsActive,
                CreatedAt = updatedUser.CreatedAt
            };

            return Ok(userResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Desativa usuário
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeactivateUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Desativando usuário {UserId}", id);

        try
        {
            var existingUser = await _systemUserRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            await _systemUserRepository.DeleteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar usuário {UserId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
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

public class CreateClientUserRequest
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

public class UpdateClientUserRequest
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
