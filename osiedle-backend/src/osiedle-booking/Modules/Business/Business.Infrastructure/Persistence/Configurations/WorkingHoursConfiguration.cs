using Business.Domain.Entities;
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("WorkingHours");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => WorkingHoursId.From(value));

        builder.Property(x => x.EmployeeId)
            .HasConversion(
                id => id.Value,
                value => EmployeeId.From(value))
            .IsRequired();

        builder.Property(x => x.Date).IsRequired();
        builder.Property(x => x.StartTime).IsRequired();
        builder.Property(x => x.EndTime).IsRequired();

        builder.HasIndex(x => new { x.EmployeeId, x.Date }).IsUnique();
    }
}
