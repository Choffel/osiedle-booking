using Files.Application.Interfaces;
using Files.Domain.Enums;
using Files.Domain.ValueObjects;

namespace Files.Infrastructure.Generators;

public class BlobPathGenerator : IBlobPathGenerator
{
    private const string DefaultContainerName = "images";

    public string GenerateBlobName(FileCategory category, Guid targetId, FileId fileId, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var targetSegment = targetId.ToString("D");

        return category switch
        {
            FileCategory.CompanyAvatar => $"companies/{targetSegment}/avatar/{fileId.Value:D}{extension}",
            FileCategory.CompanyCover => $"companies/{targetSegment}/cover/{fileId.Value:D}{extension}",
            FileCategory.EmployeeAvatar => $"employees/{targetSegment}/avatar/{fileId.Value:D}{extension}",
            FileCategory.ServicePhoto => $"services/{targetSegment}/photos/{fileId.Value:D}{extension}",
            _ => $"misc/{targetSegment}/{fileId.Value:D}{extension}"
        };
    }

    public string GetContainerName(FileCategory category) => DefaultContainerName;
}
