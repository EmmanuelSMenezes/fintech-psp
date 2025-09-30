namespace SicoobIntegration.API.Models;

/// <summary>
/// Configurações da API do Sicoob
/// </summary>
public class SicoobSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AuthUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificatePassword { get; set; } = string.Empty;
    public ScopesSettings Scopes { get; set; } = new();
    public EndpointsSettings Endpoints { get; set; } = new();
}

/// <summary>
/// Escopos disponíveis para cada API
/// </summary>
public class ScopesSettings
{
    public List<string> CobrancaBancaria { get; set; } = new();
    public List<string> Pagamentos { get; set; } = new();
    public List<string> ContaCorrente { get; set; } = new();
    public List<string> OpenFinance { get; set; } = new();
    public List<string> PixPagamentos { get; set; } = new();
    public List<string> PixRecebimentos { get; set; } = new();
    public List<string> SPB { get; set; } = new();

    /// <summary>
    /// Retorna todos os escopos concatenados
    /// </summary>
    public string GetAllScopes()
    {
        var allScopes = new List<string>();
        allScopes.AddRange(CobrancaBancaria);
        allScopes.AddRange(Pagamentos);
        allScopes.AddRange(ContaCorrente);
        allScopes.AddRange(OpenFinance);
        allScopes.AddRange(PixPagamentos);
        allScopes.AddRange(PixRecebimentos);
        allScopes.AddRange(SPB);
        
        return string.Join(" ", allScopes.Distinct());
    }
}

/// <summary>
/// URLs dos endpoints de cada API
/// </summary>
public class EndpointsSettings
{
    public string CobrancaBancaria { get; set; } = string.Empty;
    public string Pagamentos { get; set; } = string.Empty;
    public string ContaCorrente { get; set; } = string.Empty;
    public string OpenFinance { get; set; } = string.Empty;
    public string PixPagamentos { get; set; } = string.Empty;
    public string PixRecebimentos { get; set; } = string.Empty;
    public string SPB { get; set; } = string.Empty;
}

