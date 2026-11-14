using osiedle_booking.BuildingBlocks;

namespace Business.Domain.Errors;

public static class CategoryErrors
{
    public static readonly Error AlreadyExists =
        Error.Conflict("Category.AlreadyExists.", "Category already exists.");
    
    public static readonly Error NotFound =
        Error.NotFound("Category.NotFound.", "Category not found.");
}