using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Users.Application.DTOs.Identity;

public record RegisterResponse(UserId UserId, string code);