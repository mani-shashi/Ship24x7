using System.Security.Cryptography;
using System.Text;
using Ship24X7.Shipment.Domain.Entities;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Domain.Events;

namespace Ship24X7.Shipment.Domain.Aggregates;

/// <summary>
/// Shipment aggregate root. Maintains the consistency boundary for
/// Shipment + ShipmentItems + Pickup + ShipmentStatusHistory.
/// All state mutations go through this class — never directly on the entity.
/// </summary>
public class ShipmentAggregate
{
    private readonly Entities.Shipment _shipment;
    private readonly List<ShipmentItem> _items;
    private Pickup? _pickup;
    private readonly List<ShipmentStatusHistory> _statusHistory;
    private readonly List<object> _domainEvents = new();

    public ShipmentAggregate(Entities.Shipment shipment)
    {
        _shipment = shipment ?? throw new ArgumentNullException(nameof(shipment));
        _items = shipment.Items?.ToList() ?? new List<ShipmentItem>();
        _pickup = shipment.Pickup;
        _statusHistory = shipment.StatusHistory?.ToList() ?? new List<ShipmentStatusHistory>();
    }

    public Entities.Shipment Shipment => _shipment;
    public IReadOnlyList<ShipmentItem> Items => _items.AsReadOnly();
    public Pickup? Pickup => _pickup;
    public IReadOnlyList<ShipmentStatusHistory> StatusHistory => _statusHistory.AsReadOnly();
    public IReadOnlyList<object> DomainEvents => _domainEvents.AsReadOnly();

    // ── Item management ───────────────────────────────────────────────────────

