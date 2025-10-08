using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using MassTransit;

namespace FintechPSP.BalanceService.Handlers;

/// <summary>
/// Consumer de teste para ContaBancariaCriada - deve funcionar
/// </summary>
public class TestContaConsumer : IConsumer<ContaBancariaCriada>
{
    public async Task Consume(ConsumeContext<ContaBancariaCriada> context)
    {
        var evento = context.Message;
        
        // Log simples no console
        System.Console.WriteLine($"🎯 TEST CONTA Consumer - ContaId: {evento.ContaId}, Cliente: {evento.ClienteId}");
        
        await Task.CompletedTask;
    }
}
