using Bookings.Appliation.Abstractions.Services;
using Bookings.Appliation.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Appliation.Abstractions.Repositories;
using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Services;
using osiedle_booking.BuildingBlocks;

namespace Bookings.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ISlotCalculator _slotCalculator;
    
    public BookingService(IBookingRepository bookingRepository, ISlotCalculator slotCalculator)
    {
        _bookingRepository = bookingRepository;
        _slotCalculator = slotCalculator;
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
        if(await _bookingRepository.HasOverlappingAsync(new StaffId(request.StaffId), slot, ct))
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
}