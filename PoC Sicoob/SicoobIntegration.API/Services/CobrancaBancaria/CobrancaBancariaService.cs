using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.CobrancaBancaria;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.CobrancaBancaria;

/// <summary>
/// Serviço para API de Cobrança Bancária do Sicoob
/// URL: https://api.sicoob.com.br/cobranca-bancaria/v3
/// </summary>
public class CobrancaBancariaService : SicoobServiceBase, ICobrancaBancariaService
{
    private readonly string _baseEndpoint;

    public CobrancaBancariaService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<CobrancaBancariaService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.CobrancaBancaria;
    }

    public async Task<BoletoResponse?> ConsultarBoletoAsync(
        string numeroBoleto,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/boletos/{numeroBoleto}";
        return await GetAsync<BoletoResponse>(url, cancellationToken);
    }

    public async Task<BoletoResponse?> IncluirBoletoAsync(
        BoletoRequest dadosBoleto,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/boletos";
        return await PostAsync<BoletoRequest, BoletoResponse>(url, dadosBoleto, cancellationToken);
    }

    public async Task<BoletoResponse?> AlterarBoletoAsync(
        string numeroBoleto,
        BoletoAlteracaoRequest dadosAlteracao,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/boletos/{numeroBoleto}";
        return await PatchAsync<BoletoAlteracaoRequest, BoletoResponse>(url, dadosAlteracao, cancellationToken);
    }

    public async Task<ListaBoletosResponse?> ListarBoletosAsync(
        DateTime? dataInicio = null,
        DateTime? dataFim = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/boletos";

        if (dataInicio.HasValue || dataFim.HasValue)
        {
            var queryParams = new List<string>();
            if (dataInicio.HasValue)
                queryParams.Add($"dataInicio={dataInicio.Value:yyyy-MM-dd}");
            if (dataFim.HasValue)
                queryParams.Add($"dataFim={dataFim.Value:yyyy-MM-dd}");

            url += "?" + string.Join("&", queryParams);
        }

        return await GetAsync<ListaBoletosResponse>(url, cancellationToken);
    }
}

