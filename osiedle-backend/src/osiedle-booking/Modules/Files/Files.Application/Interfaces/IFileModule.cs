using Files.Application.DTOs;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Interfaces;

public interface IFileModule
{
    Task<Result<FileResponseDto>> UploadImageAsync(
        FileCategory category,
        Guid? targetId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> GetImageByIdAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);

    Task<Result<string>> GetLatestImageUrlAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<FileResponseDto>>> GetImagesByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);
}
