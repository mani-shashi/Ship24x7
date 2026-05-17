using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Application.Interfaces;

/// <summary>
/// Validator for IPickupSlot ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public interface IPickupSlotValidator
{
    List<PickupSlotResponse> GetAvailableSlots(DateTime date);
    bool IsSlotAvailable(DateTime date, PickupTimeSlot timeSlot);
}
