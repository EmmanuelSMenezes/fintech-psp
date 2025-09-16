namespace FintechPSP.Shared.Domain.Constants;

/// <summary>
/// Códigos dos bancos brasileiros
/// </summary>
public static class BankCodes
{
    public const string BANCO_DO_BRASIL = "001";
    public const string SANTANDER = "033";
    public const string CAIXA_ECONOMICA = "104";
    public const string BRADESCO = "237";
    public const string ITAU = "341";
    public const string SICOOB = "756";
    public const string BANCO_GENIAL = "125";
    public const string EFI = "364";
    public const string CELCOIN = "329";
    public const string STARK_BANK = "20018183";

    /// <summary>
    /// Verifica se o código do banco é válido
    /// </summary>
    public static bool IsValidBankCode(string bankCode)
    {
        return bankCode switch
        {
            BANCO_DO_BRASIL or SANTANDER or CAIXA_ECONOMICA or BRADESCO or 
            ITAU or SICOOB or BANCO_GENIAL or EFI or CELCOIN or STARK_BANK => true,
            _ => false
        };
    }

    /// <summary>
    /// Obtém o nome do banco pelo código
    /// </summary>
    public static string GetBankName(string bankCode)
    {
        return bankCode switch
        {
            BANCO_DO_BRASIL => "Banco do Brasil",
            SANTANDER => "Santander",
            CAIXA_ECONOMICA => "Caixa Econômica Federal",
            BRADESCO => "Bradesco",
            ITAU => "Itaú",
            SICOOB => "Sicoob",
            BANCO_GENIAL => "Banco Genial",
            EFI => "Efí Bank",
            CELCOIN => "Celcoin",
            STARK_BANK => "Stark Bank",
            _ => "Banco Desconhecido"
        };
    }
}
