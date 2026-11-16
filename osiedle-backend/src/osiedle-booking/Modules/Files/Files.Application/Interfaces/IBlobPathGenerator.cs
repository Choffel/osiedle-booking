using Files.Domain.Enums;
using Files.Domain.ValueObjects;

namespace Files.Application.Interfaces;

public interface IBlobPathGenerator
{
    string GenerateBlobName(FileCategory category, Guid? targetId, FileId fileId, string fileName);

    string GetContainerName(FileCategory category);
}
