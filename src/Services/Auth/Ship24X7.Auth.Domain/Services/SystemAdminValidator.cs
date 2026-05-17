namespace Ship24X7.Auth.Domain.Services;

/// <summary>
/// Validates System Admin rules and constraints
/// </summary>
public static class SystemAdminValidator
{
    public const string SystemAdminEmail = "system-admin@ship24x7.local";
    public const string SystemAdminRoleName = "System_Admin";
    
    /// <summary>
    /// Validates if a role assignment violates System Admin rules
    /// </summary>
    /// <param name="roleName">Role being assigned</param>
    /// <param name="userEmail">User email</param>
    /// <param name="existingSystemAdminCount">Current count of System Admins</param>
    /// <exception cref="InvalidOperationException">Thrown when System Admin rules are violated</exception>
    public static void ValidateRoleAssignment(string roleName, string userEmail, int existingSystemAdminCount)
    {
        // Rule 1: System_Admin role can only be assigned to system-admin@ship24x7.local
        if (roleName == SystemAdminRoleName && userEmail != SystemAdminEmail)
        {
            throw new InvalidOperationException(
                $"System_Admin role can only be assigned to {SystemAdminEmail}. " +
                "This is a protected role that cannot be assigned to other users.");
        }
        
        // Rule 2: Only one System Admin can exist
        if (roleName == SystemAdminRoleName && existingSystemAdminCount > 0)
        {
            throw new InvalidOperationException(
                "Only one System Admin is allowed in the system. " +
                $"System Admin ({SystemAdminEmail}) already exists and cannot be modified.");
        }
    }
    
    /// <summary>
    /// Validates if a user can be modified (System Admin cannot be modified)
    /// </summary>
    /// <param name="userEmail">User email being modified</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to modify System Admin</exception>
    public static void ValidateUserModification(string userEmail)
    {
        if (userEmail == SystemAdminEmail)
        {
            throw new InvalidOperationException(
                $"System Admin ({SystemAdminEmail}) cannot be modified. " +
                "This is a protected account created at service initialization.");
        }
    }
    
    /// <summary>
    /// Validates if a user can be deleted (System Admin cannot be deleted)
    /// </summary>
    /// <param name="userEmail">User email being deleted</param>
    /// <exception cref="InvalidOperationException">Thrown when trying to delete System Admin</exception>
    public static void ValidateUserDeletion(string userEmail)
    {
        if (userEmail == SystemAdminEmail)
        {
            throw new InvalidOperationException(
                $"System Admin ({SystemAdminEmail}) cannot be deleted. " +
                "This is a protected account that must always exist.");
        }
    }
    
    /// <summary>
    /// Checks if an email is the System Admin email
    /// </summary>
    public static bool IsSystemAdmin(string email)
    {
        return email?.Equals(SystemAdminEmail, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
