using SicoobIntegration.API.Models.ContaCorrente;

namespace SicoobIntegration.API.Services.ContaCorrente;

/// <summary>
/// Interface para serviço de Conta Corrente (v4)
/// </summary>
public interface IContaCorrenteService
{
    /// <summary>
    /// Consulta o saldo da conta corrente
    /// GET /conta-corrente/v4/saldo?numeroContaCorrente={string}
    /// </summary>
    Task<SaldoResponse?> ConsultarSaldoAsync(string numeroConta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o extrato da conta corrente
    /// GET /conta-corrente/v4/extrato/{mes}/{ano}?diaInicial={dd}&diaFinal={dd}&agruparCNAB={bool}&numeroContaCorrente={string}
    /// </summary>
    Task<ExtratoResponse?> ConsultarExtratoAsync(
        string numeroConta,
        int mes,
        int ano,
        int diaInicial,
        int diaFinal,
        bool agruparCNAB = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza uma transferência entre contas
    /// POST /conta-corrente/v4/transferencias
    /// </summary>
    Task<TransferenciaResponse?> RealizarTransferenciaAsync(TransferenciaRequest dadosTransferencia, CancellationToken cancellationToken = default);
}

