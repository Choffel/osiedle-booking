namespace Business.Domain.Errors;

using osiedle_booking.BuildingBlocks;

public static class EmployeeErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Employee.NotFound", "Employee was not found");
    
    public static readonly Error AlreadyExists = 
        Error.Conflict("Employee.AlreadyExists", "Employee with this email already exists");
    
    public static readonly Error Archived =
        Error.Conflict("Employee.Archived", "Employee is archived, modification is not possible");

    public static readonly Error BusinessNotFound =
        Error.Validation("Employee.BusinessNotFound", "Business with the given id does not exist");
    
    public static readonly Error InvalidDateRange =
        Error.Validation("Employee.InvalidDateRange", "End date must be after start date");
}