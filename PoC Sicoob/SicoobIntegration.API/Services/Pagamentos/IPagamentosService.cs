using SicoobIntegration.API.Models.Pagamentos;

namespace SicoobIntegration.API.Services.Pagamentos;

public interface IPagamentosService
{
    Task<PagamentoResponse?> IncluirPagamentoBoletoAsync(PagamentoBoletoRequest dadosPagamento, CancellationToken cancellationToken = default);
    Task<PagamentoResponse?> ConsultarPagamentoAsync(string idPagamento, CancellationToken cancellationToken = default);
    Task<PagamentoResponse?> AlterarPagamentoAsync(string idPagamento, PagamentoAlteracaoRequest dadosAlteracao, CancellationToken cancellationToken = default);
}

