using Microsoft.Extensions.Options;
using SicoobIntegration.API.Models;
using SicoobIntegration.API.Models.Pix;
using SicoobIntegration.API.Services.Base;

namespace SicoobIntegration.API.Services.Pix;

/// <summary>
/// Serviço para API de PIX Recebimentos do Sicoob
/// URL: https://api.sicoob.com.br/pix/api/v2
/// </summary>
public class PixRecebimentosService : SicoobServiceBase, IPixRecebimentosService
{
    private readonly string _baseEndpoint;

    public PixRecebimentosService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<PixRecebimentosService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.PixRecebimentos;
    }

    public async Task<CobrancaResponse?> CriarCobrancaImediataAsync(
        CobrancaImediataRequest dadosCobranca,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/cob";
        return await PostAsync<CobrancaImediataRequest, CobrancaResponse>(url, dadosCobranca, cancellationToken);
    }

    public async Task<CobrancaResponse?> ConsultarCobrancaAsync(
        string txid,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/cob/{txid}";
        return await GetAsync<CobrancaResponse>(url, cancellationToken);
    }

    public async Task<ListaCobrancasResponse?> ListarCobrancasAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/cob" +
                  $"?inicio={dataInicio:yyyy-MM-ddTHH:mm:ssZ}" +
                  $"&fim={dataFim:yyyy-MM-ddTHH:mm:ssZ}";
        return await GetAsync<ListaCobrancasResponse>(url, cancellationToken);
    }

    public async Task<CobrancaResponse?> CriarCobrancaComVencimentoAsync(
        CobrancaImediataRequest dadosCobranca,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/cobv";
        return await PostAsync<CobrancaImediataRequest, CobrancaResponse>(url, dadosCobranca, cancellationToken);
    }

    public async Task<object?> CriarCobrancaComVencimentoAsync(
        object dadosCobranca,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/cobv";
        return await PostAsync<object, object>(url, dadosCobranca, cancellationToken);
    }

    public async Task<object?> ConsultarPixRecebidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        var url = $"{_baseEndpoint}/pix" +
                  $"?inicio={dataInicio:yyyy-MM-ddTHH:mm:ssZ}" +
                  $"&fim={dataFim:yyyy-MM-ddTHH:mm:ssZ}";
        return await GetAsync<object>(url, cancellationToken);
    }
}

