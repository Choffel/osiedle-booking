using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using osiedle_booking.BuildingBlocks.ValueObjects;
using Users.Domain.Entity.RefreshToken;
using Users.Domain.Entity.RefreshToken.ValueObjects;
using Users.Domain.Entity.User;
using Users.Infrastructure.Identity.Models;

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
                    value => UserId.From(value))
                .ValueGeneratedNever();

            entity.OwnsOne(e => e.Name, name =>
            {
                name.Property(n => n.FirstName).IsRequired();
                name.Property(n => n.LastName).IsRequired();
            });

            entity.Property(e => e.Email)
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

            entity.Property(e => e.Id)
                .HasConversion<Guid>(
                    id => id.Value,
                    value => RefreshTokenId.From(value))
                .ValueGeneratedNever();

            entity.Property(e => e.UserId)
                .HasConversion<Guid>(
                    id => id.Value,
                    value => UserId.From(value))
                .IsRequired();

            entity.Property(e => e.Token).IsRequired();
        });
        
    }
}