using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;

namespace Bookings.Domain.Tests.Entities;

public class BookingTests
{
    private static readonly BusinessId Business = BusinessId.From(Guid.NewGuid());
    private static readonly StaffId Staff = StaffId.From(Guid.NewGuid());
    private static readonly ClientId Client = ClientId.From(Guid.NewGuid());
    private static readonly ServiceId Service = ServiceId.From(Guid.NewGuid());
    private static readonly Money Price = Money.Pln(100m);

    // The fixture stays in the future so the suite never depends on the wall clock.
    private static readonly TimeSlot Slot =
        TimeSlot.Create(
            DateTime.UtcNow.Date.AddDays(7).AddHours(9),
            DateTime.UtcNow.Date.AddDays(7).AddHours(10));

    [Fact]
    public void Create_ValidInput_ProducesConfirmedBookingWithGivenValues()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.Equal(Business, booking.BusinessId);
        Assert.Equal(Staff, booking.StaffId);
        Assert.Equal(Client, booking.ClientId);
        Assert.Equal(Service, booking.ServiceId);
        Assert.Equal(Slot, booking.Slot);
        Assert.Equal(Price, booking.Price);
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.Null(booking.Cancellation);
    }

    [Fact]
    public void Create_AssignsNewId_AndSetsCreatedAtToToday()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.NotEqual(Guid.Empty, booking.Id.Value);
        Assert.Equal(DateTime.UtcNow, booking.CreatedAt, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_TwoBookings_HaveDifferentIds()
    {
        var first = Booking.Create(Staff, Business, Client, Service, Slot, Price);
        var second = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void Create_ZeroPrice_IsAllowed()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Money.Pln(0));

        Assert.Equal(0m, booking.Price.Amount);
    }

    [Fact]
    public void CancelByClient_Owner_SetsCancelledStatusAndCancellationInfo()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);
        var cancelledAt = DateTime.UtcNow;

        booking.CancelByClient(Client, cancelledAt);

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.Cancellation);
        Assert.Equal(CancelledBy.Client, booking.Cancellation!.By);
        Assert.Equal(Client.Value, booking.Cancellation.CancelledById);
        Assert.False(string.IsNullOrWhiteSpace(booking.Cancellation.Reason));
        Assert.Equal(cancelledAt, booking.Cancellation.CancelledAt);
    }

    [Fact]
    public void CancelByClient_NotOwner_ThrowsArgumentException()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.Throws<ArgumentException>(() =>
            booking.CancelByClient(ClientId.From(Guid.NewGuid()), DateTime.UtcNow));
    }

    [Fact]
    public void CancelByClient_Twice_ThrowsInvalidOperationException()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        booking.CancelByClient(Client, DateTime.UtcNow);

        Assert.Throws<InvalidOperationException>(() =>
            booking.CancelByClient(Client, DateTime.UtcNow));
    }

    [Fact]
    public void IsOwnedBy_MatchingClient_ReturnsTrue()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.True(booking.IsOwnedBy(Client));
    }

    [Fact]
    public void IsOwnedBy_DifferentClient_ReturnsFalse()
    {
        var booking = Booking.Create(Staff, Business, Client, Service, Slot, Price);

        Assert.False(booking.IsOwnedBy(ClientId.From(Guid.NewGuid())));
    }
}
