using Microsoft.Extensions.Caching.Memory;
using FintechPSP.IntegrationService.Models.Sicoob.OAuth;

namespace FintechPSP.IntegrationService.Services.Sicoob;

/// <summary>
/// Cache para tokens OAuth do Sicoob
/// </summary>
public class SicoobTokenCache : ISicoobTokenCache
{
    private readonly IMemoryCache _cache;
    private readonly SicoobAuthService _authService; // Usar implementação concreta para evitar circular dependency
    private readonly ILogger<SicoobTokenCache> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    private const string TOKEN_CACHE_KEY = "sicoob_oauth_token";
    private const int TOKEN_EXPIRY_MINUTES = 50; // 10 minutos antes do real (60min)

    public SicoobTokenCache(
        IMemoryCache cache,
        SicoobAuthService authService, // Usar implementação concreta
        ILogger<SicoobTokenCache> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken = default)
    {
        // Tentar obter do cache primeiro
        if (_cache.TryGetValue(TOKEN_CACHE_KEY, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
        {
            _logger.LogDebug("Token OAuth obtido do cache");
            return cachedToken;
        }

        // Se não estiver no cache, obter novo token (thread-safe)
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check locking pattern
            if (_cache.TryGetValue(TOKEN_CACHE_KEY, out cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                _logger.LogDebug("Token OAuth obtido do cache (double-check)");
                return cachedToken;
            }

            _logger.LogInformation("Obtendo novo token OAuth do Sicoob");
            var newToken = await _authService.GetAccessTokenAsync();

            if (string.IsNullOrEmpty(newToken))
            {
                throw new InvalidOperationException("Falha ao obter token OAuth do Sicoob");
            }

            // Armazenar no cache com expiração
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(TOKEN_EXPIRY_MINUTES),
                Priority = CacheItemPriority.High,
                Size = 1
            };

            _cache.Set(TOKEN_CACHE_KEY, newToken, cacheOptions);
            _logger.LogInformation("Token OAuth armazenado no cache por {Minutes} minutos", TOKEN_EXPIRY_MINUTES);

            return newToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void InvalidateToken()
    {
        _cache.Remove(TOKEN_CACHE_KEY);
        _logger.LogInformation("Token OAuth removido do cache");
    }

    public async Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Forçando refresh do token OAuth");
        
        InvalidateToken();
        return await GetTokenAsync(cancellationToken);
    }

    public TokenCacheStatus GetCacheStatus()
    {
        var hasToken = _cache.TryGetValue(TOKEN_CACHE_KEY, out string? token);
        
        return new TokenCacheStatus
        {
            HasCachedToken = hasToken && !string.IsNullOrEmpty(token),
            TokenPreview = hasToken && !string.IsNullOrEmpty(token) ? $"{token[..10]}..." : null,
            CacheKey = TOKEN_CACHE_KEY,
            ExpiryMinutes = TOKEN_EXPIRY_MINUTES
        };
    }
}

/// <summary>
/// Interface para cache de tokens Sicoob
/// </summary>
public interface ISicoobTokenCache
{
    Task<string> GetTokenAsync(CancellationToken cancellationToken = default);
    void InvalidateToken();
    Task<string> RefreshTokenAsync(CancellationToken cancellationToken = default);
    TokenCacheStatus GetCacheStatus();
}

/// <summary>
/// Status do cache de tokens
/// </summary>
public class TokenCacheStatus
{
    public bool HasCachedToken { get; set; }
    public string? TokenPreview { get; set; }
    public string CacheKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; }
}

/// <summary>
/// Extensão do SicoobAuthService para usar cache
/// </summary>
public class CachedSicoobAuthService : ISicoobAuthService
{
    private readonly ISicoobTokenCache _tokenCache;
    private readonly ILogger<CachedSicoobAuthService> _logger;

