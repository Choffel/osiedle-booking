using osiedle_booking.BuildingBlocks;

namespace Business.Domain.Errors;

public static class BookableServiceErrors
{
    public static readonly Error AlreadyExists =
        Error.Conflict("BookableService.AlreadyExists", "Bookable service already exists");
    
    public static readonly Error NotFound =
        Error.NotFound("BookableService.NotFound", "Bookable service was not found");
    
    public static readonly Error NotActive =
        Error.Conflict("BookableService.NotActive", "Bookable service is not active");
    
    public static readonly Error AlreadyActive =
        Error.Conflict("BookableService.AlreadyActive", "Bookable service is already active");
    
    public static readonly Error NameAlreadyTaken =
        Error.Conflict("BookableService.NameAlreadyTaken", "Bookable service name is already taken");
    
    public static readonly Error EmployeeNotFound =
        Error.NotFound("BookableService.EmployeeNotFound", "Employee not found");
    
    public static readonly Error NotFoundOrAccessDenied =
        Error.NotFound("BookableService.NotfoundOrAccessDenied", "Bookable service not found or access denied");
    
    public static readonly Error AlreadyArchived =
        Error.Conflict("BookableService.AlreadyArchived", "Bookable service is already archived");
}
