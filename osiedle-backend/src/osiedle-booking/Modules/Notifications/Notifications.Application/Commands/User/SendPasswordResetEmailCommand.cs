using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Notifications.Application.Commands.User;

public record SendPasswordResetEmailCommand(
    Guid UserId,
    string Email,
    string ResetLink
    );