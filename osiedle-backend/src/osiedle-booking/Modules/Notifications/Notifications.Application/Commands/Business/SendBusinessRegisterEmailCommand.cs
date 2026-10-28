using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Notifications.Application.Commands.Business;

public record SendBusinessRegisterEmailCommand(
    Guid BusinessId,
    Guid OwnerId, //userId
    string Email,
    string BusinessName,
    string BusinessAddress
    );