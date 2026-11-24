namespace Bookings.Application.DTOs;

public record CreateBookingRequest(
    Guid BusinessId,
    Guid StaffId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime);