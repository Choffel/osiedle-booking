using Files.Domain.Enums;

namespace Files.Application.DTOs;

public record FileResponseDto(
    Guid Id,
    string FileName,
    string ContentType,
    FileCategory Category,
    string BlobUri,
    Guid? RelatedEntityId,
    long SizeInBytes,
    DateTime CreatedAt);
