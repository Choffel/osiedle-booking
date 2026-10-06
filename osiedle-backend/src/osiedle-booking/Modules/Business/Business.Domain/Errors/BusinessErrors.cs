using osiedle_booking.BuildingBlocks;

namespace Business.Domain.Errors;

public static  class BusinessErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Business.NotFound", "Business not found");

    public static readonly Error Archived =
        Error.Conflict("Business.Archived", "Business is archived, modification is not possible");

    public static readonly Error AlreadyArchived =
        Error.Conflict("Business.AlreadyArchived", "Business is already archived");

    public static readonly Error NameEmpty =
        Error.Validation("Business.NameEmpty", "Name cannot be empty");
}