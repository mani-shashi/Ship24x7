namespace Ship24X7.Shipment.Application.DTOs;

/// <summary>
/// Read model for a hub returned to API consumers.
/// Excludes internal fields (audit columns, address lines) that are not needed by the UI.
/// </summary>
public class HubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int CurrentLoad { get; set; }
    public bool IsActive { get; set; }
}
