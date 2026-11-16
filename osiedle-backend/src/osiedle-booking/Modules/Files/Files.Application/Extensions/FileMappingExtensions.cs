using Files.Application.DTOs;
using Files.Domain.Aggregates;

namespace Files.Application.Extensions;

public static class FileMappingExtensions
{
    public static FileResponseDto ToResponseDto(this StoredFile file)
    {
        return new FileResponseDto(
            Id: file.Id.Value,
            FileName: file.OriginalFileName,
            ContentType: file.ContentType.Value,
            Category: file.Category,
            BlobUri: file.BlobUri,
            RelatedEntityId: file.RelatedEntityId,
            SizeInBytes: file.SizeInBytes,
            CreatedAt: file.CreatedAt);
    }
}
