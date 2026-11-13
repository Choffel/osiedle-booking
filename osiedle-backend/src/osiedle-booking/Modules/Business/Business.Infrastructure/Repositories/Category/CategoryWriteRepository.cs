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
    
    public async Task<Domain.Aggregates.Category?> GetCategoryByIdAsync(CategoryId id, CancellationToken ct = default)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public void Add(Domain.Aggregates.Category category)
    {
        _context.Categories.Add(category);
    }
}