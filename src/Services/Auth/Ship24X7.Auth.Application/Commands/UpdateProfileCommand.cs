using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to update the authenticated user's profile (name, phone, photo URL).
/// Returns the updated UserResponse.
/// </summary>
public class UpdateProfileCommand : IRequest<UserResponse>
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? ProfilePhotoUrl { get; set; }
}
