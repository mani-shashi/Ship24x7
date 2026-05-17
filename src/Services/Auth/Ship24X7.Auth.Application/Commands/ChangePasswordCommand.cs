using MediatR;

namespace Ship24X7.Auth.Application.Commands;

/// <summary>
/// Command to change the authenticated user's password.
/// Requires the current password for verification before updating.
/// </summary>
public class ChangePasswordCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
