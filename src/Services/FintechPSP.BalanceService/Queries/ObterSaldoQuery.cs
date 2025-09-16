using FintechPSP.BalanceService.DTOs;
using MediatR;

namespace FintechPSP.BalanceService.Queries;

/// <summary>
/// Query para obter saldo de uma conta
/// </summary>
public class ObterSaldoQuery : IRequest<BalanceResponse>
{
    public Guid ClientId { get; set; }
    public string? AccountId { get; set; }

    public ObterSaldoQuery(Guid clientId, string? accountId = null)
    {
        ClientId = clientId;
        AccountId = accountId;
    }
}
