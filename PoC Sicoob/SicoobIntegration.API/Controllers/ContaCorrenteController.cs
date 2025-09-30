using Microsoft.AspNetCore.Mvc;
using SicoobIntegration.API.Models.ContaCorrente;
using SicoobIntegration.API.Services.ContaCorrente;

namespace SicoobIntegration.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContaCorrenteController : ControllerBase
{
    private readonly IContaCorrenteService _service;
    private readonly ILogger<ContaCorrenteController> _logger;

    public ContaCorrenteController(
        IContaCorrenteService service,
        ILogger<ContaCorrenteController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Consulta o saldo de uma conta corrente
    /// GET /conta-corrente/v4/saldo?numeroContaCorrente={string}
    /// </summary>
    /// <param name="numeroConta">Número da conta corrente</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do saldo da conta</returns>
    [HttpGet("saldo")]
    [ProducesResponseType(typeof(SaldoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SaldoResponse>> ConsultarSaldo(
        [FromQuery] string numeroConta,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ConsultarSaldoAsync(numeroConta, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Saldo não encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar saldo da conta {NumeroConta}", numeroConta);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta o extrato de uma conta corrente
    /// GET /conta-corrente/v4/extrato/{mes}/{ano}?diaInicial={dd}&diaFinal={dd}&agruparCNAB={bool}&numeroContaCorrente={string}
    /// </summary>
    /// <param name="mes">Mês do extrato (1-12)</param>
    /// <param name="ano">Ano do extrato (yyyy)</param>
    /// <param name="diaInicial">Dia inicial (1-31)</param>
    /// <param name="diaFinal">Dia final (1-31)</param>
    /// <param name="numeroContaCorrente">Número da conta corrente</param>
    /// <param name="agruparCNAB">Agrupar por CNAB (padrão: true)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do extrato da conta</returns>
    [HttpGet("extrato/{mes}/{ano}")]
    [ProducesResponseType(typeof(ExtratoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExtratoResponse>> ConsultarExtrato(
        int mes,
        int ano,
        [FromQuery] int diaInicial,
        [FromQuery] int diaFinal,
        [FromQuery] string numeroContaCorrente,
        [FromQuery] bool agruparCNAB = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resultado = await _service.ConsultarExtratoAsync(
                numeroContaCorrente,
                mes,
                ano,
                diaInicial,
                diaFinal,
                agruparCNAB,
                cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Extrato não encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar extrato da conta {NumeroConta} - Mês: {Mes}/{Ano}", numeroContaCorrente, mes, ano);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Realiza uma transferência entre contas
    /// </summary>
    /// <param name="dadosTransferencia">Dados da transferência</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Resultado da transferência</returns>
    [HttpPost("transferencias")]
    [ProducesResponseType(typeof(TransferenciaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransferenciaResponse>> RealizarTransferencia(
        [FromBody] TransferenciaRequest dadosTransferencia,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.RealizarTransferenciaAsync(dadosTransferencia, cancellationToken);

            if (resultado == null)
            {
                return BadRequest(new { error = "Não foi possível realizar a transferência" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar transferência");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

