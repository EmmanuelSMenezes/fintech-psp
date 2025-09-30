using SicoobIntegration.API.Models.Pix;

namespace SicoobIntegration.API.Services.Pix;

public interface IPixPagamentosService
{
    Task<PixPagamentoResponse?> RealizarPagamentoPixAsync(PixPagamentoRequest dadosPagamento, CancellationToken cancellationToken = default);
    Task<PixPagamentoResponse?> ConsultarPagamentoAsync(string e2eId, CancellationToken cancellationToken = default);
    Task<ListaPixPagamentosResponse?> ListarPagamentosAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}

