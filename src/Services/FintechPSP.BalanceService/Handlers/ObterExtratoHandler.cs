using System.Threading;
using System.Threading.Tasks;
using FintechPSP.BalanceService.DTOs;
using FintechPSP.BalanceService.Queries;
using FintechPSP.BalanceService.Repositories;
using MediatR;

namespace FintechPSP.BalanceService.Handlers;

/// <summary>
/// Handler para obter extrato
/// </summary>
public class ObterExtratoHandler : IRequestHandler<ObterExtratoQuery, StatementResponse>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionHistoryRepository _transactionHistoryRepository;

    public ObterExtratoHandler(
        IAccountRepository accountRepository,
        ITransactionHistoryRepository transactionHistoryRepository)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _transactionHistoryRepository = transactionHistoryRepository ?? throw new ArgumentNullException(nameof(transactionHistoryRepository));
    }

    public async Task<StatementResponse> Handle(ObterExtratoQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByClientIdAsync(request.ClientId, request.AccountId);
        
        if (account == null)
        {
            throw new InvalidOperationException($"Conta não encontrada para cliente {request.ClientId}");
        }

        var transactions = await _transactionHistoryRepository.GetTransactionHistoryAsync(
            request.ClientId,
            request.AccountId,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize);

        var totalCount = await _transactionHistoryRepository.GetTransactionCountAsync(
            request.ClientId,
            request.AccountId,
            request.StartDate,
            request.EndDate);

        return new StatementResponse
        {
            ClientId = account.ClientId,
            AccountId = account.AccountId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Transactions = transactions.Select(t => new StatementTransaction
            {
                TransactionId = t.TransactionId,
                ExternalId = t.ExternalId,
                Type = t.Type,
                Amount = t.Amount,
                Description = t.Description,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
                Operation = t.Operation
            }).ToList(),
            TotalCount = totalCount,
            CurrentBalance = account.AvailableBalance.Amount
        };
    }
}
