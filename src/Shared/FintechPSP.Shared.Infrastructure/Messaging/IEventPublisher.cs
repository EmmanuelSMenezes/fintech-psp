using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Infrastructure.Messaging;

/// <summary>
/// Interface para publicação de eventos
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publica um evento
    /// </summary>
    Task PublishAsync<T>(T @event) where T : class, IDomainEvent;

    /// <summary>
    /// Publica múltiplos eventos
    /// </summary>
    Task PublishAsync<T>(params T[] events) where T : class, IDomainEvent;
}
