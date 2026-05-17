namespace Ship24X7.Auth.Domain.ValueObjects;

/// <summary>
/// Value object representing a hashed password.
/// Ensures password hashes are never empty and provides type safety.
/// </summary>
public class PasswordHash : IEquatable<PasswordHash>
{
    /// <summary>
    /// Gets the hashed password value.
    /// </summary>
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new PasswordHash instance from a hash string.
    /// </summary>
    /// <param name="hash">The hashed password string.</param>
    /// <returns>A new PasswordHash instance.</returns>
    /// <exception cref="ArgumentException">Thrown when hash is null or whitespace.</exception>
    public static PasswordHash Create(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty", nameof(hash));

        return new PasswordHash(hash);
    }

    /// <summary>
    /// Determines whether the specified PasswordHash is equal to the current PasswordHash.
    /// </summary>
    /// <param name="other">The PasswordHash to compare with the current instance.</param>
    /// <returns>True if the specified PasswordHash is equal to the current PasswordHash; otherwise, false.</returns>
    public bool Equals(PasswordHash? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current PasswordHash.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns>True if the specified object is equal to the current PasswordHash; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as PasswordHash);
    
    /// <summary>
    /// Returns the hash code for this PasswordHash.
    /// </summary>
    /// <returns>A hash code for the current PasswordHash.</returns>
    public override int GetHashCode() => Value.GetHashCode();
    
    /// <summary>
    /// Returns the string representation of the password hash.
    /// </summary>
    /// <returns>The hashed password string.</returns>
    public override string ToString() => Value;

    /// <summary>
    /// Implicitly converts a PasswordHash to a string.
    /// </summary>
    /// <param name="hash">The PasswordHash to convert.</param>
    public static implicit operator string(PasswordHash hash) => hash.Value;
}
