namespace Ship24X7.Shared.Domain;

/// <summary>
/// Base class for all domain events in the Ship24X7 platform.
/// Domain events represent significant business occurrences that other parts of the system may need to react to.
/// Implements event sourcing pattern with automatic event identification and timestamping.
/// </summary>
public abstract class BaseDomainEvent : IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this domain event.
    /// Automatically generated when the event is created.
    /// </summary>
    public Guid EventId { get; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets the UTC timestamp when this domain event occurred.
    /// Automatically set to the current UTC time when the event is created.
    /// </summary>
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the correlation identifier for distributed tracing.
    /// Links this event to the originating request across all microservices.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
}
