using Ship24X7.Shared.Domain;

namespace Ship24X7.Auth.Domain.Entities;

/// <summary>
/// Represents a saved address in a user's address book.
/// Stores shipping/contact addresses that can be reused across shipments.
/// </summary>
public class UserAddress : BaseEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this address.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the user ID this address belongs to.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets a friendly label for the address (e.g. "Home", "Office").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full name of the contact at this address.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number of the contact at this address.
    /// </summary>
    public string ContactPhone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first line of the street address.
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional second line of the street address.
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal/ZIP code.
    /// </summary>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country code (e.g. "IN", "US").
    /// </summary>
    public string Country { get; set; } = "IN";

    /// <summary>
    /// Gets or sets the address type tag (Home, Office, Warehouse, Other).
    /// </summary>
    public string Type { get; set; } = "Other";

    /// <summary>
    /// Gets or sets whether this is the user's default address.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Navigation property back to the owning user.
    /// </summary>
    public User User { get; set; } = null!;
}
