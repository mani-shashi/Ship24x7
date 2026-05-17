using MediatR;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Command for updateshipmentstatus operation. Encapsulates request data and validation rules.
/// Used by the admin override endpoint and the PaymentEventConsumer.
/// For operational transitions prefer the specialized commands:
///   AssignHubCommand, InitiateTransitCommand, MarkOutForDeliveryCommand, CompleteDeliveryCommand.
/// </summary>
public class UpdateShipmentStatusCommand : IRequest<bool>
{
    /// <summary>Gets or sets the shipmentId.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>Gets or sets the New status.</summary>
    public ShipmentStatus NewStatus { get; set; }

    /// <summary>Gets or sets the reason.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// ID of the user or system actor triggering the change.
    /// Written to ShipmentStatusHistory.ChangedBy.
    /// Defaults to Guid.Empty for system-generated transitions (e.g. PaymentEventConsumer).
    /// </summary>
    public Guid ChangedBy { get; set; } = Guid.Empty;
}
