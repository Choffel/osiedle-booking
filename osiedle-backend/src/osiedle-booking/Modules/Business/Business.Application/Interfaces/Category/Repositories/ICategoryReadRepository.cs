using Business.Application.DTOs.Category;
using Business.Domain.ValueObjects.Category;

namespace Business.Application.Interfaces.Category.Repositories;

public interface ICategoryReadRepository
{
    Task<CategoryInfoDTO?> GetCategoryByIdAsync(CategoryId id, CancellationToken ct = default);
    Task<Domain.Aggregates.Category?> GetCategoryBySlugAsync(Slug slug, CancellationToken ct = default);
    Task<bool> ValidateCategory(CategoryInfoDTO category, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryInfoDTO?>> GetAllAsync(CancellationToken ct = default);
}