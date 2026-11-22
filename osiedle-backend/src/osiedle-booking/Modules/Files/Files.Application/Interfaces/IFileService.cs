using Files.Application.DTOs;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Interfaces;

public interface IFileService
{
    Task<Result<FileResponseDto>> UploadCompanyAvatarAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UploadCompanyCoverAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UploadEmployeeAvatarAsync(
        Guid employeeId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UploadServicePhotoAsync(
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> GetFileByIdAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<FileResponseDto>>> GetServicePhotosAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default);

    Task<Result<FileDownloadDto>> DownloadFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);
}
