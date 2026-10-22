using Bookings.Domain.Entities.Booking;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Domain.Exceptions;

namespace Bookings.Domain.Tests.Entities;

public class BookingTests
{
    private static readonly BusinessId Business = new(Guid.NewGuid());
    private static readonly StaffId Staff = new(Guid.NewGuid());
    private static readonly ClientId Client = new(Guid.NewGuid());
    private static readonly ServiceId Service = new(Guid.NewGuid());
    private static readonly Money Price = new(100m);

    // Booking.Create rejects slots in the past, so the fixture must always be in the future.
    private static readonly TimeSlot Slot =
        TimeSlot.Create(
            DateTime.UtcNow.Date.AddDays(7).AddHours(9),
            DateTime.UtcNow.Date.AddDays(7).AddHours(10));

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

    [Fact]
    public void Create_SlotInPast_ThrowsBookingInPastException()
    {
        var pastSlot = TimeSlot.Create(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.Date);

        var ex = Assert.Throws<BookingInPastException>(() =>
            Booking.Create(Business, Staff, Client, Service, pastSlot, Price));

        Assert.Equal(pastSlot.Start, ex.AttemptedStart);
        Assert.Contains("past", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_NegativePrice_ThrowsInvalidBookingPriceException()
    {
        var ex = Assert.Throws<InvalidBookingPriceException>(() =>
            Booking.Create(Business, Staff, Client, Service, Slot, new Money(-1m)));

        Assert.Equal(-1m, ex.AttemptedAmount);
    }

    [Fact]
    public void Create_ZeroPrice_IsAllowed()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, new Money(0));

        Assert.Equal(0m, booking.Price.Amount);
    }

    [Fact]
    public void Create_NullSlot_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Booking.Create(Business, Staff, Client, Service, null!, Price));
    }

    [Fact]
    public void Create_NullBusinessId_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            Booking.Create(null!, Staff, Client, Service, Slot, Price));
    }

    [Fact]
    public void Delete_SetsCancelledStatusAndCancellationInfo()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        booking.Delete();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.Cancellation);
        Assert.Equal(CancelledBy.Client, booking.Cancellation!.By);
        Assert.Equal("cancel", booking.Cancellation.Reason);
        Assert.True(booking.Cancellation.CancelledAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Delete_Twice_PreservesOriginalCancellationInfo()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        booking.Delete();
        var original = booking.Cancellation;
        booking.Delete();

        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Same(original, booking.Cancellation);
    }

    [Fact]
    public void IsOwnedBy_MatchingClient_ReturnsTrue()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        Assert.True(booking.IsOwnedBy(Client));
    }

    [Fact]
    public void IsOwnedBy_DifferentClient_ReturnsFalse()
    {
        var booking = Booking.Create(Business, Staff, Client, Service, Slot, Price);

        Assert.False(booking.IsOwnedBy(new ClientId(Guid.NewGuid())));
    }
}
