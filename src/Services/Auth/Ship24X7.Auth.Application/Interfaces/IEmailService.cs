namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Service interface for sending authentication-related emails.
/// Handles verification, password reset, and welcome emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email verification link to the user's email address.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="fullName">The recipient's full name for personalization.</param>
    /// <param name="verificationToken">The verification token to include in the email link.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendVerificationEmailAsync(string email, string fullName, string verificationToken);
    
    /// <summary>
    /// Sends a password reset link to the user's email address.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="fullName">The recipient's full name for personalization.</param>
    /// <param name="resetToken">The password reset token to include in the email link.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken);
    
    /// <summary>
    /// Sends a welcome email with temporary password to newly created users.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="fullName">The recipient's full name for personalization.</param>
    /// <param name="temporaryPassword">The temporary password for first login.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendWelcomeEmailAsync(string email, string fullName, string temporaryPassword);
}
