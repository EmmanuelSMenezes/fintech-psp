using FintechPSP.IntegrationService.Models.Sicoob.CobrancaBancaria;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Services.Sicoob.Base;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace FintechPSP.IntegrationService.Services.Sicoob.CobrancaBancaria;

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

    public async Task<BoletoResponse?> ConsultarBoletoAsync(string nossoNumero, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando boleto: {NossoNumero}", nossoNumero);

            var url = $"{_baseEndpoint}/boletos/{nossoNumero}";
            var response = await GetAsync<BoletoResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Boleto consultado com sucesso. NossoNumero: {NossoNumero}, Situacao: {Situacao}, Valor: {Valor}", 
                    response.NossoNumero, response.Situacao, response.Valor);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar boleto: {NossoNumero}", nossoNumero);
            throw;
        }
    }

    public async Task<BoletoResponse?> IncluirBoletoAsync(BoletoRequest dadosBoleto, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Incluindo boleto para pagador: {Nome}, valor: {Valor}",
                dadosBoleto.Pagador.Nome, dadosBoleto.Valor);

            // Log do JSON que será enviado
            var json = System.Text.Json.JsonSerializer.Serialize(dadosBoleto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            Logger.LogInformation("JSON do boleto que será enviado: {Json}", json);

            var url = $"{_baseEndpoint}/boletos";
            var response = await PostAsync<BoletoRequest, BoletoResponse>(url, dadosBoleto, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Boleto incluído com sucesso. NossoNumero: {NossoNumero}, CodigoBarras: {CodigoBarras}", 
                    response.NossoNumero, 
                    string.IsNullOrEmpty(response.CodigoBarras) ? "N/A" : "PRESENTE");
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao incluir boleto para pagador: {Nome}", dadosBoleto.Pagador.Nome);
            throw;
        }
    }

    public async Task<BoletoResponse?> AlterarBoletoAsync(string nossoNumero, BoletoAlteracaoRequest dadosAlteracao, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Alterando boleto: {NossoNumero}", nossoNumero);

            var url = $"{_baseEndpoint}/boletos/{nossoNumero}";
            var response = await PatchAsync<BoletoAlteracaoRequest, BoletoResponse>(url, dadosAlteracao, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Boleto alterado com sucesso. NossoNumero: {NossoNumero}", response.NossoNumero);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao alterar boleto: {NossoNumero}", nossoNumero);
            throw;
        }
    }

    public async Task<List<BoletoResponse>?> ListarBoletosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        string? situacao = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Listando boletos de {DataInicio} até {DataFim}, situação: {Situacao}", 
                dataInicio, dataFim, situacao ?? "TODAS");

            var queryParams = new List<string>
            {
                $"dataInicio={dataInicio:yyyy-MM-dd}",
                $"dataFim={dataFim:yyyy-MM-dd}"
            };

            if (!string.IsNullOrEmpty(situacao))
            {
                queryParams.Add($"situacao={situacao}");
            }

            var url = $"{_baseEndpoint}/boletos?{string.Join("&", queryParams)}";
            var response = await GetAsync<List<BoletoResponse>>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Listagem de boletos concluída. Total: {Total}", response.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao listar boletos de {DataInicio} até {DataFim}", dataInicio, dataFim);
            throw;
        }
    }

    public async Task<bool> BaixarBoletoAsync(string nossoNumero, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Baixando boleto: {NossoNumero}", nossoNumero);

            var url = $"{_baseEndpoint}/boletos/{nossoNumero}/baixa";
            var response = await PostAsync<object, object>(url, new { }, cancellationToken);

            Logger.LogInformation("Boleto baixado com sucesso: {NossoNumero}", nossoNumero);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao baixar boleto: {NossoNumero}", nossoNumero);
            return false;
        }
    }
}
