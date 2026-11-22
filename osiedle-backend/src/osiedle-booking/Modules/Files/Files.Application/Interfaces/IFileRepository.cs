using Files.Domain.Aggregates;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;

namespace Files.Application.Interfaces;

public interface IFileRepository
{
    Task<StoredFile?> GetByIdAsync(FileId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredFile>> GetByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default);

    Task<StoredFile?> GetLatestByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default);

    void Add(StoredFile file);

    void Remove(StoredFile file);
}
