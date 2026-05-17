using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles password change requests for authenticated users.
/// Verifies the current password before applying the new one.
/// </summary>
public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId)
            ?? throw new InvalidOperationException("User not found");

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = request.UserId;

        await _userRepository.UpdateAsync(user);

        return Unit.Value;
    }
}
