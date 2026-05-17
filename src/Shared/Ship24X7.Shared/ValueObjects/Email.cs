using System.Text.RegularExpressions;

namespace Ship24X7.Shared.ValueObjects;

/// <summary>
/// Represents an email address value object in the Ship24X7 platform.
/// Immutable value object that encapsulates email validation logic and ensures only valid email addresses are used throughout the system.
/// Implements equality comparison based on email value.
/// </summary>
public class Email : IEquatable<Email>
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Gets the email address value in lowercase format.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class.
    /// Private constructor to enforce creation through the Create factory method.
    /// </summary>
    /// <param name="value">The validated email address value.</param>
    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new Email value object from a string.
    /// Validates the email format using regex pattern and converts to lowercase for consistency.
    /// </summary>
    /// <param name="email">The email address string to validate and encapsulate.</param>
    /// <returns>A new Email value object containing the validated email address.</returns>
    /// <exception cref="ArgumentException">Thrown when email is null, empty, whitespace, or has invalid format.</exception>
    /// <remarks>
    /// Validation rules:
    /// - Must not be null, empty, or whitespace
    /// - Must match pattern: localpart@domain.extension
    /// - Converted to lowercase for case-insensitive comparison
    /// </remarks>
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!EmailRegex.IsMatch(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        return new Email(email.ToLowerInvariant());
    }

    /// <summary>
    /// Determines whether the specified Email is equal to the current Email.
    /// Equality is based on the email address value.
    /// </summary>
    /// <param name="other">The Email to compare with the current Email.</param>
    /// <returns>True if the email values are equal; otherwise, false.</returns>
    public bool Equals(Email? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Email.
    /// </summary>
    /// <param name="obj">The object to compare with the current Email.</param>
    /// <returns>True if the object is an Email with the same value; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Email);
    
    /// <summary>
    /// Returns the hash code for this Email based on its value.
    /// </summary>
    /// <returns>A hash code for the current Email.</returns>
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <summary>
    /// Returns the email address as a string.
    /// </summary>
    /// <returns>The email address value.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts an Email value object to a string.
    /// </summary>
    /// <param name="email">The Email to convert.</param>
    public static implicit operator string(Email email) => email.Value;
}
