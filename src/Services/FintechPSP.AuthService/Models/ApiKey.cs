using System.ComponentModel.DataAnnotations;

namespace FintechPSP.AuthService.Models;

/// <summary>
/// Modelo para API Keys de clientes
/// </summary>
public class ApiKey
{
    /// <summary>
    /// ID único da API Key
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Chave pública (visível para o cliente)
    /// </summary>
    [Required]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Hash da chave secreta (nunca exposta)
    /// </summary>
    [Required]
    public string SecretHash { get; set; } = string.Empty;

    /// <summary>
    /// ID da empresa proprietária
    /// </summary>
    [Required]
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Nome/descrição da API Key
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Escopos/permissões da API Key
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Se a API Key está ativa
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data de expiração (opcional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Último uso da API Key
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// IP de origem permitido (opcional)
    /// </summary>
    public string? AllowedIp { get; set; }

    /// <summary>
    /// Limite de requests por minuto
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 100;

    /// <summary>
    /// Usuário que criou a API Key
    /// </summary>
    public Guid CreatedBy { get; set; }
}

/// <summary>
/// Request para criar API Key
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// ID da empresa
    /// </summary>
    [Required]
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Nome/descrição da API Key
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Escopos solicitados
    /// </summary>
    public List<string> Scopes { get; set; } = new() { "transactions", "balance" };

    /// <summary>
    /// Data de expiração (opcional)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// IP permitido (opcional)
    /// </summary>
    public string? AllowedIp { get; set; }

    /// <summary>
    /// Limite de requests por minuto
    /// </summary>
    public int RateLimitPerMinute { get; set; } = 100;
}

/// <summary>
/// Response da criação de API Key
/// </summary>
public class CreateApiKeyResponse
{
    /// <summary>
    /// ID da API Key criada
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Chave pública
    /// </summary>
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Chave secreta (só retornada na criação)
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Nome da API Key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Escopos concedidos
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data de expiração
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}

/// <summary>
/// Request para autenticação via API Key
/// </summary>
public class ApiKeyAuthRequest
{
    /// <summary>
    /// Chave pública
    /// </summary>
    [Required]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>
    /// Chave secreta
    /// </summary>
    [Required]
    public string SecretKey { get; set; } = string.Empty;
}

/// <summary>
/// Response da autenticação via API Key
/// </summary>
public class ApiKeyAuthResponse
{
    /// <summary>
    /// Token JWT gerado
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo do token
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tempo de expiração em segundos
    /// </summary>
    public int ExpiresIn { get; set; } = 3600;

    /// <summary>
    /// Escopos do token
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// ID da empresa
    /// </summary>
    public Guid CompanyId { get; set; }
}

/// <summary>
/// Escopos disponíveis para API Keys
/// </summary>
public static class ApiKeyScopes
{
    public const string Transactions = "transactions";
    public const string Balance = "balance";
    public const string Companies = "companies";
    public const string Users = "users";
    public const string Webhooks = "webhooks";
    public const string Reports = "reports";
    public const string Admin = "admin";

    public static readonly List<string> AllScopes = new()
    {
        Transactions, Balance, Companies, Users, Webhooks, Reports, Admin
    };

    public static readonly List<string> ClientScopes = new()
    {
        Transactions, Balance, Webhooks, Reports
    };
}
