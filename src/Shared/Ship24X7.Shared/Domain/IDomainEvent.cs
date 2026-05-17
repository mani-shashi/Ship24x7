using MediatR;

namespace Ship24X7.Shared.Domain;

/// <summary>
/// Defines the contract for domain events in the Ship24X7 platform.
/// Domain events represent significant business occurrences that trigger side effects or notifications.
/// Extends MediatR's INotification to enable in-process event publishing and handling.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Gets the UTC timestamp when this domain event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Gets the correlation identifier for distributed tracing across services.
    /// </summary>
    string CorrelationId { get; }
}
