using Ship24X7.Shared.Domain;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// IShipmentEventPublisher implementation. Provides functionality for the application.
/// </summary>
public interface IShipmentEventPublisher
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
}
