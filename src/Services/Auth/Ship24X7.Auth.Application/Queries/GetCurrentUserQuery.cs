using MediatR;
using Ship24X7.Auth.Application.DTOs;

namespace Ship24X7.Auth.Application.Queries;

/// <summary>
/// Query for retrieving the current authenticated user's profile information.
/// Used by the /me endpoint to return fresh user data from the database.
/// Requires the user ID from JWT claims.
/// </summary>
public class GetCurrentUserQuery : IRequest<UserResponse>
{
    /// <summary>
    /// Gets or sets the unique identifier of the current user.
    /// Extracted from JWT claims (ClaimTypes.NameIdentifier).
    /// </summary>
    public Guid UserId { get; set; }
}
