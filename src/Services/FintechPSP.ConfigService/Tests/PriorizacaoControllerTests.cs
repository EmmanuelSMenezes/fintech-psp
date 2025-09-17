using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.ConfigService.Commands;
using FintechPSP.ConfigService.Controllers;
using FintechPSP.ConfigService.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FintechPSP.ConfigService.Tests;

/// <summary>
/// Testes para os controllers de priorização
/// </summary>
public class PriorizacaoControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<RoteamentoBankingController>> _loggerMock;
    private readonly RoteamentoBankingController _controller;
    private readonly Guid _clienteId = Guid.NewGuid();

    public PriorizacaoControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<RoteamentoBankingController>>();
        _controller = new RoteamentoBankingController(_mediatorMock.Object, _loggerMock.Object);

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
    public async Task GetRoteamento_DeveRetornarConfiguracaoExistente()
    {
        // Arrange
        var configuracaoEsperada = new RoteamentoResponse
        {
            ClienteId = _clienteId,
            Prioridades = new List<PrioridadeResponse>
            {
                new() { ContaId = Guid.NewGuid(), BankCode = "STARK", Percentual = 50.0m },
                new() { ContaId = Guid.NewGuid(), BankCode = "EFI", Percentual = 30.0m },
                new() { ContaId = Guid.NewGuid(), BankCode = "SICOOB", Percentual = 20.0m }
            },
            TotalPercentual = 100.0m,
            IsValid = true,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ObterRoteamentoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(configuracaoEsperada);

        // Act
        var result = await _controller.GetRoteamento();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RoteamentoResponse>(okResult.Value);
        Assert.Equal(_clienteId, response.ClienteId);
        Assert.Equal(3, response.Prioridades.Count);
        Assert.Equal(100.0m, response.TotalPercentual);
        Assert.True(response.IsValid);
    }

    [Fact]
    public async Task GetRoteamento_SemConfiguracao_DeveRetornarNotFound()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ObterRoteamentoQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RoteamentoResponse?)null);

        // Act
        var result = await _controller.GetRoteamento();

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AtualizarRoteamento_ComPercentuaisValidos_DeveRetornarSucesso()
    {
        // Arrange
        var request = new AtualizarRoteamentoRequest
        {
            Prioridades = new List<PrioridadeRequest>
            {
                new() { ContaId = Guid.NewGuid(), Percentual = 60.0m },
                new() { ContaId = Guid.NewGuid(), Percentual = 40.0m }
            }
        };

        var responseEsperada = new RoteamentoResponse
        {
            ClienteId = _clienteId,
            Prioridades = request.Prioridades.Select(p => new PrioridadeResponse
            {
                ContaId = p.ContaId,
                BankCode = "STARK", // Seria obtido do banco de dados
                Percentual = p.Percentual
            }).ToList(),
            TotalPercentual = 100.0m,
            IsValid = true,
            UpdatedAt = DateTime.UtcNow
        };

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<AtualizarRoteamentoCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseEsperada);

        // Act
        var result = await _controller.AtualizarRoteamento(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<RoteamentoResponse>(okResult.Value);
        Assert.Equal(100.0m, response.TotalPercentual);
        Assert.True(response.IsValid);
    }

    [Fact]
    public async Task AtualizarRoteamento_ComPercentuaisInvalidos_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AtualizarRoteamentoRequest
        {
            Prioridades = new List<PrioridadeRequest>
            {
                new() { ContaId = Guid.NewGuid(), Percentual = 60.0m },
                new() { ContaId = Guid.NewGuid(), Percentual = 50.0m } // Total = 110%
            }
        };

        _controller.ModelState.AddModelError("Prioridades", "Total dos percentuais deve ser 100%");

        // Act
        var result = await _controller.AtualizarRoteamento(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarRoteamento_ComPercentualZero_DeveRetornarBadRequest()
    {
        // Arrange
        var request = new AtualizarRoteamentoRequest
        {
            Prioridades = new List<PrioridadeRequest>
            {
                new() { ContaId = Guid.NewGuid(), Percentual = 0.0m }, // Inválido
                new() { ContaId = Guid.NewGuid(), Percentual = 100.0m }
            }
        };

        _controller.ModelState.AddModelError("Prioridades", "Percentual deve ser maior que 0");

        // Act
        var result = await _controller.AtualizarRoteamento(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task AtualizarRoteamento_ComContaDuplicada_DeveRetornarBadRequest()
    {
        // Arrange
        var contaId = Guid.NewGuid();
        var request = new AtualizarRoteamentoRequest
        {
            Prioridades = new List<PrioridadeRequest>
            {
                new() { ContaId = contaId, Percentual = 50.0m },
                new() { ContaId = contaId, Percentual = 50.0m } // Duplicada
            }
        };

        _controller.ModelState.AddModelError("Prioridades", "Conta duplicada na configuração");

        // Act
        var result = await _controller.AtualizarRoteamento(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task RemoverRoteamento_DeveRetornarNoContent()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RemoverRoteamentoCommand>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoverRoteamento();

        // Assert
        Assert.IsType<NoContentResult>(result);
    }
}
