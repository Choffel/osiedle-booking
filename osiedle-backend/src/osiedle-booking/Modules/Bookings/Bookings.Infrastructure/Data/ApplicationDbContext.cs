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
                    value => BookingId.From(value));

            booking.Property(x => x.BusinessId)
                .HasConversion(
                    id => id.Value,
                    value => BusinessId.From(value));

            booking.Property(x => x.StaffId)
                .HasConversion(
                    id => id.Value,
                    value => StaffId.From(value));

            booking.Property(x => x.ClientId)
                .HasConversion(
                    id => id.Value,
                    value => ClientId.From(value));

            booking.Property(x => x.ServiceId)
                .HasConversion(
                    id => id.Value,
                    value => ServiceId.From(value));

            booking.Property(x => x.Status)
                .HasConversion<string>();

            booking.ComplexProperty(x => x.Slot, slot =>
            {
                slot.Property(p => p.Start).HasColumnName("Slot_Start");
                slot.Property(p => p.End).HasColumnName("Slot_End");
            });

            booking.ComplexProperty(x => x.Price, price =>
            {
                price.Property(p => p.Amount).HasColumnName("Price_Amount");
                price.Property(p => p.Currency).HasColumnName("Price_Currency");
            });

            booking.ComplexProperty(x => x.Cancellation, cancellation =>
            {
                cancellation.Property(p => p.By).HasColumnName("Cancellation_By");
                cancellation.Property(p => p.Reason).HasColumnName("Cancellation_Reason");
                cancellation.Property(p => p.CancelledAt).HasColumnName("Cancellation_CancelledAt");
            });
        });
    }
    
}