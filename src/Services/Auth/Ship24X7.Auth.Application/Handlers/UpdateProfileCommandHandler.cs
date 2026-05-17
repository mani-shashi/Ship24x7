using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.DTOs;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles profile update requests (full name, phone number, profile photo URL).
/// </summary>
public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UserResponse>
{
    private readonly IUserRepository _userRepository;

    public UpdateProfileCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found");

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();

        if (request.ProfilePhotoUrl != null)
            user.ProfilePhotoUrl = request.ProfilePhotoUrl;

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = request.UserId;

        await _userRepository.UpdateAsync(user);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            EmailVerified = user.EmailVerified,
            IsActive = user.IsActive,
            Roles = roles,
            MfaEnabled = user.MfaSettings?.IsEnabled ?? false,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
