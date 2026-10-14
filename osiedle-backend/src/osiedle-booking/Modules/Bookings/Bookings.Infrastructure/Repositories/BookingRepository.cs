using Bookings.Appliation.Abstractions.Repositories;
using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly ApplicationDbContext _dbContext;
    
    public BookingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task<IReadOnlyList<Booking>> GetBookingsBydDateAsync(DateOnly date, StaffId staffId, CancellationToken ct)
    {
        return await _dbContext.Bookings.Where(b =>
         DateOnly.FromDateTime(b.Slot.Start.Date) == date &&
         DateOnly.FromDateTime(b.Slot.End.Date) == date &&
         b.StaffId == staffId)
         .AsNoTracking()
         .ToListAsync(ct);
    }
    
    public Task<bool> HasOverlappingAsync(StaffId staffId, TimeSlot slot, CancellationToken ct) =>
        _dbContext.Bookings.AnyAsync(b =>
            b.StaffId == staffId &&
            b.Status == BookingStatus.Confirmed &&
            b.Slot.Start < slot.End &&
            slot.Start < b.Slot.End, ct);
    
    public async Task<Booking> CreateBookingAsync(Booking booking, CancellationToken ct)
    {
         var result = _dbContext.Bookings.Add(booking);
         await _dbContext.SaveChangesAsync(ct);
         
         return result.Entity;
    }
}