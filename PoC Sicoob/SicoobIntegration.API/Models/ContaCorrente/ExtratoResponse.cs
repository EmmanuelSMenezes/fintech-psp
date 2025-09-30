using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.ContaCorrente;

/// <summary>
/// Resposta da API de consulta de extrato (v4)
/// Baseado na documentação Postman oficial
/// </summary>
public class ExtratoResponse
{
    /// <summary>
    /// Saldo atual da conta
    /// </summary>
    [JsonPropertyName("saldoAtual")]
    public string SaldoAtual { get; set; } = string.Empty;

    /// <summary>
    /// Saldo bloqueado
    /// </summary>
    [JsonPropertyName("saldoBloqueado")]
    public string SaldoBloqueado { get; set; } = string.Empty;

    /// <summary>
    /// Saldo limite disponível
    /// </summary>
    [JsonPropertyName("saldoLimite")]
    public string SaldoLimite { get; set; } = string.Empty;

    /// <summary>
    /// Saldo anterior
    /// </summary>
    [JsonPropertyName("saldoAnterior")]
    public string SaldoAnterior { get; set; } = string.Empty;

    /// <summary>
    /// Saldo de bloqueio judicial
    /// </summary>
    [JsonPropertyName("saldoBloqueioJudicial")]
    public string SaldoBloqueioJudicial { get; set; } = string.Empty;

    /// <summary>
    /// Saldo de bloqueio judicial anterior
    /// </summary>
    [JsonPropertyName("saldoBloqueioJudicialAnterior")]
    public string SaldoBloqueioJudicialAnterior { get; set; } = string.Empty;

    /// <summary>
    /// Lista de transações do extrato
    /// </summary>
    [JsonPropertyName("transacoes")]
    public List<Transacao> Transacoes { get; set; } = new();
}

/// <summary>
/// Transação no extrato
/// </summary>
public class Transacao
{
    /// <summary>
    /// Tipo da transação
    /// </summary>
    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Valor da transação
    /// </summary>
    [JsonPropertyName("valor")]
    public string Valor { get; set; } = string.Empty;

    /// <summary>
    /// Data da transação
    /// </summary>
    [JsonPropertyName("data")]
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Data do lote
    /// </summary>
    [JsonPropertyName("dataLote")]
    public string DataLote { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da transação
    /// </summary>
    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Número do documento
    /// </summary>
    [JsonPropertyName("numeroDocumento")]
    public string NumeroDocumento { get; set; } = string.Empty;

    /// <summary>
    /// CPF/CNPJ relacionado à transação
    /// </summary>
    [JsonPropertyName("cpfCnpj")]
    public string CpfCnpj { get; set; } = string.Empty;

    /// <summary>
    /// Descrição de informação complementar
    /// </summary>
    [JsonPropertyName("descInfComplementar")]
    public string DescInfComplementar { get; set; } = string.Empty;
}

