using Bookings.Application.Abstractions.Services;
using Bookings.Domain.Entities.Booking.Value_objects;
using Bookings.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingsModule.Integration;

public class AvailableSlotsFlowTests : IntegrationTestBase
{
    // Working hours 09:00-18:00, 30-min service, 10-min step:
    // starts 09:00, 09:10, ... 17:30 -> 52 slots.
    private const int FullDaySlotCount = 52;

    [Fact]
    public async Task GetAvailableSlots_EmptyDay_ReturnsFullSchedule()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var result = await bookingService.GetAvailableSlotsAsync(SlotsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        var slots = result.Value;

        slots.Should().HaveCount(FullDaySlotCount);
        slots.First().Start.Should().Be(At(9, 0));
        slots.First().End.Should().Be(At(9, 30));
        slots.Last().Start.Should().Be(At(17, 30));
        slots.Last().End.Should().Be(At(18, 0));
    }

    [Fact]
    public async Task GetAvailableSlots_WithOccupiedSlot_ExcludesBusyTime()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30));

        var result = await bookingService.GetAvailableSlotsAsync(SlotsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        var slots = result.Value;

        // 4 slots before the busy period (09:00-09:30) + 43 after it (10:30-17:30).
        slots.Should().HaveCount(47);
        slots.Should().NotContain(s =>
            s.Start < At(10, 30) && At(10, 0) < s.End);
        slots.Should().Contain(s => s.Start == At(9, 30));
        slots.Should().Contain(s => s.Start == At(10, 30));
        slots.Should().NotContain(s => s.Start == At(10, 0));
    }

    [Fact]
    public async Task GetAvailableSlots_FullyBookedDay_ReturnsEmpty()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(9, 0), At(12, 0));
        await CreateBookingAsync(bookingService, At(12, 0), At(15, 0));
        await CreateBookingAsync(bookingService, At(15, 0), At(18, 0));

        var result = await bookingService.GetAvailableSlotsAsync(SlotsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAvailableSlots_BookingsOfOtherStaff_AreIgnored()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        await CreateBookingAsync(bookingService, At(10, 0), At(10, 30), staff: OtherStaff);

        var result = await bookingService.GetAvailableSlotsAsync(SlotsRequest(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue(result.Error);
        result.Value.Should().HaveCount(FullDaySlotCount);
    }

    [Fact]
    public async Task GetAvailableSlots_InvalidDuration_Throws()
    {
        using var scope = CreateScope();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var act = () => bookingService.GetAvailableSlotsAsync(
            new Bookings.Application.DTOs.GetAvailableSlotsRequest(Staff, new TimeOnly(0, 0), TestDate),
            CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
