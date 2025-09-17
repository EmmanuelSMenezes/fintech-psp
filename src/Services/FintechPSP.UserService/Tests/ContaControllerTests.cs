using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.Controllers;
using FintechPSP.UserService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FintechPSP.UserService.Tests;

/// <summary>
/// Testes para o ContaController (Banking scope)
/// </summary>
public class ContaControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<ContaController>> _loggerMock;
    private readonly ContaController _controller;
    private readonly Guid _clienteId = Guid.NewGuid();

    public ContaControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ContaController>>();
        _controller = new ContaController(_mediatorMock.Object, _loggerMock.Object);

        // Setup do contexto HTTP com claims do usuário
        var claims = new List<Claim>
        {
            new("sub", _clienteId.ToString()),
            new("clienteId", _clienteId.ToString()),
            new("scope", "banking")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetContas_DeveRetornarContasDoCliente()
    {
        // Arrange
        var contasEsperadas = new List<ContaResponse>
        {
            new()
            {
                ContaId = Guid.NewGuid(),
                BankCode = "STARK",
                AccountNumber = "12345-6",
                Description = "Conta Principal",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ObterContasQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contasEsperadas);

        // Act
        var result = await _controller.GetContas();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsAssignableFrom<IEnumerable<ContaResponse>>(okResult.Value);
        Assert.Single(response);
    }

    [Fact]
    public async Task CriarConta_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var request = new CriarContaRequest
        {
            BankCode = "EFI",
            AccountNumber = "98765-4",
            Description = "Conta Efí",
            Credentials = new CredentialsRequest
            {
                ClientId = "client_efi_001",
                ClientSecret = "secret_efi_001"
            }
        };

        var contaEsperada = new ContaResponse
        {
            ContaId = Guid.NewGuid(),
            BankCode = request.BankCode,
            AccountNumber = request.AccountNumber,
            Description = request.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CriarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contaEsperada);

        // Act
        var result = await _controller.CriarConta(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<ContaResponse>(createdResult.Value);
        Assert.Equal(request.BankCode, response.BankCode);
        Assert.Equal(request.AccountNumber, response.AccountNumber);
    }

    [Fact]
    public async Task CriarConta_ComDadosInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new CriarContaRequest
        {
            BankCode = "", // Inválido
            AccountNumber = "98765-4",
            Description = "Conta Teste"
        };

        _controller.ModelState.AddModelError("BankCode", "BankCode é obrigatório");

        // Act
        var result = await _controller.CriarConta(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarConta_ComDadosValidos_DeveRetornarSucesso()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var request = new AtualizarContaRequest
        {
            Description = "Conta Atualizada",
            IsActive = true,
            Credentials = new CredentialsRequest
            {
                ClientId = "client_updated",
                ClientSecret = "secret_updated"
            }
        };

        var contaAtualizada = new ContaResponse
        {
            ContaId = contaId,
            BankCode = "EFI",
            AccountNumber = "98765-4",
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AtualizarContaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(contaAtualizada);

        // Act
        var result = await _controller.AtualizarConta(contaId, request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ContaResponse>(okResult.Value);
        Assert.Equal(request.Description, response.Description);
        Assert.Equal(request.IsActive, response.IsActive);
    }

    [Fact]
    public async Task RemoverConta_ComIdValido_DeveRetornarNoContent()
    {
        // Arrange
        var contaId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoverContaCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoverConta(contaId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemoverConta_ContaNaoEncontrada_DeveRetornarNotFound()
    {
        // Arrange
        var contaId = Guid.NewGuid();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoverContaCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Conta não encontrada"));

        // Act
        var result = await _controller.RemoverConta(contaId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = badRequestResult.Value;
        Assert.NotNull(errorResponse);
    }
}
