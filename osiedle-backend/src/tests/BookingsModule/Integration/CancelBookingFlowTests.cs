using Bookings.Application.Abstractions.Services;
using Bookings.Application.DTOs;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsModule.Integration;

public class CancelBookingFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task CancelBooking_ByOwner_SetsCancelledStatusAndCancellationInfo()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingId = await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.CancelBookingAsync(
            new CancelBookingRequest(bookingId), ClientId.From(Client), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var booking = await db.Bookings.AsNoTracking().SingleAsync();

        booking.Status.Should().Be(BookingStatus.Cancelled);
        booking.Cancellation.Should().NotBeNull();
        booking.Cancellation!.By.Should().Be(CancelledBy.Client);
        booking.Cancellation.Reason.Should().NotBeNullOrWhiteSpace();
        booking.Cancellation.CancelledAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task CancelBooking_ByAnotherUser_FailsAndKeepsBookingIntact()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingId = await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.CancelBookingAsync(
            new CancelBookingRequest(bookingId), ClientId.From(OtherClient), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookingError.NotOwnedByClient);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var booking = await db.Bookings.AsNoTracking().SingleAsync();

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.Cancellation.Should().BeNull();
    }

    [Fact]
    public async Task CancelBooking_UnknownId_Fails()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await bookingService.CancelBookingAsync(
            new CancelBookingRequest(Guid.NewGuid()), ClientId.From(Client), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(BookingError.NotFound);
    }

    [Fact]
    public async Task CancelBooking_FreesSlot_ForAvailabilityAndRebooking()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var bookingId = await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var cancelResult = await bookingService.CancelBookingAsync(
            new CancelBookingRequest(bookingId), ClientId.From(Client), CancellationToken.None);
        cancelResult.IsSuccess.Should().BeTrue(cancelResult.Error.Description);

        // The cancelled booking must no longer block the slot:
        // availability shows the whole day again and rebooking the same slot succeeds.
        var slotsResult = await bookingService.GetAvailableSlotsAsync(SlotsRequest(), CancellationToken.None);
        slotsResult.IsSuccess.Should().BeTrue(slotsResult.Error.Description);
        slotsResult.Value.Should().Contain(s => s.Start == At(10, 0));

        var rebookResult = await bookingService.CreateBookingAsync(
            Request(At(10, 0), At(10, 30)), CancellationToken.None);
        rebookResult.IsSuccess.Should().BeTrue(rebookResult.Error.Description);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var bookings = await db.Bookings.AsNoTracking().ToListAsync();
        bookings.Should().HaveCount(2);
        bookings.Count(b => b.Status == BookingStatus.Cancelled).Should().Be(1);
        bookings.Count(b => b.Status == BookingStatus.Confirmed).Should().Be(1);
    }
}
