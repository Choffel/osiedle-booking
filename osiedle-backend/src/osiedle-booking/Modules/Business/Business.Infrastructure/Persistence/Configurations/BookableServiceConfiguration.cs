using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence.Configurations;

public class BookableServiceConfiguration : IEntityTypeConfiguration<Business.Domain.Entities.BookableService>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Business.Domain.Entities.BookableService> builder)
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
        
        builder.HasIndex(s => s.EmployeeId);
        
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Price).IsRequired().HasColumnType("numeric(18,2)");
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Duration).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt);
    }
}