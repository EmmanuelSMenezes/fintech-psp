using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.Pagamentos;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.Pagamentos;

public class PagamentosService : SicoobServiceBase, IPagamentosService
{
    private readonly string _baseEndpoint;

    public PagamentosService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<PagamentosService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.Pagamentos;
    }

    public async Task<PagamentoResponse?> IncluirPagamentoBoletoAsync(PagamentoBoletoRequest dadosPagamento, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pagamentos";
        return await PostAsync<PagamentoBoletoRequest, PagamentoResponse>(url, dadosPagamento, cancellationToken);
    }

    public async Task<PagamentoResponse?> ConsultarPagamentoAsync(string idPagamento, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pagamentos/{idPagamento}";
        return await GetAsync<PagamentoResponse>(url, cancellationToken);
    }

    public async Task<PagamentoResponse?> AlterarPagamentoAsync(string idPagamento, PagamentoAlteracaoRequest dadosAlteracao, CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pagamentos/{idPagamento}";
        return await PatchAsync<PagamentoAlteracaoRequest, PagamentoResponse>(url, dadosAlteracao, cancellationToken);
    }
}

