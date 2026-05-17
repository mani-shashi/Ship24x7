namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// Validator for IPaymentSignature ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public interface IPaymentSignatureValidator
{
    bool ValidatePaymentSignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
}
