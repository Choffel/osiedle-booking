using Files.Application.Interfaces;
using Files.Infrastructure.Persistence;

namespace Files.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly FilesDbContext _context;

    public UnitOfWork(FilesDbContext context)
    {
        _context = context;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
