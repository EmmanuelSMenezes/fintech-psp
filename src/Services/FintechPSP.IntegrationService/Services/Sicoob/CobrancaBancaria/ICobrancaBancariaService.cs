using FintechPSP.IntegrationService.Models.Sicoob.CobrancaBancaria;

namespace FintechPSP.IntegrationService.Services.Sicoob.CobrancaBancaria;

/// <summary>
/// Interface para serviço de Cobrança Bancária (Boletos) do Sicoob
/// </summary>
public interface ICobrancaBancariaService
{
    /// <summary>
    /// Consulta um boleto pelo nosso número
    /// </summary>
    /// <param name="nossoNumero">Nosso número do boleto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do boleto</returns>
    Task<BoletoResponse?> ConsultarBoletoAsync(string nossoNumero, CancellationToken cancellationToken = default);

    /// <summary>
    /// Inclui um novo boleto
    /// </summary>
    /// <param name="dadosBoleto">Dados do boleto a ser criado</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do boleto criado</returns>
    Task<BoletoResponse?> IncluirBoletoAsync(BoletoRequest dadosBoleto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Altera um boleto existente
    /// </summary>
    /// <param name="nossoNumero">Nosso número do boleto</param>
    /// <param name="dadosAlteracao">Dados para alteração</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do boleto alterado</returns>
    Task<BoletoResponse?> AlterarBoletoAsync(string nossoNumero, BoletoAlteracaoRequest dadosAlteracao, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lista boletos por período
    /// </summary>
    /// <param name="dataInicio">Data de início</param>
    /// <param name="dataFim">Data de fim</param>
    /// <param name="situacao">Situação do boleto (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de boletos</returns>
    Task<List<BoletoResponse>?> ListarBoletosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        string? situacao = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Baixa um boleto (cancelamento)
    /// </summary>
    /// <param name="nossoNumero">Nosso número do boleto</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da operação</returns>
    Task<bool> BaixarBoletoAsync(string nossoNumero, CancellationToken cancellationToken = default);
}
