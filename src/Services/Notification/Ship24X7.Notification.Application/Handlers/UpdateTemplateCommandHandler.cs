using MediatR;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Interfaces;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the UpdateTemplateCommand by updating an existing notification template's content and metadata.
/// Retrieves template from database, validates it exists, updates all fields, and persists changes.
/// Used by TemplateController when administrators modify templates through admin UI.
/// Allows updating template content, metadata, and active status without creating new template version.
/// Maintains template ID so existing notification logs retain reference to original template.
/// </summary>
public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, Unit>
{
    private readonly INotificationTemplateRepository _templateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateTemplateCommandHandler"/> class.
    /// Injects template repository for querying and persisting template data.
    /// </summary>
    /// <param name="templateRepository">Repository for retrieving and updating notification templates.</param>
    public UpdateTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// Updates an existing notification template with new content and metadata.
    /// Logic flow: 1) Queries database for existing template by ID,
    /// 2) Throws InvalidOperationException if template not found (results in 404 Not Found response),
    /// 3) Updates all template fields: name, type, channel, subject, body template, required placeholders, active status,
    /// 4) Sets UpdatedAt timestamp to current UTC time and UpdatedBy to admin user ID from command,
    /// 5) Persists updated template to database via UpdateAsync,
    /// 6) Returns Unit.Value indicating successful update without returning data.
    /// Controller validates placeholder existence in body before calling handler.
    /// </summary>
    /// <param name="request">Update template command containing template ID, updated content, metadata, and admin user ID.</param>
    /// <param name="cancellationToken">Cancellation token to abort operation if request is cancelled.</param>
    /// <returns>
    /// Unit.Value indicating successful update of template.
    /// No data returned as command is fire-and-forget operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown when template with specified ID is not found in database.</exception>
    public async Task<Unit> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        // Query database for existing template - throws if not found
        var template = await _templateRepository.GetByIdAsync(request.TemplateId, cancellationToken);
        if (template == null)
        {
            throw new InvalidOperationException($"Template with ID {request.TemplateId} not found");
        }

        // Update all template fields with values from command
        template.Name = request.Name;
        template.Type = request.Type;
        template.Channel = request.Channel;
        template.Subject = request.Subject;
        template.BodyTemplate = request.BodyTemplate;
        template.RequiredPlaceholders = request.RequiredPlaceholders;
        template.IsActive = request.IsActive;
        template.UpdatedAt = DateTime.UtcNow; // Set update timestamp
        template.UpdatedBy = request.UpdatedBy; // Track who updated template

        // Persist updated template to database
        await _templateRepository.UpdateAsync(template, cancellationToken);

        return Unit.Value; // Indicate successful completion
    }
}
