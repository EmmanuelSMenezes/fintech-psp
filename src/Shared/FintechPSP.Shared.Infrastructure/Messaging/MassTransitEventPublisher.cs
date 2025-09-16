using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Common;
using MassTransit;

namespace FintechPSP.Shared.Infrastructure.Messaging;

/// <summary>
/// Implementação do Event Publisher usando MassTransit
/// </summary>
public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
    }

    public async Task PublishAsync<T>(T @event) where T : class, IDomainEvent
    {
        await _publishEndpoint.Publish(@event);
    }

    public async Task PublishAsync<T>(params T[] events) where T : class, IDomainEvent
    {
        foreach (var @event in events)
        {
            await _publishEndpoint.Publish(@event);
        }
    }
}
