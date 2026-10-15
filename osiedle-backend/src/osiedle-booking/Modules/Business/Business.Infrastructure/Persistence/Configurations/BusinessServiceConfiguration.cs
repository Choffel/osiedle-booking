using Business.Domain.ValueObject;
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence.Configurations;

public class BusinessServiceConfiguration : IEntityTypeConfiguration<Business.Domain.Entities.BusinessService>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Business.Domain.Entities.BusinessService> builder)
    {
        builder.ToTable("BusinessServices");


        builder.HasKey(x => x.Id);
        
        builder.Property(s => s.Id)
            .HasConversion(
                id => id.Value,                     
                value => BusinessServiceId.From(value) 
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
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}