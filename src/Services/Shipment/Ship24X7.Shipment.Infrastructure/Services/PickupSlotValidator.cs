using Ship24X7.Shipment.Application.DTOs;
using Ship24X7.Shipment.Application.Interfaces;
using Ship24X7.Shipment.Domain.Enums;

namespace Ship24X7.Shipment.Infrastructure.Services;

/// <summary>
/// Pickup slot validator service for managing pickup time slot availability.
/// Defines pickup time windows and validates slot availability based on day of week.
/// Implements business rules for pickup scheduling (no Sunday pickups, limited weekend hours).
/// </summary>
public class PickupSlotValidator : IPickupSlotValidator
{
    /// <summary>
    /// Gets all available pickup slots for a specific date with availability status.
    /// Process flow:
    /// 1. Determines day of week for the requested date
    /// 2. Defines four time slots with time ranges:
    ///    - Morning: 09:00-12:00 (Mon-Sat)
    ///    - Afternoon: 12:00-16:00 (Mon-Sat)
    ///    - Evening: 16:00-19:00 (Mon-Fri only)
    ///    - Night: 19:00-21:00 (Mon-Fri only)
    /// 3. Marks each slot as available or unavailable based on day of week
    /// 4. Returns complete list with availability flags
    /// Business rules:
    /// - No pickups on Sunday (all slots unavailable)
    /// - Evening and Night slots unavailable on Saturday
    /// - All slots available Monday-Friday
    /// </summary>
    /// <param name="date">The date to check pickup slot availability.</param>
    /// <returns>List of pickup slot responses with time ranges and availability status.</returns>
    public List<PickupSlotResponse> GetAvailableSlots(DateTime date)
    {
        var dayOfWeek = date.DayOfWeek;
        var slots = new List<PickupSlotResponse>();

        // Morning (09:00-12:00) - Available Mon-Sat
        slots.Add(new PickupSlotResponse
        {
            TimeSlot = PickupTimeSlot.Morning,
            TimeRange = "09:00 - 12:00",
            IsAvailable = dayOfWeek != DayOfWeek.Sunday
        });

        // Afternoon (12:00-16:00) - Available Mon-Sat
        slots.Add(new PickupSlotResponse
        {
            TimeSlot = PickupTimeSlot.Afternoon,
            TimeRange = "12:00 - 16:00",
            IsAvailable = dayOfWeek != DayOfWeek.Sunday
        });

        // Evening (16:00-19:00) - Available Mon-Fri only
        slots.Add(new PickupSlotResponse
        {
            TimeSlot = PickupTimeSlot.Evening,
            TimeRange = "16:00 - 19:00",
            IsAvailable = dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday
        });

        // Night (19:00-21:00) - Available Mon-Fri only
        slots.Add(new PickupSlotResponse
        {
            TimeSlot = PickupTimeSlot.Night,
            TimeRange = "19:00 - 21:00",
            IsAvailable = dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday
        });

        return slots;
    }

    /// <summary>
    /// Validates whether a specific pickup slot is available on a given date.
    /// Used during pickup scheduling to prevent booking unavailable time slots.
    /// </summary>
    /// <param name="date">The date to check slot availability.</param>
    /// <param name="timeSlot">The time slot to validate.</param>
    /// <returns>True if the slot is available on the specified date; false otherwise.</returns>
    public bool IsSlotAvailable(DateTime date, PickupTimeSlot timeSlot)
    {
        var dayOfWeek = date.DayOfWeek;

        return timeSlot switch
        {
            PickupTimeSlot.Morning => dayOfWeek != DayOfWeek.Sunday,
            PickupTimeSlot.Afternoon => dayOfWeek != DayOfWeek.Sunday,
            PickupTimeSlot.Evening => dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday,
            PickupTimeSlot.Night => dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday,
            _ => false
        };
    }
}
