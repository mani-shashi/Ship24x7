using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ship24X7.Auth.Application.Interfaces;
using Ship24X7.Auth.Domain.Entities;

namespace Ship24X7.Auth.API.Controllers;

/// <summary>
/// API controller for managing user accounts and administrative operations in the Ship24X7 auth service.
/// Handles HTTP requests for user retrieval, activation, and deactivation.
/// Restricted to System_Admin and Admin_User roles for security and compliance.
/// Provides administrative capabilities for managing user accounts across the platform.
/// Workflow: Admin logs in -> Access user management -> View users -> Activate/deactivate accounts -> Monitor user activity.
/// Used by admin dashboards for user account management and moderation.
/// </summary>
[ApiController]
[Route("api/v1/auth/users")]
[Authorize(Roles = "System_Admin,Admin_User")]
public class UserManagementController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ILogger<UserManagementController> _logger;

    /// <summary>
    /// Initializes a new instance of the UserManagementController class.
    /// Sets up dependencies for repository access and logging.
    /// </summary>
    /// <param name="userRepository">Repository for accessing and managing user data in the database</param>
    /// <param name="roleRepository">Repository for looking up roles by name for role assignment</param>
    /// <param name="logger">Logger instance for recording user management operations, security events, errors, and diagnostic information</param>
    public UserManagementController(IUserRepository userRepository, IRoleRepository roleRepository, ILogger<UserManagementController> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated list of all users in the system.
    /// Endpoint: GET /api/v1/usermanagement?page={page}&pageSize={pageSize}
    /// Logic flow:
    /// 1. Receives page number and page size from query parameters (defaults: page=1, pageSize=50)
    /// 2. Validates page number is positive (>= 1)
    /// 3. Validates page size is within allowed range (1-100)
    /// 4. Calls repository GetAllAsync method with pagination parameters
    /// 5. Repository retrieves users from database with Skip and Take for pagination
    /// 6. Repository includes related entities: Roles, MfaSettings, ExternalLogins
    /// 7. Repository orders users by creation date (newest first)
    /// 8. Returns list of users with basic information (ID, email, name, roles, status, timestamps)
    /// Used by admin dashboards to view and manage all user accounts.
    /// Supports pagination for performance with large user bases.
    /// </summary>
    /// <param name="page">Page number to retrieve (default: 1). Must be positive integer.</param>
    /// <param name="pageSize">Number of users per page (default: 50). Range: 1-100.</param>
    /// <returns>
    /// 200 OK with paginated list of users including ID, email, full name, roles, active status, email verified status, MFA enabled status, and timestamps.
    /// 500 Internal Server Error for unexpected errors during user retrieval.
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var users = await _userRepository.GetAllAsync(page, pageSize);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(500, new { error = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Retrieves detailed information about a specific user by their unique identifier.
    /// Endpoint: GET /api/v1/usermanagement/{id}
    /// Logic flow:
    /// 1. Receives user ID from route parameter
    /// 2. Calls repository GetByIdAsync method with user ID
    /// 3. Repository retrieves user from database by ID
    /// 4. Repository includes related entities: Roles, UserRoles, MfaSettings, ExternalLogins, RefreshTokens
    /// 5. If user not found, returns 404 Not Found with error message
    /// 6. Returns complete user details including:
    ///    - Basic info: ID, email, full name, phone number
    ///    - Security: Password hash, email verified status, MFA settings
    ///    - Status: Active status, locked status, failed login attempts
    ///    - Roles: Assigned roles and permissions
    ///    - External logins: Linked OAuth providers
    ///    - Timestamps: Created, updated, last login
    /// Used by admin dashboards to view complete user profile and account details.
    /// Useful for troubleshooting user issues and account verification.
    /// </summary>
    /// <param name="id">Unique identifier (GUID) of the user to retrieve</param>
    /// <returns>
    /// 200 OK with complete user details including all related entities and security information.
    /// 404 Not Found if user with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during user retrieval.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user");
            return StatusCode(500, new { error = "An error occurred while retrieving user" });
        }
    }

    /// <summary>
    /// Deactivates a user account, preventing the user from logging in.
    /// Endpoint: POST /api/v1/usermanagement/{id}/deactivate
    /// Logic flow:
    /// 1. Receives user ID from route parameter
    /// 2. Calls repository GetByIdAsync method to retrieve user
    /// 3. If user not found, returns 404 Not Found with error message
    /// 4. Updates user IsActive property to false
    /// 5. Calls repository UpdateAsync method to save changes to database
    /// 6. Optionally revokes all active refresh tokens for the user (force logout)
    /// 7. Returns success message
    /// User's data is retained but account is marked as inactive.
    /// User cannot login until account is reactivated by admin.
    /// Existing sessions remain valid until access token expires (15 minutes).
    /// Used by admin dashboards to temporarily or permanently disable user accounts.
    /// Common use cases: Policy violations, suspicious activity, user request.
    /// </summary>
    /// <param name="id">Unique identifier (GUID) of the user to deactivate</param>
    /// <returns>
    /// 200 OK with success message if deactivation succeeds.
    /// 404 Not Found if user with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during deactivation.
    /// </returns>
    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            user.IsActive = false;
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "User deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating user");
            return StatusCode(500, new { error = "An error occurred while deactivating user" });
        }
    }

    /// <summary>
    /// Activates a previously deactivated user account, allowing the user to log in again.
    /// Endpoint: POST /api/v1/usermanagement/{id}/activate
    /// Logic flow:
    /// 1. Receives user ID from route parameter
    /// 2. Calls repository GetByIdAsync method to retrieve user
    /// 3. If user not found, returns 404 Not Found with error message
    /// 4. Updates user IsActive property to true
    /// 5. Optionally resets failed login attempts to 0
    /// 6. Optionally unlocks account if it was locked
    /// 7. Calls repository UpdateAsync method to save changes to database
    /// 8. Returns success message
    /// User can login immediately after activation.
    /// Used by admin dashboards to restore access to deactivated accounts.
    /// Common use cases: Issue resolved, appeal approved, temporary suspension ended.
    /// </summary>
    /// <param name="id">Unique identifier (GUID) of the user to activate</param>
    /// <returns>
    /// 200 OK with success message if activation succeeds.
    /// 404 Not Found if user with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during activation.
    /// </returns>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            user.IsActive = true;
            await _userRepository.UpdateAsync(user);

            return Ok(new { message = "User activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating user");
            return StatusCode(500, new { error = "An error occurred while activating user" });
        }
    }

    /// <summary>
    /// Updates a user's profile fields and role assignments.
    /// Endpoint: PUT /api/v1/auth/users/{id}
    /// Logic flow:
    /// 1. Loads user with eagerly-tracked UserRoles collection via GetByIdWithRolesAsync
    /// 2. Updates FullName and Email fields
    /// 3. Clears existing UserRoles from the tracked collection
    /// 4. Looks up each requested role by name via IRoleRepository
    /// 5. Constructs a new tracked UserRole join entity for each found role
    /// 6. Persists changes via UpdateAsync
    /// Roles not found in the catalog are skipped with a warning log entry.
    /// </summary>
    /// <param name="id">Unique identifier (GUID) of the user to update</param>
    /// <param name="dto">DTO containing updated FullName, Email, and Roles list</param>
    /// <returns>
    /// 200 OK with success message if update succeeds.
    /// 404 Not Found if user with specified ID does not exist.
    /// 500 Internal Server Error for unexpected errors during update.
    /// </returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        try
        {
            // Eagerly load user with tracked UserRoles collection
            var user = await _userRepository.GetByIdWithRolesAsync(id);
            if (user == null)
            {
                return NotFound(new { error = "User identity not found" });
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            // Clear current tracked role assignments
            user.UserRoles.Clear();

            // Look up each role by name and construct tracked join entities
            foreach (var roleName in dto.Roles)
            {
                var roleEntity = await _roleRepository.GetByNameAsync(roleName);
                if (roleEntity != null)
                {
                    user.UserRoles.Add(new UserRole
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleId = roleEntity.Id,
                        User = user,
                        Role = roleEntity
                    });
                }
                else
                {
                    _logger.LogWarning("Identity Sync Alert: Role '{RoleName}' was not found in catalog.", roleName);
                }
            }

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Identity policies updated for user: {UserId}", id);
            return Ok(new { message = "Identity configuration updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update identity profiles for user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred" });
        }
    }
}

/// <summary>
/// DTO for updating a user's profile and role assignments.
/// </summary>
public class UpdateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
