using Business.Application.DTOs.CategoryDTOs;
using Business.Application.Interfaces.Categories.Services;
using Business.Domain.Enums;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Services.CategoryServices;

public class CategoryService : ICategoryService
{
    public Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default)
    {
        var categories = Enum.GetValues<Domain.Enums.Category>()
            .Select(c => new CategoryResponseDTO((int)c, c.ToString(), c.ToDisplayName()))
            .ToList();

        return Task.FromResult(Result<IReadOnlyList<CategoryResponseDTO>>.Success(categories));
    }
}
