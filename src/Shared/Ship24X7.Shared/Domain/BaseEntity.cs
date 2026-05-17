namespace Ship24X7.Shared.Domain;

/// <summary>
/// Base class for all domain entities in the Ship24X7 platform.
/// Provides common audit fields for tracking entity lifecycle (creation, updates, soft deletion) and correlation for distributed tracing.
/// Implements soft delete pattern to preserve data history.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets or sets the correlation identifier for distributed tracing across services.
    /// Used to track requests and events across the entire microservices architecture.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was created.
    /// Automatically set during entity creation and never modified.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who created this entity.
    /// References the User entity in the Auth service.
    /// </summary>
    public Guid CreatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was last updated.
    /// Null if the entity has never been modified after creation.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who last updated this entity.
    /// Null if the entity has never been modified after creation.
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether this entity has been soft deleted.
    /// Soft deleted entities are hidden from normal queries but preserved in the database for audit purposes.
    /// </summary>
    public bool IsDeleted { get; set; }
    
    /// <summary>
    /// Gets or sets the UTC timestamp when the entity was soft deleted.
    /// Null if the entity has not been deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who soft deleted this entity.
    /// Null if the entity has not been deleted.
    /// </summary>
    public Guid? DeletedBy { get; set; }
}
