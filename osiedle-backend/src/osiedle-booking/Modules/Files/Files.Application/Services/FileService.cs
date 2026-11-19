using Files.Application.DTOs;
using Files.Application.Extensions;
using Files.Application.Interfaces;
using Files.Domain.Aggregates;
using Files.Domain.Enums;
using Files.Domain.Errors;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Application.Services;

public class FileService : IFileService, IFileModule
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IFileRepository _fileRepository;
    private readonly IImageValidator _imageValidator;
    private readonly IBlobPathGenerator _blobPathGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public FileService(
        IBlobStorageService blobStorageService,
        IFileRepository fileRepository,
        IImageValidator imageValidator,
        IBlobPathGenerator blobPathGenerator,
        IUnitOfWork unitOfWork)
    {
        _blobStorageService = blobStorageService;
        _fileRepository = fileRepository;
        _imageValidator = imageValidator;
        _blobPathGenerator = blobPathGenerator;
        _unitOfWork = unitOfWork;
    }

    public Task<Result<FileResponseDto>> UploadCompanyAvatarAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
            return Task.FromResult(Result<FileResponseDto>.Failure(FileErrors.TargetEntityRequired));

        return UploadInternalAsync(FileCategory.CompanyAvatar, companyId, request, cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadCompanyCoverAsync(
        Guid companyId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
            return Task.FromResult(Result<FileResponseDto>.Failure(FileErrors.TargetEntityRequired));

        return UploadInternalAsync(FileCategory.CompanyCover, companyId, request, cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadEmployeeAvatarAsync(
        Guid employeeId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return Task.FromResult(Result<FileResponseDto>.Failure(FileErrors.TargetEntityRequired));

        return UploadInternalAsync(FileCategory.EmployeeAvatar, employeeId, request, cancellationToken);
    }

    public Task<Result<FileResponseDto>> UploadServicePhotoAsync(
        Guid serviceId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (serviceId == Guid.Empty)
            return Task.FromResult(Result<FileResponseDto>.Failure(FileErrors.TargetEntityRequired));

        return UploadInternalAsync(FileCategory.ServicePhoto, serviceId, request, cancellationToken);
    }

    public async Task<Result<FileResponseDto>> GetFileByIdAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file is null)
            return Result<FileResponseDto>.Failure(FileErrors.NotFound);

        return file.ToResponseDto();
    }

    public async Task<Result<FileResponseDto>> GetLatestImageAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetLatestByTargetIdAsync(targetId, category, cancellationToken);
        if (file is null)
            return Result<FileResponseDto>.Failure(FileErrors.NotFound);

        return file.ToResponseDto();
    }

    public async Task<Result<IReadOnlyList<FileResponseDto>>> GetServicePhotosAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var files = await _fileRepository.GetByTargetIdAsync(
            serviceId,
            FileCategory.ServicePhoto,
            cancellationToken);

        IReadOnlyList<FileResponseDto> result = files.Select(f => f.ToResponseDto()).ToList();
        return Result<IReadOnlyList<FileResponseDto>>.Success(result);
    }

    public async Task<Result<FileDownloadDto>> DownloadFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file is null)
            return Result<FileDownloadDto>.Failure(FileErrors.NotFound);

        var stream = await _blobStorageService.OpenReadStreamAsync(
            file.ContainerName,
            file.BlobName,
            cancellationToken);

        return new FileDownloadDto(stream, file.ContentType.Value, file.OriginalFileName);
    }

    public async Task<Result> DeleteFileAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetByIdAsync(fileId, cancellationToken);
        if (file is null)
            return Result.Failure(FileErrors.NotFound);

        await _blobStorageService.DeleteAsync(
            file.ContainerName,
            file.BlobName,
            cancellationToken);

        _fileRepository.Remove(file);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public Task<Result<FileResponseDto>> UploadImageAsync(
        FileCategory category,
        Guid? targetId,
        FileUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        return UploadInternalAsync(category, targetId, request, cancellationToken);
    }

    public Task<Result<FileResponseDto>> GetImageByIdAsync(
        FileId fileId,
        CancellationToken cancellationToken = default)
    {
        return GetFileByIdAsync(fileId, cancellationToken);
    }

    public async Task<Result<string>> GetLatestImageUrlAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        var file = await _fileRepository.GetLatestByTargetIdAsync(targetId, category, cancellationToken);
        if (file is null)
            return Result<string>.Failure(FileErrors.NotFound);

        return Result<string>.Success(file.BlobUri);
    }

    public async Task<Result<IReadOnlyList<FileResponseDto>>> GetImagesByTargetIdAsync(
        Guid targetId,
        FileCategory category,
        CancellationToken cancellationToken = default)
    {
        var files = await _fileRepository.GetByTargetIdAsync(targetId, category, cancellationToken);
        IReadOnlyList<FileResponseDto> result = files.Select(f => f.ToResponseDto()).ToList();
        return Result<IReadOnlyList<FileResponseDto>>.Success(result);
    }

    private async Task<Result<FileResponseDto>> UploadInternalAsync(
        FileCategory category,
        Guid? targetId,
        FileUploadRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = _imageValidator.Validate(request, category);
        if (validationResult.IsFailure)
            return Result<FileResponseDto>.Failure(validationResult.Error);

        var fileId = FileId.New();
        var containerName = _blobPathGenerator.GetContainerName(category);
        var blobName = _blobPathGenerator.GenerateBlobName(category, targetId, fileId, request.FileName);

        var blobUri = await _blobStorageService.UploadAsync(
            containerName,
            blobName,
            request.Content,
            request.ContentType,
            cancellationToken);

        var contentType = ContentType.From(request.ContentType);
        var storedFile = StoredFile.Create(
            fileId,
            request.FileName,
            contentType,
            category,
            containerName,
            blobName,
            blobUri,
            request.Length,
            targetId);

        _fileRepository.Add(storedFile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return storedFile.ToResponseDto();
    }
}
