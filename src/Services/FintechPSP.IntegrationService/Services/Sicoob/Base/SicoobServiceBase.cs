using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;

namespace FintechPSP.IntegrationService.Services.Sicoob.Base;

/// <summary>
/// Classe base para todos os serviços do Sicoob
/// </summary>
public abstract class SicoobServiceBase
{
    protected readonly HttpClient HttpClient;
    protected readonly ISicoobAuthService AuthService;
    protected readonly SicoobSettings Settings;
    protected readonly ILogger Logger;
    protected readonly JsonSerializerOptions JsonOptions;

    protected SicoobServiceBase(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger logger,
        string httpClientName = "SicoobApi")
    {
        HttpClient = httpClientFactory.CreateClient(httpClientName);
        AuthService = authService;
        Settings = settings.Value;
        Logger = logger;
        JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Adiciona o token de autenticação e client_id ao header da requisição
    /// </summary>
    protected async Task<HttpRequestMessage> CreateAuthenticatedRequestAsync(
        HttpMethod method,
        string url,
        CancellationToken cancellationToken = default)
    {
        var token = await AuthService.GetAccessTokenAsync(cancellationToken);
        var request = new HttpRequestMessage(method, url);

        // Adiciona o token Bearer
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Adiciona o client_id no header (obrigatório para Sicoob)
        request.Headers.Add("client_id", Settings.ClientId);

        Logger.LogDebug("Headers adicionados - Authorization: Bearer {Token}, client_id: {ClientId}",
            token.Substring(0, Math.Min(20, token.Length)) + "...",
            Settings.ClientId);

        return request;
    }

    /// <summary>
    /// Faz uma requisição GET autenticada
    /// </summary>
    protected async Task<T?> GetAsync<T>(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("GET: {Url}", url);

            var request = await CreateAuthenticatedRequestAsync(HttpMethod.Get, url, cancellationToken);
            var response = await HttpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<T>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao fazer GET em {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Faz uma requisição POST autenticada
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("POST: {Url}", url);

            var request = await CreateAuthenticatedRequestAsync(HttpMethod.Post, url, cancellationToken);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao fazer POST em {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Faz uma requisição PUT autenticada
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("PUT: {Url}", url);

            var request = await CreateAuthenticatedRequestAsync(HttpMethod.Put, url, cancellationToken);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao fazer PUT em {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Faz uma requisição PATCH autenticada
    /// </summary>
    protected async Task<TResponse?> PatchAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("PATCH: {Url}", url);

            var request = await CreateAuthenticatedRequestAsync(HttpMethod.Patch, url, cancellationToken);
            var json = JsonSerializer.Serialize(data, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await HttpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<TResponse>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao fazer PATCH em {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Faz uma requisição DELETE autenticada
    /// </summary>
    protected async Task<bool> DeleteAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("DELETE: {Url}", url);

            var request = await CreateAuthenticatedRequestAsync(HttpMethod.Delete, url, cancellationToken);
            var response = await HttpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessStatusCodeAsync(response, cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao fazer DELETE em {Url}", url);
            throw;
        }
    }

    /// <summary>
    /// Verifica se a resposta foi bem-sucedida e loga erros
    /// </summary>
    private async Task EnsureSuccessStatusCodeAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Logger.LogError(
                "Erro na requisição. Status: {StatusCode}, Resposta: {Response}",
                response.StatusCode,
                errorContent);

            throw new HttpRequestException(
                $"Erro na requisição: {response.StatusCode} - {errorContent}");
        }
    }
}
