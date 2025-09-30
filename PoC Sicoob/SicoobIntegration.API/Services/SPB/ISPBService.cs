using SicoobIntegration.API.Models.SPB;

namespace SicoobIntegration.API.Services.SPB;

public interface ISPBService
{
    Task<TEDResponse?> RealizarTEDAsync(TEDRequest dadosTED, CancellationToken cancellationToken = default);
    Task<TEDResponse?> ConsultarTEDAsync(string idTransferencia, CancellationToken cancellationToken = default);
    Task<ListaTEDsResponse?> ListarTEDsAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}

