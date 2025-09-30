using FintechPSP.IntegrationService.Models.Sicoob.Pix;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Interface para serviço de recebimentos PIX (cobranças) do Sicoob
/// </summary>
public interface IPixRecebimentosService
{
    /// <summary>
    /// Cria uma cobrança PIX imediata
    /// </summary>
    /// <param name="dadosCobranca">Dados da cobrança</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta da cobrança criada</returns>
    Task<CobrancaResponse?> CriarCobrancaImediataAsync(
        CobrancaRequest dadosCobranca, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma cobrança PIX com vencimento
    /// </summary>
    /// <param name="dadosCobranca">Dados da cobrança com vencimento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta da cobrança criada</returns>
    Task<CobrancaResponse?> CriarCobrancaComVencimentoAsync(
        CobrancaRequest dadosCobranca, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta uma cobrança PIX pelo TxId
    /// </summary>
    /// <param name="txId">Transaction ID da cobrança</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da cobrança</returns>
    Task<CobrancaResponse?> ConsultarCobrancaAsync(
        string txId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista cobranças PIX em um período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de cobranças</returns>
    Task<object?> ListarCobrancasAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta PIX recebidos em um período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de PIX recebidos</returns>
    Task<object?> ConsultarPixRecebidosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default);
}
