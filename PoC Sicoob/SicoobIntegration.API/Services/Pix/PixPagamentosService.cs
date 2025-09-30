using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.Pix;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.Pix;

public class PixPagamentosService : SicoobServiceBase, IPixPagamentosService
{
    private readonly string _baseEndpoint;

    public PixPagamentosService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<PixPagamentosService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.PixPagamentos;
    }

    public async Task<PixPagamentoResponse?> RealizarPagamentoPixAsync(PixPagamentoRequest dadosPagamento, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pix";
        return await PostAsync<PixPagamentoRequest, PixPagamentoResponse>(url, dadosPagamento, cancellationToken);
    }

    public async Task<PixPagamentoResponse?> ConsultarPagamentoAsync(string e2eId, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pix/{e2eId}";
        return await GetAsync<PixPagamentoResponse>(url, cancellationToken);
    }

    public async Task<ListaPixPagamentosResponse?> ListarPagamentosAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pix?inicio={dataInicio:yyyy-MM-ddTHH:mm:ssZ}&fim={dataFim:yyyy-MM-ddTHH:mm:ssZ}";
        return await GetAsync<ListaPixPagamentosResponse>(url, cancellationToken);
    }
}