    public void AddItem(ShipmentItem item)
    {
        if (_shipment.Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Cannot add items to a shipment that is not in Draft status");
        _items.Add(item);
    }

    public void RemoveItem(Guid itemId)
    {
        if (_shipment.Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Cannot remove items from a shipment that is not in Draft status");
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        if (item != null) _items.Remove(item);
    }

    // ── Booking ───────────────────────────────────────────────────────────────

    public void ConfirmBooking()
    {
        if (_shipment.Status != ShipmentStatus.Draft)
            throw new InvalidOperationException("Only Draft shipments can be confirmed");
        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm shipment without items");

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.Booked;

        AppendHistory(oldStatus, ShipmentStatus.Booked, _shipment.CustomerId, "Booking confirmed by customer");

        _domainEvents.Add(new ShipmentBooked
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            CustomerId = _shipment.CustomerId,
            TotalCost = _shipment.TotalCost,
            Currency = _shipment.Currency,
            EstimatedDeliveryDate = _shipment.EstimatedDeliveryDate,
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Cancellation ──────────────────────────────────────────────────────────

    public void CancelShipment(string reason)
    {
        if (_shipment.Status == ShipmentStatus.Delivered || _shipment.Status == ShipmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel shipment in {_shipment.Status} status");

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.Cancelled;

        AppendHistory(oldStatus, ShipmentStatus.Cancelled, _shipment.CustomerId, reason);

        _domainEvents.Add(new ShipmentCancelled
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            CustomerId = _shipment.CustomerId,
            CancellationReason = reason,
            CorrelationId = _shipment.CorrelationId
        });

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = ShipmentStatus.Cancelled,
            Reason = reason,
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Pickup ────────────────────────────────────────────────────────────────

    public void SchedulePickup(Pickup pickup)
    {
        if (_shipment.Status != ShipmentStatus.Booked && _shipment.Status != ShipmentStatus.Paid)
            throw new InvalidOperationException("Can only schedule pickup for Booked or Paid shipments");
        if (_pickup != null && _pickup.Status == PickupStatus.Scheduled)
            throw new InvalidOperationException("Pickup is already scheduled for this shipment");

        _pickup = pickup;

        _domainEvents.Add(new PickupScheduled
        {
            PickupId = pickup.Id,
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            ConfirmationNumber = pickup.ConfirmationNumber,
            PickupDate = pickup.PickupDate,
            TimeSlot = pickup.TimeSlot,
            CorrelationId = _shipment.CorrelationId
        });
    }

    public void CompletePickup(Guid? driverId)
    {
        if (_pickup == null)
            throw new InvalidOperationException("No pickup scheduled for this shipment");
        if (_pickup.Status != PickupStatus.Scheduled && _pickup.Status != PickupStatus.EnRoute)
            throw new InvalidOperationException($"Cannot complete pickup in {_pickup.Status} status");

        _pickup.Status = PickupStatus.Completed;
        _pickup.CompletedAt = DateTime.UtcNow;
        _pickup.AssignedDriverId = driverId;

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.PickedUp;

        AppendHistory(oldStatus, ShipmentStatus.PickedUp,
            driverId ?? _shipment.CustomerId, "Pickup completed by driver");

        _domainEvents.Add(new PickupCompleted
        {
            PickupId = _pickup.Id,
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            CompletedAt = _pickup.CompletedAt.Value,
            DriverId = driverId,
            CorrelationId = _shipment.CorrelationId
        });

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = ShipmentStatus.PickedUp,
            Reason = "Pickup completed",
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Hub assignment ────────────────────────────────────────────────────────

    /// <summary>
    /// Updates the shipment's current hub location.
    /// Called when a hub operator scans the package on arrival or departure.
    /// Also updates Hub.CurrentLoad via the caller (handler decrements old hub, increments new hub).
    /// </summary>
    public void AssignToHub(Guid hubId, string hubName, Guid operatorId, string reason)
    {
        if (_shipment.Status == ShipmentStatus.Draft
            || _shipment.Status == ShipmentStatus.Booked
            || _shipment.Status == ShipmentStatus.Delivered
            || _shipment.Status == ShipmentStatus.Cancelled
            || _shipment.Status == ShipmentStatus.Returned)
        {
            throw new InvalidOperationException(
                $"Cannot assign hub for shipment in {_shipment.Status} status");
        }

        _shipment.CurrentHubId = hubId;
        _shipment.UpdatedAt = DateTime.UtcNow;
        _shipment.UpdatedBy = operatorId;

        // Record in history (no status change, but still auditable)
        _statusHistory.Add(new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipment.Id,
            FromStatus = _shipment.Status,
            ToStatus = _shipment.Status,
            ChangedBy = operatorId,
            Reason = $"Hub assignment: {hubName}. {reason}",
            ChangedAt = DateTime.UtcNow,
            HubId = hubId,
            CorrelationId = _shipment.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = operatorId
        });

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = _shipment.Status,
            NewStatus = _shipment.Status,
            Reason = $"Arrived at hub: {hubName}",
            Location = hubName,
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Specialized transit commands ──────────────────────────────────────────

    /// <summary>
    /// Transitions PickedUp → InTransit.
    /// Requires CurrentHubId to be set (package must be physically at a hub).
    /// </summary>
    public void InitiateTransit(Guid hubId, string hubName, Guid operatorId)
    {
        if (_shipment.Status != ShipmentStatus.PickedUp)
            throw new InvalidOperationException("Only PickedUp shipments can be moved to InTransit");
        if (_shipment.CurrentHubId == null)
            throw new InvalidOperationException(
                "Shipment must be assigned to a hub before initiating transit. " +
                "Call PUT /shipment/{id}/hub first.");

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.InTransit;
        _shipment.UpdatedAt = DateTime.UtcNow;
        _shipment.UpdatedBy = operatorId;

        AppendHistory(oldStatus, ShipmentStatus.InTransit, operatorId,
            $"Departed hub: {hubName}", hubId);

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = ShipmentStatus.InTransit,
            Reason = $"Departed hub: {hubName}",
            Location = hubName,
            CorrelationId = _shipment.CorrelationId
        });
    }

    /// <summary>
    /// Transitions InTransit → OutForDelivery.
    /// Assigns a delivery agent and generates a 6-digit OTP for delivery verification.
    /// The raw OTP is returned once (to be sent via Notification Service) and never stored.
    /// Only the SHA-256 hash is persisted on the shipment.
    /// </summary>
    /// <returns>The raw 6-digit OTP to be dispatched to the customer.</returns>
    public string MarkOutForDelivery(string deliveryAgentId, Guid operatorId, string hubName)
    {
        if (_shipment.Status != ShipmentStatus.InTransit && _shipment.Status != ShipmentStatus.Delayed)
            throw new InvalidOperationException(
                "Only InTransit or Delayed shipments can be marked OutForDelivery");

        // Generate OTP
        var rawOtp = GenerateOtp();
        _shipment.DeliveryOtpHash = HashOtp(rawOtp);
        _shipment.OtpExpiresAt = DateTime.UtcNow.AddHours(24);
        _shipment.DeliveryAgentId = deliveryAgentId;

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.OutForDelivery;
        _shipment.UpdatedAt = DateTime.UtcNow;
        _shipment.UpdatedBy = operatorId;

        AppendHistory(oldStatus, ShipmentStatus.OutForDelivery, operatorId,
            $"Out for delivery. Agent: {deliveryAgentId}. Hub: {hubName}",
            _shipment.CurrentHubId);

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = ShipmentStatus.OutForDelivery,
            Reason = $"Out for delivery. Agent: {deliveryAgentId}",
            Location = hubName,
            CorrelationId = _shipment.CorrelationId
        });

        // Return raw OTP — caller must send it to the customer via Notification Service
        return rawOtp;
    }

    /// <summary>
    /// Transitions OutForDelivery → Delivered.
    /// Validates the OTP provided by the delivery agent against the stored hash.
    /// Falls back to supervisor override when otpOverride is true (requires elevated role check in handler).
    /// </summary>
    public void CompleteDelivery(string providedOtp, Guid operatorId, bool supervisorOverride = false)
    {
        if (_shipment.Status != ShipmentStatus.OutForDelivery)
            throw new InvalidOperationException("Only OutForDelivery shipments can be marked Delivered");

        if (!supervisorOverride)
        {
            if (string.IsNullOrWhiteSpace(_shipment.DeliveryOtpHash))
                throw new InvalidOperationException("No OTP was generated for this shipment");

            if (_shipment.OtpExpiresAt.HasValue && DateTime.UtcNow > _shipment.OtpExpiresAt.Value)
                throw new InvalidOperationException(
                    "Delivery OTP has expired. Request a supervisor override or regenerate.");

            if (!VerifyOtp(providedOtp, _shipment.DeliveryOtpHash))
                throw new InvalidOperationException("Invalid delivery OTP. Delivery cannot be confirmed.");
        }

        var oldStatus = _shipment.Status;
        _shipment.Status = ShipmentStatus.Delivered;
        _shipment.ActualDeliveryDate = DateTime.UtcNow;
        // Clear OTP fields after successful delivery
        _shipment.DeliveryOtpHash = null;
        _shipment.OtpExpiresAt = null;
        _shipment.UpdatedAt = DateTime.UtcNow;
        _shipment.UpdatedBy = operatorId;

        var reason = supervisorOverride
            ? $"Delivered with supervisor override by {operatorId}"
            : "Delivered — OTP verified";

        AppendHistory(oldStatus, ShipmentStatus.Delivered, operatorId, reason);

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = ShipmentStatus.Delivered,
            Reason = reason,
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Generic status update (admin override / event-driven) ─────────────────

    /// <summary>
    /// Generic status update used by the admin override endpoint and the
    /// PaymentEventConsumer. Prefer specialized methods for operational transitions.
    /// </summary>
    public void UpdateStatus(ShipmentStatus newStatus, string reason, Guid? changedBy = null)
    {
        if (_shipment.Status == newStatus) return;

        ValidateStatusTransition(_shipment.Status, newStatus);

        var oldStatus = _shipment.Status;
        _shipment.Status = newStatus;

        if (newStatus == ShipmentStatus.Delivered)
            _shipment.ActualDeliveryDate = DateTime.UtcNow;

        _shipment.UpdatedAt = DateTime.UtcNow;
        if (changedBy.HasValue) _shipment.UpdatedBy = changedBy;

        AppendHistory(oldStatus, newStatus, changedBy ?? _shipment.CustomerId, reason);

        _domainEvents.Add(new ShipmentStatusChanged
        {
            ShipmentId = _shipment.Id,
            TrackingNumber = _shipment.TrackingNumber,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            Reason = reason,
            CorrelationId = _shipment.CorrelationId
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void AppendHistory(
        ShipmentStatus from, ShipmentStatus to,
        Guid changedBy, string reason, Guid? hubId = null)
    {
        _statusHistory.Add(new ShipmentStatusHistory
        {
            Id = Guid.NewGuid(),
            ShipmentId = _shipment.Id,
            FromStatus = from,
            ToStatus = to,
            ChangedBy = changedBy,
            Reason = reason,
            ChangedAt = DateTime.UtcNow,
            HubId = hubId,
            CorrelationId = _shipment.CorrelationId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy
        });
    }

    private static string GenerateOtp()
    {
        // Cryptographically random 6-digit OTP
        var bytes = new byte[4];
        RandomNumberGenerator.Fill(bytes);
        var value = Math.Abs(BitConverter.ToInt32(bytes, 0)) % 1_000_000;
        return value.ToString("D6");
    }

    private static string HashOtp(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }

    private static bool VerifyOtp(string provided, string storedHash)
    {
        var providedHash = HashOtp(provided.Trim());
        // Constant-time comparison to prevent timing attacks
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedHash),
            Encoding.UTF8.GetBytes(storedHash));
    }

    private static void ValidateStatusTransition(ShipmentStatus currentStatus, ShipmentStatus newStatus)
    {
        var validTransitions = new Dictionary<ShipmentStatus, List<ShipmentStatus>>
        {
            { ShipmentStatus.Draft,          [ShipmentStatus.Booked, ShipmentStatus.Cancelled] },
            { ShipmentStatus.Booked,         [ShipmentStatus.PaymentPending, ShipmentStatus.Paid, ShipmentStatus.Cancelled] },
            { ShipmentStatus.PaymentPending, [ShipmentStatus.Paid, ShipmentStatus.PaymentFailed, ShipmentStatus.Cancelled] },
            { ShipmentStatus.Paid,           [ShipmentStatus.PickedUp, ShipmentStatus.Cancelled] },
            { ShipmentStatus.PickedUp,       [ShipmentStatus.InTransit, ShipmentStatus.Failed, ShipmentStatus.Cancelled] },
            { ShipmentStatus.InTransit,      [ShipmentStatus.OutForDelivery, ShipmentStatus.Delayed, ShipmentStatus.Failed] },
            { ShipmentStatus.OutForDelivery, [ShipmentStatus.Delivered, ShipmentStatus.Failed, ShipmentStatus.Delayed] },
            { ShipmentStatus.Delayed,        [ShipmentStatus.InTransit, ShipmentStatus.OutForDelivery, ShipmentStatus.Failed] },
            { ShipmentStatus.Failed,         [ShipmentStatus.Returned, ShipmentStatus.InTransit] },
            { ShipmentStatus.PaymentFailed,  [ShipmentStatus.Paid, ShipmentStatus.Cancelled] },
            { ShipmentStatus.Delivered,      [] },
            { ShipmentStatus.Cancelled,      [] },
            { ShipmentStatus.Returned,       [] },
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowed))
            throw new InvalidOperationException($"No valid transitions defined for status {currentStatus}");

        if (!allowed.Contains(newStatus))
            throw new InvalidOperationException($"Cannot transition from {currentStatus} to {newStatus}");
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
