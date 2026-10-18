using osiedle_booking.BuildingBlocks;

namespace Business.Domain.Errors;

public static class BookableServiceErrors
{
    public static readonly Error NotFound =
        Error.NotFound("BookableService.NotFound", "Bookable service was not found");
    
    public static readonly Error NotActive =
        Error.Conflict("BookableService.NotActive", "Bookable service is not active");
}
