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
    
    public async Task<CategoryInfoDTO?> GetCategoryByIdAsync(CategoryId id, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryInfoDTO(c.Id.Value, c.Name, c.Slug.Value))
            .FirstOrDefaultAsync(ct);
    }
    
    public async Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken ct = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .AnyAsync(ct);
    }
}