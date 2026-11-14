using Business.Domain.ValueObjects.Category;

namespace Business.Application.Interfaces.Category.Repositories;

public interface ICategoryWriteRepository
{
    void Add(Domain.Aggregates.Category category);
    Task<Domain.Aggregates.Category?> GetCategoryBySlugAsync(Slug slug, CancellationToken ct = default);
}