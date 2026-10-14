namespace Bookings.Appliation.DTOs;

public record GetAvailableSlotsRequest(
    Guid StaffId,
    TimeOnly TimeForServices,
    DateOnly Date);