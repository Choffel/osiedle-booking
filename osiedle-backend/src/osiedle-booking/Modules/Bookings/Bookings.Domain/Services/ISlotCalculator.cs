using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Domain.Services;

public interface ISlotCalculator
{
    Task<IReadOnlyList<TimeSlot>> CalculateAvailableSlots(TimeSlot workingHours,
        IReadOnlyList<TimeSlot> occupiedSlots,
        TimeSpan serviceDuration,
        TimeSpan step);
}