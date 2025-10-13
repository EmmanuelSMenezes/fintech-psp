using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FintechPSP.AuthService.Services;

/// <summary>
/// Serviço para geração de tokens JWT
/// </summary>
public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gera token JWT com claims customizados
    /// </summary>
    public string GenerateToken(Dictionary<string, object> claims)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Mortadela";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "Mortadela";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenClaims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Adicionar claims customizados
        foreach (var claim in claims)
        {
            tokenClaims.Add(new Claim(claim.Key, claim.Value.ToString() ?? string.Empty));
        }

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: tokenClaims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        _logger.LogInformation("Token JWT gerado para subject: {Subject}", claims.GetValueOrDefault("sub", "unknown"));
        
        return tokenString;
    }

    /// <summary>
    /// Gera token JWT para usuário do sistema
    /// </summary>
    public string GenerateToken(Models.SystemUser user)
    {
        var claims = new Dictionary<string, object>
        {
            ["sub"] = user.Id.ToString(),
            ["email"] = user.Email,
            ["name"] = user.Name,
            ["role"] = user.Role,
            ["is_master"] = user.IsMaster.ToString().ToLower(),
            ["scope"] = "admin", // Todos os usuários do backoffice têm scope admin
            ["auth_type"] = "user_login"
        };

        return GenerateToken(claims);
    }
}
