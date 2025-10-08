using System.Text.Json;
using System.Text.Json.Serialization;

namespace FintechPSP.IntegrationService.Services.ReceitaFederal;

/// <summary>
/// Serviço para validação de CNPJ via Receita Federal
/// </summary>
public class ReceitaFederalService : IReceitaFederalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ReceitaFederalService> _logger;

    public ReceitaFederalService(HttpClient httpClient, ILogger<ReceitaFederalService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CnpjValidationResult> ValidateCnpjAsync(string cnpj, CancellationToken cancellationToken = default)
    {
        try
        {
            // Limpar CNPJ (remover pontos, barras, etc.)
            var cnpjLimpo = LimparCnpj(cnpj);
            
            _logger.LogInformation("Validando CNPJ via Receita Federal: {Cnpj}", cnpjLimpo);

            // Validação básica de formato
            if (!IsValidCnpjFormat(cnpjLimpo))
            {
                return new CnpjValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Formato de CNPJ inválido",
                    ValidationSource = "FORMAT_CHECK"
                };
            }

            // Consulta na Receita Federal (API pública)
            var url = $"https://receitaws.com.br/v1/cnpj/{cnpjLimpo}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erro na consulta Receita Federal - Status: {StatusCode}", response.StatusCode);
                
                // Fallback para validação de formato apenas
                return new CnpjValidationResult
                {
                    IsValid = true, // Se formato OK, considera válido
                    ErrorMessage = "Consulta Receita Federal indisponível, validação por formato",
                    ValidationSource = "FORMAT_FALLBACK",
                    CompanyName = "Não disponível",
                    Status = "Não verificado"
                };
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var receitaData = JsonSerializer.Deserialize<ReceitaFederalResponse>(jsonContent);

            if (receitaData?.Status == "ERROR")
            {
                return new CnpjValidationResult
                {
                    IsValid = false,
                    ErrorMessage = receitaData.Message ?? "CNPJ não encontrado na Receita Federal",
                    ValidationSource = "RECEITA_FEDERAL"
                };
            }

            _logger.LogInformation("CNPJ validado com sucesso - Empresa: {Nome}, Situação: {Situacao}", 
                receitaData?.Nome, receitaData?.Situacao);

            return new CnpjValidationResult
            {
                IsValid = receitaData?.Situacao?.ToUpper() == "ATIVA",
                CompanyName = receitaData?.Nome,
                TradeName = receitaData?.Fantasia,
                Status = receitaData?.Situacao,
                OpeningDate = ParseDate(receitaData?.Abertura),
                MainActivity = receitaData?.AtividadePrincipal?.FirstOrDefault()?.Text,
                ValidationSource = "RECEITA_FEDERAL",
                Address = new CnpjAddress
                {
                    Street = receitaData?.Logradouro,
                    Number = receitaData?.Numero,
                    Complement = receitaData?.Complemento,
                    District = receitaData?.Bairro,
                    City = receitaData?.Municipio,
                    State = receitaData?.Uf,
                    ZipCode = receitaData?.Cep
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar CNPJ: {Cnpj}", cnpj);
            
            // Em caso de erro, fazer validação básica de formato
            var cnpjLimpo = LimparCnpj(cnpj);
            return new CnpjValidationResult
            {
                IsValid = IsValidCnpjFormat(cnpjLimpo),
                ErrorMessage = "Erro na validação online, validação por formato apenas",
                ValidationSource = "FORMAT_FALLBACK"
            };
        }
    }

    private string LimparCnpj(string cnpj)
    {
        return new string(cnpj.Where(char.IsDigit).ToArray());
    }

    private bool IsValidCnpjFormat(string cnpj)
    {
        if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14)
            return false;

        // Verificar se todos os dígitos são iguais
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        // Validação dos dígitos verificadores
        var digits = cnpj.Select(c => int.Parse(c.ToString())).ToArray();
        
        // Primeiro dígito verificador
        var sum = 0;
        var multiplier = 5;
        for (int i = 0; i < 12; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        
        if (digits[12] != digit1)
            return false;

        // Segundo dígito verificador
        sum = 0;
        multiplier = 6;
        for (int i = 0; i < 13; i++)
        {
            sum += digits[i] * multiplier;
            multiplier = multiplier == 2 ? 9 : multiplier - 1;
        }
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        
        return digits[13] == digit2;
    }

    private DateTime? ParseDate(string? dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return null;

        if (DateTime.TryParseExact(dateString, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        return null;
    }
}

/// <summary>
/// Interface para validação de CNPJ
/// </summary>
public interface IReceitaFederalService
{
    Task<CnpjValidationResult> ValidateCnpjAsync(string cnpj, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resultado da validação de CNPJ
/// </summary>
public class CnpjValidationResult
{
    public bool IsValid { get; set; }
    public string? CompanyName { get; set; }
    public string? TradeName { get; set; }
    public string? Status { get; set; }
    public DateTime? OpeningDate { get; set; }
    public string? MainActivity { get; set; }
    public string? ErrorMessage { get; set; }
    public string ValidationSource { get; set; } = string.Empty;
    public CnpjAddress? Address { get; set; }
}

/// <summary>
/// Endereço da empresa
/// </summary>
public class CnpjAddress
{
    public string? Street { get; set; }
    public string? Number { get; set; }
    public string? Complement { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
}

/// <summary>
/// Resposta da API da Receita Federal
/// </summary>
internal class ReceitaFederalResponse
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("nome")]
    public string? Nome { get; set; }

    [JsonPropertyName("fantasia")]
    public string? Fantasia { get; set; }

    [JsonPropertyName("situacao")]
    public string? Situacao { get; set; }

    [JsonPropertyName("abertura")]
    public string? Abertura { get; set; }

    [JsonPropertyName("atividade_principal")]
    public List<AtividadeResponse>? AtividadePrincipal { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("numero")]
    public string? Numero { get; set; }

    [JsonPropertyName("complemento")]
    public string? Complemento { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("municipio")]
    public string? Municipio { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("cep")]
    public string? Cep { get; set; }
}

internal class AtividadeResponse
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
