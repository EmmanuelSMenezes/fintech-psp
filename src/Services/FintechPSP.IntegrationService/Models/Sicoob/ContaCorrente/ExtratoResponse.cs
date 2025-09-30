using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.ContaCorrente;

/// <summary>
/// Resposta do extrato da conta corrente
/// </summary>
public class ExtratoResponse
{
    [JsonPropertyName("contaCorrente")]
    public string? ContaCorrente { get; set; }

    [JsonPropertyName("dataInicio")]
    public DateTime DataInicio { get; set; }

    [JsonPropertyName("dataFim")]
    public DateTime DataFim { get; set; }

    [JsonPropertyName("lancamentos")]
    public List<LancamentoExtrato> Lancamentos { get; set; } = new();

    [JsonPropertyName("saldoInicial")]
    public decimal SaldoInicial { get; set; }

    [JsonPropertyName("saldoFinal")]
    public decimal SaldoFinal { get; set; }
}

/// <summary>
/// Lançamento do extrato
/// </summary>
public class LancamentoExtrato
{
    [JsonPropertyName("data")]
    public DateTime Data { get; set; }

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("tipo")]
    public string? Tipo { get; set; } // "D" para débito, "C" para crédito

    [JsonPropertyName("saldo")]
    public decimal Saldo { get; set; }

    [JsonPropertyName("documento")]
    public string? Documento { get; set; }

    [JsonPropertyName("historico")]
    public string? Historico { get; set; }
}
