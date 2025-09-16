using System;

namespace FintechPSP.Shared.Domain.Common;

/// <summary>
/// Interface base para eventos de domínio
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único do evento
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp de quando o evento ocorreu
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Versão do evento para versionamento de schema
    /// </summary>
    int Version { get; }
    
    /// <summary>
    /// Tipo do evento
    /// </summary>
    string EventType { get; }
}
