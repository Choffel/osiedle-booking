using Business.Application.DTOs.Category;
using Business.Domain.ValueObjects.Category;
using osiedle_booking.BuildingBlocks;

namespace Business.Application.Interfaces.Category.Services;

public interface ICategoryService
{
    Task<Result> CreateAsync(CategoryCreateRequestDTO request, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CategoryResponseDTO>>> GetCategoriesAsync(CancellationToken ct = default);
    Task<Result> ArchiveBySlug(Slug slug, CancellationToken ct = default);
}