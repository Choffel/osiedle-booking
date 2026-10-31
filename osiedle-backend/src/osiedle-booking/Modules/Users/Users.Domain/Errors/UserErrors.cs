using osiedle_booking.BuildingBlocks;

namespace Users.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User was not found");
    
    
    public static readonly Error EmailNotConfirmed =
        Error.Validation("User.EmailNotConfirmed", "Email is not confirmed");
    
    public static readonly Error EmailNotRegistered =
        Error.NotFound("User.EmailNotRegistered", "Email address is not registered");
    
    public static readonly Error PasswordResetFailed =
        Error.Failure("User.PasswordResetFailed", "Failed to reset password");
    
    public static readonly Error EmailConfirmationFailed =
        Error.Failure("User.EmailConfirmationFailed", "Failed to confirm email");
    
    /*public static readonly Error RoleNotFound =
        Error.NotFound("User.RoleNotFound", "Role 'User' does not exist");
    
    public static readonly Error InvalidRefreshToken =
        Error.Validation("User.InvalidRefreshToken", "Invalid or expired refresh token");
    
    public static readonly Error CannotRevokeCurrentSession =
        Error.Conflict("User.CannotRevokeCurrentSession", "Cannot revoke the current session");
        */
    
    public static readonly Error RegistrationFailed =
        Error.Validation("User.RegistrationFailed", "User registration failed");

    public static readonly Error UpdateFailed =
        Error.Failure("User.UpdateFailed", "Failed to update user");
    
    /*public static readonly Error LockedOut =
        Error.Conflict("User.IsLockedOut", "User is locked out");
    
    public static readonly Error ResetPasswordFailed =
        Error.Failure("User.FailedToResetPassword", "Failed to reset password");
        */
    
}