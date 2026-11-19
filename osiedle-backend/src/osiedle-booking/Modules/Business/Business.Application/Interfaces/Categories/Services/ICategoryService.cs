using Business.Application.DTOs.CategoryDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Categories.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default);
}
