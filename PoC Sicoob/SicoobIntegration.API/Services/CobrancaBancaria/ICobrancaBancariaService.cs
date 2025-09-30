using SicoobIntegration.API.Models.CobrancaBancaria;

namespace SicoobIntegration.API.Services.CobrancaBancaria;

/// <summary>
/// Interface para serviço de Cobrança Bancária (Boletos)
/// </summary>
public interface ICobrancaBancariaService
{
    /// <summary>
    /// Consulta um boleto pelo número
    /// </summary>
    Task<BoletoResponse?> ConsultarBoletoAsync(string numeroBoleto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inclui um novo boleto
    /// </summary>
    Task<BoletoResponse?> IncluirBoletoAsync(BoletoRequest dadosBoleto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Altera um boleto existente
    /// </summary>
    Task<BoletoResponse?> AlterarBoletoAsync(string numeroBoleto, BoletoAlteracaoRequest dadosAlteracao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista boletos com filtros
    /// </summary>
    Task<ListaBoletosResponse?> ListarBoletosAsync(DateTime? dataInicio = null, DateTime? dataFim = null, CancellationToken cancellationToken = default);
}

