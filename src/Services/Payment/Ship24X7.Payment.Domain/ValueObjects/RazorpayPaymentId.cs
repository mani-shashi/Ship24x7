namespace Ship24X7.Payment.Domain.ValueObjects;

/// <summary>
/// Value object representing RazorpayPaymentId in the domain model. Immutable type with value-based equality.
/// </summary>
public class RazorpayPaymentId : IEquatable<RazorpayPaymentId>
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; }

    private RazorpayPaymentId(string value)
    {
        Value = value;
    }

    public static RazorpayPaymentId Create(string paymentId)
    {
        if (string.IsNullOrWhiteSpace(paymentId))
            throw new ArgumentException("Razorpay payment ID cannot be empty", nameof(paymentId));

        if (!paymentId.StartsWith("pay_"))
            throw new ArgumentException("Invalid Razorpay payment ID format", nameof(paymentId));

        return new RazorpayPaymentId(paymentId);
    }

    public bool Equals(RazorpayPaymentId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as RazorpayPaymentId);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(RazorpayPaymentId paymentId) => paymentId.Value;
}
