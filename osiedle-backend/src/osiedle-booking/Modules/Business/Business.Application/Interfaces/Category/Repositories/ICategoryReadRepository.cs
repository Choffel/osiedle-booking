using Business.Application.DTOs.Category;
using Business.Domain.ValueObjects.Category;

namespace Business.Application.Interfaces.Category.Repositories;

public interface ICategoryReadRepository
{
    Task<CategoryInfoDTO?> GetCategoryByIdAsync(CategoryId id, CancellationToken ct = default);
    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken ct = default);
}