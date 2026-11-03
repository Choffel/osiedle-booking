using System;

namespace Notifications.Application.DTOs.Booking;

public record BookingCancellationDto(
    Guid? ClientId,
    Guid? BookingId,
    string? ClientEmail,
    string? ClientPhoneNumber
);