using MediatR;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Interfaces;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles DeleteTemplateCommand by removing a notification template from the database.
/// Throws KeyNotFoundException if the template does not exist.
/// </summary>
public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, bool>
{
    private readonly INotificationTemplateRepository _templateRepository;

    public DeleteTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<bool> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template == null)
            throw new KeyNotFoundException($"Notification template {request.TemplateId} not found");

        await _templateRepository.DeleteAsync(request.TemplateId, cancellationToken);
        return true;
    }
}
