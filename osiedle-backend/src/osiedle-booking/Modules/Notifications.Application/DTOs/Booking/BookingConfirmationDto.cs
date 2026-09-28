namespace Notifications.Application.DTOs.Booking;

public record BookingConfirmationDto(
    string ClientName,
    string BusinessName,
    string ServiceName,
    DateTime AppointmentTime,
    string Address,
    decimal Price
    );