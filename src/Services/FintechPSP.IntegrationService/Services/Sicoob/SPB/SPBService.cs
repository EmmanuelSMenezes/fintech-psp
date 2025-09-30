using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.SPB;
using FintechPSP.IntegrationService.Services.Sicoob.Base;

namespace FintechPSP.IntegrationService.Services.Sicoob.SPB;

/// <summary>
/// Serviço para operações SPB (TED) do Sicoob
/// </summary>
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

    public async Task<TEDResponse?> RealizarTEDAsync(
        TEDRequest dadosTED, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Realizando TED para conta: {Banco}-{Agencia}-{Conta}, valor: {Valor}", 
                dadosTED.ContaDestino.Banco, dadosTED.ContaDestino.Agencia, 
                dadosTED.ContaDestino.Conta, dadosTED.Valor);

            var url = $"{_baseEndpoint}/ted";
            var response = await PostAsync<TEDRequest, TEDResponse>(url, dadosTED, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("TED realizada com sucesso. Documento: {NumeroDocumento}, Status: {Status}", 
                    response.NumeroDocumento, response.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao realizar TED para conta: {Banco}-{Agencia}-{Conta}", 
                dadosTED.ContaDestino.Banco, dadosTED.ContaDestino.Agencia, dadosTED.ContaDestino.Conta);
            throw;
        }
    }

    public async Task<TEDResponse?> ConsultarTEDAsync(
        string numeroDocumento, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando TED com documento: {NumeroDocumento}", numeroDocumento);

            var url = $"{_baseEndpoint}/ted/{numeroDocumento}";
            var response = await GetAsync<TEDResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("TED consultada. Status: {Status}, Valor: {Valor}", 
                    response.Status, response.Valor);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar TED com documento: {NumeroDocumento}", numeroDocumento);
            throw;
        }
    }

    public async Task<object?> ListarTEDsAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Listando TEDs de {DataInicio} até {DataFim}", dataInicio, dataFim);

            var inicioFormatado = dataInicio.ToString("yyyy-MM-dd");
            var fimFormatado = dataFim.ToString("yyyy-MM-dd");
            
            var url = $"{_baseEndpoint}/ted?dataInicio={inicioFormatado}&dataFim={fimFormatado}";
            var response = await GetAsync<object>(url, cancellationToken);

            Logger.LogInformation("Listagem de TEDs concluída");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao listar TEDs de {DataInicio} até {DataFim}", dataInicio, dataFim);
            throw;
        }
    }
}
