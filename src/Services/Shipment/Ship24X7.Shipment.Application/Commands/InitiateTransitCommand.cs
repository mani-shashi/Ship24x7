using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Transitions a shipment from PickedUp → InTransit.
/// Enforces that CurrentHubId is set before allowing the transition —
/// the package must be physically scanned at a hub first.
/// </summary>
public class InitiateTransitCommand : IRequest<bool>
{
    /// <summary>Shipment to move to InTransit.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>Hub the shipment is departing from.</summary>
    public Guid HubId { get; set; }

    /// <summary>ID of the hub operator initiating transit.</summary>
    public Guid OperatorId { get; set; }

    /// <summary>Optional note.</summary>
    public string Reason { get; set; } = "Departed origin hub";
}
