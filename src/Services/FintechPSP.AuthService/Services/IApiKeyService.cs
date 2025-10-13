using FintechPSP.AuthService.Models;
using FintechPSP.AuthService.Repositories;

namespace FintechPSP.AuthService.Services;

/// <summary>
/// Interface para serviço de API Keys
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Cria uma nova API Key para uma empresa
    /// </summary>
    Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request, Guid createdBy);

    /// <summary>
    /// Autentica usando API Key e retorna JWT token
    /// </summary>
    Task<ApiKeyAuthResponse?> AuthenticateAsync(ApiKeyAuthRequest request, string? clientIp = null);

    /// <summary>
    /// Lista API Keys de uma empresa
    /// </summary>
    Task<(List<ApiKey> apiKeys, int total)> GetCompanyApiKeysAsync(Guid companyId, int page = 1, int limit = 10);

    /// <summary>
    /// Obtém API Key por ID
    /// </summary>
    Task<ApiKey?> GetApiKeyAsync(Guid id);

    /// <summary>
    /// Ativa/desativa API Key
    /// </summary>
    Task<bool> UpdateApiKeyStatusAsync(Guid id, bool isActive);

    /// <summary>
    /// Deleta API Key
    /// </summary>
    Task<bool> DeleteApiKeyAsync(Guid id);

    /// <summary>
    /// Valida se API Key tem permissão para um escopo
    /// </summary>
    Task<bool> ValidateApiKeyScopeAsync(string publicKey, string scope);
}

/// <summary>
/// Implementação do serviço de API Keys
/// </summary>
public class ApiKeyService : IApiKeyService
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IJwtService _jwtService;
    private readonly ILogger<ApiKeyService> _logger;

    public ApiKeyService(
        IApiKeyRepository apiKeyRepository,
        IJwtService jwtService,
        ILogger<ApiKeyService> logger)
    {
        _apiKeyRepository = apiKeyRepository;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<CreateApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request, Guid createdBy)
    {
        // Gerar chaves únicas
        var publicKey = GeneratePublicKey();
        var secretKey = GenerateSecretKey();
        var secretHash = BCrypt.Net.BCrypt.HashPassword(secretKey);

        // Verificar se a chave pública já existe (improvável, mas seguro)
        while (await _apiKeyRepository.PublicKeyExistsAsync(publicKey))
        {
            publicKey = GeneratePublicKey();
        }

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            PublicKey = publicKey,
            SecretHash = secretHash,
            CompanyId = request.CompanyId,
            Name = request.Name,
            Scopes = ValidateScopes(request.Scopes),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt,
            AllowedIp = request.AllowedIp,
            RateLimitPerMinute = request.RateLimitPerMinute,
            CreatedBy = createdBy
        };

        var createdApiKey = await _apiKeyRepository.CreateAsync(apiKey);

        _logger.LogInformation("API Key criada para empresa {CompanyId}: {PublicKey}",
            request.CompanyId, publicKey);

        return new CreateApiKeyResponse
        {
            Id = createdApiKey.Id,
            PublicKey = publicKey,
            SecretKey = secretKey, // Só retornado na criação
            Name = createdApiKey.Name,
            Scopes = createdApiKey.Scopes,
            CreatedAt = createdApiKey.CreatedAt,
            ExpiresAt = createdApiKey.ExpiresAt
        };
    }

    public async Task<ApiKeyAuthResponse?> AuthenticateAsync(ApiKeyAuthRequest request, string? clientIp = null)
    {
        try
        {
            var apiKey = await _apiKeyRepository.GetByPublicKeyAsync(request.PublicKey);

            if (apiKey == null || !apiKey.IsActive)
            {
                _logger.LogWarning("Tentativa de autenticação com API Key inválida: {PublicKey}", request.PublicKey);
                return null;
            }

            // Verificar se expirou
            if (apiKey.ExpiresAt.HasValue && apiKey.ExpiresAt.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Tentativa de autenticação com API Key expirada: {PublicKey}", request.PublicKey);
                return null;
            }

            // Verificar IP se configurado
            if (!string.IsNullOrEmpty(apiKey.AllowedIp) && apiKey.AllowedIp != clientIp)
            {
                _logger.LogWarning("Tentativa de autenticação com IP não autorizado. API Key: {PublicKey}, IP: {ClientIp}",
                    request.PublicKey, clientIp);
                return null;
            }

            // Verificar secret
            if (!BCrypt.Net.BCrypt.Verify(request.SecretKey, apiKey.SecretHash))
            {
                _logger.LogWarning("Tentativa de autenticação com secret inválido: {PublicKey}", request.PublicKey);
                return null;
            }

            // Atualizar último uso
            await _apiKeyRepository.UpdateLastUsedAsync(apiKey.Id, DateTime.UtcNow);

            // Gerar JWT token
            var token = _jwtService.GenerateToken(new Dictionary<string, object>
            {
                ["sub"] = apiKey.CompanyId.ToString(),
                ["company_id"] = apiKey.CompanyId.ToString(),
                ["api_key_id"] = apiKey.Id.ToString(),
                ["scopes"] = string.Join(",", apiKey.Scopes),
                ["auth_type"] = "api_key"
            });

            _logger.LogInformation("Autenticação via API Key bem-sucedida: {PublicKey} para empresa {CompanyId}",
                request.PublicKey, apiKey.CompanyId);

            return new ApiKeyAuthResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                Scopes = apiKey.Scopes,
                CompanyId = apiKey.CompanyId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na autenticação via API Key: {PublicKey}", request.PublicKey);
            return null;
        }
    }

    public async Task<(List<ApiKey> apiKeys, int total)> GetCompanyApiKeysAsync(Guid companyId, int page = 1, int limit = 10)
    {
        return await _apiKeyRepository.GetByCompanyAsync(companyId, page, limit);
    }

    public async Task<ApiKey?> GetApiKeyAsync(Guid id)
    {
        return await _apiKeyRepository.GetByIdAsync(id);
    }

    public async Task<bool> UpdateApiKeyStatusAsync(Guid id, bool isActive)
    {
        try
        {
            await _apiKeyRepository.UpdateStatusAsync(id, isActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar status da API Key {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteApiKeyAsync(Guid id)
    {
        try
        {
            await _apiKeyRepository.DeleteAsync(id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar API Key {Id}", id);
            return false;
        }
    }

    public async Task<bool> ValidateApiKeyScopeAsync(string publicKey, string scope)
    {
        var apiKey = await _apiKeyRepository.GetByPublicKeyAsync(publicKey);
        return apiKey?.Scopes.Contains(scope) == true;
    }

    private static string GeneratePublicKey()
    {
        return $"pk_{Guid.NewGuid():N}";
    }

    private static string GenerateSecretKey()
    {
        return $"sk_{Guid.NewGuid():N}{Guid.NewGuid():N}";
    }

    private static List<string> ValidateScopes(List<string> requestedScopes)
    {
        return requestedScopes
            .Where(scope => ApiKeyScopes.ClientScopes.Contains(scope))
            .ToList();
    }
}
