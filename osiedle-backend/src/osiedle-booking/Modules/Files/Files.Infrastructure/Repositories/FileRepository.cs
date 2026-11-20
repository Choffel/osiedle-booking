using Files.Application.Interfaces;
using Files.Domain.Aggregates;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;
using Files.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Files.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly FilesDbContext _context;

    public FileRepository(FilesDbContext context)
    {
        _context = context;
    }

    public async Task<StoredFile?> GetByIdAsync(
        FileId id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<StoredFile>> GetByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .AsNoTracking()
            .Where(x => x.Category == category && x.RelatedEntityId == targetId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<StoredFile?> GetLatestByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .AsNoTracking()
            .Where(x => x.Category == category && x.RelatedEntityId == targetId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(StoredFile file)
    {
        _context.Files.Add(file);
    }

    public void Remove(StoredFile file)
    {
        _context.Files.Remove(file);
    }
}
