using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.ContaCorrente;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.ContaCorrente;

/// <summary>
/// Serviço para API de Conta Corrente do Sicoob
/// URL: https://api.sicoob.com.br/conta-corrente/v4
/// </summary>
public class ContaCorrenteService : SicoobServiceBase, IContaCorrenteService
{
    private readonly string _baseEndpoint;

    public ContaCorrenteService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<ContaCorrenteService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.ContaCorrente;
    }

    public async Task<SaldoResponse?> ConsultarSaldoAsync(
        string numeroConta,
        CancellationToken cancellationToken = default)
    {
        // API v4: GET /conta-corrente/v4/saldo?numeroContaCorrente={string}
        var url = $"{_baseEndpoint}/saldo?numeroContaCorrente={numeroConta}";
        return await GetAsync<SaldoResponse>(url, cancellationToken);
    }

    public async Task<ExtratoResponse?> ConsultarExtratoAsync(
        string numeroConta,
        int mes,
        int ano,
        int diaInicial,
        int diaFinal,
        bool agruparCNAB = true,
        CancellationToken cancellationToken = default)
    {
        // API v4: GET /conta-corrente/v4/extrato/{mes}/{ano}?diaInicial={dd}&diaFinal={dd}&agruparCNAB={bool}&numeroContaCorrente={string}
        var url = $"{_baseEndpoint}/extrato/{mes:D2}/{ano}" +
                  $"?diaInicial={diaInicial:D2}" +
                  $"&diaFinal={diaFinal:D2}" +
                  $"&agruparCNAB={agruparCNAB.ToString().ToLower()}" +
                  $"&numeroContaCorrente={numeroConta}";
        return await GetAsync<ExtratoResponse>(url, cancellationToken);
    }

    public async Task<TransferenciaResponse?> RealizarTransferenciaAsync(
        TransferenciaRequest dadosTransferencia,
        CancellationToken cancellationToken = default)
    {
        // API v4: POST /conta-corrente/v4/transferencias
        var url = $"{_baseEndpoint}/transferencias";
        return await PostAsync<TransferenciaRequest, TransferenciaResponse>(url, dadosTransferencia, cancellationToken);
    }
}

