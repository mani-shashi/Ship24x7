using MediatR;

namespace Ship24X7.Notification.Application.Commands;

/// <summary>
/// Command to delete a notification template by its unique identifier.
/// </summary>
public class DeleteTemplateCommand : IRequest<bool>
{
    public Guid TemplateId { get; set; }
}
