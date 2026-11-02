using Bookings.Application.Abstractions.Repositories;
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
    
    /*public async Task<IReadOnlyList<Domain.Entities.Booking.Booking>> GetBookingsBydDateAsync(DateOnly date, StaffId staffId, CancellationToken ct)
    {
        return await _dbContext.Bookings.Where(b =>
         DateOnly.FromDateTime(b.Slot.Start.Date) == date &&
         DateOnly.FromDateTime(b.Slot.End.Date) == date &&
         b.StaffId == staffId)
         .AsNoTracking()
         .ToListAsync(ct);
    }*/

    public async Task<Domain.Entities.Booking.Booking?> GetByIdAsync(BookingId id, CancellationToken ct = default)
    {
        return await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public Task<bool> HasOverlappingAsync(StaffId staffId, TimeSlot slot, CancellationToken ct)
    {
        return _dbContext.Bookings.AnyAsync(b =>
            b.StaffId == staffId &&
            b.Status == BookingStatus.Confirmed &&
            b.Slot.Start < slot.End &&
            slot.Start < b.Slot.End, ct);
    }
    
    public void Add(Domain.Entities.Booking.Booking booking)
    {
        _dbContext.Bookings.Add(booking);
    }

    /*public async Task<Domain.Entities.Booking.Booking> CreateBookingAsync(Domain.Entities.Booking.Booking booking, CancellationToken ct)
    {
         var result = _dbContext.Bookings.Add(booking);
         await _dbContext.SaveChangesAsync(ct);
         
         return result.Entity;
    }*/

    /*public async Task<IReadOnlyCollection<Domain.Entities.Booking.Booking>> FindBookingsByClientIdIdAsync(ClientId id, CancellationToken ct)
    {
        var result = _dbContext.Bookings.Where(b => b.ClientId == id).AsNoTracking().ToList();
        return result;
    }*/
    
    /*public async Task<IReadOnlyCollection<Domain.Entities.Booking.Booking>> GetBookingsByClientIdIdAsync(ClientId id, CancellationToken ct)
    {
        var result = _dbContext.Bookings.Where(b => b.ClientId == id).ToList();
        return result;
    }
    
    public async Task<Domain.Entities.Booking.Booking> FindBookingByIdAsync(BookingId id, CancellationToken ct)
    {
        var result = await _dbContext.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        return result;
    }
    
    public async Task<Domain.Entities.Booking.Booking> GetBookingByIdAsync(BookingId id, CancellationToken ct)
    {
        var result = await _dbContext.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        return result;
    }*/
    
}