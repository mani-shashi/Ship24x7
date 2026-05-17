namespace Ship24X7.Payment.Domain.ValueObjects;

/// <summary>
/// Value object representing RazorpayOrderId in the domain model. Immutable type with value-based equality.
/// </summary>
public class RazorpayOrderId : IEquatable<RazorpayOrderId>
{
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public string Value { get; }

    private RazorpayOrderId(string value)
    {
        Value = value;
    }

    public static RazorpayOrderId Create(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Razorpay order ID cannot be empty", nameof(orderId));

        if (!orderId.StartsWith("order_"))
            throw new ArgumentException("Invalid Razorpay order ID format", nameof(orderId));

        return new RazorpayOrderId(orderId);
    }

    public bool Equals(RazorpayOrderId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as RazorpayOrderId);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(RazorpayOrderId orderId) => orderId.Value;
}
