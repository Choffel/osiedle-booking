using Business.Domain.Aggregates;
using Business.Domain.ValueObjects.Category;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CategoryId.From(value))
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasConversion(
                slug => slug.Value,
                value => Slug.Create(value))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.DisplayOrder)
            .IsRequired();
        
        builder.HasIndex(c => c.Name)
            .IsUnique();

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.HasQueryFilter(c => c.DeletedAt == null);
    }
}