using MediatR;
using Ship24X7.Notification.Application.DTOs;

namespace Ship24X7.Notification.Application.Queries;

/// <summary>
/// Query for retrieving getuserpreference data. Defines query parameters and result type.
/// </summary>
public class GetUserPreferenceQuery : IRequest<NotificationPreferenceResponse?>
{
    /// <summary>
    /// Gets or sets the userId.
    /// </summary>
    public Guid UserId { get; set; }
}
