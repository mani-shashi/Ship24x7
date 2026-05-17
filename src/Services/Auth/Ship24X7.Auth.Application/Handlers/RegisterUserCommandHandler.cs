using MediatR;
using Ship24X7.Auth.Application.Commands;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;
using Ship24X7.Auth.Domain.Events;

namespace Ship24X7.Auth.Application.Handlers;

/// <summary>
/// Handles user registration by creating a new user account with email verification.
/// Assigns default Customer role and sends verification email.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the RegisterUserCommandHandler class.
    /// </summary>
    /// <param name="userRepository">Repository for user data operations.</param>
    /// <param name="roleRepository">Repository for role data operations.</param>
    /// <param name="passwordHasher">Service for hashing passwords securely.</param>
    /// <param name="emailService">Service for sending verification emails.</param>
    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
    }

    /// <summary>
    /// Handles the user registration command by creating a new user account with email verification.
    /// Process flow:
    /// 1. Validates email uniqueness in the system
    /// 2. Hashes password using secure PBKDF2 algorithm
    /// 3. Creates user entity with email (lowercase), full name, phone number
    /// 4. Sets EmailVerified=false, PhoneVerified=false, IsActive=true
    /// 5. Assigns default "Customer" role to the new user
    /// 6. Generates verification token (24-hour expiry) for email verification
    /// 7. Persists user with role and verification token in single transaction
    /// 8. Sends verification email (non-blocking, failures don't prevent registration)
    /// 9. Returns new user ID for client reference
    /// </summary>
    /// <param name="request">The registration command containing email, password, full name, and phone number.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The unique identifier (GUID) of the newly created user.</returns>
    /// <exception cref="InvalidOperationException">Thrown when email is already registered in the system.</exception>
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            throw new InvalidOperationException("Email is already registered");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            EmailVerified = false,
            PhoneVerified = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        // Assign Customer role by default
        var customerRole = await _roleRepository.GetByNameAsync("Customer");
        if (customerRole != null)
        {
            user.UserRoles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = customerRole.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            });
        }

        // Generate verification token
        var verificationToken = new VerificationToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            Type = VerificationTokenType.EmailVerification,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user.Id
        };

        user.VerificationTokens.Add(verificationToken);

        // Add user with all related entities in one go
        await _userRepository.AddAsync(user);

        // Send verification email (don't fail registration if email fails)
        try
        {
            await _emailService.SendVerificationEmailAsync(user.Email, user.FullName, verificationToken.Token);
        }
        catch (Exception)
        {
            // Log but don't fail registration if email service is not configured
            // In production, this should be handled by a background job with retry logic
        }

        return user.Id;
    }
}
