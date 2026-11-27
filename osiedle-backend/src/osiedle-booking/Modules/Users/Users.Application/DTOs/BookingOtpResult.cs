using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Users.Application.DTOs;

public sealed record BookingOtpResult(UserId UserId, string Code, bool IsNewUser);