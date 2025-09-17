using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FintechPSP.IntegrationService.Services;

/// <summary>
/// Implementação do serviço de roteamento ponderado
/// </summary>
public class RoutingService : IRoutingService
{
    private readonly IAccountDataService _accountDataService;
    private readonly IPriorityConfigService _priorityConfigService;
    private readonly ILogger<RoutingService> _logger;
    private readonly Random _random;

    public RoutingService(
        IAccountDataService accountDataService,
        IPriorityConfigService priorityConfigService,
        ILogger<RoutingService> logger)
    {
        _accountDataService = accountDataService ?? throw new ArgumentNullException(nameof(accountDataService));
        _priorityConfigService = priorityConfigService ?? throw new ArgumentNullException(nameof(priorityConfigService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    public async Task<SelectedAccountInfo?> SelectAccountForTransactionAsync(Guid clienteId, string? bankCode = null, decimal? amount = null)
    {
        try
        {
            // Obter contas com prioridades
            var accountsWithPriority = await GetAccountsWithPriorityAsync(clienteId);
            
            // Filtrar por banco se especificado
            if (!string.IsNullOrEmpty(bankCode))
            {
                accountsWithPriority = accountsWithPriority
                    .Where(a => a.BankCode.Equals(bankCode, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrar apenas contas ativas
            accountsWithPriority = accountsWithPriority.Where(a => a.IsActive).ToList();

            if (!accountsWithPriority.Any())
            {
                _logger.LogWarning("Nenhuma conta ativa encontrada para cliente {ClienteId} e banco {BankCode}", 
                    clienteId, bankCode ?? "ANY");
                return null;
            }

            // Se há configuração de prioridade, usar roteamento ponderado
            var accountsWithConfig = accountsWithPriority.Where(a => a.HasPriorityConfig).ToList();
            if (accountsWithConfig.Any())
            {
                var selectedAccount = SelectAccountByWeight(accountsWithConfig);
                return await CreateSelectedAccountInfo(selectedAccount, "Weighted selection based on priority configuration");
            }

            // Se não há configuração de prioridade, usar round-robin ou primeira conta disponível
            var firstAccount = accountsWithPriority.First();
            return await CreateSelectedAccountInfo(firstAccount, "Default selection - no priority configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao selecionar conta para transação do cliente {ClienteId}", clienteId);
            return null;
        }
    }

    public async Task<List<AccountWithPriority>> GetAccountsWithPriorityAsync(Guid clienteId)
    {
        try
        {
            // Obter contas do cliente
            var accounts = await _accountDataService.GetAccountsByClienteIdAsync(clienteId);
            
            // Obter configuração de prioridade
            var priorityConfig = await _priorityConfigService.GetPriorityConfigAsync(clienteId);
            
            var result = new List<AccountWithPriority>();
            
            foreach (var account in accounts)
            {
                var accountWithPriority = new AccountWithPriority
                {
                    ContaId = account.ContaId,
                    BankCode = account.BankCode,
                    AccountNumber = account.AccountNumber,
                    Description = account.Description,
                    IsActive = account.IsActive
                };

                // Verificar se há configuração de prioridade para esta conta
                var priority = priorityConfig?.Prioridades?.FirstOrDefault(p => p.ContaId == account.ContaId);
                if (priority != null)
                {
                    accountWithPriority.Priority = priority.Percentual;
                    accountWithPriority.HasPriorityConfig = true;
                }

                result.Add(accountWithPriority);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contas com prioridade para cliente {ClienteId}", clienteId);
            return new List<AccountWithPriority>();
        }
    }

    public async Task<bool> ValidateAccountForTransactionAsync(Guid contaId, Guid clienteId, decimal amount)
    {
        try
        {
            // Verificar se a conta existe e pertence ao cliente
            var account = await _accountDataService.GetAccountByIdAsync(contaId);
            if (account == null || account.ClienteId != clienteId || !account.IsActive)
            {
                return false;
            }

            // Aqui você pode adicionar validações adicionais:
            // - Limites de transação
            // - Saldo disponível
            // - Horário de funcionamento do banco
            // - Status da conta no banco
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao validar conta {ContaId} para transação", contaId);
            return false;
        }
    }

    private AccountWithPriority SelectAccountByWeight(List<AccountWithPriority> accounts)
    {
        // Implementação de seleção ponderada baseada nos percentuais
        var totalWeight = accounts.Sum(a => a.Priority);
        var randomValue = _random.NextDouble() * (double)totalWeight;
        
        decimal cumulativeWeight = 0;
        foreach (var account in accounts.OrderBy(a => a.Priority))
        {
            cumulativeWeight += account.Priority;
            if (randomValue <= (double)cumulativeWeight)
            {
                _logger.LogDebug("Conta selecionada: {ContaId} ({BankCode}) com peso {Priority}%", 
                    account.ContaId, account.BankCode, account.Priority);
                return account;
            }
        }

        // Fallback para a última conta (não deveria acontecer)
        return accounts.Last();
    }

    private async Task<SelectedAccountInfo> CreateSelectedAccountInfo(AccountWithPriority account, string reason)
    {
        // Obter dados completos da conta incluindo credenciais tokenizadas
        var fullAccount = await _accountDataService.GetAccountByIdAsync(account.ContaId);
        
        return new SelectedAccountInfo
        {
            ContaId = account.ContaId,
            BankCode = account.BankCode,
            AccountNumber = account.AccountNumber,
            Description = account.Description,
            CredentialsTokenId = fullAccount?.CredentialsTokenId ?? string.Empty,
            Priority = account.Priority,
            SelectionReason = reason
        };
    }
}
