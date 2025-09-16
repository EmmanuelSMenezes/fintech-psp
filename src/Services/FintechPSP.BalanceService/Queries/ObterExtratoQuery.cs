using FintechPSP.BalanceService.DTOs;
using MediatR;

namespace FintechPSP.BalanceService.Queries;

/// <summary>
/// Query para obter extrato de uma conta
/// </summary>
public class ObterExtratoQuery : IRequest<StatementResponse>
{
    public Guid ClientId { get; set; }
    public string? AccountId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;

    public ObterExtratoQuery(Guid clientId, DateTime startDate, DateTime endDate, 
        string? accountId = null, int page = 1, int pageSize = 50)
    {
        ClientId = clientId;
        AccountId = accountId;
        StartDate = startDate;
        EndDate = endDate;
        Page = page;
        PageSize = pageSize;
    }
}
