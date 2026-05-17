using OtpNet;
using Ship24X7.Auth.Application.Interfaces;

namespace Ship24X7.Auth.Infrastructure.Services;

/// <summary>
/// Implementation of Multi-Factor Authentication service using Time-based One-Time Password (TOTP) algorithm.
/// Generates secrets, QR codes, validates codes, and creates backup codes for MFA.
/// </summary>
public class TotpMfaService : IMfaService
{
    /// <summary>
    /// Generates a new TOTP secret key for MFA setup.
    /// </summary>
    /// <returns>A base32-encoded TOTP secret string.</returns>
    public string GenerateTotpSecret()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }

    /// <summary>
    /// Generates a QR code URI compatible with authenticator apps like Google Authenticator.
    /// </summary>
    /// <param name="email">The user's email address for the QR code label.</param>
    /// <param name="secret">The TOTP secret to encode in the QR code.</param>
    /// <returns>An otpauth:// URI string that can be converted to a QR code image.</returns>
    public string GenerateQrCodeUri(string email, string secret)
    {
        var issuer = "Ship24X7";
        return $"otpauth://totp/{issuer}:{email}?secret={secret}&issuer={issuer}";
    }

    /// <summary>
    /// Validates a TOTP code against the secret with a time window for clock drift.
    /// Allows codes from 2 time steps before and after current time.
    /// </summary>
    /// <param name="secret">The user's TOTP secret.</param>
    /// <param name="code">The TOTP code to validate.</param>
    /// <returns>True if the code is valid within the time window; otherwise, false.</returns>
    public bool ValidateTotpCode(string secret, string code)
    {
        var key = Base32Encoding.ToBytes(secret);
        var totp = new Totp(key);
        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
    }

    /// <summary>
    /// Generates backup codes for account recovery when MFA device is unavailable.
    /// </summary>
    /// <param name="count">The number of backup codes to generate (default is 10).</param>
    /// <returns>An array of 8-character uppercase alphanumeric backup codes.</returns>
    public string[] GenerateBackupCodes(int count = 10)
    {
        var codes = new string[count];
        for (int i = 0; i < count; i++)
        {
            codes[i] = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        }
        return codes;
    }
}
