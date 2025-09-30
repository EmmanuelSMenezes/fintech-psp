using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.Pix;

/// <summary>
/// Request para criar cobrança PIX imediata
/// </summary>
public class CobrancaImediataRequest
{
    [JsonPropertyName("calendario")]
    public Calendario Calendario { get; set; } = new();

    [JsonPropertyName("devedor")]
    public Devedor? Devedor { get; set; }

    [JsonPropertyName("valor")]
    public Valor Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    [JsonPropertyName("solicitacaoPagador")]
    public string? SolicitacaoPagador { get; set; }

    [JsonPropertyName("infoAdicionais")]
    public List<InfoAdicional>? InfoAdicionais { get; set; }
}

/// <summary>
/// Calendário da cobrança
/// </summary>
public class Calendario
{
    [JsonPropertyName("expiracao")]
    public int Expiracao { get; set; }

    [JsonPropertyName("criacao")]
    public string? Criacao { get; set; }
}

/// <summary>
/// Dados do devedor
/// </summary>
public class Devedor
{
    [JsonPropertyName("cpf")]
    public string? Cpf { get; set; }

    [JsonPropertyName("cnpj")]
    public string? Cnpj { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;
}

/// <summary>
/// Valor da cobrança
/// </summary>
public class Valor
{
    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    [JsonPropertyName("modalidadeAlteracao")]
    public int? ModalidadeAlteracao { get; set; }
}

/// <summary>
/// Informação adicional
/// </summary>
public class InfoAdicional
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;
}

