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

    public Task<Result<FileResponseDto>> UploadServicePhotoAsync(
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UploadImageAsync(
            FileCategory.ServicePhoto,
            serviceId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadCompanyAvatarAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UploadImageAsync(
            FileCategory.CompanyAvatar,
            companyId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadCompanyCoverAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UploadImageAsync(
            FileCategory.CompanyCover,
            companyId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadEmployeeAvatarAsync(
        Guid employeeId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UploadImageAsync(
            FileCategory.EmployeeAvatar,
            employeeId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UpdateServicePhotoAsync(
        FileId fileId,
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UpdateImageAsync(
            fileId,
            FileCategory.ServicePhoto,
            serviceId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UpdateCompanyAvatarAsync(
        FileId fileId,
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UpdateImageAsync(
            fileId,
            FileCategory.CompanyAvatar,
            companyId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UpdateCompanyCoverAsync(
        FileId fileId,
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UpdateImageAsync(
            fileId,
            FileCategory.CompanyCover,
            companyId,
            request,
            cancellationToken);
    }

    public Task<Result<FileResponseDto>> UpdateEmployeeAvatarAsync(
        FileId fileId,
        Guid employeeId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.UpdateImageAsync(
            fileId,
            FileCategory.EmployeeAvatar,
            employeeId,
            request,
            cancellationToken);
    }

    public Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        return _fileModule.DeleteFileAsync(fileId, cancellationToken);
    }
}
