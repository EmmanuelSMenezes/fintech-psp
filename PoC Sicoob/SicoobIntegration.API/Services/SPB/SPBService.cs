using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.SPB;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.SPB;

public class SPBService : SicoobServiceBase, ISPBService
{
    private readonly string _baseEndpoint;

    public SPBService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<SPBService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.SPB;
    }

    public async Task<TEDResponse?> RealizarTEDAsync(TEDRequest dadosTED, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/transferencias";
        return await PostAsync<TEDRequest, TEDResponse>(url, dadosTED, cancellationToken);
    }

    public async Task<TEDResponse?> ConsultarTEDAsync(string idTransferencia, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/transferencias/{idTransferencia}";
        return await GetAsync<TEDResponse>(url, cancellationToken);
    }

    public async Task<ListaTEDsResponse?> ListarTEDsAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/transferencias?dataInicio={dataInicio:yyyy-MM-dd}&dataFim={dataFim:yyyy-MM-dd}";
        return await GetAsync<ListaTEDsResponse>(url, cancellationToken);
    }
}

