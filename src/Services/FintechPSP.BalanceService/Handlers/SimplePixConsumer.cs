using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Events;
using MassTransit;

namespace FintechPSP.BalanceService.Handlers;

/// <summary>
/// Consumer simples para PixConfirmado - sem dependências
/// </summary>
public class SimplePixConsumer : IConsumer<PixConfirmado>
{
    public async Task Consume(ConsumeContext<PixConfirmado> context)
    {
        var evento = context.Message;
        
        // Log simples no console
        System.Console.WriteLine($"🎯 SIMPLE PIX Consumer - TxId: {evento.TxId}, Valor: R$ {evento.Amount}");
        
        await Task.CompletedTask;
    }
}
