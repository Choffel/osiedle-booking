using Business.Application.DTOs.Category;
using Business.Application.Interfaces.Category.Repositories;
using Business.Domain.ValueObjects.Category;
using Business.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Repositories.Category;

public class CategoryReadRepository : ICategoryReadRepository
{
    private readonly BusinessDbContext _context;
    public CategoryReadRepository(BusinessDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> ValidateCategory(CategoryInfoDTO category, CancellationToken ct = default)
    {
        var slug = Slug.From(category.Slug);
        var name = category.Name.ToLower();

        return await _context.Categories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(c =>
                    c.Name.ToLower() == name ||
                    c.Slug == slug,
                ct);
    }
    
    public async Task<IReadOnlyList<CategoryInfoDTO?>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder == 0)
            .ThenBy(c => c.DisplayOrder)       
            .ThenBy(c => c.Name)
            .Select(c => new CategoryInfoDTO(
                c.Id.Value,
                c.Name,
                c.Slug.Value,
                c.DisplayOrder))
            .ToListAsync(ct);
    }
    
    public async Task<Domain.Aggregates.Category?> GetCategoryBySlugAsync(Slug slug, CancellationToken ct = default)
    {
        return await _context.Categories
            .Where(c => c.Slug == slug)
            .OrderBy(c => c.DisplayOrder)
            .FirstOrDefaultAsync(ct);
    }
}