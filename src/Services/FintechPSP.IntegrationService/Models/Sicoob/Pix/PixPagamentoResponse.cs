using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Resposta do pagamento PIX
/// </summary>
public class PixPagamentoResponse
{
    [JsonPropertyName("e2eId")]
    public string? E2eId { get; set; }

    [JsonPropertyName("txid")]
    public string? TxId { get; set; }

    [JsonPropertyName("valor")]
    public string? Valor { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("dataHoraSolicitacao")]
    public DateTime? DataHoraSolicitacao { get; set; }

    [JsonPropertyName("dataHoraEfetivacao")]
    public DateTime? DataHoraEfetivacao { get; set; }

    [JsonPropertyName("pagador")]
    public PagadorPixResponse? Pagador { get; set; }

    [JsonPropertyName("favorecido")]
    public FavorecidoPixResponse? Favorecido { get; set; }
}

/// <summary>
/// Dados do pagador na resposta
/// </summary>
public class PagadorPixResponse
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("contaCorrente")]
    public string? ContaCorrente { get; set; }
}

/// <summary>
/// Dados do favorecido na resposta
/// </summary>
public class FavorecidoPixResponse
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("chave")]
    public string? Chave { get; set; }
}

/// <summary>
/// Lista de pagamentos PIX
/// </summary>
public class ListaPixPagamentosResponse
{
    [JsonPropertyName("pagamentos")]
    public List<PixPagamentoResponse> Pagamentos { get; set; } = new();

    [JsonPropertyName("parametros")]
    public ParametrosPaginacao? Parametros { get; set; }
}

/// <summary>
/// Parâmetros de paginação
/// </summary>
public class ParametrosPaginacao
{
    [JsonPropertyName("inicio")]
    public DateTime Inicio { get; set; }

    [JsonPropertyName("fim")]
    public DateTime Fim { get; set; }

    [JsonPropertyName("paginacao")]
    public PaginacaoInfo? Paginacao { get; set; }
}

/// <summary>
/// Informações de paginação
/// </summary>
public class PaginacaoInfo
{
    [JsonPropertyName("paginaAtual")]
    public int PaginaAtual { get; set; }

    [JsonPropertyName("itensPorPagina")]
    public int ItensPorPagina { get; set; }

    [JsonPropertyName("quantidadeDePaginas")]
    public int QuantidadeDePaginas { get; set; }

    [JsonPropertyName("quantidadeTotalDeItens")]
    public int QuantidadeTotalDeItens { get; set; }
}
