using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Queries;

/// <summary>
/// Query to retrieve the authenticated user's preferences.
/// Returns defaults if no preferences record exists yet.
/// </summary>
public class GetPreferencesQuery : IRequest<UserPreferencesDto>
{
    public Guid UserId { get; set; }
}
