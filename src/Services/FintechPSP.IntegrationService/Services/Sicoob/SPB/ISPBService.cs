using FintechPSP.IntegrationService.Models.Sicoob.SPB;

namespace FintechPSP.IntegrationService.Services.Sicoob.SPB;

/// <summary>
/// Interface para serviço SPB (Sistema de Pagamentos Brasileiro) do Sicoob
/// </summary>
public interface ISPBService
{
    /// <summary>
    /// Realiza uma TED (Transferência Eletrônica Disponível)
    /// </summary>
    /// <param name="dadosTED">Dados da TED</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resposta da TED</returns>
    Task<TEDResponse?> RealizarTEDAsync(
        TEDRequest dadosTED, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta uma TED pelo número do documento
    /// </summary>
    /// <param name="numeroDocumento">Número do documento da TED</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados da TED</returns>
    Task<TEDResponse?> ConsultarTEDAsync(
        string numeroDocumento, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista TEDs em um período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de TEDs</returns>
    Task<object?> ListarTEDsAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default);
}
