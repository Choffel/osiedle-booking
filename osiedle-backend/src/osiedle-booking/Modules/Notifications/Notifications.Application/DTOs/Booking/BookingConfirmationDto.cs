using System;

namespace Notifications.Application.DTOs.Booking;

//TODO:Time slot
public record BookingConfirmationDto(
    string? ClientPhoneNumber,
    string? ClientEmail,
    TimeSpan ServiceDuration,
    string ServiceDescription,
    decimal Price
    );