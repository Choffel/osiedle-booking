namespace Bookings.Application.DTOs;

public record GetAvailableSlotsRequest(
    Guid StaffId,
    TimeOnly TimeForServices,
    DateOnly Date);