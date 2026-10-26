using Bookings.Application.Abstractions.Services;
using Bookings.Domain.Entities.Booking.Enum;
using Bookings.Domain.Exceptions;
using Bookings.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsModule.Integration;

public class CreateBookingFlowTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateBooking_ValidSlot_PersistsConfirmedBooking()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await bookingService.CreateBookingAsync(Request(At(10, 0), At(10, 30)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Value.Should().BeTrue();

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var booking = await db.Bookings.AsNoTracking().SingleAsync();

        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.BusinessId.Value.Should().Be(Business);
        booking.StaffId.Value.Should().Be(Staff);
        booking.ClientId.Value.Should().Be(Client);
        booking.ServiceId.Value.Should().Be(Service);
        booking.Slot.Start.Should().Be(At(10, 0));
        booking.Slot.End.Should().Be(At(10, 30));
        booking.Price.Amount.Should().Be(100m);
        booking.Cancellation.Should().BeNull();
    }

    [Fact]
    public async Task CreateBooking_OverlappingSlot_FailsAndNothingIsPersisted()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.CreateBookingAsync(
            Request(At(10, 15), At(10, 45)), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Overlapping booking found.");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task CreateBooking_AdjacentSlot_Succeeds()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.CreateBookingAsync(
            Request(At(10, 30), At(11, 0)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task CreateBooking_SameTimeDifferentStaff_Succeeds()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.CreateBookingAsync(
            Request(At(10, 0), At(10, 30), staff: OtherStaff), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task CreateBooking_BeforeWorkingHours_Fails()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await bookingService.CreateBookingAsync(Request(At(8, 0), At(8, 30)), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Overlapping booking found.");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateBooking_EndingAfterWorkingHours_Fails()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await bookingService.CreateBookingAsync(Request(At(17, 45), At(18, 15)), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Overlapping booking found.");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateBooking_SlotInPast_ThrowsBookingInPastException()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var past = DateTime.UtcNow.Date.AddDays(-1).AddHours(10);

        // BookingService no longer converts domain violations to Result — the invariant
        // surfaces as a domain exception instead.
        var act = () => bookingService.CreateBookingAsync(
            Request(past, past.AddMinutes(30)), CancellationToken.None);

        await act.Should().ThrowAsync<BookingInPastException>()
            .WithMessage("*past*");

        using var verifyScope = CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Bookings.AsNoTracking().CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateBooking_EndBeforeStart_ThrowsInvalidTimeSlotRangeException()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var act = () => bookingService.CreateBookingAsync(
            Request(At(10, 30), At(10, 0)), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidTimeSlotRangeException>();
    }
}
