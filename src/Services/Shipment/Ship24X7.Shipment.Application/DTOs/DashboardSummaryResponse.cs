namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Data transfer object for DashboardSummary data. Used for API responses and data serialization.
/// </summary>
public class DashboardSummaryResponse
{
    /// <summary>
    /// Gets or sets the TodayBooking count.
    /// </summary>
    public int TodayBookingCount { get; set; }
    /// <summary>
    /// Gets or sets the PendingPickup count.
    /// </summary>
    public int PendingPickupCount { get; set; }
    /// <summary>
    /// Gets or sets the InTransit count.
    /// </summary>
    public int InTransitCount { get; set; }
    /// <summary>
    /// Gets or sets the OutForDelivery count.
    /// </summary>
    public int OutForDeliveryCount { get; set; }
    /// <summary>
    /// Gets or sets the DeliveredToday count.
    /// </summary>
    public int DeliveredTodayCount { get; set; }
}
