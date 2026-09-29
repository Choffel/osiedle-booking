using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.User;
using Users.Domain.Entity.User.Value_objects;
using Users.Infrastructure.Identity;

namespace Users.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, RoleIdentity, Guid>
{
    

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasConversion<Guid>(
                    id => id.Value,
                    value => new UserId(value))
                .ValueGeneratedNever();

            entity.Property(e => e.DisplayName)
                .IsRequired();
        });

        modelBuilder.Entity<IdentityUserRole<Guid>>(ur =>
        {
            ur.HasKey(r => new { r.UserId, r.RoleId });
            ur.HasOne<RoleIdentity>().WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
        });
        
    }
}