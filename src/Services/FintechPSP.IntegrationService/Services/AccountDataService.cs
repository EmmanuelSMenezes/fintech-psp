using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FintechPSP.IntegrationService.Services;

/// <summary>
/// Implementação do serviço de dados de contas bancárias
/// Faz chamadas HTTP para o UserService para obter dados das contas
/// </summary>
public class AccountDataService : IAccountDataService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccountDataService> _logger;
    private readonly string _userServiceBaseUrl;

    public AccountDataService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<AccountDataService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _userServiceBaseUrl = _configuration["Services:UserService:BaseUrl"] ?? "http://localhost:5006";
    }

    public async Task<AccountData?> GetAccountByIdAsync(Guid contaId)
    {
        try
        {
            _logger.LogDebug("Obtendo dados da conta {ContaId}", contaId);

            // Fazer chamada HTTP real para UserService
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // TODO: Configurar URL base do UserService via configuração
            var userServiceUrl = "http://localhost:5006"; // ou via IConfiguration
            var response = await httpClient.GetAsync($"{userServiceUrl}/admin/contas/{contaId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var accountData = System.Text.Json.JsonSerializer.Deserialize<AccountData>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return accountData;
            }

            _logger.LogWarning("Conta {ContaId} não encontrada no UserService", contaId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter dados da conta {ContaId}", contaId);
            return null;
        }
    }

    public async Task<List<AccountData>> GetAccountsByClienteIdAsync(Guid clienteId)
    {
        try
        {
            _logger.LogDebug("Obtendo contas do cliente {ClienteId}", clienteId);
            
            // Mock data - em produção fazer chamada HTTP para UserService
            // GET /admin/contas/{clienteId} ou usar cache local
            
            var accounts = new List<AccountData>
            {
                new()
                {
                    ContaId = Guid.NewGuid(),
                    ClienteId = clienteId,
                    BankCode = "STARK",
                    AccountNumber = "12345-6",
                    Description = "Conta Principal Stark Bank",
                    CredentialsTokenId = "token_stark_001",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new()
                {
                    ContaId = Guid.NewGuid(),
                    ClienteId = clienteId,
                    BankCode = "EFI",
                    AccountNumber = "67890-1",
                    Description = "Conta Secundária Efí",
                    CredentialsTokenId = "token_efi_001",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new()
                {
                    ContaId = Guid.NewGuid(),
                    ClienteId = clienteId,
                    BankCode = "SICOOB",
                    AccountNumber = "11111-2",
                    Description = "Conta Backup Sicoob",
                    CredentialsTokenId = "token_sicoob_001",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return accounts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contas do cliente {ClienteId}", clienteId);
            return new List<AccountData>();
        }
    }

    public async Task<AccountCredentials?> GetAccountCredentialsAsync(string credentialsTokenId)
    {
        try
        {
            _logger.LogDebug("Obtendo credenciais para token {TokenId}", credentialsTokenId);
            
            // Em produção, fazer chamada para UserService para descriptografar credenciais
            // Isso requer autenticação de serviço-para-serviço
            
            // Mock credentials - em produção obter via API segura
            return new AccountCredentials
            {
                ClientId = "integration_client_001",
                ClientSecret = "super_secret_key_001",
                MtlsCert = null, // Certificado mTLS se necessário
                AdditionalData = new Dictionary<string, string>
                {
                    { "environment", "sandbox" },
                    { "version", "v1" }
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter credenciais para token {TokenId}", credentialsTokenId);
            return null;
        }
    }
}

/// <summary>
/// Implementação do serviço de configuração de prioridade
/// Faz chamadas HTTP para o ConfigService para obter configurações de roteamento
/// </summary>
public class PriorityConfigService : IPriorityConfigService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PriorityConfigService> _logger;
    private readonly string _configServiceBaseUrl;

    public PriorityConfigService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<PriorityConfigService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _configServiceBaseUrl = _configuration["Services:ConfigService:BaseUrl"] ?? "http://localhost:5007";
    }

    public async Task<PriorityConfiguration?> GetPriorityConfigAsync(Guid clienteId)
    {
        try
        {
            _logger.LogDebug("Obtendo configuração de prioridade para cliente {ClienteId}", clienteId);

            // Fazer chamada HTTP real para ConfigService
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // TODO: Configurar URL base do ConfigService via configuração
            var configServiceUrl = "http://localhost:5007"; // ou via IConfiguration
            var response = await httpClient.GetAsync($"{configServiceUrl}/banking/configs/roteamento?clienteId={clienteId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var priorityConfig = System.Text.Json.JsonSerializer.Deserialize<PriorityConfiguration>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return priorityConfig;
            }

            // Se não encontrar configuração, retornar configuração padrão
            _logger.LogInformation("Configuração de prioridade não encontrada para cliente {ClienteId}, usando padrão", clienteId);
            var defaultPriorities = new List<AccountPriority>
            {
                new() { ContaId = Guid.NewGuid(), BankCode = "SICOOB", Percentual = 100.0m }
            };

            return new PriorityConfiguration
            {
                ConfigId = Guid.NewGuid(),
                ClienteId = clienteId,
                Prioridades = defaultPriorities,
                TotalPercentual = defaultPriorities.Sum(p => p.Percentual),
                IsValid = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter configuração de prioridade para cliente {ClienteId}", clienteId);
            return null;
        }
    }
}
