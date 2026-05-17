using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Infrastructure.Services;

/// <summary>
/// SMTP email service implementation for sending verification, password reset, and welcome emails.
/// Uses SMTP protocol with TLS encryption for secure email delivery.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly string _fromName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
    /// Loads SMTP configuration including host, port, credentials, and sender information.
    /// </summary>
    /// <param name="configuration">Application configuration containing SMTP settings.</param>
    /// <exception cref="InvalidOperationException">Thrown when SMTP username or password is not configured.</exception>
    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? throw new InvalidOperationException("SMTP Username not configured");
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? throw new InvalidOperationException("SMTP Password not configured");
        _fromEmail = _configuration["Email:FromEmail"] ?? _smtpUsername;
        _fromName = _configuration["Email:FromName"] ?? "Ship24X7";
    }

    /// <summary>
    /// Sends an email verification link to the user's email address.
    /// Email contains a clickable link with verification token that expires in 24 hours.
    /// </summary>
    /// <param name="email">Recipient's email address.</param>
    /// <param name="fullName">User's full name for personalization.</param>
    /// <param name="verificationToken">Unique token for email verification.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    public async Task SendVerificationEmailAsync(string email, string fullName, string verificationToken)
    {
        // Route through API Gateway — stable entry point, never changes port
        var gatewayUrl = _configuration["App:GatewayUrl"] ?? "http://localhost:8000";
        var verificationUrl = $"{gatewayUrl}/api/v1/auth/verify-email?token={verificationToken}";
        
        var subject = "Verify Your Email - Ship24X7";
        var body = $@"
            <html>
            <body>
                <h2>Welcome to Ship24X7, {fullName}!</h2>
                <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationUrl}'>Verify Email Address</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't create an account, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>Ship24X7 Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a password reset link to the user's email address.
    /// Email contains a clickable link with reset token that expires in 1 hour.
    /// </summary>
    /// <param name="email">Recipient's email address.</param>
    /// <param name="fullName">User's full name for personalization.</param>
    /// <param name="resetToken">Unique token for password reset.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    public async Task SendPasswordResetEmailAsync(string email, string fullName, string resetToken)
    {
        var gatewayUrl = _configuration["App:GatewayUrl"] ?? "http://localhost:8000";
        var resetUrl = $"{gatewayUrl}/api/v1/auth/reset-password?token={resetToken}";
        
        var subject = "Password Reset Request - Ship24X7";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>Hello {fullName},</p>
                <p>We received a request to reset your password. Click the link below to reset it:</p>
                <p><a href='{resetUrl}'>Reset Password</a></p>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request a password reset, please ignore this email.</p>
                <br/>
                <p>Best regards,<br/>Ship24X7 Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends a welcome email with temporary password to newly created users.
    /// Used when administrators create user accounts. Users must change password on first login.
    /// </summary>
    /// <param name="email">Recipient's email address.</param>
    /// <param name="fullName">User's full name for personalization.</param>
    /// <param name="temporaryPassword">Temporary password for initial login.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    public async Task SendWelcomeEmailAsync(string email, string fullName, string temporaryPassword)
    {
        var subject = "Welcome to Ship24X7";
        var body = $@"
            <html>
            <body>
                <h2>Welcome to Ship24X7, {fullName}!</h2>
                <p>Your account has been created by an administrator.</p>
                <p>Your temporary password is: <strong>{temporaryPassword}</strong></p>
                <p>Please log in and change your password immediately.</p>
                <br/>
                <p>Best regards,<br/>Ship24X7 Team</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    /// <summary>
    /// Sends an HTML email using SMTP with TLS encryption.
    /// Internal helper method for all email sending operations.
    /// </summary>
    /// <param name="toEmail">Recipient's email address.</param>
    /// <param name="subject">Email subject line.</param>
    /// <param name="body">HTML email body content.</param>
    /// <returns>A task representing the asynchronous email sending operation.</returns>
    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_fromEmail, _fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        await smtpClient.SendMailAsync(mailMessage);
    }
}
