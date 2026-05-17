namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// Validator for IWebhookSignature ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public interface IWebhookSignatureValidator
{
    bool ValidateWebhookSignature(string payload, string signature);
}
