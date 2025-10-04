using System.Text.Json;
using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.OAuth;

namespace FintechPSP.IntegrationService.Services.Sicoob;

/// <summary>
/// Serviço de autenticação OAuth 2.0 com mTLS para APIs do Sicoob
/// </summary>
public class SicoobAuthService : ISicoobAuthService
{
    private readonly SicoobSettings _settings;
    private readonly ILogger<SicoobAuthService> _logger;
    private readonly HttpClient _httpClient;
    private TokenResponse? _cachedToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public SicoobAuthService(
        IOptions<SicoobSettings> settings,
        ILogger<SicoobAuthService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _settings = settings.Value;
        _logger = logger;

        // Cria o HttpClient com o nome "SicoobAuth" que já está configurado com mTLS
        _httpClient = httpClientFactory.CreateClient("SicoobAuth");

        _logger.LogInformation("✅ SicoobAuthService inicializado com HttpClient 'SicoobAuth' (mTLS configurado)");
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        // Para sandbox, usar token fixo
        if (!string.IsNullOrEmpty(_settings.AccessToken))
        {
            _logger.LogDebug("Usando token fixo do Sandbox: {Token}", _settings.AccessToken.Substring(0, 8) + "...");
            return _settings.AccessToken;
        }

        // Verifica se tem token em cache e se está válido
        if (_cachedToken != null && !_cachedToken.IsExpired && !_cachedToken.IsExpiringSoon)
        {
            _logger.LogDebug("Usando token em cache. Expira em: {ExpiresAt}", _cachedToken.ExpiresAt);
            return _cachedToken.AccessToken;
        }

        // Obtém novo token
        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check após obter o lock
            if (_cachedToken != null && !_cachedToken.IsExpired && !_cachedToken.IsExpiringSoon)
            {
                return _cachedToken.AccessToken;
            }

            _logger.LogInformation("Obtendo novo token de acesso...");
            var tokenResponse = await RefreshTokenAsync(cancellationToken);
            return tokenResponse.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public async Task<TokenResponse> RefreshTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando novo token OAuth 2.0 ao Sicoob...");

            // Prepara os dados do formulário
            var formData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _settings.ClientId }
            };

            // Adiciona client_secret se configurado
            if (!string.IsNullOrEmpty(_settings.ClientSecret))
            {
                formData.Add("client_secret", _settings.ClientSecret);
            }

            // Adiciona todos os escopos
            var allScopes = _settings.Scopes.GetAllScopes();
            if (!string.IsNullOrEmpty(allScopes))
            {
                formData.Add("scope", allScopes);
            }

            var content = new FormUrlEncodedContent(formData);

            // Faz a requisição com mTLS (certificado já configurado no HttpClient)
            _logger.LogDebug("🔐 Enviando requisição OAuth 2.0 com mTLS para: {AuthUrl}", _settings.AuthUrl);
            _logger.LogDebug("   Client ID: {ClientId}", _settings.ClientId);
            _logger.LogDebug("   Scopes: {Scopes}", allScopes);

            var response = await _httpClient.PostAsync(_settings.AuthUrl, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "Erro ao obter token. Status: {StatusCode}, Resposta: {Response}",
                    response.StatusCode,
                    errorContent);
                throw new HttpRequestException(
                    $"Erro ao obter token de acesso: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse == null)
            {
                throw new InvalidOperationException("Resposta de token inválida");
            }

            // Calcula a data de expiração
            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

            // Armazena em cache
            _cachedToken = tokenResponse;

            _logger.LogInformation(
                "Token obtido com sucesso. Tipo: {TokenType}, Expira em: {ExpiresAt}",
                tokenResponse.TokenType,
                tokenResponse.ExpiresAt);

            return tokenResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter token de acesso do Sicoob");
            throw;
        }
    }

    public bool IsTokenValid()
    {
        return _cachedToken != null && !_cachedToken.IsExpired;
    }
}
