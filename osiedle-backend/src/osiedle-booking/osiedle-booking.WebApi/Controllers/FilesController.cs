using Files.Application.DTOs;
using Files.Application.Interfaces;
using Files.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using osiedle_booking.WebApi.Extensions;

namespace osiedle_booking.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("companies/{companyId:guid}/avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileResponseDto>> UploadCompanyAvatarAsync(
        Guid companyId,
        IFormFile file,
        CancellationToken ct = default)
    {
        var request = ToUploadRequest(file);
        return (await _fileService.UploadCompanyAvatarAsync(companyId, request, ct)).ToActionResult();
    }

    [HttpPost("companies/{companyId:guid}/cover")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileResponseDto>> UploadCompanyCoverAsync(
        Guid companyId,
        IFormFile file,
        CancellationToken ct = default)
    {
        var request = ToUploadRequest(file);
        return (await _fileService.UploadCompanyCoverAsync(companyId, request, ct)).ToActionResult();
    }

    [HttpPost("employees/{employeeId:guid}/avatar")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileResponseDto>> UploadEmployeeAvatarAsync(
        Guid employeeId,
        IFormFile file,
        CancellationToken ct = default)
    {
        var request = ToUploadRequest(file);
        return (await _fileService.UploadEmployeeAvatarAsync(employeeId, request, ct)).ToActionResult();
    }

    [HttpPost("services/{serviceId:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<FileResponseDto>> UploadServicePhotoAsync(
        Guid serviceId,
        IFormFile file,
        CancellationToken ct = default)
    {
        var request = ToUploadRequest(file);
        return (await _fileService.UploadServicePhotoAsync(serviceId, request, ct)).ToActionResult();
    }

    [HttpGet("services/{serviceId:guid}/photos")]
    public async Task<ActionResult<IReadOnlyList<FileResponseDto>>> GetServicePhotosAsync(
        Guid serviceId,
        CancellationToken ct = default)
    {
        return (await _fileService.GetServicePhotosAsync(serviceId, ct)).ToActionResult();
    }

    [HttpGet("{fileId:guid}")]
    public async Task<ActionResult<FileResponseDto>> GetFileByIdAsync(
        Guid fileId,
        CancellationToken ct = default)
    {
        return (await _fileService.GetFileByIdAsync(FileId.From(fileId), ct)).ToActionResult();
    }

    [HttpGet("{fileId:guid}/download")]
    public async Task<IActionResult> DownloadFileAsync(
        Guid fileId,
        CancellationToken ct = default)
    {
        var result = await _fileService.DownloadFileAsync(FileId.From(fileId), ct);
        if (result.IsFailure)
            return result.ToActionResult().Result!;

        return File(result.Value.Stream, result.Value.ContentType, result.Value.FileName);
    }

    [HttpDelete("{fileId:guid}")]
    public async Task<IActionResult> DeleteFileAsync(
        Guid fileId,
        CancellationToken ct = default)
    {
        return (await _fileService.DeleteFileAsync(FileId.From(fileId), ct)).ToActionResult();
    }

    private static FileUploadRequest ToUploadRequest(IFormFile? file)
    {
        if (file is null)
            return new FileUploadRequest(Stream.Null, string.Empty, string.Empty, 0);

        return new FileUploadRequest(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType,
            file.Length);
    }
}
