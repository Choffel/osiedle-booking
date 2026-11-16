using Files.Domain.Enums;
using Files.Domain.ValueObjects;
using osiedle_booking.BuildingBlocks.Domain;

namespace Files.Domain.Aggregates;

public class StoredFile : AggregateRoot<FileId>
{
    public string OriginalFileName { get; private set; } = null!;
    public ContentType ContentType { get; private set; }
    public FileCategory Category { get; private set; }
    public string ContainerName { get; private set; } = null!;
    public string BlobName { get; private set; } = null!;
    public string BlobUri { get; private set; } = null!;
    public long SizeInBytes { get; private set; }
    public Guid? RelatedEntityId { get; private set; }

    private StoredFile() { }

    private StoredFile(
        FileId id,
        string originalFileName,
        ContentType contentType,
        FileCategory category,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid? relatedEntityId) : base(id)
    {
        if (string.IsNullOrWhiteSpace(originalFileName))
            throw new ArgumentException("Original file name cannot be empty.", nameof(originalFileName));

        if (string.IsNullOrWhiteSpace(containerName))
            throw new ArgumentException("Container name cannot be empty.", nameof(containerName));

        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("Blob name cannot be empty.", nameof(blobName));

        if (string.IsNullOrWhiteSpace(blobUri))
            throw new ArgumentException("Blob URI cannot be empty.", nameof(blobUri));

        if (sizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "File size must be greater than zero.");

        if (relatedEntityId.HasValue && relatedEntityId.Value == Guid.Empty)
            throw new ArgumentException("Related entity ID cannot be empty Guid.", nameof(relatedEntityId));

        OriginalFileName = originalFileName;
        ContentType = contentType;
        Category = category;
        ContainerName = containerName;
        BlobName = blobName;
        BlobUri = blobUri;
        SizeInBytes = sizeInBytes;
        RelatedEntityId = relatedEntityId;
        CreatedAt = DateTime.UtcNow;
    }

    public static StoredFile Create(
        FileId id,
        string originalFileName,
        ContentType contentType,
        FileCategory category,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid? relatedEntityId = null)
    {
        return new StoredFile(
            id,
            originalFileName,
            contentType,
            category,
            containerName,
            blobName,
            blobUri,
            sizeInBytes,
            relatedEntityId);
    }

    public static StoredFile CreateCompanyAvatar(
        FileId id,
        string originalFileName,
        ContentType contentType,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

        return new StoredFile(
            id,
            originalFileName,
            contentType,
            FileCategory.CompanyAvatar,
            containerName,
            blobName,
            blobUri,
            sizeInBytes,
            companyId);
    }

    public static StoredFile CreateCompanyCover(
        FileId id,
        string originalFileName,
        ContentType contentType,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

        return new StoredFile(
            id,
            originalFileName,
            contentType,
            FileCategory.CompanyCover,
            containerName,
            blobName,
            blobUri,
            sizeInBytes,
            companyId);
    }

    public static StoredFile CreateEmployeeAvatar(
        FileId id,
        string originalFileName,
        ContentType contentType,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid employeeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID cannot be empty.", nameof(employeeId));

        return new StoredFile(
            id,
            originalFileName,
            contentType,
            FileCategory.EmployeeAvatar,
            containerName,
            blobName,
            blobUri,
            sizeInBytes,
            employeeId);
    }

    public static StoredFile CreateServicePhoto(
        FileId id,
        string originalFileName,
        ContentType contentType,
        string containerName,
        string blobName,
        string blobUri,
        long sizeInBytes,
        Guid serviceId)
    {
        if (serviceId == Guid.Empty)
            throw new ArgumentException("Service ID cannot be empty.", nameof(serviceId));

        return new StoredFile(
            id,
            originalFileName,
            contentType,
            FileCategory.ServicePhoto,
            containerName,
            blobName,
            blobUri,
            sizeInBytes,
            serviceId);
    }

    public void UpdateStorageLocation(string blobName, string blobUri, long sizeInBytes)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            throw new ArgumentException("Blob name cannot be empty.", nameof(blobName));

        if (string.IsNullOrWhiteSpace(blobUri))
            throw new ArgumentException("Blob URI cannot be empty.", nameof(blobUri));

        if (sizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "File size must be greater than zero.");

        BlobName = blobName;
        BlobUri = blobUri;
        SizeInBytes = sizeInBytes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContentType(ContentType contentType)
    {
        ContentType = contentType;
        UpdatedAt = DateTime.UtcNow;
    }
}
