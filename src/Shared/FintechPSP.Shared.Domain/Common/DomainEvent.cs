using System;

namespace FintechPSP.Shared.Domain.Common;

/// <summary>
/// Classe base para eventos de domínio
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    protected DomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        Version = 1;
        EventType = GetType().Name;
    }

    public Guid EventId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public virtual int Version { get; protected set; }
    public string EventType { get; private set; }
}
