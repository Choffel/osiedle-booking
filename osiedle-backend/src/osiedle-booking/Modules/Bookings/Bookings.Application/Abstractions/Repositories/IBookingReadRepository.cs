using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Application.Abstractions.Repositories;

public interface IBookingReadRepository
{
    Task<IReadOnlyList<Booking>> GetByStaffAndDateAsync(StaffId staffId, DateOnly date, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByClientIdAsync(ClientId clientId, CancellationToken ct = default);
}