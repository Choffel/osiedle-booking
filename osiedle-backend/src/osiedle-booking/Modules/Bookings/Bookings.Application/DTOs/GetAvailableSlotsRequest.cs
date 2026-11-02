namespace Bookings.Application.DTOs;

public record GetAvailableSlotsRequest(
    Guid StaffId,
    Guid ServiceId,
    DateOnly Date); // TODO 