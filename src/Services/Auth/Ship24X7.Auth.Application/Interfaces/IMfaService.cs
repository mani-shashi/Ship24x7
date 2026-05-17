namespace Ship24X7.Auth.Application.Interfaces;

/// <summary>
/// Service interface for Multi-Factor Authentication operations.
/// Handles TOTP secret generation, QR code creation, code validation, and backup codes.
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret key for MFA setup.
    /// </summary>
    /// <returns>A base32-encoded TOTP secret string.</returns>
    string GenerateTotpSecret();
    
    /// <summary>
    /// Generates a QR code URI for scanning with authenticator apps.
    /// </summary>
    /// <param name="email">The user's email address for the QR code label.</param>
    /// <param name="secret">The TOTP secret to encode in the QR code.</param>
    /// <returns>A URI string that can be converted to a QR code image.</returns>
    string GenerateQrCodeUri(string email, string secret);
    
    /// <summary>
    /// Validates a TOTP code against the secret.
    /// </summary>
    /// <param name="secret">The user's TOTP secret.</param>
    /// <param name="code">The TOTP code to validate.</param>
    /// <returns>True if the code is valid; otherwise, false.</returns>
    bool ValidateTotpCode(string secret, string code);
    
    /// <summary>
    /// Generates backup codes for account recovery.
    /// </summary>
    /// <param name="count">The number of backup codes to generate (default is 10).</param>
    /// <returns>An array of backup code strings.</returns>
    string[] GenerateBackupCodes(int count = 10);
}
