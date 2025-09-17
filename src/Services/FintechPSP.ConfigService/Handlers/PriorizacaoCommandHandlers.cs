using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FintechPSP.ConfigService.Commands;
using FintechPSP.ConfigService.DTOs;
using FintechPSP.ConfigService.Models;
using FintechPSP.ConfigService.Repositories;
using FintechPSP.Shared.Domain.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FintechPSP.ConfigService.Handlers;

/// <summary>
/// Handler para comando de configurar priorização
/// </summary>
public class ConfigurarPriorizacaoCommandHandler : IRequestHandler<ConfigurarPriorizacaoCommand, PriorizacaoResponse>
{
    private readonly IPriorizacaoRepository _priorizacaoRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ConfigurarPriorizacaoCommandHandler> _logger;

    public ConfigurarPriorizacaoCommandHandler(
        IPriorizacaoRepository priorizacaoRepository,
        IPublishEndpoint publishEndpoint,
        ILogger<ConfigurarPriorizacaoCommandHandler> logger)
    {
        _priorizacaoRepository = priorizacaoRepository ?? throw new ArgumentNullException(nameof(priorizacaoRepository));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PriorizacaoResponse> Handle(ConfigurarPriorizacaoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar percentuais
            var validationErrors = ValidatePercentuais(request.Prioridades);
            if (validationErrors.Any())
            {
                return new PriorizacaoResponse
                {
                    Status = "VALIDATION_ERROR",
                    Message = "Erro de validação nos percentuais",
                    ValidationErrors = validationErrors
                };
            }

            // Converter para modelo de domínio
            var prioridades = request.Prioridades.Select(p => new PrioridadeConta
            {
                ContaId = p.ContaId,
                BankCode = p.BankCode,
                Percentual = p.Percentual
            }).ToList();

            var totalPercentual = prioridades.Sum(p => p.Percentual);
            var isValid = Math.Abs(totalPercentual - 100m) < 0.01m; // Tolerância para arredondamento

            var configuracao = new ConfiguracaoPriorizacao
            {
                ClienteId = request.ClienteId,
                Prioridades = prioridades,
                TotalPercentual = totalPercentual,
                IsValid = isValid
            };

            // Salvar configuração
            var configuracaoSalva = await _priorizacaoRepository.UpsertAsync(configuracao);

            // Publicar evento
            var eventoPrioridades = prioridades.Select(p => new PrioridadeConta(p.ContaId, p.BankCode, p.Percentual)).ToList();
            var evento = new PriorizacaoAtualizada(
                configuracaoSalva.ClienteId,
                configuracaoSalva.Id,
                eventoPrioridades);

            await _publishEndpoint.Publish(evento, cancellationToken);

            _logger.LogInformation("Priorização configurada com sucesso para cliente {ClienteId}", request.ClienteId);

            return new PriorizacaoResponse
            {
                ConfigId = configuracaoSalva.Id,
                Status = isValid ? "CONFIGURED" : "CONFIGURED_WITH_WARNING",
                Message = isValid ? "Priorização configurada com sucesso" : "Priorização configurada, mas total não soma 100%",
                ValidationErrors = isValid ? null : new List<string> { $"Total dos percentuais: {totalPercentual:F2}% (esperado: 100%)" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar priorização para cliente {ClienteId}", request.ClienteId);
            return new PriorizacaoResponse
            {
                Status = "ERROR",
                Message = "Erro interno ao configurar priorização"
            };
        }
    }

    private static List<string> ValidatePercentuais(List<PrioridadeContaRequest> prioridades)
    {
        var errors = new List<string>();

        if (!prioridades.Any())
        {
            errors.Add("Lista de prioridades não pode estar vazia");
            return errors;
        }

        // Validar percentuais individuais
        foreach (var prioridade in prioridades)
        {
            if (prioridade.Percentual <= 0 || prioridade.Percentual > 100)
            {
                errors.Add($"Percentual da conta {prioridade.ContaId} deve estar entre 0.01 e 100");
            }
        }

        // Validar duplicatas de conta
        var contasDuplicadas = prioridades
            .GroupBy(p => p.ContaId)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        foreach (var contaId in contasDuplicadas)
        {
            errors.Add($"Conta {contaId} aparece mais de uma vez na lista");
        }

        // Validar total (com tolerância)
        var total = prioridades.Sum(p => p.Percentual);
        if (total > 100.01m)
        {
            errors.Add($"Soma dos percentuais ({total:F2}%) não pode exceder 100%");
        }

        return errors;
    }
}
