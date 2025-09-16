using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FintechPSP.Shared.Domain.Common;
using Marten;

namespace FintechPSP.Shared.Infrastructure.EventStore;

/// <summary>
/// Implementação do Event Store usando Marten
/// </summary>
public class MartenEventStore : IEventStore
{
    private readonly IDocumentStore _documentStore;

    public MartenEventStore(IDocumentStore documentStore)
    {
        _documentStore = documentStore ?? throw new ArgumentNullException(nameof(documentStore));
    }

    public async Task SaveEventsAsync<T>(Guid aggregateId, IEnumerable<IDomainEvent> events, int expectedVersion) 
        where T : AggregateRoot
    {
        await using var session = _documentStore.LightweightSession();
        
        var eventsList = events.ToList();
        if (!eventsList.Any()) return;

        // Verifica a versão esperada para controle de concorrência
        var existingEvents = await session.Events.FetchStreamAsync(aggregateId);
        if (existingEvents.Count != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Concurrency conflict. Expected version {expectedVersion}, but found {existingEvents.Count}");
        }

        // Adiciona os eventos ao stream
        session.Events.Append(aggregateId, eventsList.ToArray());
        
        await session.SaveChangesAsync();
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId)
    {
        await using var session = _documentStore.QuerySession();
        
        var events = await session.Events.FetchStreamAsync(aggregateId);
        return events.Select(e => e.Data).Cast<IDomainEvent>();
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, int fromVersion)
    {
        await using var session = _documentStore.QuerySession();
        
        var events = await session.Events.FetchStreamAsync(aggregateId, fromVersion);
        return events.Select(e => e.Data).Cast<IDomainEvent>();
    }
}
