using Files.Domain.Aggregates;
using Files.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Files.Infrastructure.Persistence.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(EntityTypeBuilder<StoredFile> builder)
    {
        builder.ToTable("files");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => FileId.From(value))
            .IsRequired();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasConversion(
                contentType => contentType.Value,
                value => ContentType.From(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.ContainerName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BlobName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.BlobUri)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.SizeInBytes)
            .IsRequired();

        builder.Property(x => x.RelatedEntityId)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(x => new { x.Category, x.RelatedEntityId });
        builder.HasIndex(x => x.CreatedAt);
    }
}
