using SicoobIntegration.API.Models.Pix;

namespace SicoobIntegration.API.Services.Pix;

/// <summary>
/// Interface para serviço de PIX Recebimentos
/// </summary>
public interface IPixRecebimentosService
{
    /// <summary>
    /// Cria uma cobrança PIX imediata
    /// </summary>
    Task<CobrancaResponse?> CriarCobrancaImediataAsync(CobrancaImediataRequest dadosCobranca, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta uma cobrança PIX pelo TxID
    /// </summary>
    Task<CobrancaResponse?> ConsultarCobrancaAsync(string txid, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista cobranças PIX
    /// </summary>
    Task<ListaCobrancasResponse?> ListarCobrancasAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma cobrança com vencimento
    /// </summary>
    Task<CobrancaResponse?> CriarCobrancaComVencimentoAsync(CobrancaImediataRequest dadosCobranca, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta PIX recebidos
    /// </summary>
    Task<object?> ConsultarPixRecebidosAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}

