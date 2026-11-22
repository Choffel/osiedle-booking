using osiedle_booking.BuildingBlocks;

namespace Files.Domain.Errors;

public static class FileErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Files.NotFound",
        "The requested file was not found.");

    public static readonly Error InvalidContentType = Error.Validation(
        "Files.InvalidContentType",
        "The provided content type is not a supported image format.");

    public static readonly Error InvalidExtension = Error.Validation(
        "Files.InvalidExtension",
        "The file extension is not supported for images.");

    public static readonly Error FileTooLarge = Error.Validation(
        "Files.FileTooLarge",
        "The file size exceeds the allowed limit.");

    public static readonly Error EmptyFile = Error.Validation(
        "Files.EmptyFile",
        "The uploaded file is empty.");

    public static readonly Error TargetEntityRequired = Error.Validation(
        "Files.TargetEntityRequired",
        "A target entity identifier is required for this file category.");

    public static readonly Error UploadFailed = Error.Failure(
        "Files.UploadFailed",
        "Failed to upload the file to storage.");

    public static readonly Error FileNotFoundInStorage = Error.NotFound(
        "Files.FileNotFoundInStorage",
        "The file content was not found in storage.");
}
