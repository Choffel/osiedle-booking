namespace Files.Infrastructure.Options;

public class AzureBlobStorageOptions
{
    public const string SectionName = "AzureBlobStorage";

    public string ConnectionString { get; set; } = string.Empty;

    public string ContainerName { get; set; } = "images";

    public string? ServiceUri { get; set; }
}
