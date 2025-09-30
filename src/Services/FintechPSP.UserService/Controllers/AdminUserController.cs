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
// [Authorize(Policy = "AdminScope")] // Temporariamente desabilitado - problema no API Gateway
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
                Document = "", // Campo não existe na tabela ainda
                Active = u.IsActive,
                CreatedAt = u.CreatedAt,
                Phone = "", // Campo não existe na tabela ainda
                Address = "", // Campo não existe na tabela ainda
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

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("🎯 ADMIN CONTROLLER - POST /admin/users CHAMADO!");
        _logger.LogInformation("Admin criando usuário: {Email}", request.Email);

        // Verificar ModelState
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { message = string.Join(", ", errors) });
        }

        try
        {
            // Validações básicas
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest(new { message = "Email e Nome são obrigatórios" });
            }

            if (!IsValidEmail(request.Email))
            {
                return BadRequest(new { message = "Email inválido" });
            }

            // Verificar se email já existe
            var existingUser = await _systemUserRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return Conflict(new { message = "Email já está em uso" });
            }

            // Criar usuário
            var newUser = new SystemUser
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Role = request.Role ?? "cliente",
                IsActive = request.IsActive ?? true,
                IsMaster = false,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? "123456"),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _systemUserRepository.CreateAsync(newUser);

            var response = new UserResponse
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Email = newUser.Email,
                Document = "",
                Active = newUser.IsActive,
                CreatedAt = newUser.CreatedAt,
                Phone = "",
                Address = "",
                Role = newUser.Role,
                LastLoginAt = newUser.LastLoginAt
            };

            _logger.LogInformation("Usuário criado com sucesso: {UserId}", newUser.Id);
            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Admin atualizando usuário: {UserId}", id);

        try
        {
            var existingUser = await _systemUserRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            // Validações
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != existingUser.Email)
            {
                if (!IsValidEmail(request.Email))
                {
                    return BadRequest(new { message = "Email inválido" });
                }

                var emailExists = await _systemUserRepository.GetByEmailAsync(request.Email);
                if (emailExists != null)
                {
                    return Conflict(new { message = "Email já está em uso" });
                }
                existingUser.Email = request.Email;
            }

            // Atualizar campos
            if (!string.IsNullOrWhiteSpace(request.Name))
                existingUser.Name = request.Name;

            if (!string.IsNullOrWhiteSpace(request.Role))
                existingUser.Role = request.Role;

            if (request.IsActive.HasValue)
                existingUser.IsActive = request.IsActive.Value;

            if (!string.IsNullOrWhiteSpace(request.Password))
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            existingUser.UpdatedAt = DateTime.UtcNow;

            await _systemUserRepository.UpdateAsync(existingUser);

            var response = new UserResponse
            {
                Id = existingUser.Id,
                Name = existingUser.Name,
                Email = existingUser.Email,
                Document = "",
                Active = existingUser.IsActive,
                CreatedAt = existingUser.CreatedAt,
                Phone = "",
                Address = "",
                Role = existingUser.Role,
                LastLoginAt = existingUser.LastLoginAt
            };

            _logger.LogInformation("Usuário atualizado com sucesso: {UserId}", id);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        _logger.LogInformation("Admin excluindo usuário: {UserId}", id);

        try
        {
            var existingUser = await _systemUserRepository.GetByIdAsync(id);
            if (existingUser == null)
            {
                return NotFound(new { message = "Usuário não encontrado" });
            }

            if (existingUser.IsMaster)
            {
                return BadRequest(new { message = "Não é possível excluir usuário master" });
            }

            await _systemUserRepository.DeleteAsync(id);

            _logger.LogInformation("Usuário excluído com sucesso: {UserId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir usuário {UserId}", id);
            return StatusCode(500, new { message = "Erro interno do servidor" });
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Obtém usuário por ID (admin)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser([FromRoute] Guid id)
    {
        try
        {
            _logger.LogInformation("Admin obtendo usuário {UserId}", id);

            var systemUser = await _systemUserRepository.GetByIdAsync(id);

            if (systemUser == null)
            {
                _logger.LogWarning("Usuário {UserId} não encontrado", id);
                return NotFound(new { error = "not_found", message = "Usuário não encontrado" });
            }

            var user = new UserResponse
            {
                Id = systemUser.Id,
                Name = systemUser.Name,
                Email = systemUser.Email,
                Document = systemUser.Document,
                Active = systemUser.Active,
                CreatedAt = systemUser.CreatedAt,
                Phone = systemUser.Phone,
                Address = systemUser.Address
            };

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário {UserId}", id);
            return StatusCode(500, new { error = "internal_error", message = "Erro interno do servidor" });
        }
    }


}
