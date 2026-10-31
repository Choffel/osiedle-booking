using osiedle_booking.BuildingBlocks;

namespace Users.Application.Errors;

public static class IdentityErrors
{
    public static readonly Error NotFound =
        Error.NotFound("IdentityError.NotFound", "User was not found");
    
    public static readonly Error LockedOut =
        Error.Conflict("Identity.IsLockedOut", "User is locked out");
    
    public static readonly Error InvalidCredentials =
        Error.Validation("Identity.InvalidCredentials", "Invalid email or password");
    
    public static readonly Error EmailConfirmationFailed =
        Error.Failure("Identity.EmailConfirmationFailed", "Failed to confirm email");
    
    public static Error FromIdentityResult( string description) =>
        Error.Validation("IdentityError", description);
}