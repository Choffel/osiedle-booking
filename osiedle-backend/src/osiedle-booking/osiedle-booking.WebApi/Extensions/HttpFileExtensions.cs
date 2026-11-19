using Files.Application.DTOs;

namespace osiedle_booking.WebApi.Extensions;

public static class HttpFileExtensions
{
    public static FileUploadRequest ToUploadRequest(this IFormFile? file)
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
