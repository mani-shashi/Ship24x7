using MediatR;
using Ship24X7.Notification.Application.DTOs;

namespace Ship24X7.Notification.Application.Queries;

/// <summary>
/// Query for retrieving gettemplatelist data. Defines query parameters and result type.
/// </summary>
public class GetTemplateListQuery : IRequest<List<NotificationTemplateResponse>>
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is active.
    /// </summary>
    public bool? IsActive { get; set; }
}
