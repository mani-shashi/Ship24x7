using Ship24X7.Shared.Domain;

namespace Ship24X7.Payment.Application.Interfaces;

/// <summary>
/// IPaymentEventPublisher implementation. Provides functionality for the application.
/// </summary>
public interface IPaymentEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
}
