using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// IShipmentStateMachine implementation. Provides functionality for the application.
/// </summary>
public interface IShipmentStateMachine
{
    bool CanTransition(ShipmentStatus currentStatus, ShipmentStatus newStatus);
    List<ShipmentStatus> GetValidTransitions(ShipmentStatus currentStatus);
}
