namespace Bookings.Application.DTOs;

public sealed record MyReservationDTO(
    string Title,
    string Company,
    string Staff,
    DateTime Date
    );