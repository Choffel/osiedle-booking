using System.Reflection;
using Files.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Files.Infrastructure.Persistence;

public class FilesDbContext : DbContext
{
    public FilesDbContext(DbContextOptions<FilesDbContext> options) : base(options)
    {
    }

    public DbSet<StoredFile> Files => Set<StoredFile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
