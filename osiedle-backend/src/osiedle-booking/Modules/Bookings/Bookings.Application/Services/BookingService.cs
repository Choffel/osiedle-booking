using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Application.Abstractions.Repositories;
using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Services;
using osiedle_booking.BuildingBlocks;
using osiedle_booking.BuildingBlocks.Interfaces;

namespace Bookings.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISlotCalculator _slotCalculator;
    private readonly IUserContext _userContext;
    
    public BookingService(IBookingRepository bookingRepository, ISlotCalculator slotCalculator, IUserContext userContext)
    {
        _bookingRepository = bookingRepository;
        _slotCalculator = slotCalculator;
        _userContext = userContext;
    }

    public async Task<Result<IReadOnlyList<TimeSlot>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request, CancellationToken ct)
    {
        var availableSlots = await _bookingRepository.GetBookingsBydDateAsync(request.Date, new StaffId(request.StaffId), ct);

        var barberWorkHours = TimeSlot.Create(
            request.Date.ToDateTime(new TimeOnly(9, 0)),
            request.Date.ToDateTime(new TimeOnly(18, 0)));
        
        var freeSlots = await _slotCalculator.CalculateAvailableSlots(
            barberWorkHours,
            availableSlots.Select(b => b.Slot).ToList(),
            new TimeSpan(request.TimeForServices.Hour, request.TimeForServices.Minute, 0),
            new TimeSpan(0, 10, 0)
            );

        var result = Result<IReadOnlyList<TimeSlot>>.Success(freeSlots);
            
        return result;
    }

    public async Task<Result<bool>> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct)
    {
        var slot = TimeSlot.Create(request.StartTime, request.EndTime);
        
        var day = request.StartTime.Date;

        var barberWorkHours = TimeSlot.Create(
            day.AddHours(9),
            day.AddHours(18)
        );
        
        if(await _bookingRepository.HasOverlappingAsync(new StaffId(request.StaffId), slot, ct) ||
           !await _slotCalculator.RelevantSlot(slot, barberWorkHours, ct))
        {
            return Result<bool>.Failure("Overlapping booking found.");
        }
        
        var moneyMok = new Money(100);
        
        var booking = Booking.Create(
            new BusinessId(request.BusinessId),
            new StaffId(request.StaffId),
            new ClientId(request.ClientId),
            new ServiceId(request.ServiceId),
            slot,
            moneyMok
        );

        await _bookingRepository.CreateBookingAsync(booking, ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyCollection<MyReservationDTO>>> GetMyReservationsAsync(CancellationToken ct)
    {
        var userId = new ClientId(_userContext.UserId);
        var reservations = await _bookingRepository.GetBookingsByClientIdIdAsync(userId, ct);
        
        var response = new List<MyReservationDTO>(
            reservations.Select( r => new MyReservationDTO(
                r.Id.ToString(),
                r.BusinessId.ToString(),
                r.StaffId.ToString(),
                r.Slot.Start
            )));

        return Result<IReadOnlyCollection<MyReservationDTO>>.Success(response);
    }

    public async Task<Result<bool>> CancelBookingAsync(CancelBookingRequest request, CancellationToken ct)
    {
        var booking = await _bookingRepository.GetBookingByIdAsync(new BookingId(request.BookingId), ct, true);
        if(booking == null)
        {
            return Result<bool>.Failure("Booking not found.");
        }
        if(!booking.IsOwnedBy(new ClientId(_userContext.UserId)))
        {
            return Result<bool>.Failure("You cannot delete a reservation that isn't yours.");
        }
        
        booking.Delete();
        await _bookingRepository.Update(booking, ct);
        
        return Result<bool>.Success(true);
    }
}