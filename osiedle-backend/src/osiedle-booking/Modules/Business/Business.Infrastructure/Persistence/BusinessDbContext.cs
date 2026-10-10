using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public class BusinessDbContext : DbContext
{
    public BusinessDbContext(DbContextOptions<BusinessDbContext> options) : base(options)
    {

    }
    public DbSet<Business.Domain.Aggregates.Business> Businesses { get; set; }
    
    public DbSet<Business.Domain.Entities.Employee> Employees { get; set; }
    
    public DbSet<Business.Domain.Entities.BusinessService> BusinessServices { get; set; }
}
        