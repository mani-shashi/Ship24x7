namespace Ship24X7.Auth.Application.DTOs;

/// <summary>
/// Response data transfer object returned when setting up Multi-Factor Authentication.
/// Contains TOTP secret, QR code URI for authenticator apps, and backup codes.
/// </summary>
public class MfaSetupResponse
{
    /// <summary>
    /// Gets or sets the TOTP secret key for manual entry into authenticator apps.
    /// </summary>
    public string Secret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the QR code URI for scanning with authenticator apps.
    /// </summary>
    public string QrCodeUri { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the backup codes for account recovery when MFA device is unavailable.
    /// </summary>
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
}
