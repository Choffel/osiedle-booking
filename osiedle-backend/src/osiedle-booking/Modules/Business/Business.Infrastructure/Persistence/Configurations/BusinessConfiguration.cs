
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using osiedle_booking.BuildingBlocks.ValueObjects;

namespace Business.Infrastructure.Persistence.Configurations;

public class BusinessConfiguration : IEntityTypeConfiguration<Business.Domain.Aggregates.Business>
{
    public void Configure(EntityTypeBuilder<Domain.Aggregates.Business> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(
                id => id.Value,
                value => BusinessId.From(value))
            .IsRequired();

        
        builder.Property(b => b.OwnerId)
            .HasConversion(
                id => id.Value,                  
                value => UserId.From(value))     
            .IsRequired();

        
        builder.HasIndex(b => b.OwnerId)
            .IsUnique()
            .HasFilter("DeletedAt IS NULL");

        
        builder.Property(b => b.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(b => b.Status)
            .HasConversion<string>() 
            .HasMaxLength(50)
            .IsRequired();

        builder.ComplexProperty(b => b.Location, locationBuilder =>
        {
            locationBuilder.Property(l => l.Street)
                .HasColumnName("location_street")
                .HasMaxLength(200)
                .IsRequired();

            locationBuilder.Property(l => l.Number)
                .HasColumnName("location_number")
                .HasMaxLength(20)
                .IsRequired();

            locationBuilder.Property(l => l.City)
                .HasColumnName("location_city")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.HasQueryFilter(b => b.DeletedAt == null);
    }
}