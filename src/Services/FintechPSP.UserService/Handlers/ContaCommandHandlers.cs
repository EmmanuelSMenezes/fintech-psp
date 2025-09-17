using System;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using FintechPSP.UserService.Commands;
using FintechPSP.UserService.DTOs;
using FintechPSP.UserService.Models;
using FintechPSP.UserService.Repositories;
using FintechPSP.UserService.Services;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FintechPSP.UserService.Handlers;

/// <summary>
/// Handler para comando de criar conta bancária
/// </summary>
public class CriarContaCommandHandler : IRequestHandler<CriarContaCommand, ContaResponse>
{
    private readonly IContaBancariaRepository _contaRepository;
    private readonly ICredentialsTokenService _credentialsService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CriarContaCommandHandler> _logger;

    public CriarContaCommandHandler(
        IContaBancariaRepository contaRepository,
        ICredentialsTokenService credentialsService,
        IPublishEndpoint publishEndpoint,
        ILogger<CriarContaCommandHandler> logger)
    {
        _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
        _credentialsService = credentialsService ?? throw new ArgumentNullException(nameof(credentialsService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContaResponse> Handle(CriarContaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar se já existe conta com mesmo bankCode e accountNumber
            var exists = await _contaRepository.ExistsAsync(request.ClienteId, request.BankCode, request.AccountNumber);
            if (exists)
            {
                return new ContaResponse
                {
                    Status = "ERROR",
                    Message = "Já existe uma conta com este código de banco e número de conta"
                };
            }

            // Criar a conta
            var conta = new ContaBancaria
            {
                ClienteId = request.ClienteId,
                BankCode = request.BankCode,
                AccountNumber = request.AccountNumber,
                Description = request.Description
            };

            // Tokenizar as credenciais
            var tokenId = await _credentialsService.TokenizeCredentialsAsync(conta.Id, request.Credentials);
            conta.CredentialsTokenId = tokenId;

            // Salvar no banco
            var contaCriada = await _contaRepository.CreateAsync(conta);

            // Publicar evento
            var evento = new ContaBancariaCriada(
                contaCriada.Id,
                contaCriada.ClienteId,
                contaCriada.BankCode,
                contaCriada.AccountNumber,
                contaCriada.Description,
                contaCriada.CredentialsTokenId);

            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation("Conta bancária criada com sucesso: {ContaId} para cliente {ClienteId}", 
                contaCriada.Id, contaCriada.ClienteId);

            return new ContaResponse
            {
                ContaId = contaCriada.Id,
                Status = "CREATED",
                Message = "Conta bancária criada com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar conta bancária para cliente {ClienteId}", request.ClienteId);
            return new ContaResponse
            {
                Status = "ERROR",
                Message = "Erro interno ao criar conta bancária"
            };
        }
    }
}

/// <summary>
/// Handler para comando de atualizar conta bancária
/// </summary>
public class AtualizarContaCommandHandler : IRequestHandler<AtualizarContaCommand, ContaResponse>
{
    private readonly IContaBancariaRepository _contaRepository;
    private readonly ICredentialsTokenService _credentialsService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<AtualizarContaCommandHandler> _logger;

    public AtualizarContaCommandHandler(
        IContaBancariaRepository contaRepository,
        ICredentialsTokenService credentialsService,
        IPublishEndpoint publishEndpoint,
        ILogger<AtualizarContaCommandHandler> logger)
    {
        _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
        _credentialsService = credentialsService ?? throw new ArgumentNullException(nameof(credentialsService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContaResponse> Handle(AtualizarContaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar se a conta existe e pertence ao cliente
            var conta = await _contaRepository.GetByIdAsync(request.ContaId);
            if (conta == null)
            {
                return new ContaResponse
                {
                    Status = "NOT_FOUND",
                    Message = "Conta não encontrada"
                };
            }

            if (conta.ClienteId != request.ClienteId)
            {
                return new ContaResponse
                {
                    Status = "FORBIDDEN",
                    Message = "Conta não pertence ao cliente"
                };
            }

            // Atualizar descrição se fornecida
            if (!string.IsNullOrEmpty(request.Description))
            {
                conta.Description = request.Description;
            }

            // Atualizar credenciais se fornecidas
            string? newTokenId = null;
            if (request.Credentials != null)
            {
                await _credentialsService.UpdateCredentialsAsync(conta.CredentialsTokenId, request.Credentials);
            }

            // Salvar alterações
            var contaAtualizada = await _contaRepository.UpdateAsync(conta);

            // Publicar evento
            var evento = new ContaBancariaAtualizada(
                contaAtualizada.Id,
                contaAtualizada.ClienteId,
                request.Description,
                newTokenId);

            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation("Conta bancária atualizada com sucesso: {ContaId}", contaAtualizada.Id);

            return new ContaResponse
            {
                ContaId = contaAtualizada.Id,
                Status = "UPDATED",
                Message = "Conta bancária atualizada com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar conta bancária {ContaId}", request.ContaId);
            return new ContaResponse
            {
                Status = "ERROR",
                Message = "Erro interno ao atualizar conta bancária"
            };
        }
    }
}

/// <summary>
/// Handler para comando de remover conta bancária
/// </summary>
public class RemoverContaCommandHandler : IRequestHandler<RemoverContaCommand, ContaResponse>
{
    private readonly IContaBancariaRepository _contaRepository;
    private readonly ICredentialsTokenService _credentialsService;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<RemoverContaCommandHandler> _logger;

    public RemoverContaCommandHandler(
        IContaBancariaRepository contaRepository,
        ICredentialsTokenService credentialsService,
        IPublishEndpoint publishEndpoint,
        ILogger<RemoverContaCommandHandler> logger)
    {
        _contaRepository = contaRepository ?? throw new ArgumentNullException(nameof(contaRepository));
        _credentialsService = credentialsService ?? throw new ArgumentNullException(nameof(credentialsService));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContaResponse> Handle(RemoverContaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar se a conta existe e pertence ao cliente
            var conta = await _contaRepository.GetByIdAsync(request.ContaId);
            if (conta == null)
            {
                return new ContaResponse
                {
                    Status = "NOT_FOUND",
                    Message = "Conta não encontrada"
                };
            }

            if (conta.ClienteId != request.ClienteId)
            {
                return new ContaResponse
                {
                    Status = "FORBIDDEN",
                    Message = "Conta não pertence ao cliente"
                };
            }

            // Remover a conta (soft delete)
            var removida = await _contaRepository.DeleteAsync(request.ContaId);
            if (!removida)
            {
                return new ContaResponse
                {
                    Status = "ERROR",
                    Message = "Erro ao remover conta bancária"
                };
            }

            // Remover as credenciais tokenizadas
            await _credentialsService.RemoveCredentialsAsync(conta.CredentialsTokenId);

            // Publicar evento
            var evento = new ContaBancariaRemovida(
                conta.Id,
                conta.ClienteId,
                conta.BankCode);

            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation("Conta bancária removida com sucesso: {ContaId}", conta.Id);

            return new ContaResponse
            {
                ContaId = conta.Id,
                Status = "DELETED",
                Message = "Conta bancária removida com sucesso"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover conta bancária {ContaId}", request.ContaId);
            return new ContaResponse
            {
                Status = "ERROR",
                Message = "Erro interno ao remover conta bancária"
            };
        }
    }
}
