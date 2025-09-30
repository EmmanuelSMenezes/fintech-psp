using Microsoft.AspNetCore.Mvc;
using SicoobIntegration.API.Models.Pix;
using SicoobIntegration.API.Services.Pix;

namespace SicoobIntegration.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PixController : ControllerBase
{
    private readonly IPixRecebimentosService _recebimentosService;
    private readonly IPixPagamentosService _pagamentosService;
    private readonly ILogger<PixController> _logger;

    public PixController(
        IPixRecebimentosService recebimentosService,
        IPixPagamentosService pagamentosService,
        ILogger<PixController> _logger)
    {
        _recebimentosService = recebimentosService;
        _pagamentosService = pagamentosService;
        this._logger = _logger;
    }

    // PIX Recebimentos

    /// <summary>
    /// Cria uma cobrança PIX imediata
    /// </summary>
    [HttpPost("recebimentos/cobranca-imediata")]
    [ProducesResponseType(typeof(CobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CobrancaResponse>> CriarCobrancaImediata(
        [FromBody] CobrancaImediataRequest dadosCobranca,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _recebimentosService.CriarCobrancaImediataAsync(dadosCobranca, cancellationToken);

            if (resultado == null)
            {
                return BadRequest(new { error = "Não foi possível criar a cobrança" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cobrança imediata");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta uma cobrança PIX pelo TxID
    /// </summary>
    [HttpGet("recebimentos/cobranca/{txid}")]
    [ProducesResponseType(typeof(CobrancaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CobrancaResponse>> ConsultarCobranca(
        string txid,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _recebimentosService.ConsultarCobrancaAsync(txid, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Cobrança não encontrada" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar cobrança {TxId}", txid);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista cobranças PIX em um período
    /// </summary>
    [HttpGet("recebimentos/cobrancas")]
    [ProducesResponseType(typeof(ListaCobrancasResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListaCobrancasResponse>> ListarCobrancas(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _recebimentosService.ListarCobrancasAsync(dataInicio, dataFim, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Nenhuma cobrança encontrada" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar cobranças");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // PIX Pagamentos

    /// <summary>
    /// Realiza um pagamento PIX
    /// </summary>
    [HttpPost("pagamentos")]
    [ProducesResponseType(typeof(PixPagamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PixPagamentoResponse>> RealizarPagamento(
        [FromBody] PixPagamentoRequest dadosPagamento,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _pagamentosService.RealizarPagamentoPixAsync(dadosPagamento, cancellationToken);

            if (resultado == null)
            {
                return BadRequest(new { error = "Não foi possível realizar o pagamento" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar pagamento PIX");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta um pagamento PIX pelo EndToEndId
    /// </summary>
    [HttpGet("pagamentos/{e2eId}")]
    [ProducesResponseType(typeof(PixPagamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PixPagamentoResponse>> ConsultarPagamento(
        string e2eId,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _pagamentosService.ConsultarPagamentoAsync(e2eId, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Pagamento não encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pagamento {E2eId}", e2eId);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Lista pagamentos PIX em um período
    /// </summary>
    [HttpGet("pagamentos")]
    [ProducesResponseType(typeof(ListaPixPagamentosResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ListaPixPagamentosResponse>> ListarPagamentos(
        [FromQuery] DateTime dataInicio,
        [FromQuery] DateTime dataFim,
        CancellationToken cancellationToken)
    {
        try
        {
            var resultado = await _pagamentosService.ListarPagamentosAsync(dataInicio, dataFim, cancellationToken);

            if (resultado == null)
            {
                return NotFound(new { error = "Nenhum pagamento encontrado" });
            }

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar pagamentos");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

