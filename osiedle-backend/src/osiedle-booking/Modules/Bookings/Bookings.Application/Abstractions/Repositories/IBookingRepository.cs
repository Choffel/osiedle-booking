using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct = default);
    Task<bool> HasOverlappingAsync(StaffId staffId, TimeSlot slot, CancellationToken ct = default);
    void Add(Booking booking);
}