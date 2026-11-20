using Business.Application.DTOs;
using Business.Application.DTOs.CategoryDTOs;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Categories.Services;

public interface ICategoryService
{
    Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default);

    Task<Result<IReadOnlyList<BusinessResponseDto>>> GetAllBusinessesByCategoryAsync(
        string slug,
        CancellationToken ct = default);
}
