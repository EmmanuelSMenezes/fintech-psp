using FintechPSP.IntegrationService.Models.Sicoob.Pix;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Interface para serviço de pagamentos PIX do Sicoob
/// </summary>
public interface IPixPagamentosService
{
    /// <summary>
    /// Realiza um pagamento PIX
    /// </summary>
    /// <param name="dadosPagamento">Dados do pagamento PIX</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta do pagamento PIX</returns>
    Task<PixPagamentoResponse?> RealizarPagamentoPixAsync(
        PixPagamentoRequest dadosPagamento, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta um pagamento PIX pelo E2E ID
    /// </summary>
    /// <param name="e2eId">End-to-End ID do pagamento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do pagamento PIX</returns>
    Task<PixPagamentoResponse?> ConsultarPagamentoAsync(
        string e2eId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista pagamentos PIX em um período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de pagamentos PIX</returns>
    Task<ListaPixPagamentosResponse?> ListarPagamentosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default);
}
