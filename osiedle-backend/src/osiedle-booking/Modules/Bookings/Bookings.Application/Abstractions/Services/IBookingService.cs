using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using osiedle_booking.BuildingBlocks;

namespace Bookings.Application.Abstractions.Services;

public interface IBookingService
{
    Task<Result<IReadOnlyList<TimeSlot>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request, CancellationToken ct);
    Task<Result<bool>> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct);
    Task<Result<IReadOnlyCollection<MyReservationDTO>>> GetMyReservationsAsync(CancellationToken ct);
    Task<Result<bool>> CancelBookingAsync(CancelBookingRequest request, CancellationToken ct);
}