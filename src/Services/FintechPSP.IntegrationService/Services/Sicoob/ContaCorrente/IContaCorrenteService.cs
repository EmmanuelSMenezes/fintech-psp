using FintechPSP.IntegrationService.Models.Sicoob.ContaCorrente;

namespace FintechPSP.IntegrationService.Services.Sicoob.ContaCorrente;

/// <summary>
/// Interface para serviço de conta corrente do Sicoob
/// </summary>
public interface IContaCorrenteService
{
    /// <summary>
    /// Consulta o saldo de uma conta corrente
    /// </summary>
    /// <param name="contaCorrente">Número da conta corrente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do saldo da conta</returns>
    Task<SaldoResponse?> ConsultarSaldoAsync(
        string contaCorrente, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o extrato de uma conta corrente
    /// </summary>
    /// <param name="contaCorrente">Número da conta corrente</param>
    /// <param name="dataInicio">Data de início do período</param>
    /// <param name="dataFim">Data de fim do período</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Extrato da conta</returns>
    Task<ExtratoResponse?> ConsultarExtratoAsync(
        string contaCorrente,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);
}