    public CachedSicoobAuthService(
        ISicoobTokenCache tokenCache,
        ILogger<CachedSicoobAuthService> logger)
    {
        _tokenCache = tokenCache ?? throw new ArgumentNullException(nameof(tokenCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _tokenCache.GetTokenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter token OAuth com cache");
            throw;
        }
    }

    public async Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var token = await _tokenCache.RefreshTokenAsync(cancellationToken);
            // Retornar um TokenResponse simples com o token obtido
            return new TokenResponse
            {
                AccessToken = token,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hora padrão
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token OAuth com cache");
            throw;
        }
    }

    public bool IsTokenValid()
    {
        try
        {
            var status = _tokenCache.GetCacheStatus();
            return status.HasCachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar validade do token");
            return false;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            // Implementar validação de token se necessário
            // Por enquanto, assumir que token não-vazio é válido
            return !string.IsNullOrEmpty(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar token OAuth");
            return false;
        }
    }

    public async Task RevokeTokenAsync(string token)
    {
        try
        {
            _tokenCache.InvalidateToken();
            _logger.LogInformation("Token OAuth revogado e removido do cache");
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao revogar token OAuth");
            throw;
        }
    }
}

/// <summary>
/// Middleware para monitoramento de tokens
/// </summary>
public class TokenMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenMonitoringMiddleware> _logger;

    public TokenMonitoringMiddleware(RequestDelegate next, ILogger<TokenMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISicoobTokenCache tokenCache)
    {
        // Verificar se é uma requisição para APIs Sicoob
        if (context.Request.Path.StartsWithSegments("/integrations/sicoob"))
        {
            var cacheStatus = tokenCache.GetCacheStatus();
            
            if (!cacheStatus.HasCachedToken)
            {
                _logger.LogWarning("Requisição Sicoob sem token em cache - Path: {Path}", context.Request.Path);
            }
            
            // Adicionar header com status do cache (para debug)
            context.Response.Headers.Add("X-Sicoob-Token-Cached", cacheStatus.HasCachedToken.ToString());
        }

        await _next(context);
    }
}

/// <summary>
/// Serviço de monitoramento de certificados
/// </summary>
public class CertificateMonitoringService : ICertificateMonitoringService
{
    private readonly ILogger<CertificateMonitoringService> _logger;
    private readonly IConfiguration _configuration;

    public CertificateMonitoringService(
        ILogger<CertificateMonitoringService> logger,
        IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<CertificateStatus> CheckCertificateStatusAsync()
    {
        try
        {
            var certificatePath = _configuration["Sicoob:CertificatePath"];
            var certificatePassword = _configuration["Sicoob:CertificatePassword"];

            if (string.IsNullOrEmpty(certificatePath))
            {
                return new CertificateStatus
                {
                    IsValid = false,
                    ErrorMessage = "Caminho do certificado não configurado"
                };
            }

            if (!File.Exists(certificatePath))
            {
                return new CertificateStatus
                {
                    IsValid = false,
                    ErrorMessage = "Arquivo de certificado não encontrado"
                };
            }

            // Carregar certificado
            var certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                certificatePath, certificatePassword);

            var daysUntilExpiry = (certificate.NotAfter - DateTime.Now).Days;

            return new CertificateStatus
            {
                IsValid = certificate.NotAfter > DateTime.Now,
                ExpiryDate = certificate.NotAfter,
                DaysUntilExpiry = daysUntilExpiry,
                Subject = certificate.Subject,
                Issuer = certificate.Issuer,
                Thumbprint = certificate.Thumbprint,
                NeedsRenewal = daysUntilExpiry <= 30 // Alerta 30 dias antes
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status do certificado");
            return new CertificateStatus
            {
                IsValid = false,
                ErrorMessage = ex.Message
            };
        }
    }
}

/// <summary>
/// Interface para monitoramento de certificados
/// </summary>
public interface ICertificateMonitoringService
{
    Task<CertificateStatus> CheckCertificateStatusAsync();
}

/// <summary>
/// Status do certificado
/// </summary>
public class CertificateStatus
{
    public bool IsValid { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int DaysUntilExpiry { get; set; }
    public string? Subject { get; set; }
    public string? Issuer { get; set; }
    public string? Thumbprint { get; set; }
    public bool NeedsRenewal { get; set; }
    public string? ErrorMessage { get; set; }
}
