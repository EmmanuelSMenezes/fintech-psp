using System;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Controllers;

/// <summary>
/// Controller para gerenciamento de usuários - Administração
/// </summary>
[ApiController]
[Route("admin/users")]
[Authorize(Policy = "AdminScope")]
[Produces("application/json")]
public class AdminUserController : ControllerBase
{
    private readonly ILogger<AdminUserController> _logger;
    private readonly ISystemUserRepository _systemUserRepository;

    public AdminUserController(ILogger<AdminUserController> logger, ISystemUserRepository systemUserRepository)
    {
        _logger = logger;
        _systemUserRepository = systemUserRepository;
    }

    /// <summary>
    /// Lista todos os usuários (admin)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        _logger.LogInformation("Admin listando usuários - página {Page}", page);

        try
        {
            var (systemUsers, totalCount) = await _systemUserRepository.GetPagedAsync(page, pageSize);

            var users = systemUsers.Select(u => new UserResponse
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Document = u.Document ?? "",
                Active = u.IsActive,
                CreatedAt = u.CreatedAt,
                Phone = u.Phone ?? "",
                Address = u.Address ?? "",
                Role = u.Role,
                LastLoginAt = u.LastLoginAt
            }).ToArray();

            _logger.LogInformation("Retornando {Count} usuários de {Total} total", users.Length, totalCount);

            return Ok(new { users, totalCount, page, pageSize });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar usuários");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    /// <summary>
    /// Obtém usuário por ID (admin)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Admin obtendo usuário {UserId}", id);
        
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
    /// Cria novo usuário (admin)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Admin criando usuário {Email}", request.Email);
        
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
    /// Atualiza usuário (admin)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Admin atualizando usuário {UserId}", id);
        
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
    /// Remove usuário (admin)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
    {
        _logger.LogInformation("Admin removendo usuário {UserId}", id);
        
        await Task.Delay(50); // Simular remoção no DB
        
        return NoContent();
    }
}
