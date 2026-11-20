using Files.Application.DTOs;
using Files.Application.Interfaces;
using Files.Domain.Enums;
using Files.Domain.Errors;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks;

namespace Files.Infrastructure.Validators;

public class ImageValidator : IImageValidator
{
    private const long FiveMegaBytes = 5 * 1024 * 1024;
    private const long TenMegaBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public Result Validate(FileUploadRequest request, FileCategory category)
    {
        if (request.Content is null || request.Length <= 0)
            return Result.Failure(FileErrors.EmptyFile);

        var extension = Path.GetExtension(request.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            return Result.Failure(FileErrors.InvalidExtension);

        if (!ContentType.IsSupported(request.ContentType))
            return Result.Failure(FileErrors.InvalidContentType);

        var maxAllowedSize = GetMaxAllowedSize(category);
        if (request.Length > maxAllowedSize)
            return Result.Failure(FileErrors.FileTooLarge);

        return Result.Success();
    }

    private static long GetMaxAllowedSize(FileCategory category)
    {
        return category switch
        {
            FileCategory.CompanyAvatar => FiveMegaBytes,
            FileCategory.EmployeeAvatar => FiveMegaBytes,
            FileCategory.CompanyCover => TenMegaBytes,
            FileCategory.ServicePhoto => TenMegaBytes,
            _ => FiveMegaBytes
        };
    }
}
