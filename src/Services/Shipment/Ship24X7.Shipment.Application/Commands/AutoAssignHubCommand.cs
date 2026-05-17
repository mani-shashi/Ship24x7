using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Triggered automatically when a shipment reaches PickedUp status.
/// Finds the most suitable hub based on:
///   1. Proximity to the sender's postal code (city match first, then state)
///   2. Hub capacity (CurrentLoad &lt; Capacity)
///   3. Hub is active
/// Assigns CurrentHubId on the shipment and increments hub CurrentLoad.
/// Falls back gracefully — if no hub is found the shipment continues without
/// a hub assignment and an operator can assign manually via PUT /hub.
/// </summary>
public class AutoAssignHubCommand : IRequest<AutoAssignHubResult>
{
    public Guid ShipmentId { get; set; }

    /// <summary>Sender's city — used for proximity matching.</summary>
    public string SenderCity { get; set; } = string.Empty;

    /// <summary>Sender's state — fallback if no city match found.</summary>
    public string SenderState { get; set; } = string.Empty;
}

public class AutoAssignHubResult
{
    public bool Assigned { get; set; }
    public Guid? HubId { get; set; }
    public string HubName { get; set; } = string.Empty;
}
