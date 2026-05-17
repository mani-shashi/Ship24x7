using MediatR;

namespace Ship24X7.Shipment.Application.Commands;

/// <summary>
/// Updates the current hub location of a shipment.
/// Called by hub operators when a package arrives at or departs from a hub.
/// Validates the hub exists and is active, updates CurrentHubId on the shipment,
/// adjusts hub CurrentLoad counters, and records an audit history entry.
/// </summary>
public class AssignHubCommand : IRequest<AssignHubResult>
{
    /// <summary>Shipment to update.</summary>
    public Guid ShipmentId { get; set; }

    /// <summary>Hub the shipment has arrived at.</summary>
    public Guid HubId { get; set; }

    /// <summary>ID of the hub operator performing the scan.</summary>
    public Guid OperatorId { get; set; }

    /// <summary>Optional note (e.g. "Arrived from Mumbai", "Sorted for last-mile").</summary>
    public string Reason { get; set; } = string.Empty;
}

public class AssignHubResult
{
    public bool Success { get; set; }
    public string HubName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
