using osiedle_booking.BuildingBlocks;

namespace Users.Application.Errors;

public static class TokenErrors
{
    public static readonly Error CannotRevokeCurrentSession =
        Error.Conflict("Token.CannotRevokeCurrentSession", "Cannot revoke the current session");
    
    public static readonly Error InvalidRefreshToken =
        Error.Validation("Token.InvalidRefreshToken", "Invalid or expired refresh token");
}