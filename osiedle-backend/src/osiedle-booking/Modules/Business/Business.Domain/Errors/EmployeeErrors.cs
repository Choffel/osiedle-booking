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
    
    public static readonly Error FirstNameEmpty =
        Error.Validation("Employee.FirstNameEmpty", "First name cannot be empty");

    public static readonly Error LastNameEmpty =
        Error.Validation("Employee.LastNameEmpty", "Last name cannot be empty");

    public static readonly Error PositionEmpty =
        Error.Validation("Employee.PositionEmpty", "Position cannot be empty");

    public static readonly Error WorkingHoursInvalid =
        Error.Validation("Employee.WorkingHoursInvalid", "End time must be after start time");
    
    public static readonly Error InvalidDateRange =
        Error.Validation("Employee.InvalidDateRange", "End date must be after start date");
}