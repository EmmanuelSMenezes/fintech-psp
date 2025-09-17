using System;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FintechPSP.BalanceService.Consumers;

/// <summary>
/// Consumer para eventos de priorização
/// </summary>
public class PriorizacaoEventConsumer : IConsumer<PriorizacaoAtualizada>
{
    private readonly ILogger<PriorizacaoEventConsumer> _logger;

    public PriorizacaoEventConsumer(ILogger<PriorizacaoEventConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<PriorizacaoAtualizada> context)
    {
        try
        {
            var evento = context.Message;
            
            _logger.LogInformation("Priorização atualizada para cliente {ClienteId} com {Count} contas configuradas", 
                evento.ClienteId, evento.Prioridades.Count);

            // Aqui você pode implementar lógica específica para quando a priorização é atualizada
            // Por exemplo, atualizar cache de roteamento, recalcular distribuições, etc.
            
            foreach (var prioridade in evento.Prioridades)
            {
                _logger.LogDebug("Conta {ContaId} ({BankCode}): {Percentual}%", 
                    prioridade.ContaId, prioridade.BankCode, prioridade.Percentual);
            }
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento PriorizacaoAtualizada");
            throw;
        }
    }
}
