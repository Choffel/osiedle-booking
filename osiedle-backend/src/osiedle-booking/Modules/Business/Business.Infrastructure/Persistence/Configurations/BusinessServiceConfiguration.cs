using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence.Configurations;

public class BusinessServiceConfiguration : IEntityTypeConfiguration<Business.Domain.Entities.BusinessService>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Business.Domain.Entities.BusinessService> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ServiceName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
    }
}