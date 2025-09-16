using System;
using System.Collections.Generic;
using System.Linq;

namespace FintechPSP.Shared.Domain.Common;

/// <summary>
/// Classe base para agregados que suportam Event Sourcing
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = new();
    
    protected AggregateRoot()
    {
        Id = Guid.NewGuid();
        Version = 0;
    }

    protected AggregateRoot(Guid id)
    {
        Id = id;
        Version = 0;
    }

    /// <summary>
    /// Identificador único do agregado
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Versão atual do agregado (número de eventos aplicados)
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// Eventos não commitados ainda
    /// </summary>
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    /// <summary>
    /// Aplica um evento ao agregado
    /// </summary>
    protected void ApplyEvent(IDomainEvent domainEvent)
    {
        ApplyEventInternal(domainEvent);
        _uncommittedEvents.Add(domainEvent);
    }

    /// <summary>
    /// Reconstrói o agregado a partir de eventos históricos
    /// </summary>
    public void LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        foreach (var domainEvent in events)
        {
            ApplyEventInternal(domainEvent);
        }
    }

    /// <summary>
    /// Marca todos os eventos como commitados
    /// </summary>
    public void MarkEventsAsCommitted()
    {
        _uncommittedEvents.Clear();
    }

    /// <summary>
    /// Aplica o evento internamente (sem adicionar à lista de não commitados)
    /// </summary>
    private void ApplyEventInternal(IDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var method = GetType().GetMethod("Apply", new[] { eventType });
        
        if (method == null)
        {
            throw new InvalidOperationException($"No Apply method found for event type {eventType.Name}");
        }

        method.Invoke(this, new object[] { domainEvent });
        Version++;
    }
}
