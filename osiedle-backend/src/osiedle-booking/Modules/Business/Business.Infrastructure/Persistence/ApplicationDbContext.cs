using Microsoft.EntityFrameworkCore;

namespace Business.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {

    }
    public DbSet<Business.Domain.Aggregates.Business> Businesses { get; set; }
    
    public DbSet<Business.Domain.Entities.Employee> Employees { get; set; }
    
    public DbSet<Business.Domain.Entities.BusinessService> BusinessServices { get; set; }
}
        