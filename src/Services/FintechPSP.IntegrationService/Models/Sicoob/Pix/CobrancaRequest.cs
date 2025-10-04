using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.Pix;

/// <summary>
/// Request para criar cobrança PIX
/// </summary>
public class CobrancaRequest
{
    [JsonPropertyName("calendario")]
    public CalendarioCobranca Calendario { get; set; } = new();

    [JsonPropertyName("devedor")]
    public DevedorCobranca? Devedor { get; set; }

    [JsonPropertyName("valor")]
    public ValorCobranca Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    [JsonPropertyName("solicitacaoPagador")]
    public string? SolicitacaoPagador { get; set; }

    [JsonPropertyName("infoAdicionais")]
    public List<InfoAdicional>? InfoAdicionais { get; set; }
}

/// <summary>
/// Request para criar cobrança PIX imediata (formato Sicoob com chave obrigatória)
/// </summary>
public class CobrancaImediataRequest
{
    [JsonPropertyName("calendario")]
    public CalendarioCobranca Calendario { get; set; } = new();

    [JsonPropertyName("valor")]
    public ValorCobranca Valor { get; set; } = new();

    [JsonPropertyName("chave")]
    public string Chave { get; set; } = string.Empty;

    [JsonPropertyName("solicitacaoPagador")]
    public string? SolicitacaoPagador { get; set; }
}

/// <summary>
/// Calendário da cobrança
/// </summary>
public class CalendarioCobranca
{
    [JsonPropertyName("expiracao")]
    public int Expiracao { get; set; } = 3600; // 1 hora por padrão

    [JsonPropertyName("criacao")]
    public DateTime? Criacao { get; set; }

    [JsonPropertyName("dataDeVencimento")]
    public string? DataDeVencimento { get; set; } // Para cobranças com vencimento
}

/// <summary>
/// Dados do devedor
/// </summary>
public class DevedorCobranca
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
public class ValorCobranca
{
    [JsonPropertyName("original")]
    public string Original { get; set; } = string.Empty;

    // Não incluir modalidadeAlteracao por padrão para evitar erro de schema
    // [JsonPropertyName("modalidadeAlteracao")]
    // public int? ModalidadeAlteracao { get; set; }
}

/// <summary>
/// Informações adicionais
/// </summary>
public class InfoAdicional
{
    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;
}

/// <summary>
/// Location para cobrança
/// </summary>
public class LocationRequest
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
}
