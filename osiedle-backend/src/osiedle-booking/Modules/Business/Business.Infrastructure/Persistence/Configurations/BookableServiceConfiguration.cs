using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public class BookableServiceConfiguration : IEntityTypeConfiguration<Business.Domain.Entities.BookableService>
{
    public void Configure(EntityTypeBuilder<Business.Domain.Entities.BookableService> builder)
    {
        builder.ToTable("BookableServices");

        builder.HasKey(x => x.Id);
        
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,                     
                value => BookableServiceId.From(value) 
            );
        
        builder.Property(s => s.EmployeeId)
            .HasConversion(
                id => id.Value,
                value => EmployeeId.From(value)
            )
            .IsRequired();

        builder.Property(s => s.BusinessId)
            .HasConversion(
                id => id.Value,
                value => BusinessId.From(value)
            )
            .IsRequired();
        
        builder.HasIndex(s => s.EmployeeId);
        builder.HasIndex(s => s.BusinessId);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasIndex(x => x.Category);
        
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Price).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Duration).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
    }
}
