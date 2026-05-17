using System.Text.RegularExpressions;

namespace Ship24X7.Shared.ValueObjects;

/// <summary>
/// Represents a phone number value object in the Ship24X7 platform.
/// Immutable value object that encapsulates phone number validation logic following E.164 international format.
/// Ensures only valid phone numbers are used throughout the system.
/// </summary>
public class PhoneNumber : IEquatable<PhoneNumber>
{
    private static readonly Regex PhoneRegex = new(
        @"^\+?[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    /// <summary>
    /// Gets the phone number value in cleaned format (digits only, with optional leading +).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PhoneNumber"/> class.
    /// Private constructor to enforce creation through the Create factory method.
    /// </summary>
    /// <param name="value">The validated phone number value.</param>
    private PhoneNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new PhoneNumber value object from a string.
    /// Validates the phone number format following E.164 standard (international phone number format).
    /// Removes formatting characters (spaces, hyphens, parentheses) before validation.
    /// </summary>
    /// <param name="phone">The phone number string to validate and encapsulate.</param>
    /// <returns>A new PhoneNumber value object containing the validated phone number.</returns>
    /// <exception cref="ArgumentException">Thrown when phone is null, empty, whitespace, or has invalid format.</exception>
    /// <remarks>
    /// Validation rules (E.164 format):
    /// - Must not be null, empty, or whitespace
    /// - Optional leading + for country code
    /// - Must start with digit 1-9 (no leading zeros)
    /// - Total length: 2-15 digits (after country code)
    /// - Formatting characters (spaces, hyphens, parentheses) are automatically removed
    /// Examples: +919876543210, 9876543210, +14155552671
    /// </remarks>
    public static PhoneNumber Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone number cannot be empty", nameof(phone));

        var cleaned = Regex.Replace(phone, @"[\s\-\(\)]", "");

        if (!PhoneRegex.IsMatch(cleaned))
            throw new ArgumentException("Invalid phone number format", nameof(phone));

        return new PhoneNumber(cleaned);
    }

    /// <summary>
    /// Determines whether the specified PhoneNumber is equal to the current PhoneNumber.
    /// Equality is based on the phone number value.
    /// </summary>
    /// <param name="other">The PhoneNumber to compare with the current PhoneNumber.</param>
    /// <returns>True if the phone number values are equal; otherwise, false.</returns>
    public bool Equals(PhoneNumber? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current PhoneNumber.
    /// </summary>
    /// <param name="obj">The object to compare with the current PhoneNumber.</param>
    /// <returns>True if the object is a PhoneNumber with the same value; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as PhoneNumber);
    
    /// <summary>
    /// Returns the hash code for this PhoneNumber based on its value.
    /// </summary>
    /// <returns>A hash code for the current PhoneNumber.</returns>
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <summary>
    /// Returns the phone number as a string.
    /// </summary>
    /// <returns>The phone number value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a PhoneNumber value object to a string.
    /// </summary>
    /// <param name="phone">The PhoneNumber to convert.</param>
    public static implicit operator string(PhoneNumber phone) => phone.Value;
}
