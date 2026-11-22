using Files.Application.DTOs;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Interfaces;

public interface IFileInternalApi
{
    Task<Result<FileResponseDto>> UploadServicePhotoAsync(
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

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

    Task<Result<FileResponseDto>> UpdateServicePhotoAsync(
        FileId fileId,
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UpdateCompanyAvatarAsync(
        FileId fileId,
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UpdateCompanyCoverAsync(
        FileId fileId,
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<FileResponseDto>> UpdateEmployeeAvatarAsync(
        FileId fileId,
        Guid employeeId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default);
}