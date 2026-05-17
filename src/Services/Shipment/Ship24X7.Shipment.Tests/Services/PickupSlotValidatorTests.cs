using FluentAssertions;
using Ship24X7.Shipment.Domain.Enums;
using Ship24X7.Shipment.Infrastructure.Services;
using Xunit;

namespace Ship24X7.Shipment.Tests.Services;

/// <summary>
/// Validator for PickupSlotTests ensuring data integrity and business rules. Defines validation rules using FluentValidation.
/// </summary>
public class PickupSlotValidatorTests
{
    private readonly PickupSlotValidator _sut;

    public PickupSlotValidatorTests()
    {
        _sut = new PickupSlotValidator();
    }

    #region GetAvailableSlots Tests

    [Fact]
    public void GetAvailableSlots_OnMonday_AllSlotsAvailable()
    {
        // Arrange - Find next Monday
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);

        // Act
        var slots = _sut.GetAvailableSlots(monday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public void GetAvailableSlots_OnTuesday_AllSlotsAvailable()
    {
        // Arrange
        var tuesday = GetNextDayOfWeek(DayOfWeek.Tuesday);

        // Act
        var slots = _sut.GetAvailableSlots(tuesday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public void GetAvailableSlots_OnWednesday_AllSlotsAvailable()
    {
        // Arrange
        var wednesday = GetNextDayOfWeek(DayOfWeek.Wednesday);

        // Act
        var slots = _sut.GetAvailableSlots(wednesday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public void GetAvailableSlots_OnThursday_AllSlotsAvailable()
    {
        // Arrange
        var thursday = GetNextDayOfWeek(DayOfWeek.Thursday);

        // Act
        var slots = _sut.GetAvailableSlots(thursday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public void GetAvailableSlots_OnFriday_AllSlotsAvailable()
    {
        // Arrange
        var friday = GetNextDayOfWeek(DayOfWeek.Friday);

        // Act
        var slots = _sut.GetAvailableSlots(friday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeTrue());
    }

    [Fact]
    public void GetAvailableSlots_OnSaturday_OnlyMorningAndAfternoonAvailable()
    {
        // Arrange
        var saturday = GetNextDayOfWeek(DayOfWeek.Saturday);

        // Act
        var slots = _sut.GetAvailableSlots(saturday);

        // Assert
        slots.Should().HaveCount(4);
        
        var morning = slots.First(s => s.TimeSlot == PickupTimeSlot.Morning);
        morning.IsAvailable.Should().BeTrue();
        morning.TimeRange.Should().Be("09:00 - 12:00");
        
        var afternoon = slots.First(s => s.TimeSlot == PickupTimeSlot.Afternoon);
        afternoon.IsAvailable.Should().BeTrue();
        afternoon.TimeRange.Should().Be("12:00 - 16:00");
        
        var evening = slots.First(s => s.TimeSlot == PickupTimeSlot.Evening);
        evening.IsAvailable.Should().BeFalse();
        evening.TimeRange.Should().Be("16:00 - 19:00");
        
        var night = slots.First(s => s.TimeSlot == PickupTimeSlot.Night);
        night.IsAvailable.Should().BeFalse();
        night.TimeRange.Should().Be("19:00 - 21:00");
    }

    [Fact]
    public void GetAvailableSlots_OnSunday_NoSlotsAvailable()
    {
        // Arrange
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);

        // Act
        var slots = _sut.GetAvailableSlots(sunday);

        // Assert
        slots.Should().HaveCount(4);
        slots.Should().AllSatisfy(s => s.IsAvailable.Should().BeFalse());
    }

    #endregion

    #region IsSlotAvailable Tests

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    public void IsSlotAvailable_MorningSlot_AvailableMonToSat(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Morning);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsSlotAvailable_MorningSlot_NotAvailableOnSunday()
    {
        // Arrange
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);

        // Act
        var isAvailable = _sut.IsSlotAvailable(sunday, PickupTimeSlot.Morning);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    [InlineData(DayOfWeek.Saturday)]
    public void IsSlotAvailable_AfternoonSlot_AvailableMonToSat(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Afternoon);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Fact]
    public void IsSlotAvailable_AfternoonSlot_NotAvailableOnSunday()
    {
        // Arrange
        var sunday = GetNextDayOfWeek(DayOfWeek.Sunday);

        // Act
        var isAvailable = _sut.IsSlotAvailable(sunday, PickupTimeSlot.Afternoon);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public void IsSlotAvailable_EveningSlot_AvailableMonToFri(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Evening);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void IsSlotAvailable_EveningSlot_NotAvailableOnWeekend(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Evening);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Theory]
    [InlineData(DayOfWeek.Monday)]
    [InlineData(DayOfWeek.Tuesday)]
    [InlineData(DayOfWeek.Wednesday)]
    [InlineData(DayOfWeek.Thursday)]
    [InlineData(DayOfWeek.Friday)]
    public void IsSlotAvailable_NightSlot_AvailableMonToFri(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Night);

        // Assert
        isAvailable.Should().BeTrue();
    }

    [Theory]
    [InlineData(DayOfWeek.Saturday)]
    [InlineData(DayOfWeek.Sunday)]
    public void IsSlotAvailable_NightSlot_NotAvailableOnWeekend(DayOfWeek dayOfWeek)
    {
        // Arrange
        var date = GetNextDayOfWeek(dayOfWeek);

        // Act
        var isAvailable = _sut.IsSlotAvailable(date, PickupTimeSlot.Night);

        // Assert
        isAvailable.Should().BeFalse();
    }

    #endregion

    #region Slot Time Ranges Tests

    [Fact]
    public void GetAvailableSlots_ReturnsCorrectTimeRanges()
    {
        // Arrange
        var monday = GetNextDayOfWeek(DayOfWeek.Monday);

        // Act
        var slots = _sut.GetAvailableSlots(monday);

        // Assert
        var morning = slots.First(s => s.TimeSlot == PickupTimeSlot.Morning);
        morning.TimeRange.Should().Be("09:00 - 12:00");

        var afternoon = slots.First(s => s.TimeSlot == PickupTimeSlot.Afternoon);
        afternoon.TimeRange.Should().Be("12:00 - 16:00");

        var evening = slots.First(s => s.TimeSlot == PickupTimeSlot.Evening);
        evening.TimeRange.Should().Be("16:00 - 19:00");

        var night = slots.First(s => s.TimeSlot == PickupTimeSlot.Night);
        night.TimeRange.Should().Be("19:00 - 21:00");
    }

    #endregion

    #region Validation Rules Summary Tests

    [Fact]
    public void PickupSlotRules_MorningAndAfternoon_AvailableMonToSat()
    {
        // This test documents the business rule:
        // Morning (09:00-12:00) and Afternoon (12:00-16:00) slots are available Monday through Saturday

        var daysToTest = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
        };

        foreach (var day in daysToTest)
        {
            var date = GetNextDayOfWeek(day);
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Morning).Should().BeTrue(
                $"Morning slot should be available on {day}");
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Afternoon).Should().BeTrue(
                $"Afternoon slot should be available on {day}");
        }
    }

    [Fact]
    public void PickupSlotRules_EveningAndNight_AvailableMonToFriOnly()
    {
        // This test documents the business rule:
        // Evening (16:00-19:00) and Night (19:00-21:00) slots are available Monday through Friday only

        var weekdays = new[]
        {
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday
        };

        foreach (var day in weekdays)
        {
            var date = GetNextDayOfWeek(day);
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Evening).Should().BeTrue(
                $"Evening slot should be available on {day}");
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Night).Should().BeTrue(
                $"Night slot should be available on {day}");
        }

        var weekend = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

        foreach (var day in weekend)
        {
            var date = GetNextDayOfWeek(day);
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Evening).Should().BeFalse(
                $"Evening slot should NOT be available on {day}");
            
            _sut.IsSlotAvailable(date, PickupTimeSlot.Night).Should().BeFalse(
                $"Night slot should NOT be available on {day}");
        }
    }

    #endregion

    private DateTime GetNextDayOfWeek(DayOfWeek dayOfWeek)
    {
        var today = DateTime.UtcNow.Date;
        var daysUntilTarget = ((int)dayOfWeek - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilTarget == 0 && today.DayOfWeek != dayOfWeek)
            daysUntilTarget = 7;
        return today.AddDays(daysUntilTarget);
    }
}
