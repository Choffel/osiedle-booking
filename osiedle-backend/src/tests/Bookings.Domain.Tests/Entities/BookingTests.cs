using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Domain.Tests.Entities;

public class BookingTests
{
    private static readonly BusinessId Business = new(Guid.NewGuid());
    private static readonly StaffId Staff = new(Guid.NewGuid());
    private static readonly ClientId Client = new(Guid.NewGuid());
    private static readonly ServiceId Service = new(Guid.NewGuid());
    private static readonly Money Price = new(100m);
    private static readonly TimeSlot Slot =
        TimeSlot.Create(new DateTime(2026, 10, 15, 9, 0, 0), new DateTime(2026, 10, 15, 10, 0, 0));

    [Fact]
    public void Create_ValidInput_ProducesConfirmedBookingWithGivenValues()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        Assert.Equal(Business, booking.BusinessId);
        Assert.Equal(Staff, booking.StaffId);
        Assert.Equal(Client, booking.ClientId);
        Assert.Equal(Service, booking.ServiceId);
        Assert.Equal(Slot, booking.Slot);
        Assert.Equal(Price, booking.Price);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
    }

    [Fact]
    public void Create_AssignsNewId_AndSetsCreatedAtToToday()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        Assert.NotEqual(Guid.Empty, booking.Id.Value);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), booking.CreatedAt);
    }

    [Fact]
    public void Create_TwoBookings_HaveDifferentIds()
    {
        var first = Booking.Create(Business, Staff, Client, Service, Slot, Price);
        var second = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        Assert.NotEqual(first.Id, second.Id);
    }
}
