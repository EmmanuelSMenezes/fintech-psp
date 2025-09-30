using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.ContaCorrente;
using FintechPSP.IntegrationService.Services.Sicoob.Base;

namespace FintechPSP.IntegrationService.Services.Sicoob.ContaCorrente;

/// <summary>
/// Serviço para operações de conta corrente do Sicoob
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
        string contaCorrente, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando saldo da conta corrente: {ContaCorrente}", contaCorrente);

            var url = $"{_baseEndpoint}/{contaCorrente}/saldo";
            var response = await GetAsync<SaldoResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Saldo consultado com sucesso. Conta: {ContaCorrente}, Saldo Disponível: {SaldoDisponivel}", 
                    contaCorrente, response.SaldoDisponivel);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar saldo da conta corrente: {ContaCorrente}", contaCorrente);
            throw;
        }
    }

    public async Task<ExtratoResponse?> ConsultarExtratoAsync(
        string contaCorrente,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando extrato da conta corrente: {ContaCorrente} de {DataInicio} até {DataFim}", 
                contaCorrente, dataInicio, dataFim);

            var inicioFormatado = dataInicio.ToString("yyyy-MM-dd");
            var fimFormatado = dataFim.ToString("yyyy-MM-dd");
            
            var url = $"{_baseEndpoint}/{contaCorrente}/extrato?dataInicio={inicioFormatado}&dataFim={fimFormatado}";
            var response = await GetAsync<ExtratoResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Extrato consultado com sucesso. Conta: {ContaCorrente}, Lançamentos: {TotalLancamentos}", 
                    contaCorrente, response.Lancamentos.Count);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar extrato da conta corrente: {ContaCorrente}", contaCorrente);
            throw;
        }
    }
}
