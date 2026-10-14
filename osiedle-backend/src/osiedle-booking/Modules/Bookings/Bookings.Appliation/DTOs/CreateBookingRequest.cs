namespace Bookings.Appliation.DTOs;

public record CreateBookingRequest(
    Guid BusinessId,
    Guid ClientId,
    Guid StaffId,
    Guid ServiceId,
    DateTime StartTime,
    DateTime EndTime);