using Microsoft.Extensions.Options;
using FintechPSP.IntegrationService.Models.Sicoob;
using FintechPSP.IntegrationService.Models.Sicoob.Pix;
using FintechPSP.IntegrationService.Services.Sicoob.Base;

namespace FintechPSP.IntegrationService.Services.Sicoob.Pix;

/// <summary>
/// Serviço para recebimentos PIX (cobranças) do Sicoob
/// </summary>
public class PixRecebimentosService : SicoobServiceBase, IPixRecebimentosService
{
    private readonly string _baseEndpoint;

    public PixRecebimentosService(
        IHttpClientFactory httpClientFactory,
        ISicoobAuthService authService,
        IOptions<SicoobSettings> settings,
        ILogger<PixRecebimentosService> logger)
        : base(httpClientFactory, authService, settings, logger)
    {
        _baseEndpoint = Settings.Endpoints.PixRecebimentos;
    }

    public async Task<CobrancaResponse?> CriarCobrancaImediataAsync(
        CobrancaImediataRequest dadosCobranca,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Criando cobrança PIX imediata para chave: {Chave}, valor: {Valor}",
                dadosCobranca.Chave, dadosCobranca.Valor.Original);

            var url = $"{_baseEndpoint}/cob";
            var response = await PostAsync<CobrancaImediataRequest, CobrancaResponse>(url, dadosCobranca, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Cobrança PIX imediata criada com sucesso. TxId: {TxId}, Status: {Status}", 
                    response.TxId, response.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao criar cobrança PIX imediata para chave: {Chave}", dadosCobranca.Chave);
            throw;
        }
    }

    public async Task<CobrancaResponse?> CriarCobrancaComVencimentoAsync(
        CobrancaRequest dadosCobranca, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Criando cobrança PIX com vencimento para chave: {Chave}, valor: {Valor}", 
                dadosCobranca.Chave, dadosCobranca.Valor.Original);

            var url = $"{_baseEndpoint}/cobv";
            var response = await PostAsync<CobrancaRequest, CobrancaResponse>(url, dadosCobranca, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Cobrança PIX com vencimento criada com sucesso. TxId: {TxId}, Status: {Status}", 
                    response.TxId, response.Status);
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao criar cobrança PIX com vencimento para chave: {Chave}", dadosCobranca.Chave);
            throw;
        }
    }



    public async Task<object?> ListarCobrancasAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Listando cobranças PIX de {DataInicio} até {DataFim}", dataInicio, dataFim);

            var inicioFormatado = dataInicio.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var fimFormatado = dataFim.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            
            var url = $"{_baseEndpoint}/cob?inicio={inicioFormatado}&fim={fimFormatado}";
            var response = await GetAsync<object>(url, cancellationToken);

            Logger.LogInformation("Listagem de cobranças PIX concluída");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao listar cobranças PIX de {DataInicio} até {DataFim}", dataInicio, dataFim);
            throw;
        }
    }

    public async Task<object?> ConsultarPixRecebidosAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando PIX recebidos de {DataInicio} até {DataFim}", dataInicio, dataFim);

            var inicioFormatado = dataInicio.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var fimFormatado = dataFim.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            
            var url = $"{_baseEndpoint}/pix?inicio={inicioFormatado}&fim={fimFormatado}";
            var response = await GetAsync<object>(url, cancellationToken);

            Logger.LogInformation("Consulta de PIX recebidos concluída");

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar PIX recebidos de {DataInicio} até {DataFim}", dataInicio, dataFim);
            throw;
        }
    }

    public async Task<CobrancaResponse?> ConsultarCobrancaAsync(string txId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando cobrança PIX imediata: {TxId}", txId);

            var url = $"{_baseEndpoint}/cob/{txId}";
            var response = await GetAsync<CobrancaResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("Cobrança PIX consultada com sucesso. TxId: {TxId}, Status: {Status}, Location: {Location}, QrCode: {QrCode}, PixCopiaECola: {PixCopiaECola}",
                    response.TxId, response.Status,
                    response.Location ?? response.Loc?.Location ?? "N/A",
                    string.IsNullOrEmpty(response.QrCode) ? "VAZIO" : "PRESENTE",
                    string.IsNullOrEmpty(response.PixCopiaECola) ? "VAZIO" : "PRESENTE");
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar cobrança PIX imediata: {TxId}", txId);
            throw;
        }
    }

    public async Task<QrCodeResponse?> ConsultarQrCodeAsync(int locationId, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Consultando QR Code PIX para Location ID: {LocationId}", locationId);

            var url = $"{_baseEndpoint}/loc/{locationId}/qrcode";
            var response = await GetAsync<QrCodeResponse>(url, cancellationToken);

            if (response != null)
            {
                Logger.LogInformation("QR Code PIX consultado com sucesso. Location ID: {LocationId}, QrCode: {QrCode}, Imagem: {Imagem}",
                    locationId,
                    string.IsNullOrEmpty(response.QrCode) ? "VAZIO" : $"{response.QrCode.Length} caracteres",
                    string.IsNullOrEmpty(response.ImagemQrCode) ? "VAZIO" : "PRESENTE");
            }

            return response;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro ao consultar QR Code PIX para Location ID: {LocationId}", locationId);
            throw;
        }
    }
}
