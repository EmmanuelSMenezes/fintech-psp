using System.Threading;
using System.Threading.Tasks;
using FintechPSP.BalanceService.DTOs;
using FintechPSP.BalanceService.Queries;
using FintechPSP.BalanceService.Repositories;
using MediatR;

namespace FintechPSP.BalanceService.Handlers;

/// <summary>
/// Handler para obter saldo
/// </summary>
public class ObterSaldoHandler : IRequestHandler<ObterSaldoQuery, BalanceResponse>
{
    private readonly IAccountRepository _accountRepository;

    public ObterSaldoHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
    }

    public async Task<BalanceResponse> Handle(ObterSaldoQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByClientIdAsync(request.ClientId, request.AccountId);
        
        if (account == null)
        {
            throw new InvalidOperationException($"Conta não encontrada para cliente {request.ClientId}");
        }

        return new BalanceResponse
        {
            ClientId = account.ClientId,
            AccountId = account.AccountId,
            AvailableBalance = account.AvailableBalance.Amount,
            BlockedBalance = account.BlockedBalance.Amount,
            TotalBalance = account.GetTotalBalance(),
            Currency = account.AvailableBalance.Currency,
            LastUpdated = account.LastUpdated
        };
    }
}
