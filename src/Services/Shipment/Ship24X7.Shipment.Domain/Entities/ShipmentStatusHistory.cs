using Ship24X7.Shared.Domain;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Domain.Entities;

/// <summary>
/// Immutable audit record of every status transition a shipment goes through.
/// Append-only — rows are never updated or deleted.
/// Provides full accountability: who changed what, when, and why.
/// </summary>
public class ShipmentStatusHistory : BaseEntity
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; set; }

    /// <summary>The shipment this history row belongs to.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>Status before the transition.</summary>
    public ShipmentStatus FromStatus { get; set; }

    /// <summary>Status after the transition.</summary>
    public ShipmentStatus ToStatus { get; set; }

    /// <summary>
    /// ID of the user (admin, hub operator, system) who triggered the change.
    /// Matches CreatedBy from BaseEntity but kept explicit for query convenience.
    /// </summary>
    public Guid ChangedBy { get; set; }

    /// <summary>Human-readable reason or note for the transition.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the transition occurred.</summary>
    public DateTime ChangedAt { get; set; }

    /// <summary>
    /// Optional hub ID if the transition was triggered at a specific hub
    /// (e.g. InTransit scan, OutForDelivery assignment).
    /// </summary>
    public Guid? HubId { get; set; }

    /// <summary>Navigation property back to the parent shipment.</summary>
    public Shipment Shipment { get; set; } = null!;
}
