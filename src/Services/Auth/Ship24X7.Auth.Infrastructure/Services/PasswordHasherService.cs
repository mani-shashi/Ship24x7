using Microsoft.AspNetCore.Identity;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Infrastructure.Services;

/// <summary>
/// Implementation of password hashing service using ASP.NET Core Identity's PasswordHasher.
/// Provides secure password hashing with PBKDF2 algorithm and automatic salt generation.
/// </summary>
public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<object> _passwordHasher;

    /// <summary>
    /// Initializes a new instance of the PasswordHasherService class.
    /// </summary>
    public PasswordHasherService()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    /// <summary>
    /// Hashes a plain text password using PBKDF2 with automatic salt generation.
    /// </summary>
    /// <param name="password">The plain text password to hash.</param>
    /// <returns>The hashed password string including algorithm version and salt.</returns>
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    /// <summary>
    /// Verifies a plain text password against a hashed password.
    /// Supports rehashing if the hash format is outdated.
    /// </summary>
    /// <param name="password">The plain text password to verify.</param>
    /// <param name="hash">The hashed password to compare against.</param>
    /// <returns>True if the password matches the hash; otherwise, false.</returns>
    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var result = _passwordHasher.VerifyHashedPassword(null!, hash, password);
            return result == PasswordVerificationResult.Success || 
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch
        {
            return false;
        }
    }
}
