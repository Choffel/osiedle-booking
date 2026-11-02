using Bookings.Application.Abstractions;
using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Application.Abstractions.Repositories;
using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Errors;
using Bookings.Domain.Services;
using Business.Application.Interfaces.BookableService;
using Business.Application.Interfaces.Employee;
using Business.Domain.ValueObject;
using osiedle_booking.BuildingBlocks;
using BusinessId = Bookings.Domain.Entities.Booking.Value_objects.BusinessId;

namespace Bookings.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IBookingReadRepository _bookingReadRepository;
    private readonly ISlotCalculator _slotCalculator;
    private readonly IEmployeeInternalApi _employeeInternalApi;
    private readonly IBookableServiceInternalApi _bookableServiceInternalApi;
    private readonly IUnitOfWork _unitOfWork;
    
    public BookingService(IBookingRepository bookingRepository, IBookingReadRepository bookingReadRepository,
        ISlotCalculator slotCalculator, IEmployeeInternalApi employeeInternalApi,
        IBookableServiceInternalApi bookableServiceInternalApi, IUnitOfWork unitOfWork)
    {
        _bookingRepository = bookingRepository;
        _bookingReadRepository = bookingReadRepository;
        _slotCalculator = slotCalculator;
        _employeeInternalApi = employeeInternalApi;
        _bookableServiceInternalApi = bookableServiceInternalApi;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<TimeSlot>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request, CancellationToken ct)
    {
        var availableSlots = await _bookingReadRepository.GetByStaffAndDateAsync(StaffId.From(request.StaffId), request.Date, ct);

        var barberWorkHours = TimeSlot.Create(
            request.Date.ToDateTime(new TimeOnly(9, 0)),
            request.Date.ToDateTime(new TimeOnly(18, 0)));

        /*var barberWorkHours = await _employeeInternalApi
            .GetWorkingHoursAsync(EmployeeId.From(request.StaffId),
                new DateOnly(request.Date.Year, request.Date.Month, request.Date.Day), ct);*/
        
        var occupiedSlots = availableSlots
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Select(b => b.Slot)
            .ToList();

        var serviceInfo = await _bookableServiceInternalApi.GetServiceInfoAsync(BookableServiceId.From(request.ServiceId), ct);

        var freeSlots = await _slotCalculator.CalculateAvailableSlots(
            barberWorkHours,
            occupiedSlots,
            serviceInfo.Value.Duration,
            new TimeSpan(0, 10, 0)
            );
        
        var result = Result<IReadOnlyList<TimeSlot>>.Success(freeSlots);
        
        return result;
    }

    public async Task<Result> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct)
    {
            var slot = TimeSlot.Create(request.StartTime, request.EndTime);

            var day = request.StartTime.Date;

            var barberWorkHours = TimeSlot.Create(  // TODO: Change this when methods from the Business service become available
                day.AddHours(9),
                day.AddHours(18)
            );
            /*var barberWorkHours = await _employeeInternalApi
            .GetWorkingHoursAsync(EmployeeId.From(request.StaffId),
                new DateOnly(request.Date.Year, request.Date.Month, request.Date.Day), ct);*/

            if(await _bookingRepository.HasOverlappingAsync(StaffId.From(request.StaffId), slot, ct) ||
               !await _slotCalculator.RelevantSlot(slot, barberWorkHours, ct))
                return Result.Failure(BookingError.Overlapping);
            
            var serviceInfo = await _bookableServiceInternalApi
                .GetServiceInfoAsync(BookableServiceId.From(request.ServiceId), ct);

            var moneyMok = Money.Pln(serviceInfo.Value.Price); // TODO: Change this when methods from the Business service become available

            var booking = Booking.Create(
                StaffId.From(request.StaffId),
                BusinessId.From(request.BusinessId),
                ClientId.From(request.ClientId),
                ServiceId.From(request.ServiceId),
                slot,
                moneyMok
            );

            _bookingRepository.Add(booking);
            
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success();
    }

    public async Task<Result<IReadOnlyCollection<MyReservationDTO>>> GetMyReservationsAsync(ClientId clientId, CancellationToken ct)
    {
        var reservations = await _bookingReadRepository.GetByClientIdAsync(clientId, ct);
        
        var response = new List<MyReservationDTO>(
            reservations.Select( r => new MyReservationDTO(
                r.Id.ToString(),
                r.BusinessId.ToString(),
                r.StaffId.ToString(),
                r.Slot.Start
            )));

        return Result<IReadOnlyCollection<MyReservationDTO>>.Success(response);
    }

    public async Task<Result> CancelBookingAsync(CancelBookingRequest request, ClientId clientId, CancellationToken ct)
    {
        var bookingId = BookingId.From(request.BookingId);
        
        var booking = await _bookingRepository.GetByIdAsync(bookingId, ct);
        
        if(booking is null)
        {
            return Result.Failure(BookingError.NotFound);
        }
        if(!booking.IsOwnedBy(clientId))
        {
            return Result.Failure(BookingError.NotOwnedByClient);
        }
        
        booking.CancelByClient(clientId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(ct);
        
        return Result.Success();
    }
}