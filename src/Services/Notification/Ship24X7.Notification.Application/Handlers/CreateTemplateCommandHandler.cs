using MediatR;
using Ship24X7.Notification.Application.Commands;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Domain.Entities;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the CreateTemplateCommand by creating a new notification template in the database.
/// Constructs NotificationTemplate entity from command data, sets initial state (active), and persists to database.
/// Used by TemplateController when administrators create new templates through admin UI.
/// Returns created template ID for immediate use in notification sending or further template management.
/// Controller validates placeholder existence in body before calling handler.
/// </summary>
public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly INotificationTemplateRepository _templateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTemplateCommandHandler"/> class.
    /// Injects template repository for persisting new template data to database.
    /// </summary>
    /// <param name="templateRepository">Repository for creating notification templates.</param>
    public CreateTemplateCommandHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// Creates a new notification template with content and metadata from command.
    /// Logic flow: 1) Constructs new NotificationTemplate entity with generated GUID,
    /// 2) Populates all fields from command: name, type, channel, subject, body template, required placeholders,
    /// 3) Sets IsActive to true (new templates are active by default),
    /// 4) Sets CreatedAt timestamp to current UTC time and CreatedBy to admin user ID from command,
    /// 5) Persists new template to database via AddAsync,
    /// 6) Returns template ID for use in API response and immediate notification sending.
    /// Controller validates that all required placeholders exist in body template before calling handler.
    /// </summary>
    /// <param name="request">Create template command containing template content, metadata, and admin user ID.</param>
    /// <param name="cancellationToken">Cancellation token to abort operation if request is cancelled.</param>
    /// <returns>
    /// The unique identifier (GUID) of the newly created template.
    /// Can be used immediately for sending notifications or further template management operations.
    /// </returns>
    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        // Construct new template entity with all fields from command
        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(), // Generate unique identifier
            Name = request.Name,
            Type = request.Type,
            Channel = request.Channel,
            Subject = request.Subject,
            BodyTemplate = request.BodyTemplate,
            RequiredPlaceholders = request.RequiredPlaceholders,
            IsActive = true, // New templates are active by default
            CreatedAt = DateTime.UtcNow, // Set creation timestamp
            CreatedBy = request.CreatedBy // Track who created template
        };

        // Persist new template to database
        await _templateRepository.AddAsync(template, cancellationToken);

        return template.Id; // Return template ID for API response
    }
}
