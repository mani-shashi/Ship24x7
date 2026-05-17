using MediatR;
using Ship24X7.Notification.Application.DTOs;

namespace Ship24X7.Notification.Application.Queries;

/// <summary>
/// Query for retrieving getnotificationlog data. Defines query parameters and result type.
/// </summary>
public class GetNotificationLogQuery : IRequest<List<NotificationLogResponse>>
{
    /// <summary>
    /// Gets or sets the userId.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Gets or sets the pagenumber.
    /// </summary>
    public int PageNumber { get; set; } = 1;
    /// <summary>
    /// Gets or sets the pagesize.
    /// </summary>
    public int PageSize { get; set; } = 50;
}
