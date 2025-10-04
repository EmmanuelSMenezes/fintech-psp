using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using FintechPSP.IntegrationService.Services.Sicoob.Base;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Serviço para pagamentos PIX do Sicoob
/// </summary>
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

    public async Task<PixPagamentoResponse?> RealizarPagamentoPixAsync(
        PixPagamentoRequest dadosPagamento, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Realizando pagamento PIX para chave: {Chave}, valor: {Valor}", 
                dadosPagamento.Favorecido.Chave, dadosPagamento.Valor);

            var url = $"{_baseEndpoint}/pix";
            var response = await PostAsync<PixPagamentoRequest, PixPagamentoResponse>(url, dadosPagamento, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Pagamento PIX realizado com sucesso. EndToEndId: {EndToEndId}, Status: {Status}",
                    response.EndToEndId, response.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao realizar pagamento PIX para chave: {Chave}", dadosPagamento.Favorecido.Chave);
            throw;
        }
    }

    public async Task<PixPagamentoResponse?> ConsultarPagamentoAsync(
        string e2eId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando pagamento PIX com E2E ID: {E2eId}", e2eId);

            var url = $"{_baseEndpoint}/pix/{e2eId}";
            var response = await GetAsync<PixPagamentoResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Pagamento PIX consultado. Status: {Status}, Valor: {Valor}", 
                    response.Status, response.Valor);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar pagamento PIX com E2E ID: {E2eId}", e2eId);
            throw;
        }
    }

    public async Task<ListaPixPagamentosResponse?> ListarPagamentosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Listando pagamentos PIX de {DataInicio} até {DataFim}", dataInicio, dataFim);

            var inicioFormatado = dataInicio.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var fimFormatado = dataFim.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            
            var url = $"{_baseEndpoint}/pix?inicio={inicioFormatado}&fim={fimFormatado}";
            var response = await GetAsync<ListaPixPagamentosResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Listagem de pagamentos PIX concluída. Total: {Total} pagamentos", 
                    response.Pagamentos.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao listar pagamentos PIX de {DataInicio} até {DataFim}", dataInicio, dataFim);
            throw;
        }
    }
}
