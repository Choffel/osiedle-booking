using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Value_objects;
using osiedle_booking.BuildingBlocks;

namespace Bookings.Application.Abstractions.Services;

public interface IBookingService
{
    Task<Result<IReadOnlyList<TimeSlot>>> GetAvailableSlotsAsync(GetAvailableSlotsRequest request, CancellationToken ct);
    Task<Result> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct);
    Task<Result<IReadOnlyCollection<MyReservationDTO>>> GetMyReservationsAsync(ClientId clientId, CancellationToken ct);
    Task<Result> CancelBookingAsync(CancelBookingRequest request, ClientId clientId, CancellationToken ct);
}