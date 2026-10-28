using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Notifications.Application.Commands;

public record SendWelcomeEmailCommand(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string ConfirmationLink
    );