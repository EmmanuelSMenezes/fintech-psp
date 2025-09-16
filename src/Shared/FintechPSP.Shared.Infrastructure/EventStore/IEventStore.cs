using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Common;

namespace FintechPSP.Shared.Infrastructure.EventStore;

/// <summary>
/// Interface para Event Store
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Salva eventos de um agregado
    /// </summary>
    Task SaveEventsAsync<T>(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion) 
        where T : AggregateRoot;

    /// <summary>
    /// Carrega eventos de um agregado
    /// </summary>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId);

    /// <summary>
    /// Carrega eventos de um agregado a partir de uma versão específica
    /// </summary>
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion);
}
