using Business.Domain.ValueObjects.Category;

namespace Business.Application.Interfaces.Category.Repositories;

public interface ICategoryWriteRepository
{
    Task<Domain.Aggregates.Category?> GetCategoryByIdAsync(CategoryId id, CancellationToken ct = default);
    void Add(Domain.Aggregates.Category category);
}