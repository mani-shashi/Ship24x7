using MediatR;
using Microsoft.EntityFrameworkCore;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Application.Queries;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handler for GetCurrentUserQuery that retrieves the authenticated user's profile.
/// Queries the database for fresh user data including roles and MFA status.
/// Returns complete user profile information for the /me endpoint.
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserResponse>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// Initializes a new instance of the GetCurrentUserQueryHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data access.</param>
    public GetCurrentUserQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Handles the GetCurrentUserQuery by retrieving user data from the database.
    /// Step-by-step logic:
    /// 1. Query user by ID from the database
    /// 2. Include related entities (UserRoles, Roles, MfaSettings)
    /// 3. If user not found, throw UnauthorizedAccessException
    /// 4. If user is inactive, throw UnauthorizedAccessException
    /// 5. Map user entity to UserResponse DTO
    /// 6. Extract role names from UserRoles collection
    /// 7. Check if MFA is enabled from MfaSettings
    /// 8. Return complete user profile
    /// </summary>
    /// <param name="request">Query containing the user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>UserResponse with complete user profile information.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user not found or inactive.</exception>
    public async Task<UserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Query user with related entities
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated");
        }

        // Map to response DTO
        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
            MfaEnabled = user.MfaSettings?.IsEnabled ?? false,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };

        return response;
    }
}
