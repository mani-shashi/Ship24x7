using MediatR;
using Ship24X7.Notification.Application.DTOs;
using Ship24X7.Notification.Application.Interfaces;
using Ship24X7.Notification.Application.Queries;

namespace Ship24X7.Notification.Application.Handlers;

/// <summary>
/// Handles the GetTemplateListQuery by retrieving all notification templates with optional filtering by active status.
/// Queries database for templates, applies IsActive filter if specified, and maps entities to response DTOs.
/// Used by administrators to view and manage available templates in the template management UI.
/// Returns templates ordered by name for consistent display and easy navigation.
/// </summary>
public class GetTemplateListQueryHandler : IRequestHandler<GetTemplateListQuery, List<NotificationTemplateResponse>>
{
    private readonly INotificationTemplateRepository _templateRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTemplateListQueryHandler"/> class.
    /// Injects template repository for querying template data from database.
    /// </summary>
    /// <param name="templateRepository">Repository for retrieving notification templates with optional filtering.</param>
    public GetTemplateListQueryHandler(INotificationTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// Retrieves all notification templates with optional active status filtering.
    /// Logic flow: 1) Queries database using repository with IsActive filter (null returns all, true returns active only, false returns inactive only),
    /// 2) Repository returns templates ordered by name, 3) Maps each template entity to NotificationTemplateResponse DTO,
    /// 4) Includes all template metadata (ID, name, type, channel, subject, body, placeholders, active status, timestamps),
    /// 5) Returns list of DTOs for API response. Empty list if no templates match filter criteria.
    /// </summary>
    /// <param name="request">Query containing optional IsActive filter (null for all templates, true for active, false for inactive).</param>
    /// <param name="cancellationToken">Cancellation token to abort query operation if request is cancelled.</param>
    /// <returns>
    /// List of notification template response DTOs containing template metadata and content.
    /// Empty list if no templates exist or no templates match the IsActive filter.
    /// Templates are ordered by name for consistent display in admin UI.
    /// </returns>
    public async Task<List<NotificationTemplateResponse>> Handle(GetTemplateListQuery request, CancellationToken cancellationToken)
    {
        // Query database with optional IsActive filter - repository handles filtering logic
        var templates = await _templateRepository.GetAllAsync(request.IsActive, cancellationToken);

        // Map domain entities to response DTOs - includes all template metadata for admin UI
        return templates.Select(template => new NotificationTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            Type = template.Type,
            Channel = template.Channel,
            Subject = template.Subject,
            BodyTemplate = template.BodyTemplate,
            RequiredPlaceholders = template.RequiredPlaceholders,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        }).ToList();
    }
}
