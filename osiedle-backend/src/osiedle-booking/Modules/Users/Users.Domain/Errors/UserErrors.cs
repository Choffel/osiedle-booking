using osiedle_booking.BuildingBlocks;

namespace Users.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound =
        Error.NotFound("User.NotFound", "User was not found");
    
    public static readonly Error EmailNotRegistered =
        Error.NotFound("User.EmailNotRegistered", "Email address is not registered");
    
    public static readonly Error PasswordResetFailed =
        Error.Failure("User.PasswordResetFailed", "Failed to reset password");
    
    public static readonly Error EmailConfirmationFailed =
        Error.Failure("User.EmailConfirmationFailed", "Failed to confirm email");
    
    public static readonly Error RegistrationFailed =
        Error.Validation("User.RegistrationFailed", "User registration failed");

    public static readonly Error UpdateFailed =
        Error.Failure("User.UpdateFailed", "Failed to update user");
}