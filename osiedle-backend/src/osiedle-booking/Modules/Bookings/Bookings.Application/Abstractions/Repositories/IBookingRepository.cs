using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Application.Abstractions.Repositories;

public interface IBookingRepository
{
    Task<IReadOnlyList<Booking>> GetBookingsBydDateAsync(DateOnly date, StaffId staffId, CancellationToken ct);
    Task<bool> HasOverlappingAsync(StaffId staffId, TimeSlot slot, CancellationToken ct);
    Task<Booking> CreateBookingAsync(Booking booking, CancellationToken ct);
    Task<IReadOnlyCollection<Booking>> FindBookingsByClientIdIdAsync(ClientId id,CancellationToken ct);
    Task<IReadOnlyCollection<Booking>> GetBookingsByClientIdIdAsync(ClientId id,CancellationToken ct);
    Task<Booking?> FindBookingByIdAsync(BookingId id, CancellationToken ct);
    Task<Booking?> GetBookingByIdAsync(BookingId id, CancellationToken ct);
    Task<Booking> Update(Booking booking, CancellationToken ct);
}