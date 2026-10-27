using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Value_objects;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Booking> Bookings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.HasKey(x => x.Id);

            booking.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => new BookingId(value));

            booking.Property(x => x.BusinessId)
                .HasConversion(
                    id => id.Value,
                    value => new BusinessId(value));

            booking.Property(x => x.StaffId)
                .HasConversion(
                    id => id.Value,
                    value => new StaffId(value));

            booking.Property(x => x.ClientId)
                .HasConversion(
                    id => id.Value,
                    value => new ClientId(value));

            booking.Property(x => x.ServiceId)
                .HasConversion(
                    id => id.Value,
                    value => new ServiceId(value));

            booking.Property(x => x.Status)
                .HasConversion<string>();

            booking.OwnsOne(x => x.Slot, slot =>
            {
                slot.Property(p => p.Start).HasColumnName("Slot_Start");
                slot.Property(p => p.End).HasColumnName("Slot_End");
            });

            booking.OwnsOne(x => x.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("Price_Amount");
                price.Property(p => p.Currency).HasColumnName("Price_Currency");
            });

            booking.OwnsOne(x => x.Cancellation, cancellation =>
            {
                cancellation.Property(p => p.By).HasColumnName("Cancellation_By");
                cancellation.Property(p => p.Reason).HasColumnName("Cancellation_Reason");
                cancellation.Property(p => p.CancelledAt).HasColumnName("Cancellation_CancelledAt");
            });
        });
    }
    
}