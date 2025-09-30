using System.Text.Json.Serialization;

namespace SicoobIntegration.API.Models.ContaCorrente;

/// <summary>
/// Resposta da API de consulta de saldo (v4)
/// Baseado na documentação Postman oficial
/// </summary>
public class SaldoResponse
{
    [JsonPropertyName("resultado")]
    public SaldoResultado? Resultado { get; set; }
}

/// <summary>
/// Dados do saldo da conta
/// </summary>
public class SaldoResultado
{
    /// <summary>
    /// Saldo atual da conta
    /// </summary>
    [JsonPropertyName("saldo")]
    public decimal Saldo { get; set; }

    /// <summary>
    /// Saldo limite disponível
    /// </summary>
    [JsonPropertyName("saldoLimite")]
    public decimal SaldoLimite { get; set; }
}

