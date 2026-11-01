using Business.Domain.Aggregates;
using Business.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => EmployeeId.From(value));
        
        builder.Property(e => e.BusinessId)
            .HasConversion(
                id => id != null ? id.Value : (Guid?)null,
                value => value.HasValue ? BusinessId.From(value.Value) : null);
        
        builder.ComplexProperty(e => e.EmployeeName, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();
        });
            
        builder.Property(e => e.Position)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Review)
            .HasMaxLength(1000);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(256);
        
        builder.HasIndex(e => e.Email)
            .IsUnique()
            .HasFilter("\"DeletedAt\" IS NULL");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasQueryFilter(e => e.DeletedAt == null);

        builder.HasMany(e => e.BookableServices)
            .WithOne() 
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade); 
        
        builder.Navigation(e => e.BookableServices)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}