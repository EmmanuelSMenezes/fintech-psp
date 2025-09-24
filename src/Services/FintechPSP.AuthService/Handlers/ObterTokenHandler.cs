using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.AuthService.Commands;
using FintechPSP.AuthService.DTOs;
using FintechPSP.AuthService.Repositories;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.Shared.Infrastructure.Messaging;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FintechPSP.AuthService.Handlers;

/// <summary>
/// Handler para obter token OAuth 2.0
/// </summary>
public class ObterTokenHandler : IRequestHandler<ObterTokenCommand, TokenResponse>
{
    private readonly IClientRepository _clientRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly IConfiguration _configuration;

    public ObterTokenHandler(
        IClientRepository clientRepository,
        IEventPublisher eventPublisher,
        IConfiguration configuration)
    {
        _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<TokenResponse> Handle(ObterTokenCommand request, CancellationToken cancellationToken)
    {
        // Validar grant_type
        if (request.GrantType != "client_credentials")
        {
            throw new UnauthorizedAccessException("Unsupported grant type");
        }

        // Validar cliente
        var client = await _clientRepository.GetByClientIdAsync(request.ClientId);
        if (client == null || !client.IsActive)
        {
            throw new UnauthorizedAccessException("Invalid client");
        }

        // Validar client_secret
        if (!VerifyClientSecret(client.ClientSecret, request.ClientSecret))
        {
            throw new UnauthorizedAccessException("Invalid client credentials");
        }

        // Validar scopes
        var requestedScopes = request.Scope?.Split(' ') ?? Array.Empty<string>();
        var allowedScopes = ValidateScopes(client.AllowedScopes, requestedScopes);

        // Gerar token JWT
        var token = GenerateJwtToken(client.ClientId, allowedScopes);
        var expiresIn = 3600; // 1 hora
        var expiresAt = DateTime.UtcNow.AddSeconds(expiresIn);

        // Publicar evento
        var tokenRenovadoEvent = new TokenRenovado(
            client.ClientId,
            token,
            expiresAt,
            allowedScopes);

        await _eventPublisher.PublishAsync(tokenRenovadoEvent);

        var authEvent = new AutenticacaoOAuthRealizada(
            client.ClientId,
            allowedScopes,
            expiresAt);

        await _eventPublisher.PublishAsync(authEvent);

        return new TokenResponse
        {
            AccessToken = token,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            Scope = string.Join(" ", allowedScopes)
        };
    }

    private static bool VerifyClientSecret(string storedSecret, string providedSecret)
    {
        // Em produção, usar hash seguro (bcrypt, scrypt, etc.)
        return storedSecret == providedSecret;
    }

    private static string[] ValidateScopes(string[] allowedScopes, string[] requestedScopes)
    {
        if (requestedScopes.Length == 0)
        {
            return allowedScopes;
        }

        var validScopes = new List<string>();
        foreach (var scope in requestedScopes)
        {
            if (allowedScopes.Contains(scope))
            {
                validScopes.Add(scope);
            }
        }

        return validScopes.ToArray();
    }

    private string GenerateJwtToken(string clientId, string[] scopes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "mortadela-super-secret-key-that-should-be-at-least-256-bits"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var scope in scopes)
        {
            claims.Add(new Claim("scope", scope));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "Mortadela",
            audience: _configuration["Jwt:Audience"] ?? "Mortadela",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
