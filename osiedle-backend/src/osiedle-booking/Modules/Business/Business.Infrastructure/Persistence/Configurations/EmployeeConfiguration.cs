using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Business.Infrastructure.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Domain.Entities.Employee>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Employee> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => Domain.ValueObject.EmployeeId.From(value))
            .IsRequired();

        builder.Property(e => e.FirstName)
            .HasMaxLength(10)
            .IsRequired();
        
        builder.Property(e => e.LastName)
            .HasMaxLength(40)
            .IsRequired();
        
        builder.HasQueryFilter(e => e.DeletedAt == null);
        
        builder.HasMany(e => e.BusinessServices)
            .WithOne(s => s.Employee) 
            .HasForeignKey(s => s.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade); 

        
        builder.Navigation(e => e.BusinessServices)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
    
}