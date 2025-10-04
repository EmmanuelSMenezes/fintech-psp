using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Models.Sicoob.SPB;

/// <summary>
/// Resposta da TED
/// </summary>
public class TEDResponse
{
    [JsonPropertyName("idTransferencia")]
    public string IdTransferencia { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrenteDebito")]
    public string NumeroContaCorrenteDebito { get; set; } = string.Empty;

    [JsonPropertyName("valor")]
    public decimal Valor { get; set; }

    [JsonPropertyName("finalidade")]
    public int Finalidade { get; set; }

    [JsonPropertyName("dataTransferencia")]
    public string DataTransferencia { get; set; } = string.Empty;

    [JsonPropertyName("situacao")]
    public string Situacao { get; set; } = string.Empty;

    [JsonPropertyName("favorecido")]
    public FavorecidoTED Favorecido { get; set; } = new();

    [JsonPropertyName("descricao")]
    public string? Descricao { get; set; }

    [JsonPropertyName("numeroDocumento")]
    public string? NumeroDocumento { get; set; }
}

/// <summary>
/// Dados do favorecido da TED
/// </summary>
public class FavorecidoTED
{
    [JsonPropertyName("numeroCpfCnpj")]
    public string NumeroCpfCnpj { get; set; } = string.Empty;

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("codigoBanco")]
    public string CodigoBanco { get; set; } = string.Empty;

    [JsonPropertyName("numeroAgencia")]
    public string NumeroAgencia { get; set; } = string.Empty;

    [JsonPropertyName("numeroContaCorrente")]
    public string NumeroContaCorrente { get; set; } = string.Empty;

    [JsonPropertyName("tipoConta")]
    public string TipoConta { get; set; } = string.Empty;
}

/// <summary>
/// Dados da conta na resposta da TED
/// </summary>
public class ContaTEDResponse
{
    [JsonPropertyName("banco")]
    public string? Banco { get; set; }

    [JsonPropertyName("agencia")]
    public string? Agencia { get; set; }

    [JsonPropertyName("conta")]
    public string? Conta { get; set; }

    [JsonPropertyName("digito")]
    public string? Digito { get; set; }

    [JsonPropertyName("tipoConta")]
    public string? TipoConta { get; set; }

    [JsonPropertyName("titular")]
    public TitularContaResponse? Titular { get; set; }
}

/// <summary>
/// Dados do titular na resposta
/// </summary>
public class TitularContaResponse
{
    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("documento")]
    public string? Documento { get; set; }

    [JsonPropertyName("tipoDocumento")]
    public string? TipoDocumento { get; set; } // "CPF" ou "CNPJ"
}
