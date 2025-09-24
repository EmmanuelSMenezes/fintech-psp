using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.AuthService.Commands;
using FintechPSP.AuthService.DTOs;
using FintechPSP.AuthService.Repositories;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FintechPSP.AuthService.Handlers;

/// <summary>
/// Handler para login de usuário
/// </summary>
public class LoginHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly ISystemUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public LoginHandler(ISystemUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Buscar usuário por email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Verificar senha
        bool isPasswordValid = false;

        try
        {
            // Tentar verificação BCrypt primeiro
            if (user.PasswordHash.StartsWith("$2"))
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            }
            else
            {
                // Fallback para senha em texto plano (apenas para desenvolvimento)
                isPasswordValid = request.Password == user.PasswordHash;
            }
        }
        catch (Exception)
        {
            // Se BCrypt falhar, tentar comparação direta
            isPasswordValid = request.Password == user.PasswordHash;
        }

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException("Email ou senha inválidos");
        }

        // Atualizar último login
        await _userRepository.UpdateLastLoginAsync(user.Id);

        // Gerar token JWT
        var token = GenerateJwtToken(user);

        return new LoginResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = 3600, // 1 hora
            User = new UserInfo
            {
                Id = user.Id.ToString(),
                Email = user.Email,
                Name = user.Name,
                Role = user.Role,
                IsMaster = user.IsMaster
            }
        };
    }

    private string GenerateJwtToken(Models.SystemUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Mortadela";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "Mortadela";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim("role", user.Role),
            new Claim("is_master", user.IsMaster.ToString().ToLower()),
            new Claim("scope", "admin"), // Todos os usuários do backoffice têm scope admin
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
