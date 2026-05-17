using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Infrastructure.Services;

/// <summary>
/// Shipment state machine for validating status transitions and enforcing business rules.
/// Defines valid state transitions to prevent invalid status changes and maintain data integrity.
/// Implements finite state machine pattern for shipment lifecycle management.
/// </summary>
public class ShipmentStateMachine : IShipmentStateMachine
{
    /// <summary>
    /// Dictionary defining valid status transitions for each shipment state.
    /// Key: Current status, Value: List of allowed next statuses.
    /// Terminal states (Delivered, Cancelled, Returned) have empty transition lists.
    /// </summary>
    private static readonly Dictionary<ShipmentStatus, List<ShipmentStatus>> _validTransitions = new()
    {
        {
            ShipmentStatus.Draft, new List<ShipmentStatus>
            {
                ShipmentStatus.Booked,
                ShipmentStatus.Cancelled
            }
        },
        {
            ShipmentStatus.Booked, new List<ShipmentStatus>
            {
                ShipmentStatus.PaymentPending,
                ShipmentStatus.Paid,
                ShipmentStatus.Cancelled
            }
        },
        {
            ShipmentStatus.PaymentPending, new List<ShipmentStatus>
            {
                ShipmentStatus.Paid,
                ShipmentStatus.PaymentFailed,
                ShipmentStatus.Cancelled
            }
        },
        {
            ShipmentStatus.Paid, new List<ShipmentStatus>
            {
                ShipmentStatus.PickedUp,
                ShipmentStatus.Cancelled
            }
        },
        {
            ShipmentStatus.PickedUp, new List<ShipmentStatus>
            {
                ShipmentStatus.InTransit,
                ShipmentStatus.Delayed,
                ShipmentStatus.Failed,
                ShipmentStatus.Returned
            }
        },
        {
            ShipmentStatus.InTransit, new List<ShipmentStatus>
            {
                ShipmentStatus.OutForDelivery,
                ShipmentStatus.Delayed,
                ShipmentStatus.Failed,
                ShipmentStatus.Returned
            }
        },
        {
            ShipmentStatus.OutForDelivery, new List<ShipmentStatus>
            {
                ShipmentStatus.Delivered,
                ShipmentStatus.Delayed,
                ShipmentStatus.Failed,
                ShipmentStatus.Returned
            }
        },
        {
            ShipmentStatus.Delayed, new List<ShipmentStatus>
            {
                ShipmentStatus.InTransit,
                ShipmentStatus.OutForDelivery,
                ShipmentStatus.Failed,
                ShipmentStatus.Returned
            }
        },
        {
            ShipmentStatus.PaymentFailed, new List<ShipmentStatus>
            {
                ShipmentStatus.PaymentPending,
                ShipmentStatus.Cancelled
            }
        },
        {
            ShipmentStatus.Delivered, new List<ShipmentStatus>() // Terminal state
        },
        {
            ShipmentStatus.Cancelled, new List<ShipmentStatus>() // Terminal state
        },
        {
            ShipmentStatus.Failed, new List<ShipmentStatus>
            {
                ShipmentStatus.Returned
            }
        },
        {
            ShipmentStatus.Returned, new List<ShipmentStatus>() // Terminal state
        }
    };

    /// <summary>
    /// Validates whether a status transition is allowed by business rules.
    /// Checks if the new status is in the list of valid transitions for the current status.
    /// </summary>
    /// <param name="currentStatus">The current shipment status.</param>
    /// <param name="newStatus">The desired new shipment status.</param>
    /// <returns>True if transition is valid; false if transition is not allowed.</returns>
    public bool CanTransition(ShipmentStatus currentStatus, ShipmentStatus newStatus)
    {
        if (!_validTransitions.ContainsKey(currentStatus))
            return false;

        return _validTransitions[currentStatus].Contains(newStatus);
    }

    /// <summary>
    /// Gets the list of valid next statuses for a given current status.
    /// Used to display available actions and prevent invalid state changes in UI.
    /// </summary>
    /// <param name="currentStatus">The current shipment status.</param>
    /// <returns>List of valid next statuses. Empty list for terminal states or unknown statuses.</returns>
    public List<ShipmentStatus> GetValidTransitions(ShipmentStatus currentStatus)
    {
        if (!_validTransitions.ContainsKey(currentStatus))
            return new List<ShipmentStatus>();

        return _validTransitions[currentStatus];
    }
}
