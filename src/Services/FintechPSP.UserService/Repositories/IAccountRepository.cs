using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.DTOs;

namespace FintechPSP.UserService.Repositories;

public interface IAccountRepository
{
    Task<(IReadOnlyList<BankAccount> contas, int total)> GetPagedAsync(int page, int limit, Guid? clienteId);
    Task<IReadOnlyList<BankAccount>> GetByClientAsync(Guid clienteId);
    Task<BankAccount?> GetByIdAsync(Guid contaId);
    Task<BankAccount> CreateAsync(CreateBankAccountRequest request);
    Task<BankAccount> UpdateAsync(Guid contaId, UpdateBankAccountRequest request);
    Task<bool> DeleteAsync(Guid contaId);
}

