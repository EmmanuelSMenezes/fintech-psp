using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.ContaCorrente;

/// <summary>
/// Resposta da consulta de saldo
/// </summary>
public class SaldoResponse
{
    [JsonPropertyName("contaCorrente")]
    public string? ContaCorrente { get; set; }

    [JsonPropertyName("saldoAtual")]
    public decimal SaldoAtual { get; set; }

    [JsonPropertyName("saldoDisponivel")]
    public decimal SaldoDisponivel { get; set; }

    [JsonPropertyName("saldoBloqueado")]
    public decimal SaldoBloqueado { get; set; }

    [JsonPropertyName("dataHoraConsulta")]
    public DateTime DataHoraConsulta { get; set; }

    [JsonPropertyName("moeda")]
    public string Moeda { get; set; } = "BRL";
}
