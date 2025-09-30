using Microsoft.AspNetCore.Mvc;
using SicoobIntegration.API.Models.CobrancaBancaria;
using SicoobIntegration.API.Services.CobrancaBancaria;

namespace SicoobIntegration.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CobrancaBancariaController : ControllerBase
{
    private readonly ICobrancaBancariaService _service;
    private readonly ILogger<CobrancaBancariaController> _logger;

    public CobrancaBancariaController(
        ICobrancaBancariaService service,
        ILogger<CobrancaBancariaController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Consulta um boleto pelo número
    /// </summary>
    [HttpGet("boletos/{numeroBoleto}")]
    [ProducesResponseType(typeof(BoletoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoletoResponse>> ConsultarBoleto(
        string numeroBoleto,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ConsultarBoletoAsync(numeroBoleto, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Boleto não encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar boleto {NumeroBoleto}", numeroBoleto);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Inclui um novo boleto
    /// </summary>
    [HttpPost("boletos")]
    [ProducesResponseType(typeof(BoletoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoletoResponse>> IncluirBoleto(
        [FromBody] BoletoRequest dadosBoleto,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.IncluirBoletoAsync(dadosBoleto, cancellationToken);

            if (resultado == null)
            {
                return BadRequest(new { error = "Não foi possível incluir o boleto" });
            }

            return CreatedAtAction(nameof(ConsultarBoleto), new { numeroBoleto = resultado.NossoNumero }, resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao incluir boleto");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Altera um boleto existente
    /// </summary>
    [HttpPatch("boletos/{numeroBoleto}")]
    [ProducesResponseType(typeof(BoletoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BoletoResponse>> AlterarBoleto(
        string numeroBoleto,
        [FromBody] BoletoAlteracaoRequest dadosAlteracao,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.AlterarBoletoAsync(numeroBoleto, dadosAlteracao, cancellationToken);

            if (resultado == null)
            {
                return BadRequest(new { error = "Não foi possível alterar o boleto" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar boleto {NumeroBoleto}", numeroBoleto);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista boletos com filtros
    /// </summary>
    [HttpGet("boletos")]
    [ProducesResponseType(typeof(ListaBoletosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListaBoletosResponse>> ListarBoletos(
        [FromQuery] DateTime? dataInicio,
        [FromQuery] DateTime? dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _service.ListarBoletosAsync(dataInicio, dataFim, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Nenhum boleto encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar boletos");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

