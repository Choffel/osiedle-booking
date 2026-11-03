using Bookings.Application.Abstractions.Repositories;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Repositories.Booking;

public class BookingReadRepository : IBookingReadRepository
{
    private readonly ApplicationDbContext _dbContext;

    public BookingReadRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Domain.Entities.Booking.Booking>> GetByStaffAndDateAsync(StaffId staffId,
        DateOnly date, CancellationToken ct = default)
    {
        return await _dbContext.Bookings.Where(b =>
            b.StaffId == staffId &&
            DateOnly.FromDateTime(b.Slot.Start.Date) == date)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Domain.Entities.Booking.Booking>> GetByClientIdAsync(ClientId clientId,
        CancellationToken ct = default)
    {
        return await _dbContext.Bookings.Where(b => b.ClientId == clientId)
            .AsNoTracking()
            .ToListAsync(ct);
    }
}