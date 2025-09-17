using System;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FintechPSP.BalanceService.Consumers;

/// <summary>
/// Consumer para eventos de conta bancária
/// </summary>
public class ContaBancariaEventConsumer : 
    IConsumer<ContaBancariaCriada>,
    IConsumer<ContaBancariaAtualizada>,
    IConsumer<ContaBancariaRemovida>
{
    private readonly ILogger<ContaBancariaEventConsumer> _logger;

    public ContaBancariaEventConsumer(ILogger<ContaBancariaEventConsumer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Consume(ConsumeContext<ContaBancariaCriada> context)
    {
        try
        {
            var evento = context.Message;
            
            _logger.LogInformation("Conta bancária criada: {ContaId} para cliente {ClienteId} no banco {BankCode}", 
                evento.ContaId, evento.ClienteId, evento.BankCode);

            // Aqui você pode implementar lógica específica para quando uma conta é criada
            // Por exemplo, atualizar cache de contas, notificar outros serviços, etc.
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento ContaBancariaCriada");
            throw;
        }
    }

    public async Task Consume(ConsumeContext<ContaBancariaAtualizada> context)
    {
        try
        {
            var evento = context.Message;
            
            _logger.LogInformation("Conta bancária atualizada: {ContaId} para cliente {ClienteId}", 
                evento.ContaId, evento.ClienteId);

            // Aqui você pode implementar lógica específica para quando uma conta é atualizada
            // Por exemplo, invalidar cache, atualizar projeções, etc.
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento ContaBancariaAtualizada");
            throw;
        }
    }

    public async Task Consume(ConsumeContext<ContaBancariaRemovida> context)
    {
        try
        {
            var evento = context.Message;
            
            _logger.LogInformation("Conta bancária removida: {ContaId} para cliente {ClienteId} do banco {BankCode}", 
                evento.ContaId, evento.ClienteId, evento.BankCode);

            // Aqui você pode implementar lógica específica para quando uma conta é removida
            // Por exemplo, limpar cache, atualizar configurações de roteamento, etc.
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar evento ContaBancariaRemovida");
            throw;
        }
    }
}
