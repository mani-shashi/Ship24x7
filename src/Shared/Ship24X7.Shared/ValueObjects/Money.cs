namespace Ship24X7.Shared.ValueObjects;

/// <summary>
/// Represents a monetary amount with currency in the Ship24X7 platform.
/// Immutable value object that encapsulates money validation, arithmetic operations, and currency handling.
/// Ensures monetary calculations are performed safely with proper rounding and currency matching.
/// </summary>
public class Money : IEquatable<Money>
{
    /// <summary>
    /// Gets the monetary amount rounded to 2 decimal places.
    /// </summary>
    public decimal Amount { get; }
    
    /// <summary>
    /// Gets the currency code in uppercase format (e.g., "INR", "USD", "EUR").
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// Private constructor to enforce creation through the Create factory method.
    /// </summary>
    /// <param name="amount">The monetary amount.</param>
    /// <param name="currency">The currency code.</param>
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Creates a new Money value object from an amount and currency.
    /// Validates that amount is non-negative and currency is specified, then rounds amount to 2 decimal places.
    /// </summary>
    /// <param name="amount">The monetary amount (must be non-negative).</param>
    /// <param name="currency">The currency code (defaults to "INR" for Indian Rupees).</param>
    /// <returns>A new Money value object with validated and rounded amount.</returns>
    /// <exception cref="ArgumentException">Thrown when amount is negative or currency is null/empty.</exception>
    /// <remarks>
    /// Validation rules:
    /// - Amount must be >= 0
    /// - Currency must not be null, empty, or whitespace
    /// - Amount is automatically rounded to 2 decimal places
    /// - Currency is converted to uppercase
    /// </remarks>
    public static Money Create(decimal amount, string currency = "INR")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty", nameof(currency));

        return new Money(Math.Round(amount, 2), currency.ToUpperInvariant());
    }

    /// <summary>
    /// Adds another Money value to this Money value.
    /// Both Money values must have the same currency.
    /// </summary>
    /// <param name="other">The Money value to add.</param>
    /// <returns>A new Money value representing the sum.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return Create(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another Money value from this Money value.
    /// Both Money values must have the same currency.
    /// </summary>
    /// <param name="other">The Money value to subtract.</param>
    /// <returns>A new Money value representing the difference.</returns>
    /// <exception cref="InvalidOperationException">Thrown when currencies don't match.</exception>
    /// <exception cref="ArgumentException">Thrown when result would be negative.</exception>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        return Create(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies this Money value by a decimal factor.
    /// Useful for calculating percentages, taxes, or discounts.
    /// </summary>
    /// <param name="factor">The multiplication factor.</param>
    /// <returns>A new Money value representing the product.</returns>
    public Money Multiply(decimal factor)
    {
        return Create(Amount * factor, Currency);
    }

    /// <summary>
    /// Determines whether the specified Money is equal to the current Money.
    /// Equality is based on both amount and currency.
    /// </summary>
    /// <param name="other">The Money to compare with the current Money.</param>
    /// <returns>True if both amount and currency are equal; otherwise, false.</returns>
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current Money.
    /// </summary>
    /// <param name="obj">The object to compare with the current Money.</param>
    /// <returns>True if the object is Money with the same amount and currency; otherwise, false.</returns>
    public override bool Equals(object? obj) => Equals(obj as Money);
    
    /// <summary>
    /// Returns the hash code for this Money based on amount and currency.
    /// </summary>
    /// <returns>A hash code for the current Money.</returns>
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    
    /// <summary>
    /// Returns a formatted string representation of the money value.
    /// </summary>
    /// <returns>A string in the format "Amount Currency" (e.g., "1250.50 INR").</returns>
    public override string ToString() => $"{Amount:F2} {Currency}";
}
