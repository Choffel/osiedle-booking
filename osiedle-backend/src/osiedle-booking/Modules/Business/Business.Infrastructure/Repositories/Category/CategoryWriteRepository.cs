using Business.Application.Interfaces.Category.Repositories;
using Business.Domain.ValueObjects.Category;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Category;

public class CategoryWriteRepository : ICategoryWriteRepository
{
    private readonly BusinessDbContext _context;
    public CategoryWriteRepository(BusinessDbContext context)
    {
        _context = context;
    }

    public void Add(Domain.Aggregates.Category category)
    {
        _context.Categories.Add(category);
    }
    
    public async Task<Domain.Aggregates.Category?> GetCategoryBySlugAsync(Slug slug, CancellationToken ct = default)
    {
        return await _context.Categories
            .Where(c => c.Slug == slug)
            .OrderBy(c => c.DisplayOrder)
            .FirstOrDefaultAsync(ct);
    }
}