using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.IntegrationService.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FintechPSP.IntegrationService.Tests;

/// <summary>
/// Testes para o serviço de roteamento ponderado
/// </summary>
public class RoutingServiceTests
{
    private readonly Mock<IAccountDataService> _accountDataServiceMock;
    private readonly Mock<IPriorityConfigService> _priorityConfigServiceMock;
    private readonly Mock<ILogger<RoutingService>> _loggerMock;
    private readonly RoutingService _routingService;
    private readonly Guid _clienteId = Guid.NewGuid();

    public RoutingServiceTests()
    {
        _accountDataServiceMock = new Mock<IAccountDataService>();
        _priorityConfigServiceMock = new Mock<IPriorityConfigService>();
        _loggerMock = new Mock<ILogger<RoutingService>>();
        _routingService = new RoutingService(
            _accountDataServiceMock.Object,
            _priorityConfigServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SelectAccountForTransaction_ComPriorizacao_DeveUsarRoteamentoPonderado()
    {
        // Arrange
        var accounts = new List<AccountData>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Stark",
                CredentialsTokenId = "token_stark",
                IsActive = true
            },
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "EFI",
                AccountNumber = "98765-4",
                Description = "Conta Efí",
                CredentialsTokenId = "token_efi",
                IsActive = true
            }
        };

        var priorityConfig = new PriorityConfiguration
        {
            ClienteId = _clienteId,
            Prioridades = new List<AccountPriority>
            {
                new() { ContaId = accounts[0].ContaId, BankCode = "STARK", Percentual = 70.0m },
                new() { ContaId = accounts[1].ContaId, BankCode = "EFI", Percentual = 30.0m }
            },
            TotalPercentual = 100.0m,
            IsValid = true
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountsByClienteIdAsync(_clienteId))
            .ReturnsAsync(accounts);

        _priorityConfigServiceMock
            .Setup(s => s.GetPriorityConfigAsync(_clienteId))
            .ReturnsAsync(priorityConfig);

        _accountDataServiceMock
            .Setup(s => s.GetAccountByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => accounts.FirstOrDefault(a => a.ContaId == id));

        // Act
        var result = await _routingService.SelectAccountForTransactionAsync(_clienteId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result.BankCode, new[] { "STARK", "EFI" });
        Assert.Contains("Weighted selection", result.SelectionReason);
    }

    [Fact]
    public async Task SelectAccountForTransaction_SemPriorizacao_DeveUsarPrimeiraConta()
    {
        // Arrange
        var accounts = new List<AccountData>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Stark",
                CredentialsTokenId = "token_stark",
                IsActive = true
            }
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountsByClienteIdAsync(_clienteId))
            .ReturnsAsync(accounts);

        _priorityConfigServiceMock
            .Setup(s => s.GetPriorityConfigAsync(_clienteId))
            .ReturnsAsync((PriorityConfiguration?)null);

        _accountDataServiceMock
            .Setup(s => s.GetAccountByIdAsync(accounts[0].ContaId))
            .ReturnsAsync(accounts[0]);

        // Act
        var result = await _routingService.SelectAccountForTransactionAsync(_clienteId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("STARK", result.BankCode);
        Assert.Contains("Default selection", result.SelectionReason);
    }

    [Fact]
    public async Task SelectAccountForTransaction_ComFiltroBanco_DeveRetornarApenasContasDoBanco()
    {
        // Arrange
        var accounts = new List<AccountData>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Stark",
                CredentialsTokenId = "token_stark",
                IsActive = true
            },
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "EFI",
                AccountNumber = "98765-4",
                Description = "Conta Efí",
                CredentialsTokenId = "token_efi",
                IsActive = true
            }
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountsByClienteIdAsync(_clienteId))
            .ReturnsAsync(accounts);

        _priorityConfigServiceMock
            .Setup(s => s.GetPriorityConfigAsync(_clienteId))
            .ReturnsAsync((PriorityConfiguration?)null);

        _accountDataServiceMock
            .Setup(s => s.GetAccountByIdAsync(accounts[1].ContaId))
            .ReturnsAsync(accounts[1]);

        // Act
        var result = await _routingService.SelectAccountForTransactionAsync(_clienteId, "EFI");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("EFI", result.BankCode);
    }

    [Fact]
    public async Task SelectAccountForTransaction_SemContasAtivas_DeveRetornarNull()
    {
        // Arrange
        var accounts = new List<AccountData>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Stark",
                CredentialsTokenId = "token_stark",
                IsActive = false // Inativa
            }
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountsByClienteIdAsync(_clienteId))
            .ReturnsAsync(accounts);

        _priorityConfigServiceMock
            .Setup(s => s.GetPriorityConfigAsync(_clienteId))
            .ReturnsAsync((PriorityConfiguration?)null);

        // Act
        var result = await _routingService.SelectAccountForTransactionAsync(_clienteId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAccountsWithPriority_DeveRetornarContasComPrioridades()
    {
        // Arrange
        var accounts = new List<AccountData>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Stark",
                IsActive = true
            },
            new()
            {
                ContaId = Guid.NewGuid(),
                ClienteId = _clienteId,
                BankCode = "EFI",
                AccountNumber = "98765-4",
                Description = "Conta Efí",
                IsActive = true
            }
        };

        var priorityConfig = new PriorityConfiguration
        {
            ClienteId = _clienteId,
            Prioridades = new List<AccountPriority>
            {
                new() { ContaId = accounts[0].ContaId, BankCode = "STARK", Percentual = 80.0m }
                // Apenas uma conta tem prioridade configurada
            }
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountsByClienteIdAsync(_clienteId))
            .ReturnsAsync(accounts);

        _priorityConfigServiceMock
            .Setup(s => s.GetPriorityConfigAsync(_clienteId))
            .ReturnsAsync(priorityConfig);

        // Act
        var result = await _routingService.GetAccountsWithPriorityAsync(_clienteId);

        // Assert
        Assert.Equal(2, result.Count);
        
        var starkAccount = result.First(a => a.BankCode == "STARK");
        Assert.True(starkAccount.HasPriorityConfig);
        Assert.Equal(80.0m, starkAccount.Priority);
        
        var efiAccount = result.First(a => a.BankCode == "EFI");
        Assert.False(efiAccount.HasPriorityConfig);
        Assert.Equal(0.0m, efiAccount.Priority);
    }

    [Fact]
    public async Task ValidateAccountForTransaction_ContaValidaEAtiva_DeveRetornarTrue()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var account = new AccountData
        {
            ContaId = contaId,
            ClienteId = _clienteId,
            BankCode = "STARK",
            IsActive = true
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountByIdAsync(contaId))
            .ReturnsAsync(account);

        // Act
        var result = await _routingService.ValidateAccountForTransactionAsync(contaId, _clienteId, 1000.0m);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateAccountForTransaction_ContaInativa_DeveRetornarFalse()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var account = new AccountData
        {
            ContaId = contaId,
            ClienteId = _clienteId,
            BankCode = "STARK",
            IsActive = false // Inativa
        };

        _accountDataServiceMock
            .Setup(s => s.GetAccountByIdAsync(contaId))
            .ReturnsAsync(account);

        // Act
        var result = await _routingService.ValidateAccountForTransactionAsync(contaId, _clienteId, 1000.0m);

        // Assert
        Assert.False(result);
    }
}
