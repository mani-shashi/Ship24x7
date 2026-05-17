using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to create or update a saved address in the user's address book.
/// When AddressId is null a new address is created; otherwise the existing one is updated.
/// </summary>
public class SaveAddressCommand : IRequest<UserAddressDto>
{
    public Guid UserId { get; set; }

    /// <summary>Null for create, populated for update.</summary>
    public Guid? AddressId { get; set; }

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
