namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Read model for a single status history entry returned to API consumers.
/// </summary>
public class ShipmentStatusHistoryDto
{
    public Guid Id { get; set; }
    public Guid ShipmentId { get; set; }
    public string FromStatus { get; set; } = string.Empty;
    public string ToStatus { get; set; } = string.Empty;
    public Guid ChangedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; }
    public Guid? HubId { get; set; }
}
