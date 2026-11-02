using osiedle_booking.BuildingBlocks;

namespace Business.Domain.Errors;

public static class WorkingHoursErrors
{
    public static readonly Error NotFound = 
        Error.NotFound("WorkingHours not found", "working hours not found");
}