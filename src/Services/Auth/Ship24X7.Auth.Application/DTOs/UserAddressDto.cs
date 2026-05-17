namespace Ship24X7.Auth.Application.DTOs;

/// <summary>
/// DTO for a saved user address.
/// </summary>
public class UserAddressDto
{
    public Guid Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = "IN";
    public string Type { get; set; } = "Other";
    public bool IsDefault { get; set; }
}
