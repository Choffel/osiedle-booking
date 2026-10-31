using osiedle_booking.BuildingBlocks;

namespace Users.Application.Errors;

public static class RoleErrors
{
    public static readonly Error AddUserToRoleFailed =
        Error.Failure("Role.FailedToAddUserToRole", "Failed to add user to role");
}