using Files.Application.DTOs;
using Files.Application.Interfaces;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Services;

public class FileInternalApi : IFileInternalApi
{
    private readonly IFileModule _fileModule;

    public FileInternalApi(IFileModule fileModule)
    {
        _fileModule = fileModule;
    }

    public Task<Result<FileResponseDto>> UploadImageAsync(
        FileCategory category,
        Guid? targetId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UploadImageAsync(category, targetId, request, cancellationToken);
    }

    public Task<Result<FileResponseDto>> GetImageByIdAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.GetImageByIdAsync(fileId, cancellationToken);
    }

    public Task<Result<string>> GetLatestImageUrlAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.GetLatestImageUrlAsync(targetId, category, cancellationToken);
    }

    public Task<Result<IReadOnlyList<FileResponseDto>>> GetImagesByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.GetImagesByTargetIdAsync(targetId, category, cancellationToken);
    }

    public Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.DeleteFileAsync(fileId, cancellationToken);
    }
}
